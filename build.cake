string target = Argument("target", "Default");
string configuration = Argument("configuration", "Release");

string baseSuffix = "Alpha1";
string dbgSuffix = configuration == "Debug" ? "-dbg" : "";
string versionSuffix = baseSuffix + dbgSuffix;

string PROJECT_DIR = Context.Environment.WorkingDirectory.FullPath + "/";

string NHIBERNATE_DATABASE_TEMPLATE = EnvironmentVariable("NHIBERNATE_DATABASE_TEMPLATE") ?? "MSSQL.cfg.xml";
string NHIBERNATE_DIALECT = EnvironmentVariable("NHIBERNATE_DIALECT"); // if null, don't change from default
string NHIBERNATE_CONNECTION_STRING = EnvironmentVariable("NHIBERNATE_CONNECTION_STRING"); // if null, don't change from default
string DOTNET_FRAMEWORK = EnvironmentVariable("DOTNET_FRAMEWORK") ?? "net461";
string TARGET_PLATFORM = EnvironmentVariable("TARGET_PLATFORM") ?? "x86";

bool IsRunningOnCircleCI = EnvironmentVariable("CIRCLECI") != null;

var ErrorDetail = new List<string>();

bool isDotNetCoreInstalled = false;
Setup(context =>
{
    string buildVersion = versionSuffix;

    if (BuildSystem.IsRunningOnAppVeyor)
    {
        var tag = AppVeyor.Environment.Repository.Tag;

        if (tag.IsTag)
        {
            versionSuffix = "" + dbgSuffix;
            buildVersion = tag.Name + dbgSuffix;
        }
        else
        {
            var buildNumber = AppVeyor.Environment.Build.Number;
            var branch = AppVeyor.Environment.Repository.Branch;
            var isPullRequest = AppVeyor.Environment.PullRequest.IsPullRequest;

            CiNonTagBuildVersion(buildNumber, branch, isPullRequest ? AppVeyor.Environment.PullRequest.Number.ToString() : null);
            buildVersion = versionSuffix;
        }

        AppVeyor.UpdateBuildVersion(buildVersion);
    }
    else if(IsRunningOnCircleCI)
    {
        var tagName = EnvironmentVariable("CIRCLE_TAG");
        if (tagName != null)
        {
            versionSuffix = "" + dbgSuffix;
            buildVersion = tagName + dbgSuffix;
        }
        else
        {
            var buildNumber = int.Parse(EnvironmentVariable("CIRCLE_BUILD_NUM"));
            var branch = EnvironmentVariable("CIRCLE_BRANCH");
            var pullRequestNumber = EnvironmentVariable("CIRCLE_PR_NUMBER");
            var isPullRequest = pullRequestNumber != null;

            CiNonTagBuildVersion(buildNumber, branch, isPullRequest ? pullRequestNumber : null);
            buildVersion = versionSuffix;
        }
    }

    Information("Building version {0} of NHibernate.", buildVersion);
    isDotNetCoreInstalled = CheckIfDotNetCoreInstalled();
});

Task("Restore")
    .Does(() => {});

Task("Build")
    .Description("Builds the .NET 4.61 and .NET Standard 2.0 versions of NHibernate")
    .Does(() =>
    {
        if(!isDotNetCoreInstalled)
        {
            Error("Was not built because .NET Core SDK is not installed");
            return;
        }
        BuildProject("src/NHibernate.sln", configuration);
    });

Task("CopyConfiguration")
    .Description("Setup Test Configuration")
    .OnError(exception => { ErrorDetail.Add(exception.Message); })
    .Does(() => 
    {
        var hibernateConfigPath = new FilePath(PROJECT_DIR + "current-test-configuration/hibernate.cfg.xml");
        CreateDirectory(PROJECT_DIR + "current-test-configuration");
        CopyFile(PROJECT_DIR + "src/NHibernate.Config.Templates/" + NHIBERNATE_DATABASE_TEMPLATE, hibernateConfigPath);


        var pokeSettings = new XmlPokeSettings();
        pokeSettings.Namespaces["nhc"] = "urn:nhibernate-configuration-2.2";

        if (!string.IsNullOrEmpty(NHIBERNATE_DIALECT))
            XmlPoke(hibernateConfigPath, "//*/nhc:property[@name='dialect']", NHIBERNATE_DIALECT, pokeSettings);

        if (!string.IsNullOrEmpty(NHIBERNATE_CONNECTION_STRING))
            XmlPoke(hibernateConfigPath, "//*/nhc:property[@name='connection.connection_string']", NHIBERNATE_CONNECTION_STRING, pokeSettings);
    });

Task("Test")
    .Description("Tests NHibernate")
    .OnError(exception => { ErrorDetail.Add(exception.Message); })
    .Does(() =>
    {
        var framework = DOTNET_FRAMEWORK;
        Information("Testing framework: " + framework);

        var dir = PROJECT_DIR + "src/NHibernate.TestDatabaseSetup/";
        RunDotnetCoreTests(dir, "NHibernate.TestDatabaseSetup.csproj", framework, ref ErrorDetail);

        dir = PROJECT_DIR + "src/NHibernate.Test/";
        RunDotnetCoreTests(dir, "NHibernate.Test.csproj", framework, ref ErrorDetail);

        dir = PROJECT_DIR + "src/NHibernate.Test.VisualBasic/";
        RunDotnetCoreTests(dir, "NHibernate.Test.VisualBasic.vbproj", framework, ref ErrorDetail);
    });

Teardown(context => CheckForError(ref ErrorDetail));

bool CheckIfDotNetCoreInstalled()
{
    try
    {
        Information("Checking if .NET Core SDK is installed");
        StartProcess("dotnet", new ProcessSettings
        {
            Arguments = "--version"
        });
    }
    catch(Exception)
    {
        Warning(".NET Core SDK is not installed. It can be installed from https://www.microsoft.com/net/core");
        return false;
    }
    return true;
}

void CheckForError(ref List<string> errorDetail)
{
    if(errorDetail.Count != 0)
    {
        var copyError = new List<string>();
        copyError = errorDetail.Select(s => s).ToList();
        errorDetail.Clear();
        throw new Exception("One or more unit tests failed, breaking the build.\n"
                              + copyError.Aggregate((x,y) => x + "\n" + y));
    }
}

void BuildProject(string projectPath, string configuration)
{
    int rc = StartProcess(
        "dotnet",
        new ProcessSettings()
            .UseWorkingDirectory(PROJECT_DIR)
            .WithArguments(args => args
                .Append("build")
                .AppendQuoted(projectPath)
                .AppendSwitch("--configuration", configuration)
                .Append("/property:VersionSuffix={0}", versionSuffix)
            )
        );
}

void RunDotnetCoreTests(DirectoryPath workingDir, FilePath testProject, string framework, ref List<string> errorDetail)
{
    string settingsPath = PROJECT_DIR + "src/NHibernate.Test/test-" + TARGET_PLATFORM + ".runsettings";
    int rc = StartProcess(
        "dotnet",
        new ProcessSettings()
            .UseWorkingDirectory(workingDir.FullPath)
            .WithArguments(args => {
                args
                    .Append("test")
                    .AppendQuoted(testProject.FullPath)
                    .AppendSwitch("--configuration", configuration)
                    .AppendSwitch("--framework", framework)
                    .AppendSwitchQuoted("--settings", settingsPath)
                    .Append("/property:VersionSuffix={0}", versionSuffix);

                if (BuildSystem.IsRunningOnAppVeyor)
                {
                    // Report results in real-time: https://github.com/Microsoft/vstest-docs/blob/master/docs/report.md
                    args.Append("--logger:Appveyor");
                }
                else if (IsRunningOnCircleCI)
                {
                    args.AppendQuoted("--logger:junit;LogFilePath=/root/test-reports/" + testProject.GetFilenameWithoutExtension() + ".xml");
                }
                else
                {
//                    args.Append("--logger:junit");
                }
            })
    );

    if (rc > 0)
        errorDetail.Add(string.Format("{0}: {1} tests failed", framework, rc));
    else if (rc < 0)
        errorDetail.Add(string.Format("{0} returned rc = {1}", testProject.GetFilename(), rc));
}

void CiNonTagBuildVersion(int buildNumber, string branch, string pullRequestNumber)
{
    string buildNumberFormatted = buildNumber.ToString("00000");
    bool isPullRequest = pullRequestNumber != null;

    if (branch == "master" && !isPullRequest)
    {
        versionSuffix = "dev-" + buildNumberFormatted + dbgSuffix;
    }
    else
    {
        var suffix = "ci-" + buildNumberFormatted + dbgSuffix;

        if (isPullRequest)
            suffix += "-pr-" + pullRequestNumber;
        else if (branch.StartsWith("release", StringComparison.OrdinalIgnoreCase))
            suffix += "-pre-" + buildNumberFormatted;
        else
            suffix += "-" + System.Text.RegularExpressions.Regex.Replace(branch, "[^0-9A-Za-z-]+", "-");

        // Nuget limits "special version part" to 20 chars. Add one for the hyphen.
        if (suffix.Length > 20)
            suffix = suffix.Substring(0, 20);

        versionSuffix = suffix;
    }
}

Task("AppVeyor")
    .Description("Builds, tests and packages on AppVeyor")
    .IsDependentOn("Build")
    .IsDependentOn("CopyConfiguration")
    .IsDependentOn("Test");

Task("CircleCI")
    .Description("Builds and tests on CircleCI")
    .IsDependentOn("CopyConfiguration")
    .IsDependentOn("Test");

Task("Default")
    .Description("Builds all versions of NHibernate")
    .IsDependentOn("Build");

RunTarget(target);

using System.Data;
using NHibernate.Cfg;
using NHibernate.Dialect.Function;
using NHibernate.Driver;

namespace NHibernate.Dialect
{
	public class MsSql2008Dialect : MsSql2005Dialect
	{
		private static readonly IInternalLogger log = LoggerProvider.LoggerFor(typeof(MsSql2008Dialect));

		protected override void RegisterDateTimeTypeMappings()
		{
			base.RegisterDateTimeTypeMappings();
			RegisterColumnType(DbType.DateTime2, "DATETIME2");
			RegisterColumnType(DbType.DateTimeOffset, "DATETIMEOFFSET");
			RegisterColumnType(DbType.Date, "DATE");
			RegisterColumnType(DbType.Time, "TIME");
		}

		protected override void RegisterFunctions()
		{
			base.RegisterFunctions();
			RegisterFunction("current_timestamp", new NoArgSQLFunction("sysdatetime", NHibernateUtil.DateTime2, true));
			RegisterFunction("current_timestamp_offset", new NoArgSQLFunction("sysdatetimeoffset", NHibernateUtil.DateTimeOffset, true));
		}

		protected override void RegisterKeywords()
		{
			base.RegisterKeywords();
			RegisterKeyword("datetimeoffset");
			RegisterKeyword("datetime2");
			RegisterKeyword("hierarchyid");
		}

		protected override void RegisterDefaultProperties()
		{
			if (!DefaultProperties.ContainsKey(Environment.ConnectionDriver))
			{
				log.Warn("Specify the connection.driver_class");
			}
			base.RegisterDefaultProperties();
//			DefaultProperties[Environment.ConnectionDriver] = typeof(Sql2008ClientDriver).AssemblyQualifiedName;
		}
	}
}
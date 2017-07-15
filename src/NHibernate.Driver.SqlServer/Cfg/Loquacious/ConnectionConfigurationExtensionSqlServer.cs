using NHibernate.Driver;

namespace NHibernate.Cfg.Loquacious
{
	public static class ConnectionConfigurationExtensionSqlServer
	{
		public static IConnectionConfiguration BySqlServerDriver(this IConnectionConfiguration cfg)
		{
			return cfg.By<SqlServerDriver>();
		}

		public static IConnectionConfiguration BySqlServer2008Driver(this IConnectionConfiguration cfg)
		{
			return cfg.By<SqlServer2008Driver>();
		}
	}
}

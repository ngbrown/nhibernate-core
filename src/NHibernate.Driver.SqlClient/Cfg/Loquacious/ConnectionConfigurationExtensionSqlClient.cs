using NHibernate.Driver;

namespace NHibernate.Cfg.Loquacious
{
	public static class ConnectionConfigurationExtensionSqlClient
	{
		public static IConnectionConfiguration BySqlClientDriver(this IConnectionConfiguration cfg)
		{
			return cfg.By<SqlClientDriver>();
		}

		public static IConnectionConfiguration BySql2008ClientDriver(this IConnectionConfiguration cfg)
		{
			return cfg.By<Sql2008ClientDriver>();
		}
	}
}

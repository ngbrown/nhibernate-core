using NHibernate.Driver;

namespace NHibernate.Cfg.Loquacious
{
	public static class ConnectionConfigurationExtensionSqlServerCompact
	{
		public static IConnectionConfiguration BySqlServerCompactDriver(this IConnectionConfiguration cfg)
		{
			return cfg.By<SqlServerCompactDriver>();
		}
	}
}

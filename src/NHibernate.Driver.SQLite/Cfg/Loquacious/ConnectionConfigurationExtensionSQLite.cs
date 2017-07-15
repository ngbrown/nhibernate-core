using NHibernate.Driver;

namespace NHibernate.Cfg.Loquacious
{
	public static class ConnectionConfigurationExtensionSQLite
	{
		public static IConnectionConfiguration BySQLiteDriver(this IConnectionConfiguration cfg)
		{
			return cfg.By<SQLiteDriver>();
		}
	}
}

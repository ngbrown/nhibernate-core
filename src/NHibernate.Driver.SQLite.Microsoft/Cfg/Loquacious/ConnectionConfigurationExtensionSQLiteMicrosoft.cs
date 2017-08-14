using NHibernate.Driver;

namespace NHibernate.Cfg.Loquacious
{
	public static class ConnectionConfigurationExtensionSQLiteMicrosoft
	{
		public static IConnectionConfiguration BySQLiteMicrosoftDriver(this IConnectionConfiguration cfg)
		{
			return cfg.By<SQLiteMicrosoftDriver>();
		}
	}
}

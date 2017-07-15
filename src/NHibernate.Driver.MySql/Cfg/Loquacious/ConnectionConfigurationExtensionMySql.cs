using NHibernate.Driver;

namespace NHibernate.Cfg.Loquacious
{
	public static class ConnectionConfigurationExtensionMySql
	{
		public static IConnectionConfiguration ByMySqlDataDriver(this IConnectionConfiguration cfg)
		{
			return cfg.By<MySqlDataDriver>();
		}
	}
}

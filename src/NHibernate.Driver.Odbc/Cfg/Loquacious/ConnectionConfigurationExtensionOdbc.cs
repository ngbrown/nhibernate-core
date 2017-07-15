using NHibernate.Driver;

namespace NHibernate.Cfg.Loquacious
{
	public static class ConnectionConfigurationExtensionOdbc
	{
		public static IConnectionConfiguration ByOdbcDriver(this IConnectionConfiguration cfg)
		{
			return cfg.By<OdbcDriver>();
		}
	}
}

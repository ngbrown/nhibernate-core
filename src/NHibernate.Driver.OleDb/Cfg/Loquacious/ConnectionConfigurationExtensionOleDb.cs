using NHibernate.Driver;

namespace NHibernate.Cfg.Loquacious
{
	public static class ConnectionConfigurationExtensionOleDb
	{
		public static IConnectionConfiguration ByOleDbDriver(this IConnectionConfiguration cfg)
		{
			return cfg.By<OleDbDriver>();
		}
	}
}

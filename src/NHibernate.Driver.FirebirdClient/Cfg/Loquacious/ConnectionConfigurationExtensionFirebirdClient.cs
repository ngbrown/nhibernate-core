using NHibernate.Driver;

namespace NHibernate.Cfg.Loquacious
{
	public static class ConnectionConfigurationExtensionFirebirdClient
	{
		public static IConnectionConfiguration ByFirebirdClientDriver(this IConnectionConfiguration cfg)
		{
			return cfg.By<FirebirdClientDriver>();
		}
	}
}

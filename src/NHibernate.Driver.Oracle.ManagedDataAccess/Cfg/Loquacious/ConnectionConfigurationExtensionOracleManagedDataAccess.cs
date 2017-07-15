using NHibernate.Driver;

namespace NHibernate.Cfg.Loquacious
{
	public static class ConnectionConfigurationExtensionOracleManagedDataAccess
	{
		public static IConnectionConfiguration ByOracleManagedDataAccessDriver(this IConnectionConfiguration cfg)
		{
			return cfg.By<OracleManagedDataAccessDriver>();
		}
	}
}

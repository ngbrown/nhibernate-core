using NHibernate.Driver;

namespace NHibernate.Cfg.Loquacious
{
	public static class ConnectionConfigurationExtensionNpgsql
	{
		public static IConnectionConfiguration ByNpgsqlDriver(this IConnectionConfiguration cfg)
		{
			return cfg.By<NpgsqlDriver>();
		}
	}
}

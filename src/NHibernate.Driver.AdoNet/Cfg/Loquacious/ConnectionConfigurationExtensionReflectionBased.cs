using NHibernate.Driver;

namespace NHibernate.Cfg.Loquacious
{
	public static class ConnectionConfigurationExtensionReflectionBased
	{
		public static IConnectionConfiguration ByReflectionDriver<TDriver>(this IConnectionConfiguration cfg) where TDriver : ReflectionBasedDriver
		{
			return cfg.By<TDriver>();
		}
	}
}

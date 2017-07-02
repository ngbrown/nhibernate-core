using NHibernate.Engine;

namespace NHibernate.AdoNet
{
	public class SqlClientBatchingBatcherFactory : IBatcherFactory
	{
		public virtual IBatcher CreateBatcher(ConnectionManager connectionManager, IInterceptor interceptor)
		{
			return SqlClientSqlCommandSet.HasBatchImplementation
				? (IBatcher) new SqlClientBatchingBatcher(connectionManager, interceptor)
				: (IBatcher) new NonBatchingBatcher(connectionManager, interceptor);
		}
	}
}
using System;
using System.Data.Common;
using System.Diagnostics;
using MySql.Data.MySqlClient;
using NHibernate.Util;

namespace NHibernate.AdoNet
{
	public class MySqlClientSqlCommandSet : IDisposable
	{
		private static readonly System.Type adapterType;
		private static readonly Action<MySqlDataAdapter> doInitialise;
		private static readonly Action<MySqlDataAdapter, DbCommand> doAppend;
		private static readonly Func<MySqlDataAdapter, int> doExecuteNonQuery;

		private readonly MySqlDataAdapter instance;
		private int countOfCommands;

		static MySqlClientSqlCommandSet()
		{
			adapterType = typeof(MySqlDataAdapter);
			Debug.Assert(adapterType != null, "Could not find MySqlDataAdapter!");

			doInitialise = DelegateHelper.BuildAction(adapterType, "InitializeBatching");
			doAppend = DelegateHelper.BuildAction<DbCommand>(adapterType, "AddToBatch");
			doExecuteNonQuery = DelegateHelper.BuildFunc<int>(adapterType, "ExecuteBatch");
		}

		public MySqlClientSqlCommandSet(int batchSize)
		{
			instance = new MySqlDataAdapter();
			instance = (MySqlDataAdapter) Activator.CreateInstance(adapterType, true);
			doInitialise(instance);
			instance.UpdateBatchSize = batchSize;
		}

		public void Append(DbCommand command)
		{
			doAppend(instance, command);
			countOfCommands++;
		}

		public void Dispose()
		{
			instance.Dispose();
		}

		public int ExecuteNonQuery()
		{
			try
			{
				if (CountOfCommands == 0)
				{
					return 0;
				}

				return doExecuteNonQuery(instance);
			}
			catch (Exception exception)
			{
				throw new HibernateException("An exception occured when executing batch queries", exception);
			}
		}

		public int CountOfCommands
		{
			get { return countOfCommands; }
		}
	}
}
using System.Data;
using System.Data.Common;

namespace NHibernate.Driver
{
	public class SQLiteDriver : DriverBase
	{
		public override bool UseNamedPrefixInSql => true;

		public override bool UseNamedPrefixInParameter => true;

		public override string NamedPrefix => "@";

		public override bool SupportsMultipleOpenReaders => false;

		public override bool SupportsMultipleQueries => true;

		public override DbConnection CreateConnection()
		{
			var connection = new System.Data.SQLite.SQLiteConnection();
			connection.StateChange += Connection_StateChange;
			return connection;
		}

		private static void Connection_StateChange(object sender, StateChangeEventArgs e)
		{
			if ((e.OriginalState == ConnectionState.Broken || e.OriginalState == ConnectionState.Closed || e.OriginalState == ConnectionState.Connecting) &&
			    e.CurrentState == ConnectionState.Open)
			{
				var connection = (DbConnection)sender;
				using (var command = connection.CreateCommand())
				{
					// Activated foreign keys if supported by SQLite.  Unknown pragmas are ignored.
					command.CommandText = "PRAGMA foreign_keys = ON";
					command.ExecuteNonQuery();
				}
			}
		}

		public override DbCommand CreateCommand()
		{
			return new System.Data.SQLite.SQLiteCommand();
		}

		public override IResultSetsCommand GetResultSetsCommand(Engine.ISessionImplementor session)
		{
			return new BasicResultSetsCommand(session);
		}
	}
}

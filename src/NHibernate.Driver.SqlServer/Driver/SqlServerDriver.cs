using System.Data.Common;
using System.Data.SqlClient;
using NHibernate.AdoNet;
using NHibernate.Engine;
using NHibernate.SqlTypes;

namespace NHibernate.Driver
{
	/// <summary>
	/// A NHibernate Driver for using the SqlClient DataProvider
	/// </summary>
	public class SqlServerDriver : DriverBase, IEmbeddedBatcherFactoryProvider
	{
		//http://stackoverflow.com/a/7264795/259946

		/// <summary>
		/// Creates an uninitialized <see cref="DbConnection" /> object for
		/// the SqlServerDriver.
		/// </summary>
		/// <value>An unitialized <see cref="System.Data.SqlClient.SqlConnection"/> object.</value>
		public override DbConnection CreateConnection()
		{
			return new SqlConnection();
		}

		/// <summary>
		/// Creates an uninitialized <see cref="DbCommand" /> object for
		/// the SqlServerDriver.
		/// </summary>
		/// <value>An unitialized <see cref="System.Data.SqlClient.SqlCommand"/> object.</value>
		public override DbCommand CreateCommand()
		{
			return new System.Data.SqlClient.SqlCommand();
		}

		/// <summary>
		/// MsSql requires the use of a Named Prefix in the SQL statement.
		/// </summary>
		/// <remarks>
		/// <see langword="true" /> because MsSql uses "<c>@</c>".
		/// </remarks>
		public override bool UseNamedPrefixInSql
		{
			get { return true; }
		}

		/// <summary>
		/// MsSql requires the use of a Named Prefix in the Parameter.
		/// </summary>
		/// <remarks>
		/// <see langword="true" /> because MsSql uses "<c>@</c>".
		/// </remarks>
		public override bool UseNamedPrefixInParameter
		{
			get { return true; }
		}

		/// <summary>
		/// The Named Prefix for parameters.
		/// </summary>
		/// <value>
		/// Sql Server uses <c>"@"</c>.
		/// </value>
		public override string NamedPrefix
		{
			get { return "@"; }
		}

		/// <summary>
		/// The SqlClient driver does NOT support more than 1 open DbDataReader
		/// with only 1 DbConnection.
		/// </summary>
		/// <value><see langword="false" /> - it is not supported.</value>
		/// <remarks>
		/// MS SQL Server 2000 (and 7) throws an exception when multiple DbDataReaders are
		/// attempted to be opened.  When SQL Server 2005 comes out a new driver will be
		/// created for it because SQL Server 2005 is supposed to support it.
		/// </remarks>
		public override bool SupportsMultipleOpenReaders
		{
			get { return false; }
		}

		protected override void InitializeParameter(DbParameter dbParam, string name, SqlType sqlType)
		{
			base.InitializeParameter(dbParam, name, sqlType);
			SqlClientParameterHelper.SetVariableLengthParameterSize(dbParam, sqlType);
		}

		#region IEmbeddedBatcherFactoryProvider Members

		System.Type IEmbeddedBatcherFactoryProvider.BatcherFactoryClass
		{
#if !NETSTANDARD2_0
			get { return typeof(SqlClientBatchingBatcherFactory); }
#else
			get { return typeof(NonBatchingBatcherFactory); }
#endif
		}

		#endregion

		public override IResultSetsCommand GetResultSetsCommand(ISessionImplementor session)
		{
			return new BasicResultSetsCommand(session);
		}

		public override bool SupportsMultipleQueries
		{
			get { return true; }
		}
	}
}

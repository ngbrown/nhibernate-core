using System.Data;
using System.Data.Common;
using NHibernate.AdoNet;
using NHibernate.Engine.Query;
using NHibernate.SqlTypes;
using Oracle.ManagedDataAccess.Client;

namespace NHibernate.Driver
{
	public class OracleManagedDataAccessDriver : DriverBase, IEmbeddedBatcherFactoryProvider
	{
		private static readonly SqlType GuidSqlType = new SqlType(DbType.Binary, 16);

		public override string NamedPrefix => ":";

		public override bool UseNamedPrefixInParameter => true;

		public override bool UseNamedPrefixInSql => true;

		System.Type IEmbeddedBatcherFactoryProvider.BatcherFactoryClass => typeof(OracleDataClientBatchingBatcherFactory);

		public override DbConnection CreateConnection()
		{
			return new OracleConnection();
		}

		public override DbCommand CreateCommand()
		{
			return new OracleCommand();
		}

		/// <remarks>
		/// This adds logic to ensure that a DbType.Boolean parameter is not created since
		/// ODP.NET doesn't support it.
		/// </remarks>
		protected override void InitializeParameter(DbParameter dbParam, string name, SqlType sqlType)
		{
			// if the parameter coming in contains a boolean then we need to convert it 
			// to another type since ODP.NET doesn't support DbType.Boolean
			switch (sqlType.DbType)
			{
				case DbType.Boolean:
					base.InitializeParameter(dbParam, name, SqlTypeFactory.Int16);
					break;
				case DbType.Guid:
					base.InitializeParameter(dbParam, name, GuidSqlType);
					break;
				case DbType.Xml:
					this.InitializeParameter(dbParam, name, OracleDbType.XmlType);
					break;
				case DbType.Binary:
					this.InitializeParameter(dbParam, name, OracleDbType.Blob);
					break;
				default:
					base.InitializeParameter(dbParam, name, sqlType);
					break;
			}
		}

		protected override void OnBeforePrepare(DbCommand command)
		{
			base.OnBeforePrepare(command);
			OracleCommand oracleCommand = (OracleCommand) command;

			// need to explicitly turn on named parameter binding
			// http://tgaw.wordpress.com/2006/03/03/ora-01722-with-odp-and-command-parameters/
			oracleCommand.BindByName = true;

			var detail = CallableParser.Parse(oracleCommand.CommandText);

			if (!detail.IsCallable)
				return;

			oracleCommand.CommandType = CommandType.StoredProcedure;
			oracleCommand.CommandText = detail.FunctionName;
			oracleCommand.BindByName = false;

			OracleParameter outCursor = (OracleParameter)oracleCommand.CreateParameter();
			outCursor.OracleDbType = OracleDbType.RefCursor;

			outCursor.Direction = detail.HasReturn ? ParameterDirection.ReturnValue : ParameterDirection.Output;

			oracleCommand.Parameters.Insert(0, outCursor);
		}

		private void InitializeParameter(DbParameter dbParam, string name, OracleDbType oracleDbType)
		{
			var oracleDbParam = (OracleParameter) dbParam;
			oracleDbParam.ParameterName = FormatNameForParameter(name);
			oracleDbParam.OracleDbType = oracleDbType;
		}
	}
}

using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.SqlServerCe;
using NHibernate.Cfg;
using NHibernate.SqlTypes;
using NHibernate.Util;

namespace NHibernate.Driver
{
	public class SqlServerCompactDriver : DriverBase
	{
		private bool prepareSql;

		/// <summary>
		/// MsSql requires the use of a Named Prefix in the SQL statement.  
		/// </summary>
		/// <remarks>
		/// <see langword="true" /> because MsSql uses "<c>@</c>".
		/// </remarks>
		public override bool UseNamedPrefixInSql => true;

		/// <summary>
		/// MsSql requires the use of a Named Prefix in the Parameter.  
		/// </summary>
		/// <remarks>
		/// <see langword="true" /> because MsSql uses "<c>@</c>".
		/// </remarks>
		public override bool UseNamedPrefixInParameter => true;

		/// <summary>
		/// The Named Prefix for parameters.  
		/// </summary>
		/// <value>
		/// Sql Server uses <c>"@"</c>.
		/// </value>
		public override string NamedPrefix => "@";

		/// <summary>
		/// The SqlClient driver does NOT support more than 1 open DbDataReader
		/// with only 1 DbConnection.
		/// </summary>
		/// <value><see langword="false" /> - it is not supported.</value>
		/// <remarks>
		/// Ms Sql 2000 (and 7) throws an Exception when multiple DataReaders are 
		/// attempted to be Opened.  When Yukon comes out a new Driver will be 
		/// created for Yukon because it is supposed to support it.
		/// </remarks>
		public override bool SupportsMultipleOpenReaders => false;

		public override void Configure(IDictionary<string, string> settings)
		{
			base.Configure(settings);
			prepareSql = PropertiesHelper.GetBoolean(Environment.PrepareSql, settings, false);
		}

		public override DbConnection CreateConnection()
		{
			return new System.Data.SqlServerCe.SqlCeConnection();
		}

		public override DbCommand CreateCommand()
		{
			return new System.Data.SqlServerCe.SqlCeCommand();
		}

		public override IResultSetsCommand GetResultSetsCommand(Engine.ISessionImplementor session)
		{
			return new BasicResultSetsCommand(session);
		}

		protected override void SetCommandTimeout(DbCommand cmd)
		{
		}

		protected override void InitializeParameter(DbParameter dbParam, string name, SqlType sqlType)
		{
			base.InitializeParameter(dbParam, name, AdjustSqlType(sqlType));

			AdjustDbParamTypeForLargeObjects(dbParam, sqlType);
			if (prepareSql)
			{
				SqlClientParameterHelper.SetVariableLengthParameterSize(dbParam, sqlType);
			}
		}

		private static SqlType AdjustSqlType(SqlType sqlType)
		{
			switch (sqlType.DbType)
			{
				case DbType.AnsiString:
					return new StringSqlType(sqlType.Length);
				case DbType.AnsiStringFixedLength:
					return new StringFixedLengthSqlType(sqlType.Length);
				case DbType.Date:
					return SqlTypeFactory.DateTime;
				case DbType.Time:
					return SqlTypeFactory.DateTime;
				default:
					return sqlType;
			}
		}

		private void AdjustDbParamTypeForLargeObjects(DbParameter dbParam, SqlType sqlType)
		{
			if (sqlType is BinaryBlobSqlType)
			{
				var sqlCeParameter = (SqlCeParameter)dbParam;
				sqlCeParameter.SqlDbType = SqlDbType.Image;
			}
			else if (sqlType is StringClobSqlType)
			{
				var sqlCeParameter = (SqlCeParameter)dbParam;
				sqlCeParameter.SqlDbType = SqlDbType.NText;
			}
		}
	}
}

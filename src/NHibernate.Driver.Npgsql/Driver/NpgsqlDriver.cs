using System;
using System.Data;
using System.Data.Common;

namespace NHibernate.Driver
{
	/// <summary>
	/// The PostgreSQL data provider provides a database driver for PostgreSQL.
	/// </summary>
	/// <remarks>
	/// Please check the products website 
	/// <a href="http://www.postgresql.org/">http://www.postgresql.org/</a>
	/// for any updates and or documentation.
	/// </remarks>
	public class NpgsqlDriver : DriverBase
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="NpgsqlDriver"/> class.
		/// </summary>
		public NpgsqlDriver()
		{
			DriverVersion = typeof(Npgsql.NpgsqlCommand).Assembly.GetName().Version;
		}

		public override bool UseNamedPrefixInSql
		{
			get { return true; }
		}

		public override bool UseNamedPrefixInParameter
		{
			get { return true; }
		}

		public override string NamedPrefix
		{
			get { return ":"; }
		}

		public override bool SupportsMultipleOpenReaders
		{
			get { return false; }
		}

		protected override bool SupportsPreparingCommands
		{
			// NH-2267 Patrick Earl
			get { return true; }
		}

		public override IResultSetsCommand GetResultSetsCommand(Engine.ISessionImplementor session)
		{
			return new BasicResultSetsCommand(session);
		}

		public override bool SupportsMultipleQueries
		{
			get { return true; }
		}

		protected Version DriverVersion { get; }

		protected override void InitializeParameter(DbParameter dbParam, string name, SqlTypes.SqlType sqlType)
		{
			base.InitializeParameter(dbParam, name, sqlType);

			// Since the .NET currency type has 4 decimal places, we use a decimal type in PostgreSQL instead of its native 2 decimal currency type.
			if (sqlType.DbType == DbType.Currency)
				dbParam.DbType = DbType.Decimal;
		}

		// Prior to v3, Npgsql was expecting DateTime for time.
		// https://github.com/npgsql/npgsql/issues/347
		public override bool RequiresTimeSpanForTime => (DriverVersion?.Major ?? 3) >= 3;

		public override DbConnection CreateConnection()
		{
			return new Npgsql.NpgsqlConnection();
		}

		public override DbCommand CreateCommand()
		{
			return new Npgsql.NpgsqlCommand();
		}
	}
}

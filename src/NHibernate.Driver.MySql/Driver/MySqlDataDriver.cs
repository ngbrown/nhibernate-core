using System.Data.Common;

namespace NHibernate.Driver
{
	/// <summary>
	/// Provides a database driver for MySQL.
	/// </summary>
	/// <remarks>
	/// Please check the product's <see href="http://www.mysql.com/products/connector/net/">website</see>
	/// for any updates and/or documentation regarding MySQL.
	/// </remarks>
	public class MySqlDataDriver : DriverBase
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="MySqlDataDriver"/> class.
		/// </summary>
		public MySqlDataDriver()
		{
		}

		/// <summary>
		/// MySql.Data uses named parameters in the sql.
		/// </summary>
		/// <value><see langword="true" /> - MySql uses <c>?</c> in the sql.</value>
		public override bool UseNamedPrefixInSql
		{
			get { return true; }
		}

		/// <summary></summary>
		public override bool UseNamedPrefixInParameter
		{
			get { return true; }
		}

		/// <summary>
		/// MySql.Data use the <c>?</c> to locate parameters in sql.
		/// </summary>
		/// <value><c>?</c> is used to locate parameters in sql.</value>
		public override string NamedPrefix
		{
			get { return "?"; }
		}

		/// <summary>
		/// The MySql.Data driver does NOT support more than 1 open DbDataReader
		/// with only 1 DbConnection.
		/// </summary>
		/// <value><see langword="false" /> - it is not supported.</value>
		public override bool SupportsMultipleOpenReaders
		{
			get { return false; }
		}

		/// <summary>
		/// MySql.Data does not support preparing of commands.
		/// </summary>
		/// <value><see langword="false" /> - it is not supported.</value>
		/// <remarks>
		/// With the Gamma MySql.Data provider it is throwing an exception with the 
		/// message "Expected End of data packet" when a select command is prepared.
		/// </remarks>
		protected override bool SupportsPreparingCommands
		{
			get { return false; }
		}

		public override bool SupportsMultipleQueries
		{
			get { return true; }
		}

		public override bool RequiresTimeSpanForTime => true;

		public override DbConnection CreateConnection()
		{
			return new MySql.Data.MySqlClient.MySqlConnection();
		}

		public override DbCommand CreateCommand()
		{
			return new MySql.Data.MySqlClient.MySqlCommand();
		}

		public override IResultSetsCommand GetResultSetsCommand(Engine.ISessionImplementor session)
		{
			return new BasicResultSetsCommand(session);
		}
	}
}

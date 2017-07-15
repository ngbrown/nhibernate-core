using System.Data;
using System.Data.Common;
using NHibernate.SqlTypes;

namespace NHibernate.Driver
{
	public static class SqlClientParameterHelper
	{
		public const int MaxSizeForAnsiClob = 2147483647; // int.MaxValue
		public const int MaxSizeForClob = 1073741823; // int.MaxValue / 2
		public const int MaxSizeForBlob = 2147483647; // int.MaxValue
		public const int MaxSizeForXml = 2147483647; // int.MaxValue
		public const int MaxSizeForLengthLimitedAnsiString = 8000;
		public const int MaxSizeForLengthLimitedString = 4000;
		public const int MaxSizeForLengthLimitedBinary = 8000;
		public const byte MaxPrecision = 28;
		public const byte MaxScale = 5;
		public const byte MaxDateTime2 = 8;
		public const byte MaxDateTimeOffset = 10;

		public static void SetDefaultParameterSize(DbParameter dbParam, SqlType sqlType)
		{
			switch (dbParam.DbType)
			{
				case DbType.AnsiString:
				case DbType.AnsiStringFixedLength:
					dbParam.Size = IsAnsiText(dbParam, sqlType) ? MaxSizeForAnsiClob : MaxSizeForLengthLimitedAnsiString;
					break;
				case DbType.Binary:
					dbParam.Size = IsBlob(dbParam, sqlType) ? MaxSizeForBlob : MaxSizeForLengthLimitedBinary;
					break;
				case DbType.Decimal:
					((IDbDataParameter) dbParam).Precision = MaxPrecision;
					((IDbDataParameter) dbParam).Scale = MaxScale;
					break;
				case DbType.String:
				case DbType.StringFixedLength:
					dbParam.Size = IsText(dbParam, sqlType) ? MaxSizeForClob : MaxSizeForLengthLimitedString;
					break;
				case DbType.DateTime2:
					dbParam.Size = MaxDateTime2;
					break;
				case DbType.DateTimeOffset:
					dbParam.Size = MaxDateTimeOffset;
					break;
				case DbType.Xml:
					dbParam.Size = MaxSizeForXml;
					break;
			}
		}

		/// <summary>
		/// Interprets if a parameter is a Clob (for the purposes of setting its default size)
		/// </summary>
		/// <param name="dbParam">The parameter</param>
		/// <param name="sqlType">The <see cref="SqlType" /> of the parameter</param>
		/// <returns>True, if the parameter should be interpreted as a Clob, otherwise False</returns>
		public static bool IsAnsiText(DbParameter dbParam, SqlType sqlType)
		{
			return ((DbType.AnsiString == dbParam.DbType || DbType.AnsiStringFixedLength == dbParam.DbType) && sqlType.LengthDefined && (sqlType.Length > MaxSizeForLengthLimitedAnsiString));
		}

		/// <summary>
		/// Interprets if a parameter is a Clob (for the purposes of setting its default size)
		/// </summary>
		/// <param name="dbParam">The parameter</param>
		/// <param name="sqlType">The <see cref="SqlType" /> of the parameter</param>
		/// <returns>True, if the parameter should be interpreted as a Clob, otherwise False</returns>
		public static bool IsText(DbParameter dbParam, SqlType sqlType)
		{
			return (sqlType is StringClobSqlType) || ((DbType.String == dbParam.DbType || DbType.StringFixedLength == dbParam.DbType) && sqlType.LengthDefined && (sqlType.Length > MaxSizeForLengthLimitedString));
		}

		/// <summary>
		/// Interprets if a parameter is a Blob (for the purposes of setting its default size)
		/// </summary>
		/// <param name="dbParam">The parameter</param>
		/// <param name="sqlType">The <see cref="SqlType" /> of the parameter</param>
		/// <returns>True, if the parameter should be interpreted as a Blob, otherwise False</returns>
		public static bool IsBlob(DbParameter dbParam, SqlType sqlType)
		{
			return (sqlType is BinaryBlobSqlType) || ((DbType.Binary == dbParam.DbType) && sqlType.LengthDefined && (sqlType.Length > MaxSizeForLengthLimitedBinary));
		}

		public static void SetVariableLengthParameterSize(DbParameter dbParam, SqlType sqlType)
		{
			SetDefaultParameterSize(dbParam, sqlType);

			// no longer override the defaults using data from SqlType, since LIKE expressions needs larger columns
			// https://nhibernate.jira.com/browse/NH-3036
			//if (sqlType.LengthDefined && !IsText(dbParam, sqlType) && !IsBlob(dbParam, sqlType))
			//{
			//	dbParam.Size = sqlType.Length;
			//}

			if (sqlType.PrecisionDefined)
			{
				((IDbDataParameter) dbParam).Precision = sqlType.Precision;
				((IDbDataParameter) dbParam).Scale = sqlType.Scale;
			}
		}
	}
}
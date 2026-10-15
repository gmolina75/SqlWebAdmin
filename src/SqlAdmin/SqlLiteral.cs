using System;
using System.Globalization;

namespace SqlAdmin {
	/// <summary>
	/// Formats CLR values as Transact-SQL literals.
	/// </summary>
	/// <remarks>
	/// Every value is escaped or encoded here instead of at the call site so that
	/// string data (which may contain quotes, backslashes or unicode) can never be
	/// interpreted as SQL. Numeric and date formatting is culture invariant: a
	/// script generated on a machine with a Spanish regional setting must not
	/// produce "3,14" and must not depend on the server's locale.
	/// </remarks>
	public static class SqlLiteral {

		/// <summary>
		/// Converts a value into a Transact-SQL literal.
		/// </summary>
		/// <param name="value">The value to convert. null and DBNull become NULL.</param>
		/// <returns>A string that can be embedded in a Transact-SQL statement.</returns>
		public static string Format(object value) {
			if (value == null || value is DBNull)
				return "NULL";

			if (value is string)
				return FormatString((string)value);

			if (value is char)
				return FormatString(((char)value).ToString());

			if (value is bool)
				return (bool)value ? "1" : "0";

			if (value is DateTime)
				return "'" + ((DateTime)value).ToString("yyyy-MM-ddTHH:mm:ss.fff", CultureInfo.InvariantCulture) + "'";

			if (value is DateTimeOffset)
				return "'" + ((DateTimeOffset)value).ToString("yyyy-MM-ddTHH:mm:ss.fffzzz", CultureInfo.InvariantCulture) + "'";

			if (value is TimeSpan)
				return "'" + ((TimeSpan)value).ToString("c", CultureInfo.InvariantCulture) + "'";

			if (value is Guid)
				return "'" + ((Guid)value).ToString("D") + "'";

			if (value is byte[])
				return FormatBinary((byte[])value);

			if (value is float)
				return FormatFloating((float)value);

			if (value is double)
				return FormatFloating((double)value);

			if (value is decimal)
				return ((decimal)value).ToString(CultureInfo.InvariantCulture);

			// All remaining numeric types render identically in every culture.
			if (value is sbyte || value is byte || value is short || value is ushort ||
				value is int || value is uint || value is long || value is ulong)
				return ((IFormattable)value).ToString(null, CultureInfo.InvariantCulture);

			// Fall back to a quoted, escaped string so nothing is ever emitted raw.
			return FormatString(Convert.ToString(value, CultureInfo.InvariantCulture));
		}

		/// <summary>
		/// Escapes a string as a unicode SQL literal (N'...').
		/// </summary>
		/// <param name="value">The string to escape. An empty string is returned as ''.</param>
		/// <returns>A quoted literal with embedded quotes doubled.</returns>
		public static string FormatString(string value) {
			if (value == null)
				return "NULL";

			return "N'" + value.Replace("'", "''") + "'";
		}

		/// <summary>
		/// Formats a byte array as a hexadecimal binary literal (0x...).
		/// </summary>
		/// <param name="value">The bytes to format. null becomes NULL.</param>
		/// <returns>A literal such as 0x0102FF, or the empty literal 0x for no bytes.</returns>
		public static string FormatBinary(byte[] value) {
			if (value == null)
				return "NULL";

			char[] digits = new char[value.Length * 2];
			const string hex = "0123456789ABCDEF";

			for (int i = 0; i < value.Length; i++) {
				digits[i * 2] = hex[value[i] >> 4];
				digits[i * 2 + 1] = hex[value[i] & 0x0F];
			}

			return "0x" + new string(digits);
		}

		private static string FormatFloating(float value) {
			if (float.IsNaN(value) || float.IsInfinity(value))
				throw new ArgumentException("NaN and Infinity have no Transact-SQL literal.", "value");

			return value.ToString("R", CultureInfo.InvariantCulture);
		}

		private static string FormatFloating(double value) {
			if (double.IsNaN(value) || double.IsInfinity(value))
				throw new ArgumentException("NaN and Infinity have no Transact-SQL literal.", "value");

			return value.ToString("R", CultureInfo.InvariantCulture);
		}
	}
}

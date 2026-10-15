using System;

namespace SqlAdmin {
	/// <summary>
	/// Helpers to quote and validate Transact-SQL identifiers (database, table,
	/// column and owner names) so that names coming from user input or from the
	/// server catalog can never break out of the statement they are embedded in.
	/// </summary>
	/// <remarks>
	/// SQL Server identifier quoting uses brackets: a name is delimited with '['
	/// and ']', and an embedded ']' is escaped by doubling it. This is the only
	/// quoting style that works for every valid identifier, including names that
	/// contain spaces, reserved words or quotes.
	/// </remarks>
	public static class SqlIdentifier {

		/// <summary>
		/// Wraps an identifier in brackets, escaping any embedded ']'.
		/// </summary>
		/// <param name="identifier">The identifier to quote (e.g. a table name).</param>
		/// <returns>The quoted identifier, e.g. "my]table" becomes "[my]]table]".</returns>
		/// <exception cref="ArgumentNullException">The identifier is null.</exception>
		/// <exception cref="ArgumentException">The identifier is empty or contains illegal characters.</exception>
		public static string Quote(string identifier) {
			if (identifier == null)
				throw new ArgumentNullException("identifier");
			if (identifier.Length == 0)
				throw new ArgumentException("Identifier cannot be empty.", "identifier");
			if (identifier.IndexOf('\0') >= 0 || identifier.IndexOf('\r') >= 0 || identifier.IndexOf('\n') >= 0)
				throw new ArgumentException("Identifier contains illegal control characters.", "identifier");

			return "[" + identifier.Replace("]", "]]") + "]";
		}

		/// <summary>
		/// Quotes a two part name (owner.object) suitable for use after FROM/INTO/UPDATE.
		/// </summary>
		/// <param name="owner">The schema/owner name. A null or empty owner yields a single part name.</param>
		/// <param name="name">The object name.</param>
		/// <returns>The quoted two part (or single part) name.</returns>
		public static string QuoteQualified(string owner, string name) {
			if (name == null)
				throw new ArgumentNullException("name");

			if (owner == null || owner.Length == 0)
				return Quote(name);

			return Quote(owner) + "." + Quote(name);
		}

		/// <summary>
		/// Determines whether a string can be used as a quoted identifier.
		/// </summary>
		/// <param name="identifier">The candidate identifier.</param>
		/// <returns>true when the name is non-empty and contains no illegal characters.</returns>
		public static bool IsValid(string identifier) {
			if (identifier == null || identifier.Length == 0)
				return false;

			for (int i = 0; i < identifier.Length; i++) {
				char c = identifier[i];
				if (c == '\0' || c == '\r' || c == '\n' || c == ';' || c == '"' || c == '\'')
					return false;
			}
			return true;
		}
	}
}

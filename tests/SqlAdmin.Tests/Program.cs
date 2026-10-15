using System;
using System.Globalization;
using SqlAdmin;

namespace SqlAdmin.Tests
{
	/// <summary>
	/// Self-contained unit tests for the SqlAdmin library.
	/// No external test framework is used so the suite builds and runs
	/// offline with nothing but MSBuild and the .NET Framework 4.8 targeting pack.
	/// Exit code 0 = all tests passed, 1 = at least one failure.
	/// </summary>
	internal static class Program
	{
		private static int failures;
		private static int passed;

		private static void Check(string name, Action test)
		{
			try
			{
				test();
				passed++;
				Console.WriteLine("PASS  " + name);
			}
			catch (Exception ex)
			{
				failures++;
				Console.WriteLine("FAIL  " + name + ": " + ex.Message);
			}
		}

		private static void AreEqual(string expected, string actual, string label)
		{
			if (!String.Equals(expected, actual, StringComparison.Ordinal))
				throw new Exception(label + " - expected [" + expected + "] got [" + actual + "]");
		}

		private static void True(bool condition, string label)
		{
			if (!condition)
				throw new Exception(label);
		}

		private static void Throws<T>(Action action, string label) where T : Exception
		{
			try
			{
				action();
			}
			catch (T)
			{
				return;
			}
			catch (Exception ex)
			{
				throw new Exception(label + " - wrong exception type " + ex.GetType().Name);
			}
			throw new Exception(label + " - no exception was thrown");
		}

		private static int Main()
		{
			// ---------------------------------------------------------------
			// SqlServer.GetConnectionString
			// ---------------------------------------------------------------
			Check("ConnectionString_SQLAuthentication", delegate
			{
				SqlServer server = new SqlServer("myserver", "sa", "secret", false);
				AreEqual("Server=myserver; User ID=sa; Password=secret", server.GetConnectionString(), "sql auth");
			});

			Check("ConnectionString_IntegratedSecurity", delegate
			{
				SqlServer server = new SqlServer("myserver");
				AreEqual("Server=myserver; Trusted_Connection=Yes;", server.GetConnectionString(), "integrated");
			});

			Check("ConnectionString_EmptyServerNameOmitted", delegate
			{
				SqlServer server = new SqlServer("", "appuser", "secret", false);
				AreEqual("User ID=appuser; Password=secret", server.GetConnectionString(), "empty name");
			});

			// ---------------------------------------------------------------
			// SqlServer.SplitSqlBatches (extracted from Query for testability)
			// ---------------------------------------------------------------
			Check("SplitSqlBatches_SingleStatement", delegate
			{
				string[] batches = SqlServer.SplitSqlBatches("select 1");
				True(batches.Length == 1, "expected 1 batch, got " + batches.Length);
				AreEqual("select 1", batches[0], "content");
			});

			Check("SplitSqlBatches_TwoBatches_LowercaseGo_Crlf", delegate
			{
				string[] batches = SqlServer.SplitSqlBatches("select 1\r\ngo\r\nselect 2");
				True(batches.Length == 2, "expected 2 batches, got " + batches.Length);
				AreEqual("select 1", batches[0], "first");
				AreEqual("select 2", batches[1], "second");
			});

			Check("SplitSqlBatches_MixedNewlines_UppercaseGo", delegate
			{
				string[] batches = SqlServer.SplitSqlBatches("select 1\nGO\nselect 2\ngo\nselect 3");
				True(batches.Length == 3, "expected 3 batches, got " + batches.Length);
				AreEqual("select 1", batches[0], "first");
				AreEqual("select 2", batches[1], "second");
				AreEqual("select 3", batches[2], "third");
			});

			Check("SplitSqlBatches_TrailingGoProducesNoEmptyBatch", delegate
			{
				string[] batches = SqlServer.SplitSqlBatches("select 1\r\ngo");
				True(batches.Length == 1, "expected 1 batch, got " + batches.Length);
				AreEqual("select 1", batches[0], "content");
			});

			Check("SplitSqlBatches_EmptyBatchesAreSkipped", delegate
			{
				string[] batches = SqlServer.SplitSqlBatches("select 1\r\ngo\r\n\r\n\r\ngo\r\nselect 2");
				True(batches.Length == 2, "expected 2 batches, got " + batches.Length);
				AreEqual("select 1", batches[0], "first");
				AreEqual("select 2", batches[1], "second");
			});

			Check("SplitSqlBatches_EmptyInputYieldsNoBatches", delegate
			{
				string[] batches = SqlServer.SplitSqlBatches("");
				True(batches.Length == 0, "expected 0 batches, got " + batches.Length);
			});

			Check("SplitSqlBatches_NullThrows", delegate
			{
				Throws<ArgumentNullException>(delegate { SqlServer.SplitSqlBatches(null); }, "null query");
			});

			// ---------------------------------------------------------------
			// SqlServer.ValidateDatabaseName (connection string injection guard)
			// ---------------------------------------------------------------
			Check("ValidateDatabaseName_AcceptsNormalNames", delegate
			{
				SqlServer.ValidateDatabaseName("my_database");
				SqlServer.ValidateDatabaseName("db with space");
				SqlServer.ValidateDatabaseName("");
				SqlServer.ValidateDatabaseName(null);
			});

			Check("ValidateDatabaseName_RejectsSemicolon", delegate
			{
				Throws<ArgumentException>(delegate { SqlServer.ValidateDatabaseName("db;Password=hacked"); }, "semicolon");
			});

			Check("ValidateDatabaseName_RejectsEqualsSign", delegate
			{
				Throws<ArgumentException>(delegate { SqlServer.ValidateDatabaseName("db=x"); }, "equals");
			});

			Check("ValidateDatabaseName_RejectsQuote", delegate
			{
				Throws<ArgumentException>(delegate { SqlServer.ValidateDatabaseName("db\"x"); }, "quote");
			});

			// ---------------------------------------------------------------
			// Security.WebServer must be safe when no config is available
			// (regression test for the NullReferenceException fix)
			// ---------------------------------------------------------------
			Check("Security_WebServer_NullConfigDoesNotThrow", delegate
			{
				Security security = new Security();
				AreEqual(String.Empty, security.WebServer, "WebServer outside a web context");
			});

			// ---------------------------------------------------------------
			// SqlIdentifier.Quote / QuoteQualified / IsValid
			// ---------------------------------------------------------------
			Check("Identifier_Quote_SimpleName", delegate
			{
				AreEqual("[customers]", SqlIdentifier.Quote("customers"), "simple");
			});

			Check("Identifier_Quote_EscapesClosingBracket", delegate
			{
				AreEqual("[my]]table]", SqlIdentifier.Quote("my]table"), "escaped bracket");
			});

			Check("Identifier_Quote_EmbeddedSemicolonIsHarmless", delegate
			{
				// The name stays inside brackets, so it cannot end the statement.
				AreEqual("[drop table x;]", SqlIdentifier.Quote("drop table x;"), "semicolon inside brackets");
			});

			Check("Identifier_Quote_ReservedWordIsSafe", delegate
			{
				AreEqual("[order]", SqlIdentifier.Quote("order"), "reserved word");
			});

			Check("Identifier_Quote_NameWithSpace", delegate
			{
				AreEqual("[my table]", SqlIdentifier.Quote("my table"), "space");
			});

			Check("Identifier_Quote_NullThrows", delegate
			{
				Throws<ArgumentNullException>(delegate { SqlIdentifier.Quote(null); }, "null");
			});

			Check("Identifier_Quote_EmptyThrows", delegate
			{
				Throws<ArgumentException>(delegate { SqlIdentifier.Quote(""); }, "empty");
			});

			Check("Identifier_Quote_CarriageReturnThrows", delegate
			{
				Throws<ArgumentException>(delegate { SqlIdentifier.Quote("a\r\nb"); }, "newline");
			});

			Check("Identifier_QuoteQualified_UsesOwnerAndName", delegate
			{
				AreEqual("[dbo].[Orders]", SqlIdentifier.QuoteQualified("dbo", "Orders"), "two part");
			});

			Check("Identifier_QuoteQualified_EmptyOwnerYieldsSinglePart", delegate
			{
				AreEqual("[Orders]", SqlIdentifier.QuoteQualified("", "Orders"), "single part");
				AreEqual("[Orders]", SqlIdentifier.QuoteQualified(null, "Orders"), "null owner");
			});

			Check("Identifier_QuoteQualified_EscapesBothParts", delegate
			{
				AreEqual("[we]]ird].[ta]]ble]", SqlIdentifier.QuoteQualified("we]ird", "ta]ble"), "both escaped");
			});

			Check("Identifier_IsValid_AcceptsNormalNames", delegate
			{
				True(SqlIdentifier.IsValid("customers"), "simple");
				True(SqlIdentifier.IsValid("order"), "reserved");
				True(SqlIdentifier.IsValid("Nombre de] cliente"), "bracket name is valid once quoted");
			});

			Check("Identifier_IsValid_RejectsBadNames", delegate
			{
				True(!SqlIdentifier.IsValid(null), "null");
				True(!SqlIdentifier.IsValid(""), "empty");
				True(!SqlIdentifier.IsValid("a;b"), "semicolon");
				True(!SqlIdentifier.IsValid("a\nb"), "newline");
				True(!SqlIdentifier.IsValid("a\"b"), "double quote");
				True(!SqlIdentifier.IsValid("a'b"), "single quote");
			});

			// ---------------------------------------------------------------
			// SqlLiteral - escaping and culture invariant formatting
			// ---------------------------------------------------------------
			Check("Literal_NullIsNull", delegate
			{
				AreEqual("NULL", SqlLiteral.Format(null), "null");
				AreEqual("NULL", SqlLiteral.Format(DBNull.Value), "dbnull");
			});

			Check("Literal_StringEscapesSingleQuote", delegate
			{
				AreEqual("N'O''Brien'", SqlLiteral.Format("O'Brien"), "quote escaped");
			});

			Check("Literal_StringKeepsInjectionAsData", delegate
			{
				// The classic injection attempt must come out as a quoted value.
				string literal = SqlLiteral.Format("'; drop table users;--");
				AreEqual("N'''; drop table users;--'", literal, "payload is data");
				True(literal.StartsWith("N'") && literal.EndsWith("'"), "still a single literal");
			});

			Check("Literal_EmptyStringIsNotEmptyLiteral", delegate
			{
				AreEqual("N''", SqlLiteral.Format(""), "empty");
			});

			Check("Literal_UnicodeIsPreserved", delegate
			{
				AreEqual("N'año'", SqlLiteral.Format("año"), "unicode");
			});

			Check("Literal_BoolIsOneOrZero", delegate
			{
				AreEqual("1", SqlLiteral.Format(true), "true");
				AreEqual("0", SqlLiteral.Format(false), "false");
			});

			Check("Literal_IntegersRenderExactly", delegate
			{
				AreEqual("42", SqlLiteral.Format(42), "int");
				AreEqual("-7", SqlLiteral.Format((long)-7), "long");
				AreEqual("9223372036854775807", SqlLiteral.Format(Int64.MaxValue), "max long");
			});

			Check("Literal_DecimalIsCultureInvariant", delegate
			{
				CultureInfo previous = CultureInfo.CurrentCulture;
				try
				{
					// es-ES uses ',' as the decimal separator. The generated SQL must not.
					CultureInfo.CurrentCulture = new CultureInfo("es-ES");
					AreEqual("3.14", SqlLiteral.Format(3.14m), "decimal");
				}
				finally
				{
					CultureInfo.CurrentCulture = previous;
				}
			});

			Check("Literal_DoubleIsCultureInvariant", delegate
			{
				CultureInfo previous = CultureInfo.CurrentCulture;
				try
				{
					CultureInfo.CurrentCulture = new CultureInfo("de-DE");
					string literal = SqlLiteral.Format(2.5d);
					True(literal.IndexOf(',') < 0, "no decimal comma in " + literal);
					True(literal.StartsWith("2.5"), "expected 2.5 but got " + literal);
				}
				finally
				{
					CultureInfo.CurrentCulture = previous;
				}
			});

			Check("Literal_DateTimeIsIso8601", delegate
			{
				AreEqual("'2026-10-15T08:30:00.000'",
					SqlLiteral.Format(new DateTime(2026, 10, 15, 8, 30, 0)), "datetime");
			});

			Check("Literal_DateTimeIsCultureInvariant", delegate
			{
				CultureInfo previous = CultureInfo.CurrentCulture;
				try
				{
					CultureInfo.CurrentCulture = new CultureInfo("de-DE");
					AreEqual("'2026-10-15T08:30:00.000'",
						SqlLiteral.Format(new DateTime(2026, 10, 15, 8, 30, 0)), "iso under de-DE");
				}
				finally
				{
					CultureInfo.CurrentCulture = previous;
				}
			});

			Check("Literal_GuidIsQuoted", delegate
			{
				Guid guid = new Guid("0f8fad5b-d9cb-469f-a165-70867728950e");
				AreEqual("'0f8fad5b-d9cb-469f-a165-70867728950e'", SqlLiteral.Format(guid), "guid");
			});

			Check("Literal_BinaryIsHexWithTwoDigitsPerByte", delegate
			{
				// Bytes below 0x10 must be padded; the old exporter wrote one digit
				// and produced an odd length literal SQL Server rejects.
				AreEqual("0x0A0BFF", SqlLiteral.FormatBinary(new byte[] { 0x0A, 0x0B, 0xFF }), "padded hex");
			});

			Check("Literal_EmptyBinary", delegate
			{
				AreEqual("0x", SqlLiteral.FormatBinary(new byte[0]), "empty");
			});

			Check("Literal_NullBinaryIsNull", delegate
			{
				AreEqual("NULL", SqlLiteral.FormatBinary(null), "null");
			});

			Check("Literal_ByteArrayValueUsesHex", delegate
			{
				AreEqual("0x0102", SqlLiteral.Format(new byte[] { 1, 2 }), "byte[] via Format");
			});

			Check("Literal_CharIsQuoted", delegate
			{
				// "'" is escaped to "''" and then wrapped: N + 4 quotes.
				AreEqual("N''''", SqlLiteral.Format('\''), "single quote char");
			});

			Check("Literal_NaNThrows", delegate
			{
				Throws<ArgumentException>(delegate { SqlLiteral.Format(Double.NaN); }, "double nan");
				Throws<ArgumentException>(delegate { SqlLiteral.Format(Single.PositiveInfinity); }, "float infinity");
			});

			Check("Literal_FormatString_NullIsNull", delegate
			{
				AreEqual("NULL", SqlLiteral.FormatString(null), "null string");
			});

			Console.WriteLine();
			Console.WriteLine(passed + " passed, " + failures + " failed");
			return failures == 0 ? 0 : 1;
		}
	}
}

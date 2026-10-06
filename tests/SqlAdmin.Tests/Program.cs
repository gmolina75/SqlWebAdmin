using System;
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

			Console.WriteLine();
			Console.WriteLine(passed + " passed, " + failures + " failed");
			return failures == 0 ? 0 : 1;
		}
	}
}

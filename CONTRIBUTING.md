# Contributing to SqlWebAdmin

Thanks for helping revive this project. It is a 2007 CodePlex codebase on
.NET Framework 4.8, so a few conventions keep changes safe.

## Ground rules

- **No NuGet packages.** The libraries and the tests build offline with nothing
  but MSBuild and the .NET Framework 4.8 targeting pack. `src/ExternalReferences`
  holds the third-party binaries.
- **.NET Framework 4.8** everywhere. The test project is SDK-style (`net48`,
  `LangVersion` 7.3); the two class libraries are legacy-style projects, so any
  new `.cs` file must also be registered in the `<Compile Include=...>` item group
  of its `.csproj` — otherwise it silently does not compile.
- **No generated XML docs in commits.** `documentation.xml` is build output.

## Building and testing

```bash
dotnet build tests/SqlAdmin.Tests/SqlAdmin.Tests.csproj -c Release
./tests/SqlAdmin.Tests/bin/Release/net48/SqlAdmin.Tests.exe
```

The runner exits `0` only when every test passes. Before pushing, also
precompile the site, because it catches broken `.aspx`/`.ascx` markup that the
library build cannot see:

```bash
mkdir -p src/SqlWebAdmin/Bin
cp src/SqlAdmin/bin/SqlAdmin.dll               src/SqlWebAdmin/Bin/
cp src/ContextMenu/bin/Release/ContextMenu.dll  src/SqlWebAdmin/Bin/
cp src/ExternalReferences/AjaxControlToolkit.dll src/SqlWebAdmin/Bin/

C:/Windows/Microsoft.NET/Framework64/v4.0.30319/aspnet_compiler.exe \
    -v /SqlWebAdmin -p "$PWD/src/SqlWebAdmin" /tmp/precompiled
```

CI runs exactly these steps on `windows-latest`; treat a local pass of both as
the bar for "done".

## Writing SQL safely

Never concatenate user input into a statement. Use the helpers that already
exist and are already tested:

| You have | Use |
| --- | --- |
| A table/column/owner name | `SqlIdentifier.Quote(name)` / `SqlIdentifier.QuoteQualified(owner, name)` |
| A value for an `INSERT`/`UPDATE` in generated script | `SqlLiteral.Format(value)` |
| A value for a statement you execute now | a `SqlParameter` via `SqlServer.ExecuteNonQuery(...)` |
| A database name for a connection string | `SqlServer.ValidateDatabaseName(name)` |

`SqlLiteral` formats numbers and dates with the **invariant** culture, so a
script generated on a machine with a Spanish or German regional setting still
runs. Keep it that way — do not call `ToString()` on a `float`, `decimal` or
`DateTime` when the result goes into SQL.

If you touch statement building, add a test to
`tests/SqlAdmin.Tests/Program.cs`. The suite has no framework: add a
`Check("Name", delegate { ... })` block using the existing `AreEqual`, `True`
and `Throws<T>` helpers.

## Tests

- Keep the suite dependency-free and offline-runnable.
- One `Check` block per behaviour, named for what it proves
  (`Identifier_Quote_EscapesClosingBracket`, not `TestQuote`).
- Test the failure paths too: null, empty, injection-shaped input.
- Do not add `try/catch` that swallows an exception to make a test pass; fix the
  code under test.

## Styles and markup

- `src/SqlWebAdmin/admin.css` is the only stylesheet. Tokens live in the `:root`
  block at the top — **use `var(--...)` rather than raw hex**, so the palette
  stays in one place.
- **Do not rename or remove existing classes.** The ~62 legacy pages reference
  them, and some are set from code-behind (`selectedLink`, `emptyState`,
  `tableHeader`). New classes are additive.
- The structural band colours (`#c0c0c0` header, `#99ccff` crumbs, `#6699ff`
  rail) match the shipped logo GIFs in `images/`. Changing them requires
  checking `logo_top.gif` and `logo_bottom.gif` for a visible rectangle.
- Every interactive element needs `:focus-visible`; every data state needs
  default, hover, focus, disabled, empty and error.

## Pull requests

1. Keep changes focused — one concern per PR.
2. Describe *why*, and name what you verified (build, tests, precompile, page
   you loaded).
3. If you could not verify something, say so explicitly rather than implying it
   passes.
4. Update `CHANGELOG.md` under `[Unreleased]` and adjust `README.md` when
   behaviour, layout or commands change.

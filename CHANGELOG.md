# Changelog

All notable changes to this project are documented here. The format follows
[Keep a Changelog](https://keepachangelog.com/en/1.1.0/); dates use
`YYYY-MM-DD`.

## [Unreleased]

### Added

- **`SqlIdentifier`** (`src/SqlAdmin/SqlIdentifier.cs`) — bracket quoting for SQL
  identifiers. `Quote` doubles an embedded `]`, `QuoteQualified` handles
  `owner.object`, and `IsValid` rejects names with control characters or
  statement delimiters. Table, column and owner names in `SqlTable` now go
  through it, so a table named `x]; drop ...` stays data instead of ending the
  statement.
- **`SqlLiteral`** (`src/SqlAdmin/SqlLiteral.cs`) — converts CLR values to
  Transact-SQL literals in one tested place: `NULL`, `N'...'` with escaped
  quotes, `1`/`0` for booleans, ISO-8601 dates, `Guid`, and `0x...` binary with
  **two hex digits per byte**. Numbers and dates use the invariant culture, so
  scripts no longer depend on the server or machine regional settings.
- **`SqlServer.ExecuteNonQuery(commandText, database, params SqlParameter[])`**
  — runs a non-query statement with parameters and returns the affected row
  count; validates the database name and disposes the connection in all paths.
- **Row limit on the table view.** `SqlTable.GetTableData(int maxRows)` and a
  200-row cap on `viewTableData.aspx`, with an on-page note pointing at the Query
  page. Viewing a huge table no longer pulls every row into memory.
- **Empty states.** `Databases.aspx` and `Tables.aspx` now explain an empty
  result instead of rendering a bare header bar.
- **Design tokens.** `admin.css` is reorganised around a `:root` block of CSS
  custom properties: surfaces, four ink levels, line weights, one accent,
  semantic colours, control tokens, a radius scale and a 4px spacing scale.
- **States that were missing.** `:focus-visible` rings on every interactive
  element, hover/active/disabled states for buttons, `prefers-reduced-motion`,
  and tabular numerals on right-aligned columns.
- **Login UX.** `Login.aspx` rebuilt as a focused card with associated labels,
  `autocomplete`/`autofocus`, a hint for the local instance name, and
  error/success banners (`.banner`, `.banner--error`, `.banner--success`).
- New classes: `.banner*`, `.emptyState`, `.loginCard`, `.loginField`,
  `.loginActions`, `.fieldHint`, `.fieldError`, `.fieldLabel`. All additive —
  no existing class was renamed or removed.
- **`CONTRIBUTING.md`** and this changelog.

### Fixed

- **SQL injection in the row editor.** `viewTableData.aspx.cs` built its
  `UPDATE` by concatenating cell values into the statement. It now quotes
  identifiers with `SqlIdentifier` and passes every value as a `SqlParameter`.
- **`NullReferenceException` when the requested table does not exist.** The
  code asked the table for its primary keys *before* checking for null, so the
  error page was never reached; the null check now comes first, and a missing
  or empty `table` query string redirects instead of throwing.
- **Row updates with no editable key column.** Previously this produced a
  malformed `UPDATE ... WHERE` and an exception; it is now rejected with a
  readable message.
- **Stack traces rendered to the browser.** The table view dumped
  `ex.ToString()` into the page. Errors are written to `Trace` and only the
  message, HTML-encoded, is shown.
- **Corrupted SQL files when loading a script.** `QueryDatabase.aspx.cs` read
  the upload one byte at a time and cast each to `char`, destroying every
  non-ASCII byte (accents, `ñ`, UTF-8). It now decodes with
  `StreamReader` + BOM detection, and reports an empty upload instead of
  throwing.
- **Database connection leaked on query failure.** `server.Disconnect()` was
  skipped whenever a non-`SqlException` escaped; it now runs in a `finally`,
  and unexpected errors are shown HTML-encoded rather than reaching the global
  error page and losing the user's query text.
- **Odd-length hex in exported scripts.** Binary values below `0x10` were
  written with a single digit (`0x0A` → `0xA`), producing a literal SQL Server
  rejects.
- **Culture-dependent export.** Dates and decimals in `ScriptData` used the
  current culture (`15/10/2026`, `3,14`); both are now ISO-8601 and
  invariant.
- **Doubled quotes** in the section divider: the `.blue_dotted_line` spacer row
  was inflated to ~20px by the text baseline; it is pinned to 3px.

### Changed

- `admin.css` rewritten around tokens while keeping every selector the pages
  already use. Typography moves to a 14px Segoe UI base with a weight-based
  hierarchy; tables get a dark header, zebra rows and row hover; buttons,
  inputs and the modal are restyled.
- Buttons read **Sign in** instead of **Login**.
- `viewTableData.aspx` shows a "Row updated." confirmation and reloads the grid
  after an edit, instead of leaving stale data on screen.

### Documentation

- README: new build commands for the class libraries, tests and
  `aspnet_compiler`; the full list of what the 47 tests cover; a section on the
  interface tokens and which colours must not change; known limitations; and an
  updated TODO.

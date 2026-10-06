# SqlWebAdmin

[![build](https://github.com/gmolina75/SqlWebAdmin/actions/workflows/build.yml/badge.svg)](https://github.com/gmolina75/SqlWebAdmin/actions/workflows/build.yml)

A tool to help people administer MSSQL through web pages.

There's **phpmyadmin** for MySQL, **WHY NO ONE FOR MSSQL!!??**

![A snapshot to show what the tool look like.](doc/e35ca15a-0c92-4ff8-9e30-8749c823a664.jpg)

I picked up this project from CodePlex archives: https://archive.codeplex.com/?p=smsweb

As HaiyanDu7@gmail.com was saying on the archive page:

> **Project Description**
>
> - SQL Server Management Studio in ASP.Net
> - ASP.NET SQL SERVER ADMIN
> - SQL Server Management
> - SQL Server Studio
> - SQL Administration
> - SQL Admin
> - SQL management
> - c# SQL Server

> this project is trying to provide similar functions as Microsoft SQL Management Studio.
> This project is under the development. Project is not reaching beta version yet.
> But it already has many functions you can use. Please try it and submit the bugs when I continue on development.
> Anyone is welcome to join this project.
> Send me an email at HaiyanDu7@gmail.com if you would like contribute something.
> My vision to use this project is for web hosting company.
> Security Policy needs more work, especially need more testing, to make sure that there is no SQL user can see other's data.

Since 2007, this project became an orphan, technique has been moving forward far from it, I decide to make it live again.

*beside I really need this functionality to assist my remote work.*

Project layout:
----------------------
- `src/SQLWebAdmin.sln` — solution with three projects.
- `src/SqlAdmin` — C# class library with the SQL Server model (databases, tables, users, roles, logins, SMO-style helpers).
- `src/ContextMenu` — custom server control used for the right-click menus.
- `src/SqlWebAdmin` — the ASP.NET Web Forms site (~90 `.aspx` pages).
- `src/ExternalReferences` — third-party binaries (`AjaxControlToolkit.dll`), no NuGet.
- `tests/SqlAdmin.Tests` — unit tests for the class library.
- `.github/workflows/build.yml` — CI pipeline.

Building:
----------------------
- Open `src/SQLWebAdmin.sln` in Visual Studio 2022+ with the **.NET Framework 4.8 Developer Pack** installed.
- Build the `SqlAdmin` and `ContextMenu` class libraries first, then run the `SqlWebAdmin` web site (IIS / IIS Express).
- For page-level debugging set `debug="true"` in `src/SqlWebAdmin/Web.config`; keep it `false` when publishing.

Tests:
----------------------
- Tests live in `tests/SqlAdmin.Tests` and have **no NuGet dependencies** (SDK-style project targeting `net48`).
- Build and run them with:

  ```
  msbuild tests/SqlAdmin.Tests/SqlAdmin.Tests.csproj /restore /p:Configuration=Release
  tests/SqlAdmin.Tests/bin/Release/net48/SqlAdmin.Tests.exe
  ```

- The runner prints a summary and exits with code `0` only when every test passes.
- Current coverage: SQL batch splitting, database-name validation, connection-string guards and null-safe security helpers.

Continuous integration:
----------------------
- `.github/workflows/build.yml` runs on every push and pull request on `windows-latest`:
  1. builds `SqlAdmin`, `ContextMenu` and the tests with MSBuild,
  2. runs the test executable and fails the job if any test fails,
  3. copies the freshly built DLLs into `src/SqlWebAdmin/Bin`,
  4. precompiles the whole site with `aspnet_compiler` so broken pages fail the build.

Security notes:
----------------------
- No connection strings live in `Web.config`; every user signs in with their own SQL Server credentials, and a guard rejects database names that try to smuggle statements into the connection string.
- The login ticket stores only the login type (never a password) and travels in an `HttpOnly` cookie with `SameSite=Lax`; signing out calls `FormsAuthentication.SignOut()` instead of just navigating away.
- Error pages HTML-encode exception details and query strings (including the error code, now parsed defensively), so crafted input cannot inject markup.
- Security headers are added on every response: `X-Frame-Options`, `X-Content-Type-Options`, `X-XSS-Protection` and `Referrer-Policy`.
- Session expiry redirects to `Login.aspx?error=sessionexpired` instead of a page that never existed.

Recent fixes:
----------------------
- Build restored from a clean clone: removed the missing strong-name key, enabled `AllowUnsafeBlocks` in Release and referenced `System.Configuration`.
- Database roles actually work now: create/delete role pages were stubs — they write to the server, the "Create new role" link is enabled again, and role removal is exposed on `SqlDatabaseRole`.
- `SqlServer.Query` no longer swallows exceptions (proper `finally`/`Dispose`), and `String.Format` placeholders that silently rendered the wrong value are fixed.
- Redirects that pass names in the query string are URL-encoded (logins, users, roles, databases).
- Impersonation restores the original Windows identity (`Undo()`), the dead `throw ex` was removed, and `Session_End` no longer pretends to clean up state.
- Obsolete `ConfigurationSettings` replaced with `ConfigurationManager`; generated XML docs are no longer tracked.
- Global error page shows a friendly message and logs the real exception server-side.

TODO:
----------------------
- [x] Make it work with **SQL standard authentication** (done)
- [x] Automated build + tests + CI (done)
- [ ] Reimplement database roles / users on modern APIs instead of the legacy DMO interop
- [ ] Move to ASP.NET **MVC** instead of WebForm

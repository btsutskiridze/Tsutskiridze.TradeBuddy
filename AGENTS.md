# AGENTS.md

## .NET
- Prefer `microsoft-learn` for official Microsoft topics such as C#, .NET, ASP.NET Core, EF Core, Azure, SQL Server integration, Microsoft Identity, middleware, DI, auth, logging, hosting, and version-specific Microsoft platform guidance.
- Prefer `context7` for third-party NuGet packages, non-Microsoft libraries, package-specific APIs, migrations, setup, and current package usage.
- When both platform guidance and package guidance matter, use both.
- Before answering, align with the actual project stack and version from files like `*.csproj`, `global.json`, `Directory.Build.props`, `Program.cs`, and package references.
- Do not default to generic latest-version examples when the project uses a specific target framework or package version.

## git
- Format: TYPE(scope - optional): message
- Message must be 4–7 words total
- Use lowercase except proper names
- No ending punctuation
- No description after short message
- Choose the correct type (feat, fix, refactor, chore, docs, test, build, ci, perf, style)
- Include scope only if clear from changes
- Base the message only on the actual diff
- Do not invent changes
- Prefer clarity over detail

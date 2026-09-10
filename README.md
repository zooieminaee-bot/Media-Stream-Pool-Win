# Media Stream Pool Win

Native Windows desktop application for indexing and analyzing media endpoints and authorized API payloads.

## Technology

- C# / .NET 10
- WinUI 3 / Windows App SDK 2.4.0
- MVVM with CommunityToolkit.Mvvm
- Microsoft.Extensions.DependencyInjection
- HttpClient
- SQLite via Microsoft.Data.Sqlite
- xUnit

Microsoft lists Windows App SDK 2.4.0 as the current stable release as of August 13, 2026. The application targets Windows 10 build 17763 or later and Windows 11. See Microsoft's support/versioning documentation for the current platform matrix.

## Architecture

```text
MediaStreamPool
├── src/MediaStreamPool.App
│   ├── WinUI shell
│   ├── MainWindow
│   └── Pages
├── src/MediaStreamPool.Presentation
│   └── ViewModels
├── src/MediaStreamPool.Domain
│   ├── Models
│   └── Interfaces
├── src/MediaStreamPool.Infrastructure
│   ├── Database
│   ├── Repositories
│   ├── Decoder
│   ├── Scanner
│   └── DependencyInjection
└── tests/MediaStreamPool.Tests
```

## Current implementation

- Native WinUI 3 shell. No browser or localhost UI is required.
- Dashboard with real SQLite counts.
- Scanner page with real HTTP fetch, progress reporting, payload analysis, URL extraction, classification and deduplication.
- Authorized Base64 → AES-256-CBC → PKCS7 → GZIP → UTF-8 decoding.
- AES key and IV are read from environment variables and are not hard-coded.
- SQLite database under the user's local application data directory.
- Repository abstraction for persisted records.
- xUnit coverage for URL extraction, classification and decoder configuration.
- Honest empty states for unfinished UI sections instead of fake records.
- Windows CI workflow for restore, build and test.

## Decoder configuration

For authorized data, configure the transport key and IV in the process environment:

```powershell
$env:MEDIA_POOL_AES_KEY_HEX="<64 hex characters>"
$env:MEDIA_POOL_AES_IV_HEX="<32 hex characters>"
```

The application does not bypass DRM, authentication, access controls or service restrictions.

## Build

Requires the .NET 10 SDK and a Windows development environment with the Windows SDK/WinUI tooling required by Windows App SDK.

```powershell
dotnet restore MediaStreamPool.slnx
dotnet build MediaStreamPool.slnx --configuration Release
dotnet test MediaStreamPool.slnx --configuration Release
```

The repository is being developed incrementally. Installer packaging, complete migrations/FTS search, the remaining data-management pages, health checks, export and release hardening are not yet claimed as complete until implemented and verified.

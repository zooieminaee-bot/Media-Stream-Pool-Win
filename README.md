# Media Stream Pool Win

Windows-oriented local source indexer for authorized media APIs and stream endpoints.

## Current MVP

- Go core with no external runtime dependency.
- Base64 → AES-256-CBC → PKCS7 → GZIP decoder when `MEDIA_POOL_AES_KEY_HEX` and `MEDIA_POOL_AES_IV_HEX` are supplied.
- URL discovery from decoded/plain payloads.
- Classification for API, HLS, DASH, M3U, RTMP and HTTP stream candidates.
- Local persistent JSON database (SQLite migration is next).
- Live search and type filtering.
- Detail drawer with source, status, format, host, path and decoded payload.
- Local HTTP UI on `127.0.0.1:8765`.

## Run

```powershell
go run ./cmd/mediapool
```

Open `http://127.0.0.1:8765`.

## Decoder configuration

For authorized data, configure the transport key and IV as environment variables instead of committing secrets:

```powershell
$env:MEDIA_POOL_AES_KEY_HEX="<64 hex characters>"
$env:MEDIA_POOL_AES_IV_HEX="<32 hex characters>"
go run ./cmd/mediapool
```

The application only indexes endpoints accessible to the user and does not attempt to bypass DRM, authentication or access controls.

## Roadmap

1. SQLite storage + FTS search.
2. GitHub repository crawler for source files.
3. Recursive JSON/M3U/HTML extraction.
4. Browser worker using Playwright for authorized JS-backed pages.
5. Windows desktop shell (WebView2/Tauri or Wails) and installer.
6. Endpoint health checks, tags, favorites and export.

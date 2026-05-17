# Changelog

## [0.2.0] - 2026-05-17

### Added
- `InvoiceUrl()` method to build public checkout URLs (`app.stendly.com/checkout?invoice=...`)
- `VerificationStatus` enum (`Unverified`, `Pending`, `Verified`, `Rejected`)
- `VerificationStatusLabel` property on `MerchantProfile`
- `Environment` property on `StendlyClient`

### Changed
- `MerchantProfile.VerificationStatus` type from `string?` to `VerificationStatus?`

### Fixed
- User-Agent string (`dotnet-sdk-sdk` → `dotnet-sdk`)

## [0.1.1] - 2026-05-17

### Fixed
- Version bump and packaging fixes

## [0.1.0] - 2026-05-11

### Added
- Initial release of `Stendly` NuGet package
- `StendlyClient` implementing `IStendlyClient` with configurable `HttpClient`
- Payment Intents: `CreateAsync`, `RetrieveAsync`
- Terminals: `CreateAsync`, `ListAsync`
- Webhooks: `UpdateAsync`, `ConstructEvent`
- Merchant: `GetProfileAsync`, `GetStatsAsync`
- Typed exceptions: `StendlyException`, `StendlyAuthenticationException`, `StendlyValidationException`, `StendlyRateLimitException`, `StendlyApiConnectionException`, `StendlySignatureVerificationException`
- Data models: `PaymentIntent`, `Terminal`, `MerchantProfile`, `MerchantStats`, `DailyStats`, `WebhookEvent`, `WebhookData`
- Automatic API key prefix validation (`st_live_`)
- Environment auto-detection from API key
- XML documentation file generation
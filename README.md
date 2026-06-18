# Stendly .NET SDK

Non-custodial payments on Solana — official .NET SDK for [Stendly API](https://stendly.com). Accept USDC payments in your .NET applications with 0% merchant fees, instant settlement, and no chargebacks.

[![NuGet version](https://img.shields.io/nuget/v/Stendly.svg)](https://www.nuget.org/packages/Stendly/)
[![.NET](https://img.shields.io/badge/.NET-8.0+-blue.svg)](https://dotnet.microsoft.com/)
[![License: MIT](https://img.shields.io/nuget/l/Stendly.svg)](https://github.com/stendly-dev/dotnet-sdk/blob/main/LICENSE)
[![Documentation](https://img.shields.io/badge/docs-latest-blue.svg)](https://stendly.com/en-us/docs/sdk/dotnet/)

## Table of Contents

- [Features](#features)
- [Installation](#installation)
- [Requirements](#requirements)
- [Quick Start](#quick-start)
- [Authentication](#authentication)
- [Payment Intents](#payment-intents)
- [Webhooks](#webhooks)
- [Error Handling](#error-handling)
- [API Reference](#api-reference)
- [Data Models](#data-models)
- [Integration Examples](#integration-examples)
- [Production Checklist](#production-checklist)
- [Troubleshooting](#troubleshooting)
- [Contributing](#contributing)
- [License & Links](#license--links)

---

## Features

- **🔒 Secure**: Webhook signature verification with constant-time comparison & replay attack protection
- **⚡ Fast**: `HttpClient`-based with connection pooling, automatic retries with exponential backoff
- **🎯 Type-safe**: Full C# nullable annotations and XML documentation
- **🛡️ Robust**: Comprehensive error handling with typed exception classes
- **💪 Production-ready**: .NET standard patterns with `IStendlyClient` interface for DI and mocking

---

## Installation

```bash
dotnet add package Stendly
```

Or via Package Manager Console:

```powershell
Install-Package Stendly
```

Or install from source:

```bash
git clone https://github.com/stendly-dev/dotnet-sdk.git
cd dotnet-sdk/
dotnet build
```

---

## Requirements

- .NET 8.0+ (.NET 10.0 recommended)

---

## Quick Start

### 1. Get your API key

Log into your [Stendly Dashboard](https://dashboard.stendly.com) and navigate to API Keys. Copy your secret key (starts
with `st_live_`). Use `environment: "devnet"` for development and `environment: "mainnet"` for production.

### 2. Install the SDK

```bash
dotnet add package Stendly
```

### 3. Initialize the client

The constructor requires an `HttpClient` and `apiKey`:

```csharp
using Stendly;

// Initialize with HttpClient and API key
var client = new StendlyClient(new HttpClient(), "st_live_your_api_key_here");

// Create a payment intent
var intent = await client.Intents.CreateIntentAsync(
    amountCents: 4999,     // $49.99
    orderId: "order_001"
);

Console.WriteLine($"Payment reference: {intent.ReferenceAddress}");
Console.WriteLine($"Destination: {intent.DestinationAddress}");
Console.WriteLine($"Expires at: {intent.ExpiresAt}");

// Check payment status
var retrieved = await client.Intents.RetrieveIntentAsync(intent.Id);
Console.WriteLine($"Status: {retrieved.Status}");
```

**With dependency injection (ASP.NET Core):**

```csharp
// Program.cs
builder.Services.AddHttpClient<IStendlyClient, StendlyClient>((sp, client) =>
{
    var config = sp.GetRequiredService<IConfiguration>();
    return new StendlyClient(client, config["Stendly:ApiKey"]);
});
```

---

## Authentication

### API Key Format

Stendly uses secret API keys that start with `st_live_`.

Use the `environment` parameter to select the network:
- `environment: "mainnet"` — Production
- `environment: "devnet"` — Development/sandbox

**Never commit API keys to version control!**

```csharp
// Good: Load from configuration
var client = new StendlyClient(new HttpClient(), configuration["Stendly:ApiKey"]);

// Good: Environment variable
var client = new StendlyClient(
    new HttpClient(),
    Environment.GetEnvironmentVariable("STENDLY_API_KEY")
);

// Bad: Hardcoded (DO NOT DO THIS)
var client = new StendlyClient(new HttpClient(), "st_live_xxxxx"); // ❌
```

### Environment Selection

The SDK uses `st_live_` key prefix for both environments. Set `environment` explicitly:

```csharp
var client = new StendlyClient(new HttpClient(), "st_live_xxx", environment: "mainnet");
var client = new StendlyClient(new HttpClient(), "st_live_xxx", environment: "devnet");
```

**Note:** The same `st_live_` key prefix is used for both environments. Set `environment: "devnet"` for development/testing.

---

## Payment Intents

### Creating a Payment Intent

```csharp
var client = new StendlyClient(new HttpClient(), "st_live_...");

var intent = await client.Intents.CreateIntentAsync(
    amountCents: 4999,         // $49.99
    orderId: "PREMIUM-001"    // Your order reference
);

Console.WriteLine($"Escrow: {intent.ReferenceAddress}");
Console.WriteLine($"Payout to: {intent.DestinationAddress}");
Console.WriteLine($"Expires: {intent.ExpiresAt}");
```

### Checking Payment Status

```csharp
var intent = await client.Intents.RetrieveIntentAsync(intentId);
if (intent.Status == PaymentIntentStatus.Paid)
{
    DeliverGoods();
}
```

### Using Terminals

```csharp
var terminal = await client.Terminals.CreateTerminalAsync("Store Counter 1");

var intent = await client.Intents.CreateIntentAsync(
    amountCents: 1000,
    orderId: "walk-in-order",
    terminalId: terminal.Id
);
```

### Payment Intent Lifecycle

```
PENDING → PAID
   ↓
EXPIRED (after 30 min)
   ↓
CANCELLED (manual)
```

---

## Webhooks

### Verifying Signatures

**Critical:** Always verify webhook signatures before processing. The method is **async** and expects `byte[]` payload.

```csharp
using Stendly;
using Stendly.Exceptions;

var client = new StendlyClient(new HttpClient(), "st_live_...");
var webhookSecret = Environment.GetEnvironmentVariable("STENDLY_WEBHOOK_SECRET");

// ASP.NET Core controller
[HttpPost("webhooks/stendly")]
public async Task<IActionResult> HandleWebhook()
{
    var signature = Request.Headers["X-Stendly-Signature"].FirstOrDefault();
    if (string.IsNullOrEmpty(signature))
    {
        return BadRequest("Missing signature");
    }

    // Read raw body as bytes (not string)
    byte[] payload;
    using (var ms = new MemoryStream())
    {
        await Request.Body.CopyToAsync(ms);
        payload = ms.ToArray();
    }

    try
    {
        // ConstructEventAsync is async and takes byte[] payload
        var webhookEvent = await client.Webhooks.ConstructEventAsync(
            payload,
            signature,
            webhookSecret!
        );

        // webhookEvent.Event contains the event type string
        if (webhookEvent.Event == "payment_intent.succeeded")
        {
            FulfillOrder(webhookEvent.Data.OrderId, webhookEvent.Data.AmountCents);
        }

        return Ok();
    }
    catch (StendlySignatureVerificationException ex)
    {
        _logger.LogWarning(ex, "Invalid webhook signature");
        return BadRequest("Invalid signature");
    }
}
```

### Webhook Signing (How It Works)

```
signature = HMAC-SHA256(secret, timestamp + payload)
header = "t={timestamp},v1={signature}"
```

---

## Error Handling

All SDK errors inherit from `StendlyException`:

```csharp
using Stendly;
using Stendly.Exceptions;

try
{
    var intent = await client.Intents.CreateIntentAsync(1000, "test");
}
catch (StendlyAuthenticationException ex)
{
    _logger.LogError(ex, "Auth failed");
}
catch (StendlyValidationException ex)
{
    _logger.LogWarning(ex, "Invalid input");
}
catch (StendlyRateLimitException ex)
{
    _logger.LogInformation("Rate limited");
}
catch (StendlyApiConnectionException ex)
{
    _logger.LogError(ex, "Network error");
}
catch (StendlyException ex)
{
    _logger.LogError(ex, "API error");
}
```

### Exception Hierarchy

```
StendlyException (base)
├── StendlyAuthenticationException (401, 403)
├── StendlyValidationException (400)
├── StendlyRateLimitException (429)
├── StendlyApiConnectionException (network failures)
└── StendlySignatureVerificationException (webhook invalid)
```

---

## API Reference

### StendlyClient

Main entry point. Implements `IStendlyClient`.

#### Constructor

```csharp
public StendlyClient(
    HttpClient httpClient,
    string apiKey,
    string environment = "mainnet",
    int maxRetries = 2
)
```

| Parameter     | Type         | Default      | Description                                          |
|---------------|--------------|--------------|------------------------------------------------------|
| `httpClient`  | `HttpClient` | **required** | HttpClient instance (from IHttpClientFactory or new) |
| `apiKey`      | `string`     | **required** | Secret API key (`st_live_*`)                 |
| `environment` | `string`     | `"mainnet"`  | API environment: `"mainnet"` or `"devnet"`           |
| `maxRetries`  | `int`        | `2`          | Maximum retry attempts for transient failures        |

#### Properties

| Property    | Type               | Description                            |
|-------------|--------------------|----------------------------------------|
| `Intents`   | `IIntentsClient`   | Payment intent operations              |
| `Terminals` | `ITerminalsClient` | POS terminal management                |
| `Webhooks`  | `IWebhooksClient`  | Webhook configuration and verification |
| `Merchant`  | `IMerchantClient`  | Merchant account data                  |

### Namespaces

#### `client.Intents`

**Methods:**

#####

`CreateIntentAsync(int amountCents, string orderId, Guid? terminalId = null, string? idempotencyKey = null, CancellationToken cancellationToken = default)`

Creates a new payment intent.

```csharp
var intent = await client.Intents.CreateIntentAsync(4999, "order_vip_001");
```

##### `RetrieveIntentAsync(Guid intentId, CancellationToken cancellationToken = default)`

Fetches a payment intent by ID.

```csharp
var intent = await client.Intents.RetrieveIntentAsync(
    Guid.Parse("123e4567-e89b-12d3-a456-426614174000")
);
Console.WriteLine(intent.Status);
```

---

#### `client.Terminals`

**Methods:**

##### `CreateTerminalAsync(string name, CancellationToken cancellationToken = default)`

Creates a new terminal.

```csharp
var terminal = await client.Terminals.CreateTerminalAsync("Main Counter");
```

##### `ListTerminalsAsync(CancellationToken cancellationToken = default)`

Returns all terminals.

```csharp
var terminals = await client.Terminals.ListTerminalsAsync();
```

---

#### `client.Webhooks`

**Methods:**

##### `UpdateWebhookUrlAsync(string url, CancellationToken cancellationToken = default)`

Updates webhook URL. Sends `PATCH /api/b2b/merchants/webhook` with `{"webhookUrl": "..."}`.

```csharp
await client.Webhooks.UpdateWebhookUrlAsync("https://myshop.com/webhooks/stendly");
```

#####

`ConstructEventAsync(byte[] payload, string signatureHeader, string webhookSecret, int toleranceSeconds = 300, CancellationToken cancellationToken = default)`

**CRITICAL SECURITY METHOD**: Verifies webhook signature. **Async** method.

- `payload` (`byte[]`, required): Raw request body as bytes.
- `signatureHeader` (`string`, required): Value of `X-Stendly-Signature` header.
- `webhookSecret` (`string`, required): Your webhook secret.
- `toleranceSeconds` (`int`, optional): Max age in seconds (default 300).

Returns: `Task<WebhookEvent>`

- `webhookEvent.Event` contains the event type string (e.g., `"payment_intent.succeeded"`)

```csharp
var webhookEvent = await client.Webhooks.ConstructEventAsync(
    payload: rawBodyBytes,
    signatureHeader: signature,
    webhookSecret: webhookSecret
);

if (webhookEvent.Event == "payment_intent.succeeded")
{
    Fulfill(webhookEvent.Data.OrderId);
}
```

---

#### `client.Merchant`

**Methods:**

##### `GetProfileAsync(CancellationToken cancellationToken = default)`

Retrieves merchant profile. Sends `GET /api/b2b/merchants/me`.

```csharp
var profile = await client.Merchant.GetProfileAsync();
Console.WriteLine(profile.Name);
Console.WriteLine(profile.PayoutAddress);
```

##### `GetStatsAsync(CancellationToken cancellationToken = default)`

Returns 30-day statistics. Sends `GET /api/b2b/merchants/stats`.

```csharp
var stats = await client.Merchant.GetStatsAsync();
Console.WriteLine($"Volume: ${stats.TotalVolumeCents / 100m:N2}");
```

---

### Data Models

#### `PaymentIntent`

| Property              | Type                  | Description                                                  |
|-----------------------|-----------------------|--------------------------------------------------------------|
| `Id`                  | `Guid`                | Unique intent ID                                             |
| `OrderId`             | `string`              | Your order reference                                         |
| `ExpectedAmountCents` | `int`                 | Expected amount (cents)                                      |
| `ReferenceAddress`    | `string`              | Escrow Solana address                                        |
| `DestinationAddress`  | `string`              | Merchant payout address                                      |
| `Status`              | `PaymentIntentStatus` | Enum: `Pending`, `Paid`, `Expired`, `Cancelled`, `Underpaid` |
| `ExpiresAt`           | `DateTime`            | Expiration timestamp (UTC)                                   |

#### `WebhookEvent`

| Property | Type          | Description                                     |
|----------|---------------|-------------------------------------------------|
| `Event`  | `string`      | Event name (e.g., `"payment_intent.succeeded"`) |
| `Data`   | `WebhookData` | Event payload                                   |

---

## Integration Examples

### ASP.NET Core Minimal API

```csharp
using Stendly;

var builder = WebApplication.CreateBuilder(args);
var apiKey = builder.Configuration["Stendly:ApiKey"]!;
var client = new StendlyClient(new HttpClient(), apiKey);

var app = builder.Build();

app.MapPost("/api/intents", async (CreateIntentRequest request) =>
{
    var intent = await client.Intents.CreateIntentAsync(
        request.AmountCents,
        request.OrderId
    );
    return Results.Ok(new
    {
        Id = intent.Id.ToString(),
        ReferenceAddress = intent.ReferenceAddress,
        ExpiresAt = intent.ExpiresAt
    });
});

app.MapGet("/api/intents/{id:guid}", async (Guid id) =>
{
    var intent = await client.Intents.RetrieveIntentAsync(id);
    return Results.Ok(new { intent.OrderId, intent.Status, intent.ReferenceAddress });
});

app.Run();

public record CreateIntentRequest(int AmountCents, string OrderId);
```

### Batch Operations

```csharp
public async Task<List<PaymentIntent>> CreateBulkIntentsAsync(
    IEnumerable<(int AmountCents, string OrderId)> orders)
{
    var client = new StendlyClient(
        new HttpClient(),
        Environment.GetEnvironmentVariable("STENDLY_API_KEY")!
    );

    var tasks = orders.Select(o =>
        client.Intents.CreateIntentAsync(o.AmountCents, o.OrderId));

    var results = await Task.WhenAll(tasks);
    return results.ToList();
}
```

---

## Production Checklist

- [ ] API key stored in configuration or environment variable
- [ ] Webhook secret stored securely
- [ ] Webhook endpoint uses HTTPS
- [ ] All webhooks verified (no exceptions)
- [ ] Client registered as singleton in DI (connection pooling)
- [ ] Proper error handling (catch `StendlyException`)
- [ ] Logging configured (structured logs)
- [ ] Timeout set appropriately (`HttpClient.Timeout`)
- [ ] Tests run in CI/CD pipeline

---

## Troubleshooting

### Common Issues

| Issue                            | Solution                                                        |
|----------------------------------|-----------------------------------------------------------------|
| `StendlyAuthenticationException` | Check API key format; regenerate if leaked                      |
| `StendlyValidationException`     | Validate input before API call                                  |
| `StendlyRateLimitException`      | Implement backoff; respect `Retry-After` header                 |
| `StendlyApiConnectionException`  | Check internet; increase timeout; retry                         |
| Webhook verification fails       | Verify webhook secret; use raw byte[] payload; check clock sync |

---

## License & Links

### License

MIT License. See [LICENSE](LICENSE).

### Links

- 📖 [API Documentation](https://stendly.com/en-us/docs/sdk/dotnet/)
- 🐙 [GitHub Repository](https://github.com/stendly-dev/dotnet-sdk)
- 📦 [NuGet Package](https://www.nuget.org/packages/Stendly/)
- 🏠 [Stendly Website](https://stendly.com)

---

**Built with ❤️ for the Solana ecosystem.**

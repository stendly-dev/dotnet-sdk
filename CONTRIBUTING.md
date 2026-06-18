# Contributing to Stendly .NET SDK

We welcome contributions! This document outlines the process.

## Development Setup

```bash
# Clone the repository
git clone https://github.com/stendly-dev/dotnet-sdk.git
cd dotnet-sdk/

# Build the project
dotnet build

# Run tests
dotnet test

# Pack for NuGet
dotnet pack
```

## Code Style

- Use .NET coding conventions (PascalCase for public members, camelCase for private)
- XML documentation comments on all public APIs
- Nullable reference types enabled (`<Nullable>enable</Nullable>`)
- Follow existing patterns in the codebase

## Testing

```bash
# Run all tests
dotnet test
```

## Pull Request Process

1. Fork the repository
2. Create a feature branch (`git checkout -b feature/amazing-feature`)
3. Commit your changes (`git commit -m 'Add amazing feature'`)
4. Push to the branch (`git push origin feature/amazing-feature`)
5. Open a Pull Request

### PR Requirements

- Tests pass (`dotnet test`)
- Build succeeds (`dotnet build`)
- New features include tests
- API changes include documentation updates
- Commit messages follow conventional commits format

## Release Process

1. Update version in `.csproj`
2. Update `CHANGELOG.md`
3. Create a GitHub release with tag
4. Publish to NuGet: `dotnet nuget push`

## Questions?

Open a [GitHub Discussion](https://github.com/stendly-dev/dotnet-sdk/discussions) or email support@stendly.com.
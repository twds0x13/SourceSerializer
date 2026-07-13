# Contributing

SourceSerializer is maintained by [Twds0x13](https://github.com/twds0x13).

## Prerequisites

- .NET SDK 9.0+
- Node.js 24+ (for doc site)

## Project Structure

```
SourceSerializer/
├── packages/sourceserializer/    # UPM package (runtime + source generator)
├── tests/                         # .NET test project
├── docs/                          # VitePress documentation
└── .github/                       # CI workflows
```

## Development

```bash
# Build source generator
dotnet build packages/sourceserializer/SourceGenerator/

# Run tests
dotnet test tests/SourceSerializer.Tests/ -c Release

# Build docs
npx vitepress build docs
```

## Commit Conventions

This project uses [Conventional Commits](https://www.conventionalcommits.org/). All commits must follow this format — semantic-release enforces it at release time.

## Documentation

Chinese (`docs/`) is the source language; English (`docs/en/`) is a 1:1 mirror. Any new Chinese page must include at least a skeleton English translation in the same commit.

# Security Policy

## Reporting

Report vulnerabilities privately via [GitHub Security Advisory](https://github.com/twds0x13/SourceSerializer/security/advisories/new). Do not create public issues.

## Supported Versions

| Version | Supported |
|---------|-----------|
| Latest  | ✅ |

## Threat Model

**In scope:**
- Malicious template strings causing unbounded compilation
- Buffer overflows in generated span scanners
- Source generator code injection via template attributes

**Out of scope:**
- Issues in consuming applications that misuse generated code
- .NET runtime or Roslyn compiler vulnerabilities

# Copilot Instructions

## General Guidelines
- When adding reusable localized text, prefer a generic resource name instead of a control-specific key if the text can be shared elsewhere.
- Keep the Help resource file only for help content; UI labels like flyout headers should go in the normal Resources file instead.
- Prefer cross-platform solutions over platform-specific native window interop when possible, as the app rewrite targets cross-platform support.
- When editing `.resx` files in this repo, do not replace existing entries; append new entries only, unless we are updating an existing entry. 

## Code Style
- Use specific formatting rules
- Follow naming conventions

## Entity Framework Core Configuration
- Implement `IEntityTypeConfiguration` directly on EF Core entity models rather than creating separate configuration classes. This keeps the model and its database configuration in the same file.
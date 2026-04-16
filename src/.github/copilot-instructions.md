# Copilot Instructions

## General Guidelines
- When adding reusable localized text, prefer a generic resource name instead of a control-specific key if the text can be shared elsewhere.
- Keep the Help resource file only for help content; UI labels like flyout headers should go in the normal Resources file instead.

## Code Style
- Use specific formatting rules
- Follow naming conventions

## Entity Framework Core Configuration
- Implement `IEntityTypeConfiguration` directly on EF Core entity models rather than creating separate configuration classes. This keeps the model and its database configuration in the same file.
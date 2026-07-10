# HYDRA_HOME

Canonical hybrid system root.

## Stack

- Rust Core
- .NET 8 Bridge
- WPF Desktop / Tray
- gRPC / Protobuf
- WiX Toolset v4 / Burn
- React Native / Expo Mobile HUD
- Figma-driven design system
- curated asset pipeline

## Forbidden

- Python
- FastAPI
- Flask
- PyInstaller
- Inno Setup
- `.iss`

## Layout

```text
contracts/proto      shared gRPC contracts
core/rust            Rust workspace
bridge/dotnet        .NET 8 gRPC bridge
desktop/wpf          WPF desktop/tray clients
mobile/expo          React Native / Expo HUD
installer/wix        WiX v4 installer/bundle
assets               curated asset registry and accepted assets
design               Figma maps and design tokens
docs                 architecture and operations documentation
```

# HYDRA_TOOLING_INTELLIGENCE_PACK_V1

Status: ACTIVE

## LOCKED boundaries

Tooling intelligence can inform HYDRA_HOME decisions, but cannot replace the canonical stack:

- Rust Core
- .NET 8 Bridge
- WPF Desktop / Tray
- gRPC / Protobuf
- WiX Toolset v4 / Burn
- React Native / Expo Mobile
- Figma master design
- curated asset pipeline

Forbidden in active implementation:

- Python
- FastAPI
- Flask
- PyInstaller
- Inno Setup
- `.iss`

## GitHub knowledge map

| Area | Watch targets | Output |
|---|---|---|
| gRPC .NET | `grpc-dotnet`, ASP.NET Core gRPC docs/releases | bridge compatibility notes |
| Rust gRPC | `tonic`, `tokio`, `prost`, `tower` | Rust runtime risk notes |
| WPF/.NET | .NET Desktop, CommunityToolkit.Mvvm | MVVM/runtime notes |
| WiX | WiX Toolset v4, Burn releases | installer risk notes |
| Expo/RN | Expo SDK, React Native releases | mobile HUD compatibility notes |
| BLE/STT | platform docs, selected vendor SDKs | voice/device integration risks |

## Color and palette tooling

External palette tools are exploratory only. Output must be normalized into HYDRA tokens before code use.

Canonical token targets:

- WPF `ResourceDictionary`
- RN TypeScript theme module
- Figma token map
- asset registry metadata

## Convex evaluation

Status: OPTIONAL / BLOCKED FROM CORE

Allowed only for optional cloud/state sync after local runtime is stable. Not allowed for local device control authority.

## Replit usage

Status: INTERNAL TOOLING ONLY

Allowed:

- asset browser prototype
- diagnostic payload viewer
- schema visualizer
- palette previewer

Forbidden:

- production backend
- command execution authority
- secret storage
- source of truth

## Zap automation plan

Automation may generate:

- weekly recap
- release digest
- issue/research mirror
- dependency risk list

Automation may not:

- modify code directly
- upgrade dependencies directly
- override architecture decisions

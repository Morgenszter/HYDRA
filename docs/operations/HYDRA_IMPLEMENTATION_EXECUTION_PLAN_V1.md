# HYDRA_IMPLEMENTATION_EXECUTION_PLAN_V1

Status: ACTIVE

## Sprint 0 — Canonical repository foundation

Create structure:

```text
contracts/proto
core/rust
bridge/dotnet
desktop/wpf
mobile/expo
installer/wix
assets/registry
assets/curated
design/figma-map
design/tokens
docs/architecture
docs/operations
docs/research
```

Acceptance:

- no Python
- no FastAPI/Flask
- no Inno Setup
- no PyInstaller
- no `.iss`

## Sprint 1 — gRPC contracts

Files:

- `contracts/proto/hydra.runtime.v1.proto`
- `contracts/proto/hydra.voice.v1.proto`
- `contracts/proto/hydra.devices.v1.proto`

Rules:

- `syntax = "proto3"`
- timestamps via `google.protobuf.Timestamp`
- explicit service boundaries
- no REST fallback for internal runtime calls

## Sprint 2 — .NET 8 Bridge

Target:

- `bridge/dotnet/Hydra.Bridge.sln`
- ASP.NET Core gRPC server using `Grpc.AspNetCore`
- application services isolated from transport
- DTO mapping at gRPC boundary

## Sprint 3 — Rust Core

Target:

- `core/rust/Cargo.toml`
- `tonic`, `tokio`, `prost`, `serde`
- no `unsafe`
- `Result<T, E>` internally
- `tonic::Status` only at gRPC boundary

## Sprint 4 — WPF Desktop / Tray

Target:

- .NET 8 WPF
- CommunityToolkit.Mvvm
- DI services/repositories
- no gRPC calls in views/code-behind
- no UI logic in gRPC layer

## Sprint 5 — Expo mobile HUD

Target:

- RN/Expo HUD shell
- dedicated voice runtime hooks/services
- theme tokens from design system
- no monolithic `App.tsx`

## Sprint 6 — WiX v4 / Burn

Target:

- `installer/wix/Hydra.Installer.wixproj`
- `Package.wxs`
- optional `Bundle.wxs`
- .NET 8 Desktop Runtime prerequisite handled through Burn if required

## Sprint 7 — curated assets

Target:

- asset registry JSON
- curated categories
- Figma-to-code mapping
- generated code references only to accepted assets

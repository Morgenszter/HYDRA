# HYDRA_LOCKED_DECISIONS

Status: LOCKED

## Canonical stack

- Rust Core
- .NET 8 Bridge / application services
- WPF Desktop / Tray on .NET 8
- gRPC / Protobuf as the only inter-component communication boundary
- WiX Toolset v4 / Burn for Windows installation and bootstrapper flows
- React Native / Expo Mobile HUD
- curated HYDRA asset pipeline

## Forbidden technologies

These are banned for active implementation, packaging and runtime use:

- Python
- FastAPI
- Flask
- PyInstaller
- Inno Setup
- `.iss` installer scripts
- REST replacement for required gRPC boundaries

## Installer decision

Inno Setup is forbidden.

The only accepted Windows installer path is:

- WiX Toolset v4 XML
- `.wixproj`
- `.wxs`
- Burn bundle where prerequisite/bootstrap logic is required
- optional .NET 8 Desktop Runtime prerequisite handling through WiX/Burn only

## Migration rule

Historical Python/FastAPI/Inno artifacts may be read only as legacy behavior references if explicitly needed, but they must not be executed, extended, packaged, or copied into the canonical HYDRA root.

## Enforcement

Any occurrence of the following in the canonical repo is a CONFLICT unless explicitly placed in archived legacy documentation:

- `*.py`
- `__pycache__/`
- `requirements.txt`
- `pyproject.toml`
- `*.iss`
- `PyInstaller`
- `FastAPI`
- `Flask`
- `Inno Setup`

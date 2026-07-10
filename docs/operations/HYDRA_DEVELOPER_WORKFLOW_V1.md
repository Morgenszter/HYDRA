# HYDRA_DEVELOPER_WORKFLOW_V1

Status: ACTIVE

## Commands

From repository root:

```powershell
.\tools\dev\Validate-Hydra.ps1
.\tools\dev\Publish-Hydra.ps1
.\tools\dev\Start-HydraBridge.ps1
.\tools\dev\Start-HydraDesktop.ps1
```

## Runtime flow

1. Start .NET gRPC bridge.
2. Start WPF desktop shell.
3. WPF resolves `IHydraRuntimeService` through DI.
4. Infrastructure uses `Grpc.Net.Client` and generated Protobuf client.
5. Bridge serves `HydraRuntimeGrpcService` through `Grpc.AspNetCore`.

## Publish and installer flow

1. Run `Publish-Hydra.ps1`.
2. Build `installer\wix\Hydra.Installer.wixproj`.
3. WiX packages framework-dependent single-file EXE outputs from `artifacts\publish`.

## Forbidden

Do not add:

- Python
- FastAPI
- Flask
- PyInstaller
- Inno Setup
- `.iss`

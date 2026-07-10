# HYDRA_FIGMA_TO_CODE_MAP_V1

Status: ACTIVE

## Mapping model

Each Figma item maps to:

- frame/layer id
- semantic component name
- target platform: WPF / RN / both
- asset class: hero / medium / micro / runtime-effect / baked-effect
- code target
- interaction state
- accessibility notes

## Component categories

| Figma area | WPF target | RN target | Notes |
|---|---|---|---|
| Loading | `LoadingView` | `LoadingScreen` | voice/runtime boot states |
| Top rail | `TopRailView` | `HydraTopRail` | status, connection, profile |
| Console | `ConsolePanelView` | `HydraConsolePanel` | event stream and diagnostics |
| Command wheel | `CommandWheelView` | `HydraCommandWheel` | runtime-effect preferred |
| Equipment tree | `EquipmentTreeView` | `HydraEquipmentTree` | device hierarchy |
| Dock | `CommandDockView` | `HydraCommandDock` | action launcher |
| Connectors | `ConnectorLayer` | `HydraConnectorLayer` | SVG or runtime lines |
| Voice | `VoiceStatusView` | `HydraVoiceStatus` | separate runtime state |

## Runtime effect vs baked graphic

Runtime effect:

- pulse
- glow
- scanline
- selection ring
- state transition

Baked graphic:

- high-detail frame texture
- hero logo
- static ornamental background

## Output targets

- WPF XAML ResourceDictionaries and Views
- RN components and theme tokens
- asset registry entries

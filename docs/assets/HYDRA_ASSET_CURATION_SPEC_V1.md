# HYDRA_ASSET_CURATION_SPEC_V1

Status: ACTIVE

## Principle

Use curated bundle only. Do not import full historical asset packs directly into runtime.

## Categories

| Category | Purpose | Preferred format |
|---|---|---|
| loading | boot/loading sequences | WebP/PNG sequence or Lottie only if approved |
| top-rail | headers, status rails | SVG/PNG |
| console | diagnostic/event panels | SVG/runtime styles |
| wheel | radial command UI | SVG + runtime effect |
| equipment-tree | device hierarchy | SVG icons + runtime layout |
| dock | command/action launcher | SVG icons + runtime styling |
| connectors | lines, paths, nodes | SVG or runtime drawing |
| hero | high-identity visuals | PNG/WebP high resolution |

## Asset classes

- hero: dominant visual identity asset
- medium: panel/frame/large widget asset
- micro: glyph/icon/indicator
- runtime-effect: generated in WPF/RN, not baked
- baked-effect: static visual effect accepted into registry

## Registry fields

```json
{
  "id": "hydra.asset.example",
  "sourcePack": "V92|V90|V87|manual",
  "category": "loading|top-rail|console|wheel|equipment-tree|dock|connectors|hero",
  "class": "hero|medium|micro|runtime-effect|baked-effect",
  "format": "svg|png|webp",
  "targets": ["wpf", "rn"],
  "status": "accepted|rejected|candidate",
  "notes": ""
}
```

## Acceptance rules

- every runtime asset must have registry entry
- no duplicate semantic asset ids
- SVG preferred for micro/connectors
- WebP/PNG preferred for hero/detail textures
- runtime effects preferred over baked glow when practical

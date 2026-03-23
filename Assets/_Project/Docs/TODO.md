# Cozy Farm Game - TODO
Last updated: 2026-03-23

---

## In Progress
- [ ] DEV-48 Mixamo animations (harvest/water/plant) — in queue

---

## Up Next (Priority Order)

### Session Goal: TileInfoUI
- [ ] Contextual hover panel showing crop name, grow time, planted date, input hints
- [ ] Replaces or augments bottom-centre hint pill

### Session Goal: DOTween Juice
- [ ] Install DOTween from Asset Store
- [ ] Crop pop/bounce on plant (transform.DOPunchScale)
- [ ] Crop pop on harvest
- [ ] Pop on growth stage change

### Session Goal: Offline Growth
- [ ] Save DateTime.UtcNow on quit/save
- [ ] On load, calculate elapsed time and advance growth progress
- [ ] Cap at max growth (ready, not over-grown)

### Canvas Scale Fix
- [ ] Investigate HUD Canvas showing scale (2,2,1) on HiDPI/Retina displays
- [ ] Permanent fix so HUD is always correctly sized (CanvasScaler issue)

### Functional Buildings
- [ ] Watering Well — auto-waters adjacent 3x3 tiles once per growth cycle
- [ ] Replace coloured box placeholders with polyperfect models (Barn, Well, Greenhouse, Market Stall)

### Art
- [ ] Find better model matches: Potato, Strawberry, Chilli, Lavender
- [ ] Cluster B flower beds (8 more, Z approx -5)

---

## Polish
- [ ] UI icons using TMP Sprite Asset
- [ ] Crop growth % indicator on HUD per tile
- [ ] Crop bloom/glow when ready to harvest
- [ ] Decorative props: paths, lanterns, fences between beds
- [ ] Shop/inventory icons

---

## Done
- [x] Unity 6 + URP setup
- [x] GitHub repo
- [x] Full farming loop (plant/water/grow/harvest) — no till step
- [x] 10 crop ScriptableObjects with polyperfect models and 4 growth stages
- [x] Building and decoration system
- [x] Shop UI (B key) with unlock levels
- [x] Inventory UI (Tab key) with colour swatches and Sell All
- [x] Build Mode UI (G key)
- [x] HUD (coins top-centre, XP bar, level, notifications)
- [x] Selected crop panel top-left (Farm Together 2 style) - swatch, name, count, timer
- [x] Tool indicator (Farming Mode / Planting: X / Watering Can / Build Mode)
- [x] Contextual hint pill bottom-centre (per tile state, ASCII separators)
- [x] Controls overlay panel (H to toggle)
- [x] XP and 15 level progression
- [x] Economy and coins (150 starting)
- [x] Save/load system (JSON)
- [x] Music, SFX and ambience - all clips assigned and working
- [x] Polyperfect farm scene integrated
- [x] Animated low-poly character (walk/idle)
- [x] Farm Together orbit camera (Q/E rotate, scroll zoom, C presets)
- [x] Auto-follow camera on W key (2s cooldown after manual input)
- [x] Dynamic pitch-to-zoom (pitch tracks zoom level)
- [x] Camera-relative WASD movement
- [x] PlayerController single Move() call (no stutter), rotationSpeed=20
- [x] Hollow square hover highlight (yellow=planted, orange=ready, green=empty)
- [x] Blue watered tile marker (scale 1.2, alpha 0.12)
- [x] TileMarker stacking bug fixed (explicit destroy before spawn)
- [x] Grid world position bug fixed (GridToWorld everywhere)
- [x] All UI strings ASCII-only (no pipes, em dashes, bullets)
- [x] UI panels on separate canvas sort order 50 (buttons clickable)
- [x] Kenney Future SDF font - no box characters
- [x] Balance progression curve (150 starting coins, rebalanced XP)
- [x] Main Menu, Pause Menu, Settings UI
- [x] Editor tools (FarmSceneSetup, HUDBuilder, CropAssetGenerator, BuildingAssetGenerator)
- [x] CozyFarm Toolkit window (Tools > CozyFarm > Open Toolkit)
- [x] Flower bed clicking fixed (Ignore Raycast layer)
- [x] Pre-placed polyperfect demo crop props removed (383 objects)
- [x] HUDBootstrapper removed - panels built entirely in HUDBuilder
- [x] Main Menu EventSystem fix
- [x] BuildModeUI null reference fix
- [x] URP shader stripping fix (ShaderIncludePreprocessor)
- [x] Particle effects for harvest and water
- [x] Mac build tested (girlfriend), Windows build tested (brother) - 2026-03-23
- [x] Playtest feedback received: core loop clear, controls confusing
- [x] FarmingManager.GetPlantedCount() and GetNearestRemainingSeconds() query methods
- [x] BuildModeUI.IsOpen public property

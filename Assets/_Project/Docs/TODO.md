# Cozy Farm Game - TODO
Last updated: 2026-03-25

---

## In Progress
- [ ] DEV-48 Mixamo animations (harvest/water/plant) — in queue

---

## Up Next (Priority Order)

### Dog Pet System
- [ ] Choose dog model (Low Poly Ultimate Pack Dog.prefab or buy Synty POLYGON Dog Pack)
- [ ] Dog follows player (NavMesh or simple follow script)
- [ ] Player can pet dog (E key proximity interaction)
- [ ] Optional: feeding gives crop growth speed boost
- [ ] Dog bark SFX from Universal Sound FX (ANIMAL_Dog_Bark_03 RR1-4)

### Bugs
- [x] Shop coin display — was showing test value (8k coins), not a bug
- [x] Icon backgrounds not transparent — fixed, re-run confirmed working
- [ ] Crop icons inconsistent: grapes, lavender, chilli too dark/small — check after Synty icon re-render
- [ ] Stone Path and Lantern — no Synty prefab assigned, still placeholder visuals

### Art
- [ ] Soil tile polish — replace procedural brown quad with proper dirt texture (polish stage)
- [ ] Find better model matches: Potato, Strawberry, Chilli, Lavender

### Polish
- [ ] Crop bloom/glow when ready to harvest
- [ ] Decorative props: paths, lanterns, fences between beds

---

## Done
- [x] Flat soil tile system — replaced raised flower beds with procedural flat quads; FarmGrid auto-generates tiles when normalTilePrefab is null; hover highlight lowered to 0.04; farm-flower-bed and fence-shrub scene objects removed
- [x] Synty POLYGON Farm crop models — all 10 crops assigned per-stage prefabs (S/M/L/Group) with tuned scales and rotations via CropModelAssigner editor tool
- [x] Synty POLYGON Farm building models — barn, greenhouse, silo, market stall, watering well, scarecrow, wooden fence, windmill assigned via BuildingModelAssigner editor tool
- [x] Collectibles loop — coins/seeds scattered around map, distance-based pickup, 5-min respawn, sparkle particles
- [x] Market Stall auto-sell — MarketStallComponent sells all inventory every 120s at 10% bonus
- [x] BuildingData description field — shown in build mode UI card under price
- [x] Icons re-rendered with Synty models
- [x] Audio Library Curator editor tool (Tools > CozyFarm > Audio Library Curator) — browse Universal Sound FX, pre-checks recommended clips, copy to _Project/Audio/SFX, delete source folder
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
- [x] TileInfoUI — contextual hover panel (crop name, stage, timer, water status, progress bar, action hints, DOTween slide in/out)
- [x] Controls overlay panel (H to toggle)
- [x] XP and 15 level progression
- [x] Economy and coins (150 starting)
- [x] Save/load system (JSON)
- [x] Offline crop growth — DateTime save/load, ApplyOfflineGrowth, notification on return
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
- [x] PauseMenuUI on dedicated canvas (sortingOrder 100, CanvasScaler)
- [x] SettingsUI sliders — fill rect anchors fixed (no green blocks)
- [x] Crop Growth Speed slider in Settings (1-60x range)
- [x] DOTween juice — crop pop on plant (OutBack), water punch, harvest scale-up then squish
- [x] Harvest particles — larger size (0.2-0.5), faster speed, more visible
- [x] Notification resized to hint-pill style (420x32, 14pt, subtle dark bg)
- [x] ShopUI stale copy fixed ("selected!" only, no tilled tile copy)
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
- [x] Watering Well functional building (WateringWellComponent, auto-waters 3x3 on timer)
- [x] WateringWell polyperfect model assigned (_M/Prefabs_M/Medieval_M/Well.prefab)
- [x] BuildingData autoWaterRadius + autoWaterInterval fields
- [x] FarmingManager.WaterTile() playEffects param (well skips audio/XP/bounce)
- [x] Building placement raycast — Plane fallback for open ground (no collider)
- [x] CanPlace blocks flower bed tiles (grid.IsValidCoord check)
- [x] IconRenderer editor tool (Tools > CozyFarm > Render Icons) — 128x128 PNGs, TMP Sprite Asset
- [x] Shop/inventory/build mode icons — sprite with colour fallback (48px, preserveAspect)
- [x] Build mode blocks tile hover highlight and all farm interactions (plant, water, harvest)

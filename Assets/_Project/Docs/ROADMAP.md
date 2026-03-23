# CozyFarmGame — Development Roadmap
*Last updated: 2026-03-23*

---

## Current Status
Core gameplay loop is complete. Milestone 3 (Alpha) in progress.
Mac and Windows builds playtested — feedback: core loop clear, controls confusing.
Focus: TileInfoUI, DOTween juice, offline growth, then functional buildings.

---

## Priority 1 — Next Session

### 1. TileInfoUI
- [ ] Contextual hover panel showing crop status, grow time, input hints
- [ ] Replaces or augments the current bottom-centre hint pill

### 2. DOTween Juice
- [ ] Install DOTween from Asset Store
- [ ] Crop pop/bounce effect on plant, harvest, growth stage change
- [ ] Scale punch: transform.DOPunchScale()

### 3. Canvas Scale Fix
- [ ] Investigate HUD Canvas showing scale (2,2,1) on some displays
- [ ] Root cause: CanvasScaler ScaleWithScreenSize on HiDPI/Retina
- [ ] Permanent fix so HUD is always correctly sized

### 4. Offline Crop Growth
- [ ] Save DateTime.UtcNow on quit/save
- [ ] On load, calculate elapsed time and advance growth progress
- [ ] Cap at max growth (ready to harvest, not over-grown)

### 5. Watering Well (Functional Building)
- [ ] Well placed on farm auto-waters adjacent 3x3 tiles once per growth cycle
- [ ] First functional building — unblocks barn/greenhouse/market stall pattern

### 6. Building Placeholder Models
- [ ] Replace coloured box placeholders with polyperfect models
- [ ] Barn, Well, Greenhouse, Market Stall

---

## Priority 2 — Polish Sprint

### Visual
- [ ] Crop bloom / glow when ready to harvest
- [ ] Better model matches: Potato, Strawberry, Chilli, Lavender
- [ ] Decorative props: paths, lanterns, fences between beds

### UI
- [ ] Shop / inventory icons (TMP Sprite Assets)
- [ ] HUD crop growth % indicator per tile

### Map
- [ ] Add Cluster B flower beds (8 more, Z approx -5)
- [ ] Mixamo animations for harvest/water/plant (DEV-48)

---

## Priority 3 — Content

- [ ] Scrounger loop: collectibles scattered around map
- [ ] Seasonal crops
- [ ] Rare / hybrid blooms
- [ ] Character cosmetics
- [ ] Achievements

---

## Out of Scope for v1.0
- Multiplayer
- Mobile / controller support
- NPCs / quests
- Crafting system

---

## Done

### Core Systems
- [x] Full farming loop (plant/water/grow/harvest) — no till step, Farm Together style
- [x] 10 crops with polyperfect 3D models and 4 growth stages
- [x] Building and decoration placement system
- [x] Shop UI (B key) with unlock levels
- [x] Inventory UI (Tab key) with colour swatches and Sell All
- [x] Build Mode UI (G key)
- [x] XP and 15 level progression system
- [x] Economy and coins (150 starting)
- [x] Save/load system (JSON)
- [x] FarmingManager query methods: GetPlantedCount(), GetNearestRemainingSeconds()
- [x] BuildModeUI.IsOpen public property

### Camera
- [x] Farm Together style orbit camera (FarmCamera.cs) — Q/E + middle-mouse rotation
- [x] Scroll zoom with dynamic pitch-to-zoom (pitch tracks distance)
- [x] C key preset cycles: Close (d=8 p=40), Mid (d=20 p=55), Far (d=35 p=68)
- [x] Auto-follow on W key only (2s cooldown after manual input)
- [x] SphereCast collision prevention (origin pivot + up*1.5f)
- [x] Camera-relative WASD movement in PlayerController

### Player
- [x] CharacterController movement (camera-relative)
- [x] Single controller.Move() per frame (no stutter)
- [x] rotationSpeed=20 for snappy turning
- [x] Idle/Walk animator swap
- [x] applyRootMotion = false

### UI / HUD
- [x] HUDBuilder editor tool (Tools > CozyFarm > Build HUD in Scene)
- [x] Coins text (top-centre)
- [x] Level, XP bar, Level Up panel (top-right)
- [x] Contextual hint pill (bottom-centre, changes per tile state, ASCII separators)
- [x] Controls overlay panel (H to toggle, visible by default)
- [x] Selected crop panel — Farm Together 2 style (top-left, 280x100)
  - Colour swatch, crop name, planted count, countdown timer
  - Live 0.5s refresh via coroutine in HUDManager
- [x] Tool indicator: Farming Mode / Planting: [Crop] / Watering Can / Build Mode
- [x] Notification system (timed pop-ups)
- [x] Shop UI (scrollable, buy seeds, coin display)
- [x] Inventory UI (colour swatches, per-item sell, Sell All, slots text)
- [x] Build Mode UI (catalogue with unlock levels)
- [x] All UI strings ASCII-only (no pipes, em dashes, or Unicode)

### Visual / Art
- [x] Polyperfect 3D crop models (all 10 crops assigned)
- [x] Growth stage scaling (4 stages via CropGrowthVisual)
- [x] Hollow square hover highlight (4-edge outline, yellow=planted, orange=ready, green=empty)
- [x] Blue watered tile marker (scale 1.2, alpha 0.12)
- [x] TileMarker stacking bug fixed (explicit destroy before spawn)
- [x] Harvest/water particle effects

### Audio
- [x] AudioManager (music + SFX, singleton, DontDestroyOnLoad)
- [x] AmbienceManager (layered ambient loops)
- [x] All SFX wired and clips assigned in Inspector

### Build / Technical
- [x] Mac build tested — girlfriend (2026-03-23)
- [x] Windows build tested — brother (2026-03-23)
- [x] Playtest feedback: core loop clear, liked it overall, controls confusing
- [x] URP shader stripping fix (ShaderIncludePreprocessor)
- [x] Main Menu EventSystem fix
- [x] BuildModeUI null reference fix
- [x] Grid world position bug fixed (GridToWorld everywhere)
- [x] Flower bed clicking fixed (Ignore Raycast layer)
- [x] Pre-placed polyperfect demo props removed (383 objects)
- [x] HUDBootstrapper removed — panels built entirely in HUDBuilder

# CozyFarmGame — Development Roadmap
*Last updated: 2026-03-23*

---

## Current Status
Core gameplay loop complete. Milestone 3 (Alpha) in progress.
Mac and Windows builds playtested — feedback: core loop clear, controls confusing.
UI polish sprint complete (TileInfoUI, DOTween, offline growth, notification redesign).
Focus: functional buildings (Watering Well), then building models and animations.

---

## Priority 1 — Next Session

### 1. Watering Well (Functional Building)
- [ ] Well placed on farm auto-waters adjacent 3x3 tiles once per growth cycle
- [ ] First functional building — unblocks barn/greenhouse/market stall pattern

### 2. Building Placeholder Models
- [ ] Replace coloured box placeholders with polyperfect models
- [ ] Barn, Well, Greenhouse, Market Stall

### 3. Mixamo Animations (DEV-48)
- [ ] Harvest animation
- [ ] Water animation
- [ ] Plant animation

### 4. Art Pass
- [ ] Better model matches: Potato, Strawberry, Chilli, Lavender
- [ ] Cluster B flower beds (8 more, Z approx -5)

---

## Priority 2 — Polish Sprint

### Visual
- [ ] Crop bloom / glow when ready to harvest
- [ ] Decorative props: paths, lanterns, fences between beds

### UI
- [ ] Shop / inventory icons (TMP Sprite Assets)
- [ ] HUD crop growth % indicator per tile

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
- [x] Offline crop growth — DateTime save/load, ApplyOfflineGrowth, return notification
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
- [x] TileInfoUI — bottom-right hover panel with crop name, stage, timer, water status, progress bar, action hints; DOTween slide in/out
- [x] Controls overlay panel (H to toggle, visible by default)
- [x] Selected crop panel — Farm Together 2 style (top-left, 280x100)
  - Colour swatch, crop name, planted count, countdown timer
  - Live 0.5s refresh via coroutine in HUDManager
- [x] Tool indicator: Farming Mode / Planting: [Crop] / Watering Can / Build Mode
- [x] Notification system — hint-pill style (420x32, 14pt, subtle dark bg)
- [x] Shop UI (scrollable, buy seeds, coin display)
- [x] Inventory UI (colour swatches, per-item sell, Sell All, slots text)
- [x] Build Mode UI (catalogue with unlock levels)
- [x] All UI strings ASCII-only (no pipes, em dashes, or Unicode)
- [x] PauseMenuUI on dedicated canvas (sortingOrder 100, CanvasScaler)
- [x] SettingsUI sliders — fill rect anchors fixed (no green blocks)
- [x] Crop Growth Speed debug slider in Settings (1-60x)

### Visual / Art
- [x] Polyperfect 3D crop models (all 10 crops assigned)
- [x] Growth stage scaling (4 stages via CropGrowthVisual)
- [x] Hollow square hover highlight (4-edge outline, yellow=planted, orange=ready, green=empty)
- [x] Blue watered tile marker (scale 1.2, alpha 0.12)
- [x] TileMarker stacking bug fixed (explicit destroy before spawn)
- [x] Harvest/water particle effects — larger, more visible (size 0.2-0.5, speed 2-6)

### DOTween Juice
- [x] Crop pop on plant — DOScale from 0 to full scale, Ease.OutBack
- [x] Crop punch on water — DOPunchScale (0.25f, 0.3s, 4 vibrato)
- [x] Harvest pop-out — scale UP to 1.3x then squish to zero (DOTween.Sequence)

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

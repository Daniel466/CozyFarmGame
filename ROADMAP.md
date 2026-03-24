# CozyFarmGame Roadmap

Last updated: 2026-03-23

---

## DONE

### Core Systems
- [x] FarmGrid + FarmTile (till, plant, water, harvest)
- [x] CropData ScriptableObjects (10 crops)
- [x] InventoryManager (add, remove, sell, sell all, SellItem per crop)
- [x] EconomyManager (coins, XP, level up)
- [x] ProgressionManager (XP thresholds, level gating)
- [x] Save / Load system (fully working)
- [x] BuildingManager + BuildingData ScriptableObjects
- [x] Offline crop growth — DateTime save/load, ApplyOfflineGrowth, return notification
- [x] Watering Well — WateringWellComponent auto-waters 3x3 radius on timer; polyperfect model assigned
- [x] IconRenderer editor tool — 128x128 PNG per asset, TMP Sprite Asset atlas (Tools > CozyFarm > Render Icons)
- [x] Build mode isolates farm — hover highlight hidden, plant/water/harvest blocked while building

### Camera
- [x] Farm Together style orbit camera (FarmCamera.cs)
- [x] Q/E + middle-mouse horizontal rotation
- [x] Scroll zoom with dynamic pitch-to-zoom
- [x] C key preset cycles: Close (d=8), Mid (d=20), Far (d=35)
- [x] Auto-follow on W key only (2s cooldown after manual input)
- [x] SphereCast collision prevention
- [x] Camera-relative WASD movement

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
- [x] Contextual hint pill (bottom-centre, changes per tile state, ASCII / separators)
- [x] TileInfoUI — bottom-right hover panel with crop name, stage, timer, water status, progress bar, action hints; DOTween slide in/out
- [x] Controls overlay panel (H to toggle, visible by default)
- [x] Selected crop panel — Farm Together 2 style (top-left)
  - Colour swatch, crop name, planted count, countdown timer
  - Live 0.5s refresh via coroutine
- [x] Tool indicator: Farming Mode / Planting: [Crop] / Watering Can / Build Mode
- [x] Notification system — hint-pill style (420x32, 14pt, subtle dark bg)
- [x] Shop UI (scrollable, buy seeds, coin display)
- [x] Inventory UI (sprite icons with colour fallback, per-item sell, Sell All, slots text)
- [x] Build Mode UI (sprite icons with colour fallback, catalogue with unlock levels)
- [x] Shop UI (sprite icons with colour fallback, buy seeds)
- [x] PauseMenuUI on dedicated canvas (sortingOrder 100, CanvasScaler)
- [x] SettingsUI sliders fixed (no green blocks), Crop Growth Speed slider (1-60x)

### Visual / Art
- [x] Synty POLYGON Farm crop models — all 10 crops with per-stage prefabs (S/M/L/Group), tuned scales/rotations
- [x] Synty POLYGON Farm building models — barn, greenhouse, silo, market stall, watering well, scarecrow, wooden fence, windmill
- [x] Flat soil tile system — procedural quad per tile; replaces raised flower beds
- [x] Icons re-rendered with Synty models (128x128 PNG, TMP Sprite Asset atlas)
- [x] Growth stage scaling tuned for Synty sizes (0.7/0.85/0.95/1.0)
- [x] Hollow square hover highlight (4-edge outline, yellow=planted, orange=ready, green=empty)
- [x] Blue watered tile marker (scale 1.2, alpha 0.12)
- [x] TileMarker stacking bug fixed
- [x] Harvest/water particle effects — larger, more visible (size 0.2-0.5, speed 2-6)

### DOTween Juice
- [x] Crop pop on plant — DOScale from 0, Ease.OutBack
- [x] Crop punch on water — DOPunchScale (0.25f, 0.3s, 4 vibrato)
- [x] Harvest pop-out — scale UP to 1.3x then squish to zero (DOTween.Sequence)

### Audio
- [x] AudioManager (music + SFX, singleton)
- [x] AmbienceManager (layered ambient loops)
- [x] All SFX wired: till, plant, water, harvest, sell, build, levelUp, uiClick
- [x] All clips assigned in Inspector

### Build / Technical
- [x] Mac build working — tested by girlfriend
- [x] Windows build working — tested by brother
- [x] URP shader stripping fix (ShaderIncludePreprocessor)
- [x] MainMenu EventSystem fix
- [x] BuildModeUI null reference fix
- [x] All UI strings ASCII-only (Kenney Future SDF compatible)

### Playtesting
- [x] Mac build to girlfriend, Windows build to brother (2026-03-23)
- [x] Feedback: core loop clear, liked it overall, controls confusing

---

## NEXT — Priority Order

### 1. Dog Pet System
- [ ] Choose dog model (Low Poly Ultimate Pack or Synty POLYGON Dog Pack)
- [ ] Dog follows player
- [ ] Pet interaction (E key proximity)
- [ ] Optional feeding — crop growth speed boost

### 2. Mixamo Animations (DEV-48)
- [ ] Harvest, water, plant animations

### 3. Art Pass
- [ ] Better model matches: Potato, Strawberry, Chilli, Lavender
- [ ] Stone Path and Lantern — assign Synty prefabs or source alternatives
- [ ] Soil tile polish — replace procedural quad with proper dirt texture (polish stage)

---

## Priority 2 — Polish Sprint

### Visual
- [ ] Crop bloom / glow when ready to harvest
- [ ] Decorative props: paths, lanterns, fences between beds

### UI
- [ ] HUD crop growth % indicator per tile

---

## Priority 3 — Player Engagement While Crops Grow
> **Pre-launch blocker.** Players need things to do during crop wait time. See GDD §11.

- [x] Collectibles loop — coins/seeds scattered around map, distance-based pickup, 5-min respawn, sparkle particles
- [ ] Animal interactions — pet/feed animals already in scene; happiness state + small bonus
- [ ] Decorating — expand BuildingDatabase with paths, fences, props; 3 XP per item placed

## Priority 4 — Content

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

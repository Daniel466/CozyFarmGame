# CozyFarmGame Roadmap

Last updated: 2026-03-23

---

## DONE

### Core Systems
- [x] FarmGrid + FarmTile (till, plant, water, harvest)
- [x] CropData ScriptableObjects (10 crops)
- [x] InventoryManager (add, remove, sell, sell all)
- [x] EconomyManager (coins, XP, level up)
- [x] ProgressionManager (XP thresholds, level gating)
- [x] Save / Load system (fully working)
- [x] BuildingManager + BuildingData ScriptableObjects

### Camera
- [x] Farm Together style orbit camera (FarmCamera.cs)
- [x] Q/E + middle-mouse horizontal rotation
- [x] Scroll zoom with dynamic pitch-to-zoom
- [x] C key preset cycles: Close (d=8), Mid (d=20), Far (d=35)
- [x] Auto-follow on W key (2s cooldown after manual input)
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
- [x] Coins, Level, XP bar, Level Up panel
- [x] Contextual hint pill (bottom-centre, changes per tile state)
- [x] Controls overlay panel (H to toggle, visible by default)
- [x] Selected crop indicator (bottom-left)
- [x] Notification system (timed pop-ups)
- [x] Shop UI (scrollable, buy seeds, coin display)
- [x] Inventory UI (color swatches, per-item sell, Sell All)
- [x] Build Mode UI (catalogue with unlock levels)

### Visual / Art
- [x] Polyperfect 3D crop models (all 10 crops assigned)
- [x] Growth stage scaling (4 stages via CropGrowthVisual)
- [x] Hollow square hover highlight (4-edge outline, colour-by-action)
- [x] Blue watered tile marker
- [x] Harvest/water particle effects

### Audio
- [x] AudioManager (music + SFX, singleton)
- [x] AmbienceManager (layered ambient loops)
- [x] All SFX wired: till, plant, water, harvest, sell, build, levelUp, uiClick
- [x] All clips assigned in Inspector

### Build / Technical
- [x] Mac build working
- [x] Windows build working
- [x] URP shader stripping fix (ShaderIncludePreprocessor)
- [x] MainMenu EventSystem fix
- [x] BuildModeUI null reference fix

### Playtesting
- [x] Mac build sent to girlfriend (2026-03-23)
- [x] Windows build sent to brother (2026-03-23)
- [x] First feedback received: plant/grow loop clear, controls confusing

---

## IN PROGRESS — M3 Alpha

### Animations (DEV-48)
- [ ] Mixamo animations: harvest, water, plant actions
- [ ] Blend tree or animator state machine for actions

---

## UP NEXT — Priority 1 (Post-Playtest Polish)

### Controls Feel
- [ ] Better controls onboarding (first playtest: controls confusing)
- [ ] Consider simplified control scheme or tooltip tutorial

### Gameplay Loops (Google AI Feedback)
- [ ] Scrounger loop: collectibles scattered around map to find while farming
- [ ] Offline growth: crops progress while game is closed (save timestamp, apply on load)

### Visual Juice
- [ ] DOTween crop pop effect on harvest and plant (scale bounce)
- [ ] Selected crop panel (Farm Together 2 style) — show active crop icon + name bottom-left

### Functional Buildings
- [ ] Well: auto-waters adjacent 3x3 tiles once per growth cycle (FIRST)
- [ ] Barn: +20 inventory slots on placement
- [ ] Greenhouse: crops inside bounds grow 50% faster
- [ ] Market Stall: auto-sells harvested crops at 10% coin bonus
- [ ] Silo: overflow crop storage beyond inventory cap

---

## Priority 2 — Polish Sprint

### Visual
- [ ] Crop bloom / glow when ready to harvest
- [ ] Better model matches: Potato, Strawberry, Chilli, Lavender (all currently placeholders)
- [ ] Building placeholder models -> polyperfect models
- [ ] Decorative props: paths, lanterns, fences between beds

### UI
- [ ] Shop / inventory icons (TMP Sprite Assets)
- [ ] HUD crop growth % indicator per tile
- [ ] Watered status indicator on HUD

### Map / World
- [ ] Add Cluster B flower beds (8 more, Z ~ -5)
- [ ] Tile type system: Bed vs Ground distinction

---

## Priority 3 — Content

- [ ] Seasonal crops (spring/summer/autumn variants)
- [ ] Rare / hybrid blooms
- [ ] Character cosmetics
- [ ] Achievements

---

## Out of Scope for v1.0
- Multiplayer
- Mobile / controller support
- NPCs / quests
- Crafting system

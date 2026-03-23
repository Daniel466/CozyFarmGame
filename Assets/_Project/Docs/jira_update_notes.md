# Jira Update Notes
Generated: 2026-03-23

---

## Session 2026-03-23 — Camera, HUD Polish, Bug Fixes

### DEV-54 — UI Polish
**Status: Done**

Done this session:
- Selected crop panel (Farm Together 2 style, top-left, 280x100)
  - Colour swatch (40x40), crop name (18pt), stats line: "N planted, Next: Xm XXs" / "N planted, Ready!"
  - Live 0.5s refresh coroutine in HUDManager
  - CropColors dict (10 crops keyed by CropId)
- Tool indicator updated: Farming Mode / Planting: [Crop] / Watering Can / Build Mode
  - Cached lastToolText — only calls HUDManager when changed
- CoinsText moved to top-centre (anchor 0.5,1 pos 0,-40) to avoid overlap with SelectedCropPanel
- All pipe chars | replaced with / or , in UI strings (Kenney Future SDF ASCII only)
- Inventory UI empty state: uses hyphen not em dash
- ControlsPanel text: uses / separators, "C - Cycle Zoom Preset" added
- HUDBuilder: removed hudCanvas.transform.localScale = Vector3.one (was fighting CanvasScaler)
- BuildModeUI.IsOpen public property added

### DEV-57 — Camera + Player
**Status: Done**

Done this session:
- FarmCamera.cs — C key zoom preset cycle: Close (d=8, p=40), Mid (d=20, p=55), Far (d=35, p=68)
- FarmCamera.cs — Dynamic pitch-to-zoom: pitch tracks zoom level via InverseLerp/Lerp
- FarmCamera.cs — Auto-follow camera: W key only (v > 0.1f), nudges 20 deg/s behind player, 2s cooldown after Q/E/drag
- FarmCamera.cs — SphereCast origin changed from pivot+up*2.5f to pivot+up*1.5f
- PlayerController.cs — Camera-relative WASD movement (Camera.main.transform.forward/right with y=0)
- PlayerController.cs — Single controller.Move() per frame (was two calls, caused stutter)
- PlayerController.cs — rotationSpeed increased 10 -> 20

### Bug Fixes This Session:
- TileMarker stacking: WaterTile() now explicitly destroys existing marker before spawning new one
  (Root cause: tileMarkers dict not removed when marker destroyed externally)
- Hover highlight blue on planted/watered tiles: removed IsWatered colour branch entirely
  Yellow = planted (watered or not), orange = ready, green = empty
  Ground marker already communicates water status
- Watered marker too large: scale 2.0 -> 1.2, alpha 0.2 -> 0.12
- Auto-follow inverted on S key: changed trigger from Abs(h)>0.1||Abs(v)>0.1 to v>0.1f only
- HUD Canvas scale 2,2: caused by manually setting transform.localScale — removed, CanvasScaler manages this

---

## Session 2026-03-22 — DEV-57 3D Assets + DEV-52 Balance

### DEV-57 — Replace placeholder assets with real 3D art
**Status: Done**

Done:
- Imported Polyperfect Low Poly Ultimate Pack
- Integrated polyperfect DEMO_11_Farm scene as main Farm scene
- Added animated low-poly character (SKM_Boy_Beach_Rig) with walk/idle animations
- All 10 CropData assets assigned polyperfect prefabs via CropModelAssigner editor tool
- CropGrowthVisual scales across 4 growth stages (0.3x, 0.5x, 0.8x, 1.0x of modelBaseScale)
- Crop model scale/rotation offsets set per CropData (e.g. Sunflower rotY=-90, Pumpkin scale=1.0)
- Particle effects for harvest and water
- Pre-placed demo props removed (383 objects)
- Placeholder crop models remaining: Potato (Bread_Round), Strawberry (Apple), Chilli (Eggplant), Lavender (Carnations)

### DEV-52 — Balance progression curve
**Status: Done**

- Starting coins reduced from 500 to 150
- XP thresholds rebalanced (Level 1->2: 80 XP quick win; steady pace to 15)
- growthSpeedMultiplier = 60 for testing (MUST change to 1 before release)

### DEV-26 — Source 3D farm environment
**Status: Done**

- Polyperfect DEMO_11_Farm scene used as base
- Scene includes barn, silo, fences, animals, flower beds, trees, props

### DEV-48 — Player character animations
**Status: In Progress**

- Walk and idle animations working via controller swap
- No harvest/water/plant animations yet (Mixamo — pending)

### Bug Fixes 2026-03-22:
- FarmingManager now uses grid.GridToWorld(coord) everywhere (was using baked tile.WorldPosition)
- tileMarkers dict changed from Vector3 key to Vector2Int coord key
- Grid aligned to flower beds: Origin(-6, 0.15, 13), TileSize 4, Width 4, Height 4
- UI panels on separate Canvas sort order 50 (buttons receive clicks)
- HUDBootstrapper removed — panels built entirely via HUDBuilder
- ProgressionManager.SetState clamps level to min 1 (fixes all-locked bug from old saves)
- MainMenu EventSystem fix
- BuildModeUI null reference fix
- URP shader stripping fix (ShaderIncludePreprocessor)

---

## DEV-58 — Playtesting
**Status: In Progress**

- Mac build sent to girlfriend 2026-03-23
- Windows build sent to brother 2026-03-23
- Feedback received: core loop clear, liked it overall, controls confusing
- Action: onboarding/controls improvements needed next

---

## Next Session Priorities:
1. TileInfoUI — contextual hover panel for crop info
2. DOTween — install + crop pop/bounce effects (plant, harvest, growth stage)
3. Offline crop growth — DateTime save/load
4. Canvas scale 2,2 fix (HiDPI/Retina root cause)
5. Watering Well functional building

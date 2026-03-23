# CozyFarmGame — Project Briefing
*Read this at the start of every session*

---

## Project Overview
- **Game:** 3D Cozy Farming game (like Stardew Valley / Farm Together)
- **Engine:** Unity 6.3 LTS with URP
- **Platform:** PC (Windows) — itch.io launch
- **GitHub:** github.com/Daniel466/CozyFarmGame
- **Unity Project Path:** ~/Documents/CozyFarmGame/CozyFarmGame

---

## Tech Stack
- Unity 6.3 LTS + URP
- TextMeshPro with Kenney Future SDF font — **ASCII ONLY. No em dashes, pipes, bullets, or any Unicode symbol. Use - / , as separators.**
- Polyperfect Low Poly Ultimate Pack (main art asset)
- Kenney UI Pack (UI elements)
- Git for version control

---

## Project Structure
```
Assets/_Project/
    Art/           - Materials, fonts, textures
    Audio/         - Music, SFX, ambience clips
    Docs/          - GDD.md, TODO.md, ROADMAP.md, CLAUDE_CONTEXT.md, jira_update_notes.md
    Editor/        - HUDBuilder, FarmSceneSetup, CropModelAssigner, ShaderIncludePreprocessor etc.
    Prefabs/       - CropVisual prefab
    Scenes/        - Farm.unity (main scene)
    ScriptableObjects/
        Crops/     - 10 CropData assets
        Buildings/ - BuildingData assets
        Decorations/
        Progression/
    Scripts/
        Audio/     - AudioManager, AmbienceManager
        Building/  - BuildingManager, BuildModeController, BuildModeUI, BuildingData, BuildingDatabase
        Camera/    - FarmCamera
        Core/      - GameManager, PlaceholderAssetGenerator, SceneBootstrapper
        Economy/   - EconomyManager
        Farming/   - CropData, CropGrowthVisual, FarmGrid, FarmingManager, FarmTile
        Inventory/ - InventoryManager
        Player/    - PlayerController, PlayerInteraction
        Progression/ - ProgressionManager
        SaveSystem/  - SaveManager
        UI/        - HUDManager, ShopUI, InventoryUI, BuildModeUI,
                     PauseMenuUI, SettingsUI, MainMenuUI, TileInfoUI
    UI/            - UI sprites
```

---

## Core Systems Built
- Full farming loop (plant/water/grow/harvest) — no till step, Farm Together style
- 10 crops with polyperfect 3D models and 4 growth stages
- Building and decoration placement system
- Shop UI (B key), Inventory UI (Tab), Build Mode (G key), Sell All (F key)
- XP + 15 level progression system
- Coin economy (150 starting coins)
- Save/load system (JSON)
- HUD: coins (top-centre), level + XP bar (top-right), contextual hint (bottom-centre), tool indicator, controls overlay
- Selected crop panel (top-left, Farm Together 2 style): swatch, name, planted count, countdown timer
- Main Menu, Pause Menu, Settings UI
- Animated polyperfect character (walk/idle)
- AudioManager + AmbienceManager — all clips assigned and working
- URP shader stripping fix (ShaderIncludePreprocessor — runs before every build)

---

## Key Scripts to Know

### FarmGrid.cs
- Manages grid of FarmTile objects
- **Confirmed values: Origin(-6, 0.15, 13), TileSize 4, Width 4, Height 4**
- GridToWorld(coord) and WorldToGrid(worldPos) for coordinate conversion
- Covers Cluster A flower beds: X in {-6,-2,2,6}, Z in {13,17,21,25}

### FarmingManager.cs
- Handles plant/water/harvest (no till step)
- **ALWAYS use grid.GridToWorld(coord), NOT tile.WorldPosition**
- tileMarkers dict keyed by Vector2Int coord (blue watered marker only)
- cropVisuals dict keyed by Vector2Int coord
- growthSpeedMultiplier = 60 for testing — **CHANGE TO 1 BEFORE RELEASE**
- WaterTile() explicitly destroys existing marker before spawning new one
- GetPlantedCount(cropId) and GetNearestRemainingSeconds(cropId) for HUD queries

### FarmTile.cs
- GetRemainingSeconds() accounts for 1.3x watered growth bonus

### CropData.cs
- ScriptableObject. Has CropId, modelBaseScale, modelRotationOffset fields
- GrowthStagePrefabs[4] — same prefab for all 4 stages, scaled by CropGrowthVisual

### PlayerController.cs
- Camera-relative movement: Camera.main.transform.forward/right with y=0 + Normalize()
- Single controller.Move() per frame: (moveDir * moveSpeed + velocity) * deltaTime
- rotationSpeed=20, moveSpeed=5, gravity=-9.81
- applyRootMotion = false

### PlayerInteraction.cs
- Left click = plant/harvest; Right click = water; F = sell all
- Hover highlight: 4-edge hollow square, yellow=planted, orange=ready, green=empty
- UpdateToolText() per frame: "Build Mode" / "Watering Can" / "Planting: X" / "Farming Mode"
- SetSelectedCrop(crop) calls HUDManager.ShowSelectedCrop(crop)

### FarmCamera.cs
- Full orbit: yaw (Q/E or middle-mouse) + pitch + distance, all SmoothDamped
- C key cycles presets: Close (d=8, p=40), Mid (d=20, p=55), Far (d=35, p=68)
- Auto-follow on W only (v > 0.1f); 2s cooldown after manual rotation
- **Inspector must be set manually: minDistance=8, maxDistance=35, distance=20, pitch=55, minPitch=30, maxPitch=75**
- Collision: SphereCast from pivot + Vector3.up * 1.5f

### HUDManager.cs
- ShowSelectedCrop(CropData): sets swatch color + name, starts 0.5s refresh coroutine
- SetContextHint(string): bottom-centre pill
- UpdateToolIndicator(string): tool mode text
- CropColors dict keyed by CropId (lowercase)
- Controls panel toggled with H key

### HUDBuilder.cs (Editor — Tools > CozyFarm > Build HUD in Scene)
- Rebuilds entire HUD Canvas + PanelsCanvas from scratch
- CanvasScaler: ScaleWithScreenSize, 1920x1080, matchWidthOrHeight 0.5
- KNOWN ISSUE: Canvas may show scale (2,2,1) on HiDPI displays — under investigation
- Run after any HUD layout change

### BuildModeUI.cs
- IsOpen property (public) — read by PlayerInteraction.UpdateToolText()

---

## Crop Model Assignments
| Crop | Prefab | Scale | RotY | Notes |
|------|--------|-------|------|-------|
| Carrot | Carrot | 3.0 | 0 | |
| Sunflower | Sunflower | 0.7 | -90 | |
| Tomato | Tomato | 4.0 | 0 | |
| Potato | Bread_Round | 3.0 | 0 | PLACEHOLDER |
| Strawberry | Apple | 3.0 | 0 | PLACEHOLDER |
| Corn | Corn | 2.5 | 0 | |
| Pumpkin | Pumkin | 1.0 | 0 | |
| Grapes | Grapes_Purple_Empire | 2.0 | 0 | |
| Chilli | Eggplant | 4.0 | 0 | PLACEHOLDER |
| Lavender | Carnations | 0.8 | 0 | PLACEHOLDER |

---

## Scene Setup (Farm.unity)
- Based on Polyperfect DEMO_11_Farm scene
- Player: SKM_Boy_Beach_Rig with walk/idle animations
- GameManager holds: FarmGrid, FarmingManager, InventoryManager, EconomyManager, ProgressionManager, SaveManager, BuildingManager
- HUD Canvas (Editor-built, sort order 10) + PanelsCanvas (sort order 50) both in scene
- AudioManager + AmbienceManager on persistent GameObjects, all clips assigned
- Growth Speed Multiplier = 60 (testing) — change to 1 for release

---

## Playtest Status
- Mac build tested by girlfriend (2026-03-23)
- Windows build tested by brother (2026-03-23)
- Feedback: core loop clear, liked it overall, controls confusing

---

## Active Known Issues
- Canvas scale 2,2 on HiDPI displays — root cause under investigation
- growthSpeedMultiplier = 60 — must change to 1 before release
- Potato, Strawberry, Chilli, Lavender use placeholder models
- No Mixamo harvest/water/plant animations yet

---

## Milestone Status
- Milestone 0 (Pre-Production): DONE
- Milestone 1 (Playable Prototype): DONE
- Milestone 2 (Core Gameplay Loop): DONE
- Milestone 3 (Alpha): IN PROGRESS
  - DEV-54 UI: DONE
  - DEV-57 3D Assets: DONE
  - DEV-52 Balance: DONE
  - DEV-48 Animations: TODO (Mixamo)
  - DEV-58 Playtesting: IN PROGRESS

---

## Git Workflow
- Always: cd ~/Documents/CozyFarmGame/CozyFarmGame
- Branch: main
- Remote: github.com/Daniel466/CozyFarmGame.git

---

## Editor Tools Available
- Tools > CozyFarm > Open Toolkit — central editor window
- Tools > CozyFarm > Build HUD in Scene — full HUD rebuild (run after any HUD change)
- Tools > CozyFarm > Build Panels in Scene — panels only
- Tools > CozyFarm > Fix Always Included Shaders — fixes pink materials in builds
- Tools > CozyFarm > Setup Farm Scene — wires all GameObjects
- Tools > CozyFarm > Fix Flower Bed Layers — sets flower beds to Ignore Raycast
- Tools > CozyFarm > Assign Crop Models — assigns polyperfect prefabs to CropData
- Tools > CozyFarm > Generate Crop Assets — creates all 10 CropData SOs
- Tools > CozyFarm > Generate Building Assets — creates building SOs
- Tools > CozyFarm > Clean Demo Scene — removes pre-placed polyperfect props

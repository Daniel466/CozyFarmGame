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
- TextMeshPro with Kenney Future SDF font
- Polyperfect Low Poly Ultimate Pack (main art asset)
- Kenney UI Pack (UI elements)
- Git for version control

---

## Project Structure
```
Assets/_Project/
    Art/           - Materials, fonts, textures
    Audio/         - Music, SFX, ambience clips
    Docs/          - GDD.md, TODO.md
    Editor/        - Editor tools (FarmSceneSetup, HUDBuilder, CropModelAssigner etc.)
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
        UI/        - HUDManager, HUDBootstrapper, ShopUI, InventoryUI, BuildModeUI,
                     PauseMenuUI, SettingsUI, MainMenuUI, TileInfoUI
    UI/            - UI sprites
```

---

## Core Systems Built
- Full farming loop (till/plant/water/grow/harvest)
- 10 crops with polyperfect 3D models and growth stages
- Building and decoration placement system
- Shop UI (B key), Inventory UI (Tab), Build Mode (G key), Sell (F key)
- XP + 15 level progression system
- Coin economy (150 starting coins)
- Save/load system (JSON)
- HUD (coins, XP bar, level, notifications)
- Main Menu, Pause Menu, Settings UI
- Animated polyperfect character (walk/idle)
- AudioManager with hooks on all actions

---

## Key Scripts to Know

### FarmGrid.cs
- Manages grid of FarmTile objects
- Has Grid Origin (Vector3) for positioning grid in world
- GridToWorld(coord) and WorldToGrid(worldPos) for coordinate conversion
- Grid Origin is set to match polyperfect flower bed positions
- Current values: Origin(-8, 0.15, 10), TileSize 2, Width 12, Height 10

### FarmingManager.cs
- Handles till/plant/water/harvest actions
- Uses grid.GridToWorld(coord) NOT tile.WorldPosition (bug was fixed)
- tileMarkers dictionary keyed by Vector2Int coord
- cropVisuals dictionary keyed by Vector2Int coord
- growthSpeedMultiplier - set to 60 for testing, 1 for real gameplay

### CropData.cs
- ScriptableObject for each crop
- Has modelBaseScale and modelRotationOffset fields
- GrowthStagePrefabs[4] - same prefab used for all 4 stages, scaled by CropGrowthVisual

### CropGrowthVisual.cs
- StageScales = { 0.3f, 0.5f, 0.8f, 1.0f } multiplied by CropData.ModelBaseScale
- Uses CropData.ModelRotationOffset for rotation
- Falls back to PlaceholderAssetGenerator if no prefabs assigned

### PlayerInteraction.cs
- Left click = till/plant/harvest
- Right click = water
- Uses groundLayer mask for raycasts
- interactionRange = 10f

### HUDBuilder (Editor tool)
- Builds HUD Canvas (sort order 10) + PanelsCanvas (sort order 50) entirely in the Editor
- PanelsCanvas holds ShopPanel, InventoryPanel, BuildModePanel — all hidden by default
- ShopUI, InventoryUI, BuildModeUI components sit on PanelsCanvas and are wired via SerializedObject + EditorUtility.SetDirty
- HUDBootstrapper has been deleted — no runtime UI building needed
- BuildScrollView uses Viewport child with RectMask2D for correct clipping; Content uses VLG with childControlWidth=true
- ShopWindow anchoredPosition (250, 0), width 500px

---

## Crop Model Assignments
| Crop | Prefab | Scale | RotY | Path |
|------|--------|-------|------|------|
| Carrot | Carrot | 3.0 | 0 | Food_M |
| Sunflower | Sunflower | 0.7 | -90 | Nature_M/Flowers_M |
| Tomato | Tomato | 4.0 | 0 | Food_M |
| Potato | Bread_Round | 3.0 | 0 | Food_M |
| Strawberry | Apple | 3.0 | 0 | Food_M |
| Corn | Corn | 2.5 | 0 | Food_M |
| Pumpkin | Pumkin | 1.0 | 0 | Nature_M/Flowers_M |
| Grapes | Grapes_Purple_Empire | 2.0 | 0 | Empire_M |
| Chilli | Eggplant | 4.0 | 0 | Food_M |
| Lavender | Carnations | 0.8 | 0 | Nature_M/Flowers_M |

*Note: Potato, Chilli, Lavender are placeholder models — buy better assets later*

---

## Scene Setup (Farm.unity)
- Based on Polyperfect DEMO_11_Farm scene
- Flower beds at roughly X:-8, spaced 8 units apart
- Grid sits on top of flower beds
- Player: SKM_Boy_Beach_Rig with walk/idle animations
- GameManager has: FarmGrid, FarmingManager, InventoryManager, EconomyManager,
  ProgressionManager, SaveManager, BuildingManager
- HUD Canvas (Editor-built) + PanelsCanvas (runtime) both in scene
- Growth Speed Multiplier on FarmingManager = 60 (testing) change to 1 for release

---

## Known Issues / TODO
- [ ] Growth Speed Multiplier set to 60 — change to 1 for real gameplay
- [ ] Audio clips not assigned in AudioManager Inspector
- [ ] No Audio Listener on Main Camera
- [ ] UI has no icons (TMP sprite asset needed)
- [ ] No harvest/water/till animations (DEV-48)
- [x] FIXED: Flower bed clicking unreliable — flower beds set to Ignore Raycast layer
- [x] FIXED: Brown tilled marker removed — flower bed is the tilled visual
- [x] FIXED: Watered indicator size fixed (3.5×3.5, Y offset 0.05)
- [x] FIXED: Pre-placed polyperfect demo crops removed (383 objects via Clean Demo Scene tool)
- [x] FIXED: Carrot unlock level corrected to 1
- [x] FIXED: Shop UI showing 0 items — HUDBuilder ScrollView now uses Viewport+RectMask2D+VLG
- [x] FIXED: All crops showing locked — ProgressionManager.SetState clamps level to min 1
- [x] FIXED: HUDBootstrapper deleted — panels now built entirely in Editor via HUDBuilder

---

## Milestone Status
- Milestone 0 (Pre-Production): DONE
- Milestone 1 (Playable Prototype): DONE
- Milestone 2 (Core Gameplay Loop): DONE
- Milestone 3 (Alpha): IN PROGRESS
  - DEV-54 UI: DONE
  - DEV-57 3D Assets: DONE
  - DEV-52 Balance: DONE
  - DEV-48 Animations: TODO
  - DEV-58 Playtesting: TODO

---

## Git Workflow
- Always: cd ~/Documents/CozyFarmGame/CozyFarmGame
- Branch: main
- Remote: github.com/Daniel466/CozyFarmGame.git
- Commit format: "Brief description of change"

---

## Editor Tools Available
- Tools > CozyFarm > Open Toolkit — central editor window with all tools (use this)
- Tools > CozyFarm > Setup Farm Scene — sets up all GameObjects in scene
- Tools > CozyFarm > Build HUD in Scene — builds HUD Canvas with correct font
- Tools > CozyFarm > Fix Flower Bed Layers — sets flower beds to Ignore Raycast layer
- Tools > CozyFarm > Clean Demo Scene — removes pre-placed polyperfect crop props
- Tools > CozyFarm > Assign Crop Models — assigns polyperfect prefabs to all CropData
- Tools > CozyFarm > Clear Crop Models — clears all crop model assignments
- Tools > CozyFarm > Generate Crop Assets — creates all 10 CropData ScriptableObjects
- Tools > CozyFarm > Generate Building Assets — creates building ScriptableObjects
- Tools > CozyFarm > List Scene Object Names — prints all unique GameObject names to Console

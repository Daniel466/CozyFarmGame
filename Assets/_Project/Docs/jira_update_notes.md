# Jira Update Notes
Generated: 2026-03-23

---

## Session 2026-03-23 — UI Polish Sprint (TileInfoUI, DOTween, Offline Growth)

### DEV-54 — UI Polish
**Status: Done**

Done this session:
- TileInfoUI — contextual hover panel bottom-right (290x150)
  - Crop colour swatch, name, growth stage text, timer, water status, action hint
  - Progress bar (green fill, RectTransform anchorMax.x = GrowthProgress)
  - DOTween slide in/out: DOAnchorPosX, tileInfoVisible bool guards re-triggering
  - OnScreenX = -155, OffScreenX = 160
- Notification resized to hint-pill style: 420x32, 14pt, color(0.05,0.05,0.05,0.72)
  - Was: 500x50, 20pt, color(0.1,0.1,0.1,0.85)
- ShopUI stale copy fixed: "$crop selected! Left-click a tilled tile to plant." -> "$crop selected!"
- PauseMenuUI now creates dedicated PauseCanvas (sortingOrder 100, CanvasScaler ScaleWithScreenSize)
  - Was using FindFirstObjectByType<Canvas>() — fragile, parented to HUD canvas
- SettingsUI slider fill rect anchors fixed (anchorMin=zero, anchorMax=one, offsetMin/Max=zero)
  - Was: no anchor setup on fillRect after AddComponent<Image>() — rendered as 100x100 green blocks
- Crop Growth Speed slider added to Settings (range 1-60, wired to FarmingManager.GrowthSpeedMultiplier)
- FarmingManager.GrowthSpeedMultiplier changed from expression body to full get/set property

### DOTween Juice
**Status: Done**

- Installed DOTween from Asset Store
- CropGrowthVisual.UpdateVisual(): replaced BounceIn coroutine with DOScale(target, 0.35s, Ease.OutBack)
  - Model spawns at scale zero, DOScale animates to final size
- CropGrowthVisual.PlayWaterBounce(): replaced WaterBounce coroutine with DOPunchScale(0.25f, 0.3s, 4, 0.5f)
- CropGrowthVisual.PopOutAndDestroy(): new method — DOTween.Sequence scale to 1.3x (0.08s OutQuad) then to zero (0.18s InBack)
  - FarmingManager.HarvestTile(): game logic fires immediately; visual destroy deferred via callback
- HarvestTile restructured: removes coord from cropVisuals dict immediately, defers Destroy via PopOutAndDestroy callback
- Harvest particles: startSize 0.08-0.22 -> 0.2-0.5, startSpeed 1.5-4 -> 2-6, burst 30 -> 40, sizeOverLifetime start 0.3 -> 0.8, spawn height +0.5 -> +1.0, added Destroy fallback (3s)
- Removed old BounceIn and WaterBounce coroutines entirely

### DEV-59 — Offline Crop Growth
**Status: Done**

- FarmTile.ApplyOfflineGrowth(elapsedSeconds, speedMultiplier): advances GrowthProgress based on elapsed real time
  - Rate: 1/GrowTimeSeconds * (IsWatered ? 1.3 : 1.0) * speedMultiplier
- SaveManager.SaveGame(): saves DateTime.UtcNow.ToString("O") as saveTimestamp
- SaveManager.LoadGame(): parses saveTimestamp, calculates offlineSeconds (capped at 7 days)
  - Calls tile.ApplyOfflineGrowth() per planted tile after LoadFromSaveData, before RestoreFromSave
  - Tracks readyCount + grewCount
  - Shows notification if away > 60s: "N crops ready to harvest!" or "Your crops grew while you were away!"
- GameSaveData: added public string saveTimestamp field
- Backwards compatible: old saves without timestamp get offlineSeconds = 0

### Bug Fixes / Canvas:
- CanvasScaler scale (2,2,1) on HiDPI: confirmed CORRECT behavior — not a bug
  - CanvasScaler.ScaleWithScreenSize sets canvas.transform.localScale to match screen DPI
  - Prior bug was manually setting transform.localScale = Vector3.one — removed in previous session

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

---

## Session 2026-03-23 — Functional Buildings, Icons, Build Mode Polish

### DEV-60 — Watering Well (Functional Building)
**Status: Done**

- `WateringWellComponent.cs` — MonoBehaviour attached to placed Well; coroutine waits 2s then calls `FarmingManager.WaterTile(coord, playEffects: false)` for all tiles in radius every interval seconds
- `BuildingData` — added `autoWaterRadius` (int) and `autoWaterInterval` (float) serialised fields with public accessors
- `BuildingManager.TryPlace` + `RestoreBuilding` — attach `WateringWellComponent` if `AutoWaterRadius > 0`
- `FarmingManager.WaterTile()` — new `bool playEffects = true` param; when false skips audio, crop bounce, particles, and XP but still applies tile state + blue marker
- WateringWell BuildingData: autoWaterRadius=1, autoWaterInterval=30; polyperfect Well prefab assigned (`_M/Prefabs_M/Medieval_M/Well.prefab`)
- Notification: "Well watered N crops." shown after each auto-water pass

### DEV-61 — Build Mode Placement Fixes
**Status: Done**

- `BuildingManager.UpdateGhostPosition` + `TryPlace` — added `Plane(Vector3.up, Vector3.zero)` fallback when physics raycast misses (terrain has no collider); uses `hit.point.y` for terrain height
- `BuildingManager.CanPlace` — added `if (grid.IsValidCoord(cell)) return false` to block placement on any flower bed tile
- Notification strings: pipe chars `|` replaced with `/` (Kenney Future ASCII rule)
- `BuildModeUI` layout: VLG padding 8→6, swatch sizeDelta 60×50→70×55, button sizeDelta 160×120→120×125

### DEV-62 — IconRenderer Editor Tool
**Status: Done**

- `Assets/_Project/Editor/IconRenderer.cs` — new editor tool at `Tools > CozyFarm > Render Icons`
- Uses `PreviewRenderUtility` (URP-compatible): `AddSingleGO`, `BeginStaticPreview`, `camera.Render()`, `EndStaticPreview()`
- Camera: orthographic, `Color.clear` background, 30°/-45° isometric angle, auto-framed to renderer bounds
- Lights: key (intensity 1.3, warm, 35/-135/0), fill (intensity 0.4, cool, -10/50/0)
- Crops: `GetCropIconPrefab()` uses last non-null `GrowthStagePrefabs[]` entry (stage 3 = harvest-ready)
- Saves individual PNGs to `Assets/_Project/Art/Icons/Crops/` and `Buildings/`
- Imports as Sprite (isReadable=true, Uncompressed, no mipmaps, alphaIsTransparency)
- Assigns sprite back to `icon` field on CropData/BuildingData via SerializedObject
- Packs power-of-2 atlas (bottom-up y for TMP), imports as Default texture
- Creates `TMP_SpriteAsset` at `Assets/_Project/Art/Icons/Icons_SpriteAsset.asset` with embedded material
- Usage in TMP text: `<sprite name="carrot">`

### DEV-63 — UI Icon Integration
**Status: Done**

- `ShopUI.cs` — swatch uses `crop.Icon` sprite when available (`Color.white`, `preserveAspect=true`); falls back to `GetCropColour()`; size 36→48px
- `InventoryUI.cs` — same pattern using `item.crop.Icon`; size 36→48px
- `BuildModeUI.cs` — swatch uses `building.Icon` when available; falls back to `PlaceholderColor`; locked items get 40% alpha on icon

### DEV-64 — Build Mode Interaction Lock
**Status: Done**

- `PlayerInteraction.cs` — added `IsInBuildMode` helper property (`BuildingManager.Instance.IsInBuildMode`)
- `UpdateHoverHighlight()` — returns early and hides hover root when in build mode
- `HandleLeftClick()` + `HandleRightClick()` — guarded with `if (IsInBuildMode) return`
- Result: hovering flower beds in build mode shows no highlight and all farm actions are blocked

---

## Session 2026-03-24 — Synty Models, Flat Soil Tiles, Collectibles, Market Stall

### DEV-65 — Synty POLYGON Farm Pack Integration
**Status: Done**

- `CropModelAssigner.cs` — fully rewritten for Synty; maps each crop to 4 stage prefabs (S/M/L/Group) from `Assets/Synty/PolygonFarm/Prefabs/Plants/`
  - Per-crop tuned scales: carrot 2.8, sunflower 1.8 (rotY -90), tomato 3.0, potato 2.6, strawberry 2.8, corn 1.0, pumpkin 1.6, grapes 1.8, chilli 3.0, lavender 1.8
  - Menu: `Tools > CozyFarm > Assign Crop Models (Synty)`
- `BuildingModelAssigner.cs` — new editor tool; maps building IDs to Synty prefab paths
  - barn->SM_Bld_Barn_01, watering_well->SM_Prop_Well_01, greenhouse->SM_Bld_Greenhouse_01, silo->SM_Bld_Silo_01, market_stall->SM_Bld_ProduceStand_01, scarecrow->SM_Chr_Scarecrow_01, wooden_fence->SM_Prop_Fence_Wood_01, windmill->SM_Prop_Windmill_01
  - Stone Path and Lantern skipped gracefully (no Synty match)
  - Menu: `Tools > CozyFarm > Assign Building Models (Synty)`
- `CropGrowthVisual.cs` — StageScales updated from {0.3, 0.5, 0.8, 1.0} to {0.7, 0.85, 0.95, 1.0} to complement Synty built-in size differences
- Icons re-rendered with Synty models via `Tools > CozyFarm > Render Icons`

### DEV-66 — Flat Soil Tile System
**Status: Done**

- `FarmGrid.cs` — added `SpawnFlatSoilTile()` procedural fallback; spawns a Quad per tile when `normalTilePrefab` is null
  - Quad: `tileSize * 0.95f` square, rotated flat, warm brown material, spawns at `worldPos - Vector3.up * 0.005f` (5mm below grid origin so all crop stages sit above)
  - When `normalTilePrefab` is assigned in Inspector, fallback is skipped automatically
- `PlayerInteraction.cs` — hover highlight lowered from `+0.3f` to `+0.04f` (no longer needs to clear raised bed height)
- Scene: removed `farm-flower-bed` and `fence-shrub` objects; `gridOrigin.y` set to 0.2 (terrain surface height)
- Soil tile colour left as plain brown placeholder — will be replaced with proper dirt texture in polish stage

### DEV-67 — Collectibles Loop
**Status: Done**

- `CollectibleItem.cs` — distance-based pickup (radius 1.8f); OnTriggerEnter not used (unreliable with CharacterController)
  - Drop types: Coins (5-15), Seed (crop seed cost), LuckyFind (25-50 coins)
  - Bob + spin idle animation; sparkle particle effect
- `CollectibleSpawner.cs` — manages spawn slots, per-slot respawn timers (300s default), drop table (60% coins, 25% seed, 15% lucky)
- `AudioManager.cs` — added `collectSFX` and `PlayCollect()` method

### DEV-68 — Market Stall Auto-Sell
**Status: Done**

- `MarketStallComponent.cs` — attached to placed Market Stall; sells all inventory every 120s at 10% bonus
- `InventoryManager.cs` — added `SellAllWithBonus(float bonus)` method
- `BuildingData.cs` — added `description`, `autoSellInterval`, `autoSellBonus` fields
- `BuildModeUI.cs` — shows description text under price in build card
- `BuildingAssetGenerator.cs` — market_stall entry sets autoSellInterval=120, autoSellBonus=0.1; PascalCase filenames

### DEV-69 — Audio Library Curator
**Status: Done**

- `AudioLibraryCurator.cs` — new editor tool at `Tools > CozyFarm > Audio Library Curator`
- Scans `Assets/PaidAssets/Universal Sound FX`, groups clips by category, pre-checks 57 recommended clips
- Recommended clips cover: farming (planting/dig/harvest), watering, selling, collectibles, level up, animals (dog/cat/sheep), ambience loops
- Play button (triangle) per clip for in-editor auditioning
- Copy Selected button — copies checked clips to `Assets/_Project/Audio/SFX/Universal/`
- Delete Folder button — removes entire Universal Sound FX folder after copying

---

## Session 2026-03-24 — Dog Companion System (DEV-70)

### DEV-70 — Dog Pet System
**Status: Done**

- `DogController.cs` — Wander/Follow/Return state machine
  - Wander: roams within `wanderRadius` (5f) of doghouse via coroutine; random bark ~10% chance per waypoint
  - Follow: player within `followTriggerRange` (12f); walks/runs to `followStopDistance` (1.8f); run threshold 7f
  - Return: player beyond `returnRange` (60f); walks home; re-engages Follow at `reFollowRange` (10f) with hysteresis
  - Animation driven by `agent.velocity.sqrMagnitude` to prevent walk flicker at stop boundary
  - Animator contract: Speed float (0=Idle, 1=Walk, 2=Gallop), Pet trigger, Eat trigger
  - Interaction highlight ring: pulsing yellow/white hollow square on ground within interaction range
  - Context hint: "E: Pet Max - Happy: N%" / "E: Feed Max - Happy: N%" via HUDManager.SetContextHint
  - Happiness: drains 0.004f/s, pet +0.30, feed +0.50; drives FarmingManager.DogGrowthBonus (0-0.5x additive)
  - Crop alert: barks and notifies when crops ready (30s check interval, 120s cooldown, happiness threshold 0.30)
- `DogManager.cs` — singleton lifecycle
  - `SpawnDog(Vector3)`: NavMesh.SamplePosition validated spawn, calls SetHome, shows HUD panel
  - `DespawnDog()`: destroys instance, hides HUD panel
  - `HasDog` property used by CanPlace to block second doghouse
- `DogHappinessHUD.cs` — fill bar with red/green colour lerp, % label; hidden until doghouse placed
- `ShibaInuSetup.cs` — editor tool at Tools > CozyFarm > Setup ShibaInu Dog
  - Builds `ShibaInu_AC.controller` with correct `AnimalArmature|` prefixed clip names
  - Unpacks FBX prefab instance, strips pack MonoBehaviours, forces all children active
  - Builds `ShibaInu_Dog.prefab`: root (NavMeshAgent + CapsuleCollider + DogController) + model child + InteractionPrompt
- `DogAnimatorGenerator.cs` — EditorWindow for manual clip assignment (fallback tool)
- `Doghouse.asset` — BuildingData: level 3, cost 300, 1x1, isDoghouse=true, Synty Outhouse placeholder prefab
- `BuildingManager.CanPlace` — blocks second doghouse with "Max already has a home!" notification
- `SaveManager` — persists `dogHappiness` float; restored via `DogController.SetHappiness()` after buildings load
- `FarmingManager.DogGrowthBonus` — additive float separate from base multiplier; zeroed on dog destroy

### Bug Fixes:
- Dog walking animation showing when idle at player's side — fixed by driving Speed from agent.velocity not distance
- Dog model disabled at runtime — caused by pack MonoBehaviour on FBX; stripped in ShibaInuSetup via UnpackPrefabInstance + DestroyImmediate
- ShibaInuSetup IndexOutOfRangeException — fixed by deleting and recreating controller instead of clearing layers in-place
- ShibaInuSetup MissingComponentException — fixed by calling PrefabUtility.UnpackPrefabInstance before adding components

## Next Session Priorities:
1. Mixamo animations (DEV-48) — harvest, water, plant on player character
2. Crop bloom/glow when ready to harvest
3. Stone Path and Lantern — find Synty prefab matches or source alternatives
4. Better crop model matches: Potato, Strawberry, Chilli, Lavender

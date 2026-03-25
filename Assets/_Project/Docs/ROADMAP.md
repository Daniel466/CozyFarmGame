# Development Roadmap — Phase 1
*Updated: 2026-03-25*

---

## Goal
Build a complete, playable Phase 1 — a cozy farm automation game with real-time crop growth, a sell box, workers, and a companion. No energy, no seasons, no fail states.

**Core loop:** Plant -> Grow (real-time) -> Harvest -> Sell -> Earn -> Hire Workers -> Automate -> Expand

---

## Phase 1 Build Order

### Step 1 — Codebase Rebuild (DONE)
- [x] Delete old systems: EnergyManager, GameTimeManager, Season, DayTransition, SleepInteraction, DogManager, DogController, ShopUI, CollectibleItem
- [x] Rewrite FarmGrid — 20x20, tileSize 1, FarmInteract layer BoxCollider
- [x] Rewrite FarmTile — real-time growth via DateTime.UtcNow.Ticks timestamps
- [x] Rewrite CropData — growthTimeSeconds replaces growthDays, no season field
- [x] Rewrite FarmingManager — till/plant/harvest/remove, no energy/water/XP
- [x] Rewrite PlayerInteraction — FarmTool enum (Hoe/Seed/Harvest/Remove/Build), walk-to + area drag
- [x] New RealTimeManager — 1-second tick, autosave every 90s, OnTick event
- [x] Update EconomyManager — lifetimeEarnings replaces XP/levels
- [x] Update SaveManager — timestamp tiles, lifetimeEarnings, no time/energy/dog data
- [x] Update CropGrowthVisual — Refresh() called by RealTimeManager, no per-frame polling
- [x] Trim CropDatabase — Wheat, Carrot, Corn only (3 starter crops)
- [x] Editor tool cleanup — delete CleanDemoScene, DogAnimatorGenerator; update Toolkit

### Step 2 — Scene Foundation (IN PROGRESS)
- [x] FarmSceneSetup: Full Setup creates ground plane + directional light + all systems
- [ ] Create Farm.unity from Basic (URP) scene
- [ ] Add Synty hybrid player character, name root "Player"
- [ ] Run Game Systems Only to wire PlayerInteraction + FarmGrid
- [ ] Confirm FarmInteract layer exists in Project Settings
- [ ] Confirm player moves, tiles highlight on hover, planting works
- [ ] Assign Poly Universal Pack stage prefabs to Wheat/Carrot/Corn CropData SOs
- [ ] Wire AudioManager clips in Inspector

### Step 3 — Real-Time Growth Verification
- [ ] Plant crop, wait, confirm growth stages change every ~30s (test at 60x speed)
- [ ] Confirm harvest works and adds to inventory
- [ ] Confirm save/load preserves planted timestamps (offline growth catch-up)

### Step 4 — Economy + Sell Box
- [ ] SellBox building — player walks up and sells all inventory
- [ ] Coins earned shown in HUD
- [ ] LifetimeEarnings updated on each sale
- [ ] Milestone gates: unlock new crops/buildings at earnings thresholds

### Step 5 — HUD Pass
- [ ] Coins display (top-centre)
- [ ] Selected crop panel (top-left): name, planted count, time remaining
- [ ] Context hint (bottom-centre): what current tool will do on hovered tile
- [ ] Tool indicator (bottom-centre): current tool name
- [ ] Notification panel: harvest ready, milestone unlocked

### Step 6 — Buildings Pass
- [ ] Sell Box — functional
- [ ] Watering Well — stub only for now (watering deferred)
- [ ] Assign Poly Universal Pack models to all buildings in BuildingDatabase

### Step 7 — Save System Verification
- [ ] Confirm tiles save/load with plantedAtUtcTicks
- [ ] Confirm lifetimeEarnings persists
- [ ] Confirm buildings save/load positions

### Step 8 — Worker System
- [ ] A* pathfinder (AStarPathfinder) — tile-based, queries FarmGrid + BuildingManager
- [ ] WorkerAgent — states: Idle / Move / Harvest / Deliver / Return
- [ ] WorkerManager — assigns tasks, spawns/despawns workers
- [ ] WorkerData ScriptableObject — speed, cost, capacity
- [ ] Workers subscribe to RealTimeManager.OnTick for state updates
- [ ] Buy first worker via milestone unlock (lifetimeEarnings gate)

### Step 9 — Companion
- [ ] Human helper companion — periodically sells inventory from Sell Box
- [ ] Companion visible in scene, simple idle/walk animation
- [ ] Unlocked at lifetimeEarnings milestone

### Step 10 — Polish + Balance
- [ ] Tune crop grow times (Wheat 120s, Carrot 180s, Corn 300s → adjust to feel)
- [ ] Tune sell prices so progression feels rewarding
- [ ] Ensure milestone gates feel reachable but require some play
- [ ] Camera presets tuned to new scene scale
- [ ] Audio fully assigned and mixed

---

## Milestone Gates (lifetime earnings)
| Earnings | Unlock |
|----------|--------|
| 0 | Start: Wheat, basic hoe, sell box |
| 500 | Carrot seed, first worker slot |
| 1500 | Corn seed |
| 3000 | Second worker slot |
| 5000 | Companion |
| 10000 | Grid expansion (Phase 2 hook) |

---

## Phase 2 Preview (after Phase 1 ships)
- Grid expansion (unlock adjacent plots)
- More crops + buildings
- Worker upgrades (speed, capacity)
- 2-3 NPCs with simple dialogue
- iPadOS touch input

---

## What Was Removed vs Kept

| System | Status |
|--------|--------|
| EnergyManager | DELETED |
| GameTimeManager, Season, DayTransition, Sleep | DELETED |
| DogManager, DogController | DELETED |
| CollectibleItem, CollectibleSpawner | DELETED |
| ShopUI | DELETED |
| XP / level system | DELETED — replaced by lifetimeEarnings milestones |
| Watering / WaterTile | REMOVED (stub only) |
| Save system | REWRITTEN — timestamp-based |
| Building placement | KEPT |
| ScriptableObject pattern | KEPT |
| Player movement + camera | KEPT |
| Audio hooks | KEPT |
| Editor toolkit | KEPT + CLEANED UP |

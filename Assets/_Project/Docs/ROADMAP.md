# Development Roadmap — Phase 1
*Updated: March 2026*

---

## Goal
Build a complete, playable Phase 1 — a proper farming game with seasons, energy, game-day time, and a meaningful Year 1 goal. Everything designed to expand cleanly into Phase 2.

---

## Phase 1 Build Order

### Step 1 — Scene Foundation
- [ ] Run scene migration tool (Farm -> DEMO_07_Farm)
- [ ] Reposition FarmGrid / Grid Origin on new terrain
- [ ] Reposition Player spawn
- [ ] Reposition CollectibleSpawner points
- [ ] Assign Poly Universal Pack crop models (run CropModelAssigner tool)
- [ ] Assign Poly Universal Pack building models

### Step 2 — Time System (Core rewrite)
- [ ] GameTimeManager — tracks Day, Season, Year
- [ ] Sleep mechanic — bed interaction advances day
- [ ] Season change screen / announcement
- [ ] Day counter and season indicator in HUD
- [ ] Remove real-time crop growth — switch to game-day based

### Step 3 — Energy System
- [ ] EnergyManager — 100 energy per day, restore on sleep
- [ ] Deduct energy on: till (10), plant (4), water (5), harvest (8)
- [ ] Block actions at 0 energy
- [ ] Energy bar in HUD
- [ ] Food items restore energy (simple pickup, no cooking yet)

### Step 4 — Seasons + Crops
- [ ] Add Season field to CropData ScriptableObject
- [ ] Assign seasons to all 10 crops (see GDD crop table)
- [ ] Block planting out-of-season crops
- [ ] Crops die at season end if not harvested
- [ ] Tilled tiles reset at season end
- [ ] Update grow time from seconds to game-days

### Step 5 — Tools
- [ ] Tool system — Hoe, Watering Can, Sickle as distinct equippable items
- [ ] Watering Can has capacity (10 uses), refill at well
- [ ] Tool toolbar in HUD (3 slots, number key switching)
- [ ] Tilling now requires Hoe equipped
- [ ] Harvesting now requires Sickle equipped

### Step 6 — Economy Rework
- [ ] Shipping Crate building — drop goods in, paid next morning
- [ ] Seasonal price modifiers (out-of-season = higher price)
- [ ] Starting coins: 500
- [ ] Farm debt goal: 2000 coins by end of Year 1
- [ ] Debt tracker in HUD
- [ ] Remove old instant-sell system

### Step 7 — Farm Grid Expansion
- [ ] Larger starting plot (expand from current small grid)
- [ ] Year 2 second plot unlock hook (just the unlock trigger for now)
- [ ] Tilled tile revert on season end

### Step 8 — UI Pass
- [ ] Day / Season / Year display (top of screen)
- [ ] Energy bar (below coins)
- [ ] Tool slot indicator (bottom centre)
- [ ] Debt progress tracker
- [ ] Season change full-screen announcement
- [ ] Remove old XP bar and level system (replaced by coin-based progression)

### Step 9 — Buildings Pass
- [ ] Farmhouse with bed interaction (sleep trigger)
- [ ] Shipping Crate with inventory drop-off UI
- [ ] Water Well refill interaction
- [ ] Assign real Poly Universal Pack models to all buildings

### Step 10 — Polish + Balance
- [ ] Tune crop day counts and prices
- [ ] Tune energy costs
- [ ] Ensure Year 1 debt goal is achievable but requires planning
- [ ] Winter feel (sparse, quiet, no crops)
- [ ] Season change ambience / music shift

---

## What We Keep From Current Codebase

| System | Status |
|--------|--------|
| Save system | Keep — extend for new time data |
| Building placement | Keep |
| ScriptableObject pattern | Keep — extend CropData / BuildingData |
| Editor toolkit | Keep — add new tools as needed |
| UI framework | Keep — rework individual panels |
| Player movement | Keep |
| Camera | Keep |
| Audio hooks | Keep — add seasonal music |
| Dog companion | Keep for now |
| Collectibles loop | Keep — good filler activity |

## What Gets Replaced

| System | Replacement |
|--------|-------------|
| Real-time crop growth | Game-day growth |
| XP + level system | Coin-based progression |
| Instant sell (shop) | Shipping crate (paid next morning) |
| Watering Well auto-water | Manual watering with can capacity |
| Old FarmGrid size | Larger expandable grid |

---

## Phase 2 Preview (after Phase 1 ships)
- Tool upgrades via blacksmith
- Crop quality tiers
- Chickens + eggs
- 2-3 NPCs with shop and dialogue
- Shipping contracts

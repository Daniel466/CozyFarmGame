# Game Design Document
## Harvest & Homestead — v2.1
*Updated: March 2026 | Direction: Life Sim x Farming Simulator*

---

## 1. Overview

| Field | Detail |
|-------|--------|
| Genre | 3D Life Sim / Farming Simulator hybrid |
| Platform | PC (Windows) — itch.io launch |
| Engine | Unity 6 (URP) |
| Inspiration | Stardew Valley, Story of Seasons, Farming Simulator 25 |
| Target Audience | Players who want a relaxing but deep farming experience |

### Elevator Pitch
You inherit a neglected farm. Work the land through seasons, invest in equipment, manage your crops and economy, and build something you are proud of. The heart of a life sim with the depth of a farming simulator.

---

## 2. Core Fantasy

Start with one small field and hand tools. Over time expand to multiple plots, upgrade equipment, and grow a thriving farm operation. The game rewards planning, patience, and good decisions — not grinding.

---

## 3. Core Loop

**Each day:**
- Wake up
- Work the farm (till, plant, water, harvest)
- Drop harvested goods in the shipping crate
- Sleep to advance to the next day

**Each season (28 days):**
- Only season-appropriate crops can be planted
- Unharvested crops die at season end
- Market prices shift with the season
- Winter: forage for items instead of farming

**Year arc:**
- Year 1: build the farm, pay off debt by season end
- Year 2+: expand plots, upgrade tools, build processing buildings

*Note: Weather system is planned for Phase 2. Not in Phase 1.*

---

## 4. Systems — Phase 1

### 4a. Time
- The day advances only when the player sleeps (bed interaction)
- Passing out at midnight: lose 100 coin penalty, wake up next morning
- No offline growth — game time only passes when playing
- Auto-saves on sleep and on quit (single save slot)

### 4b. Energy

| Action | Energy Cost |
|--------|------------|
| Tilling | 10 |
| Planting | 4 |
| Watering | 2 |
| Harvesting | 8 |

- Max energy: 100 per day, fully restored on sleep
- At 0 energy all farm actions are blocked — player must eat or sleep
- Eating food items restores 20 energy
- **Typical full day:** 10 tiles tilled (100e) or 20 tiles watered (40e) + 5 harvested (40e) + 5 planted (20e) = 100e. Balanced.

### 4c. Seasons
- 4 seasons: Spring / Summer / Fall / Winter — 28 days each
- Crops are season-locked (see crop table)
- Season end: unharvested crops die, all tilled empty tiles reset
- Season change announced with a full-screen UI moment
- **Winter:** no crops grow — player forages instead (see 4i)

### 4d. Crops

| Crop | Season | Days | Seed Cost | Sell Value | Profit/Tile |
|------|--------|------|-----------|------------|-------------|
| Carrot | Spring | 4 | 20 | 50 | 30 |
| Potato | Spring | 6 | 30 | 80 | 50 |
| Strawberry | Spring | 8 | 50 | 120 | 70 |
| Tomato | Summer | 5 | 35 | 90 | 55 |
| Corn | Summer | 7 | 45 | 110 | 65 |
| Watermelon | Summer | 10 | 80 | 200 | 120 |
| Pumpkin | Fall | 8 | 60 | 150 | 90 |
| Sunflower | Fall | 5 | 30 | 75 | 45 |
| Wheat | Fall | 4 | 15 | 40 | 25 |
| Leek | Fall + Winter | 6 | 25 | 65 | 40 |

- 4 growth stages: Seedling / Young / Mature / Ripe
- Watered crops grow 1 day per day. Unwatered crops pause growth (no death)
- Leek is the only Winter crop — gives players a reason to farm in Winter

### 4e. Tools

| Tool | Key | Action | Energy | Notes |
|------|-----|--------|--------|-------|
| Hoe | 1 | Tills one tile | 10 | Required before planting |
| Watering Can | 2 | Waters one tile | 2 | Capacity: 20 uses, refill at well |
| Sickle | 3 | Harvests ripe crop | 8 | Must be equipped |
| — | — | Plant seed | 4 | No tool needed, uses selected crop from inventory |

- Tool toolbar always visible at bottom of HUD
- Watering can shows remaining capacity as a number badge
- Bare hands (no tool): can interact with buildings and objects only
- Tile hover highlight changes by equipped tool:
  - Hoe → brown highlight on untilled tiles
  - Watering Can → blue highlight on unwatered planted tiles
  - Sickle → gold highlight on ripe tiles

### 4f. Farm Grid
- Start: one 10×10 plot (100 tiles)
- Practical active area: ~20–30 tiles given energy limits
- Second plot unlocks start of Year 2
- Tilled but unplanted tiles reset at season end

### 4g. Economy
- Coins are the only Phase 1 currency
- **Shipping crate** (free, starter): drop goods in, paid next morning
- Prices vary by season — selling out of season earns up to 50% more
- **Silo**: store harvested crops to sell in a later season at higher prices
- Starting coins: 500
- Farm debt: 2000 coins owed by end of Year 1
  - Debt not paid → rolls over to Year 2 with 20% penalty added (not game over)
  - First-time players who struggle in Spring can still recover in Summer/Fall

### 4h. Progression
- **Farm level based on total shipping revenue only** (coins earned via the crate)
- Buying and selling without farming does not count
- "Total Shipped" displayed as farm milestone tracker on HUD
- Levels unlock buildings and bonuses — no XP, no level grinding

### 4i. Winter Foraging
- Winter has no plantable crops (except Leek)
- Enhanced collectibles spawn around the farm: pinecones, winter berries, frost mushrooms
- Higher spawn rate and higher coin value than normal collectibles (15–40 coins each)
- Foraging gives players 28 days of meaningful activity without complex systems
- Foraged seeds can drop as rare finds — safety net for players who run low on coins

### 4j. Fail States

| Situation | Outcome |
|-----------|---------|
| 0 coins, can't buy seeds | Foraged seeds drop in winter; collectibles always provide some income |
| Debt not paid by Year 1 end | Debt rolls over + 20% penalty; game continues |
| Pass out (midnight) | Wake up next day, lose 100 coins |
| Run out of energy | Farm actions blocked; eat or sleep to continue |

- No game over states in Phase 1 — the game always lets you continue
- No way to sell tools or buildings — no softlock risk

### 4k. Save System
- Auto-saves on sleep (every day advance)
- Auto-saves on quit
- Single save slot
- Saves: coins, farm time (day/season/year), energy, tool state, inventory, all tiles, all buildings

---

## 5. Systems — Phase 2 (Planned)

- Weather: sunny (default) / rainy (auto-waters all tiles) — 30% rain chance per day
- Tool upgrades via blacksmith NPC
- Crop quality tiers (Standard / Premium / Artisan)
- Livestock: chickens (eggs), cows (milk)
- 2–3 NPCs with basic shop and dialogue
- Shipping contracts (bonus pay for bulk orders)
- Soil quality and fertilizer

---

## 6. Systems — Phase 3 (Full vision)

- Processing buildings (mill, press, dairy)
- Full NPC town with relationships
- Tractor and machinery
- Multiple farm plots with distinct soil types
- Marriage / story arc

---

## 7. Buildings — Phase 1

| Building | Cost | Function |
|----------|------|----------|
| Farmhouse | Free (starter) | Sleep to advance day |
| Shipping Crate | Free (starter) | Drop off goods, paid next morning |
| Water Well | 300 coins | Refill watering can to full |
| Barn | 500 coins | Increases carry capacity (+20 crop slots) |
| Silo | 800 coins | Store harvested crops to sell later at better prices |
| Greenhouse | 1500 coins | Grow any crop regardless of season |

---

## 8. Player

- Synty POLYGON character with Mixamo animations
- Animations: Idle, Walk, Till, Plant, Water, Harvest
- Energy bar in HUD
- Tool toolbar in HUD (always visible, shows can capacity)

---

## 9. Camera

- Isometric third-person (Stardew / Farm Together style)
- WASD moves player, scroll zooms, Q/E rotates
- Camera follows player with soft lag

---

## 10. Controls

| Action | Input |
|--------|-------|
| Move | WASD |
| Use tool / interact | Left Click |
| Switch tool | 1 / 2 / 3 |
| Open inventory | Tab |
| Open shop | B |
| Toggle build mode | G |
| Sleep (near bed) | E |
| Zoom | Scroll Wheel |
| Rotate camera | Q / E |
| Pause | Escape |

---

## 11. Audio

| Element | Style |
|---------|-------|
| Music | Acoustic, seasonal — gentle spring, warm summer, melancholic fall, sparse winter |
| Tilling | Earthy thud |
| Watering | Gentle splash |
| Planting | Soft pat |
| Harvesting | Satisfying pop |
| Season change | Warm chime |
| Ambience | Birds, wind, seasonal sounds layered under music |

*Full audio spec (track count, looping, volume priorities) to be defined when sourcing assets.*

---

## 12. Economy Stress Test

| Scenario | Math |
|----------|------|
| Debt target | 2000 coins by end of Year 1 (112 days) |
| Spring max (Strawberry) | 20 tiles × 70 profit × 3 harvests = 4200 coins |
| Summer max (Tomato) | 20 tiles × 55 profit × 5 harvests = 5500 coins |
| Fall max (Pumpkin) | 20 tiles × 90 profit × 3 harvests = 5400 coins |
| New player (Carrot only) | 10 tiles × 30 profit × 7 harvests = 2100 coins (Spring alone clears debt) |

Debt is achievable even for a cautious first-time player in Spring alone. The 20% rollover penalty is a soft lesson, not a punishment.

---

## 13. Out of Scope for Phase 1

- Weather system (Phase 2)
- Multiplayer
- Mobile / controller support
- NPCs (Phase 2)
- Crafting / processing (Phase 3)
- Livestock (Phase 2)
- Character customisation

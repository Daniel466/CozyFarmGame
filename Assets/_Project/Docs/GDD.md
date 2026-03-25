# Solo Farm Automation Game — GDD v1.0
*Updated: March 2026 | Direction: Cozy Farming Simulation with Automation*

---

## 1. Game Overview

| Field | Detail |
|-------|--------|
| Genre | Cozy Farming Simulation with Automation |
| Engine | Unity 6.3 LTS |
| Primary Platform | PC (Keyboard / Mouse) |
| Secondary Platform | iPadOS (touch input adaptation later) |
| Perspective | Third-person, Farm Together-style camera |
| World Type | Single isolated expandable farm |
| Core Theme | Build, automate, and expand a relaxing large-scale farm using AI helpers |
| Multiplayer | Single player v1 — co-op hook planned later |

### Elevator Pitch
Start farming by hand. Hire workers and a companion to automate the grind. Watch your farm run itself while you focus on expanding and optimising. No pressure, no fail states — just building something you're proud of.

---

## 2. Core Gameplay Loop

Plant Crops → Grow in Real-Time → Harvest → Sell → Earn Money → Expand Farm → Hire Helpers → Automate Tasks → Repeat

---

## 3. Grid System

| Setting | Value |
|---------|-------|
| Starting Grid Size | 20x20 tiles |
| Tile Size | 1x1 Unity units |
| Tile Types | Grass / Soil / Occupied (buildings) |

**Features:**
- Grid snapping
- Area drag actions (drag to till / plant / harvest multiple tiles)
- Tile highlighting (colour by action type)

---

## 4. Crop System

**Starter Crops (Milestone 1):**
- Wheat
- Carrot
- Corn

**Full Starter Set Goal (later):** 6-10 crops total

**Growth Model:**
- Real-time growth
- Offline growth supported (timestamp-based)
- 4 growth stages:
  - Stage 1: Seed
  - Stage 2: Sprout
  - Stage 3: Growing
  - Stage 4: Ready
- Growth timing: minutes to ~1 hour depending on crop
- Crops do not die if not harvested — stay Ready until collected

---

## 5. Inventory System

| Setting | Value |
|---------|-------|
| Type | Stack-based |
| Initial Capacity | Unlimited stacks |
| Stores | Crop items, Seeds |

**Future:** storage buildings expand or limit capacity

---

## 6. Sell Box System

- Sell Box placed near farmhouse
- Player interacts to open sell panel
- Items removed from inventory, money added to funds
- **Future automation:**
  - Companion delivers items to Sell Box
  - Workers deliver harvested goods to Sell Box

---

## 7. Storage Shed

- First unlockable building
- Size: 2x2 tiles
- Purpose: store large quantities of items, support future logistics systems

---

## 8. Building Placement System

| Feature | Detail |
|---------|--------|
| Placement Mode | Instant grid snapping |
| Rotation | 90 degree steps |
| Validation | Multi-tile footprint check |
| Preview | Green = valid, Red = invalid |

---

## 9. Worker System

- Initial worker count: 1
- **First worker abilities:**
  - Harvest ripe crops
  - Deliver harvested items to Sell Box

**Worker States:**
Idle → Move → Harvest → Deliver → Return

**Movement:** Tile-based A* pathfinding
**Obstacles:** Buildings, water, locked tiles

---

## 10. Companion System

- Type: Human companion
- **First ability:** periodically sells items from inventory
- **Idle behaviour:** remains near farmhouse, moves only when selling
- **Future upgrades:**
  - Carry more items
  - Faster delivery
  - Additional helper tasks

---

## 11. Tool System

- All tools are instant-use
- **Supports area drag** — hold and drag across tiles to apply action to multiple tiles

**Starter Tools:**

| Tool | Action |
|------|--------|
| Hoe | Till tile(s) |
| Seed | Plant selected crop |
| Harvest | Harvest ripe crop(s) |
| Remove | Remove crop or building |
| Build | Enter building placement mode |

---

## 12. Real-Time Manager

- **Tick rate:** every 1 second
- **Handles:**
  - Crop growth timing
  - Companion task triggers
  - Worker state checks
  - Autosave timing

---

## 13. Save System

- **Auto-save only** (no manual save)
- **Save triggers:**
  - Every 90 seconds
  - On key events (sell, hire worker, place building)
  - On exit
- **Saved data:** grid state, crops + timestamps, inventory, buildings, workers, money, lifetime earnings

---

## 14. UI Layout

**Main HUD (bottom of screen):**
- Tool bar
- Selected crop indicator
- Inventory access button

**Additional UI:**
- Inventory window
- Storage window
- Sell interface
- Milestone / unlock notifications

---

## 15. Pathfinding System

- Worker navigation uses tile-based A* pathfinding
- Movement restricted to walkable tiles
- **Obstacles:** buildings, water, locked tiles

---

## 16. Platform Strategy

| Platform | Input |
|----------|-------|
| PC (primary) | Keyboard + Mouse |
| iPadOS (secondary, later) | Touch input |

- Unity New Input System throughout
- Supports: keyboard, mouse, controller, future touch

---

## 17. First Playable Milestone

**Includes:**
- [ ] Player movement + camera
- [ ] 20x20 grid system
- [ ] Crop planting (Wheat, Carrot, Corn)
- [ ] Real-time growth with 4 stages
- [ ] Harvesting
- [ ] Sell Box interaction
- [ ] Inventory
- [ ] Autosave

---

## 18. Future Expansion Systems

- Multiple workers with specialisations
- Expanded storage and logistics
- Additional crops (6-10 total)
- Advanced buildings (barn, greenhouse, silo)
- Automation upgrades (worker speed, carry capacity)
- Farm expansion zones (unlock more land)
- Companion upgrades
- iPad touch input

---

## 19. Out of Scope for Phase 1

- Seasons or weather
- Energy system
- Debt or fail states
- NPCs / dialogue
- Multiplayer / co-op
- Crop quality tiers
- Crafting / processing
- Controller support

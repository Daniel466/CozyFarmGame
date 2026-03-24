# 🌾 Game Design Document (GDD)
## Cozy Farming Game — v0.1
*Created: March 2026 | Status: Living document — update as design evolves*

---

## 1. Overview

| Field | Detail |
|-------|--------|
| Genre | 3D Cozy Farming / Building |
| Platform | PC (Windows) — itch.io launch |
| Engine | Unity 6 (URP) |
| Inspiration | Farm Together, Hokko Life, Stardew Valley |
| Team | Game Designer/Producer + Unity Developer |
| Target Audience | Casual gamers who enjoy relaxing, creative experiences |

### Elevator Pitch
A warm, low-pressure 3D farming game where you plant and harvest crops, build and decorate your dream farm, and progress at your own pace. No fail states. No timers. Just cozy.

---

## 2. Core Gameplay Loop

```
Plant seed → Water crop → Wait for growth → Harvest → Sell for coins
→ Buy seeds & items → Earn XP & level up → Unlock new crops & buildings
→ Decorate & expand farm
```

---

## 3. Crops

Total: **10 crops** — 4 starter, 6 unlocked through progression.

| # | Crop | Unlock Level | Grow Time | Seed Cost | Sell Value | Notes |
|---|------|-------------|-----------|-----------|------------|-------|
| 1 | Carrot | Starter | 5 min | 5 coins | 10 coins | First crop, tutorial crop |
| 2 | Sunflower | Starter | 8 min | 8 coins | 15 coins | Decorative feel |
| 3 | Tomato | Starter | 12 min | 12 coins | 25 coins | Mid-value starter |
| 4 | Potato | Starter | 10 min | 10 coins | 20 coins | Reliable earner |
| 5 | Strawberry | Level 3 | 15 min | 20 coins | 35 coins | First unlock |
| 6 | Corn | Level 5 | 20 min | 30 coins | 50 coins | Tall, visually striking |
| 7 | Pumpkin | Level 7 | 30 min | 45 coins | 80 coins | Slow but high value |
| 8 | Grapes | Level 9 | 25 min | 38 coins | 65 coins | On a trellis structure |
| 9 | Chilli | Level 11 | 18 min | 32 coins | 55 coins | Spicy personality |
| 10 | Lavender | Level 14 | 35 min | 60 coins | 100 coins | Rare, prestige crop |

### Crop Growth Stages (all crops)
0. **Planted** — seed in soil
1. **Sprouting** — small shoot
2. **Growing** — mid-size plant
3. **Ready** — fully grown, glowing/sparkle effect

### Watering
Watering a crop reduces its grow time by **30%**. Each crop needs watering once per growth cycle.

### Seasons
Not in v1.0 — consider for a future update post-launch.

---

## 4. Buildings

### 4a. Functional Buildings

| Building | Unlock Level | Cost | Function |
|----------|-------------|------|----------|
| Barn | Starter | Free | Increases inventory capacity (+20 slots) |
| Watering Well | Level 2 | 200 coins | Auto-waters adjacent 3x3 tiles once per cycle |
| Greenhouse | Level 6 | 800 coins | Crops inside grow 50% faster |
| Silo | Level 8 | 600 coins | Stores harvested crops (overflow storage) |
| Market Stall | Level 10 | 1000 coins | Auto-sells crops at a 10% bonus rate |

### 4b. Decorative Items

| Category | Items |
|----------|-------|
| Fencing | Wooden fence, stone wall, picket fence, hedge |
| Paths | Dirt path, stone path, brick path, grass stepping stones |
| Flowers | Rose bush, daisy patch, tulip bed, wildflowers |
| Structures | Scarecrow, mailbox, well, signpost, bird bath, lantern |
| Trees | Apple tree, cherry blossom, oak tree, pine tree |
| Misc | Hay bale, wooden crate, garden bench, windmill (animated) |

---

## 5. Player Progression

### XP Sources

| Action | XP Earned |
|--------|-----------|
| Planting a crop | 2 XP |
| Watering a crop | 1 XP |
| Harvesting a crop | 5 XP |
| Placing a building | 10 XP |
| Placing a decoration | 3 XP |
| Selling crops | 1 XP per 10 coins earned |

### Level Table (1–15)

| Level | XP Required | Unlock |
|-------|------------|--------|
| 1 | 0 | Starter crops, Barn |
| 2 | 100 | Watering Well, Wooden Fence |
| 3 | 250 | Strawberry, Stone Path |
| 4 | 450 | Flower decorations, Bird Bath |
| 5 | 700 | Corn, Picket Fence |
| 6 | 1,000 | Greenhouse, Cherry Blossom Tree |
| 7 | 1,400 | Pumpkin, Scarecrow |
| 8 | 1,900 | Silo, Hay Bale |
| 9 | 2,500 | Grapes, Trellis, Lantern |
| 10 | 3,200 | Market Stall, Windmill |
| 11 | 4,000 | Chilli, Garden Bench |
| 12 | 5,000 | Expanded farm tiles, Apple Tree |
| 13 | 6,200 | Premium fence & path sets |
| 14 | 7,500 | Lavender (prestige crop) |
| 15 | 9,000 | MAX — all items unlocked, cosmetic title "Master Farmer" |

---

## 6. Economy

| Item | Seed Cost | Sell Value | Profit Margin |
|------|-----------|------------|---------------|
| Carrot | 5 coins | 10 coins | 2× |
| Sunflower | 8 coins | 15 coins | 1.9× |
| Tomato | 12 coins | 25 coins | 2.1× |
| Potato | 10 coins | 20 coins | 2× |
| Strawberry | 20 coins | 35 coins | 1.75× |
| Corn | 30 coins | 50 coins | 1.67× |
| Pumpkin | 45 coins | 80 coins | 1.78× |
| Grapes | 38 coins | 65 coins | 1.71× |
| Chilli | 32 coins | 55 coins | 1.72× |
| Lavender | 60 coins | 100 coins | 1.67× |

**Economy Rule of Thumb:** Sell value is ~2–3× seed cost for starter crops, tapering to ~1.5–2× for later crops to keep the economy balanced.

---

## 7. Player Character

- **Type:** Fixed, pre-designed character
- **Style:** Friendly, gender-neutral, cozy aesthetic (overalls, boots, sun hat)
- **Animations:** Idle, walk, watering, harvesting, building placement, wave (emote)
- No character customisation in v1.0 — consider cosmetic unlocks (hats, outfits) post-launch

---

## 8. Camera

- **Type:** Soft isometric / angled third-person (similar to Farm Together)
- **Controls:** WASD to move character, scroll wheel to zoom, middle-mouse drag to pan
- **Angle:** Fixed ~45° angle, rotatable in 90° increments (Q/E keys)

---

## 9. Controls (PC)

| Action | Input |
|--------|-------|
| Move | WASD |
| Interact / Plant / Harvest | Left Click |
| Water | Right Click (with watering can equipped) |
| Open inventory | Tab |
| Open shop | B |
| Toggle build mode | G |
| Rotate object (build mode) | R |
| Zoom | Scroll Wheel |
| Rotate camera | Q / E |
| Pause menu | Escape |

---

## 10. Audio Direction

| Element | Style |
|---------|-------|
| Background music | Lo-fi acoustic, gentle piano, soft guitar loops |
| Harvesting | Satisfying pop/pluck sound |
| Watering | Gentle splashing |
| Planting | Soft thud/pat |
| Building placement | Wooden click/thunk |
| Level up | Warm chime fanfare |
| UI buttons | Soft click |
| Ambience | Birds, breeze, distant nature sounds |

---

## 11. Player Engagement While Crops Grow

> **Design Priority:** This is a core gap that must be solved before itch.io launch.
> The farming loop creates mandatory wait time (5–35 min per crop). Without things to do during that window, players will quit. Every feature below addresses this directly.

### The Problem
After planting and watering, the player has nothing to do until crops are ready. In a real session this could be 10–30 minutes of dead time. The game needs a secondary activity layer that:
- Fills wait time without feeling like a chore
- Rewards exploration and attention
- Fits the cozy, low-pressure tone

### Solutions (Priority Order for Launch)

#### 1. Collectibles Loop (Scrounger)
*Already in roadmap — highest priority*

Coins, seeds, and small items scattered around the map, respawning on a timer. Player explores between harvests to find them.
- Collectibles should feel discoverable, not grind-y — visible glint/sparkle from a distance
- Respawn every 5–10 real minutes at random map positions
- Drop types: coins (most common), bonus seeds, rare decoration items
- Ties into the economy loop: extra coins = more seeds = more planting

#### 2. Animal Interactions
*Sheep, dog, and cat already exist in the polyperfect scene*

Simple one-button interactions: pet, feed, or play with the farm animals.
- Each animal has a "happiness" state — interacting fills it, it drains slowly over time
- Happy animals could give a small passive bonus (e.g. fed dog barks to alert when crops are ready)
- Low implementation cost — the models and placement already exist
- Very high feel-good / cozy value for the player

#### 3. Decorating
*Paths, fences, and props are already in building system design*

Players can spend their wait time arranging and personalising the farm.
- Placing a path, fence section, or decorative prop takes ~5 seconds
- Decorating gives small XP (3 XP per item — already in progression table)
- Players naturally do a "decorating pass" while waiting for the next harvest
- Requires: unlock more decoration items in BuildingDatabase

### Design Notes
- These three loops should all be active at the same time — a player session looks like:
  *plant → water → explore for collectibles → pet animals → place a fence → harvest → repeat*
- None of these should feel required — they're options, not obligations
- Collectibles are the highest-leverage feature: cheap to implement, immediately adds a reason to walk around

---

## 12. Out of Scope for v1.0

- Multiplayer / co-op
- Seasonal crop system
- Character customisation
- NPCs / quests / story
- Mobile support
- Controller support
- Crafting system

*All great candidates for post-launch updates!*

---

## 13. Implementation Status
*Updated: 2026-03-23*

| Feature | Status |
|---------|--------|
| Core farming loop | Done |
| All 10 crops | Done |
| Functional buildings (Barn, Well, Greenhouse) | Partial |
| Decorative items | Partial |
| XP and 15 level progression | Done |
| Economy and coins | Done |
| Save/load | Done |
| Player character (polyperfect model) | Done |
| Camera (isometric, zoom, rotate) | Done |
| Shop UI | Done |
| Inventory UI | Done |
| Build mode UI | Done |
| HUD (coins, XP, level) | Done |
| TileInfoUI (hover panel, progress bar, action hints) | Done |
| Offline crop growth | Done |
| Audio (hooks in place, clips needed) | Done |
| Main Menu, Pause, Settings | Done |
| Real 3D crop visuals | Done |
| Real 3D building visuals | To Do |
| Watering Well auto-water function | To Do |
| Greenhouse grow speed bonus | To Do |
| Silo overflow storage | To Do |
| Market Stall auto-sell | To Do |
| Windmill animation | To Do |
| Harvest/water/till animations | To Do |
| Alpha playtesting | In Progress |

# CozyFarmGame — Development Roadmap
*Last updated: 2026-03-22*

---

## Current Status
Core gameplay loop is complete and working. Game is in Milestone 3 (Alpha).
Focus is on polish, visual feedback and functional buildings before playtesting.

---

## Priority 1 — Before First Playtest

### Gameplay Feel
- [ ] Particle effects for watering (water splash)
- [ ] Particle effects for harvesting (sparkles/leaves)
- [ ] Harvest ready glow/bloom effect on crops (GDD spec)
- [ ] Growth stage scaling — sprout → full bloom visually distinct
- [ ] Hover highlight transparency fix (currently too solid)
- [ ] Hover highlight colour by action (green=plant, blue=water, orange=harvest)

### Audio
- [ ] Replace placeholder SFX with Universal Sound FX pack clips
- [ ] Music volume balance (currently 0.01 — too quiet)
- [ ] Fix ShopUI RefreshShop() being called too frequently

### Technical
- [ ] Fix save/load for growth stages and tile state
- [ ] Offline growth — crops progress while player is away
- [ ] Growth Speed Multiplier back to 1x for release (currently testing at 60x)

---

## Priority 2 — Polish Sprint

### Visual Polish
- [ ] Crop highlight/subtle bloom when ready to harvest
- [ ] Decorative spacing between beds (paths, lanterns, small props)
- [ ] Replace placeholder building visuals with polyperfect models
- [ ] Find better models for Potato, Chilli, Lavender (currently using substitutes)

### UI Polish
- [ ] Shop and inventory icons (TMP Sprite Assets)
- [ ] HUD crop growth indicator (show % grown)
- [ ] Watered status indicator on HUD
- [ ] Level up visual feedback (warm chime + screen effect)
- [ ] Camera zoom and rotation feel improvements

### Buildings — Functional
- [ ] Watering Well → auto-waters adjacent 3×3 tiles once per cycle
- [ ] Greenhouse → crops inside grow 50% faster
- [ ] Silo → overflow crop storage
- [ ] Market Stall → auto-sells crops at 10% bonus

---

## Priority 3 — Content Expansion

### Grid & Layout
- [ ] Add Cluster B flower beds (8 more beds at Z=-5 area)
- [ ] Tile types: Bed vs Ground (ground = decorative flowers or slower crops)
- [ ] Decorative ground tiles between beds (paths, fences)

### Character
- [ ] Mixamo harvest animation
- [ ] Mixamo water animation
- [ ] Mixamo plant animation

### Audio
- [ ] More music tracks (lo-fi acoustic loops)
- [ ] Additional ambience layers

---

## Priority 4 — Post Launch / Stretch Goals

- [ ] Rare blooms / hybrid flowers (prestige crops)
- [ ] Decorative scoring / aesthetic points
- [ ] Bed upgrades (faster growth, more XP, higher coins)
- [ ] Seasonal crops / events
- [ ] Optional ground planting (freeform garden)
- [ ] Multiplayer / co-op (Farm Together vibe)
- [ ] Character cosmetics (hats, outfits)
- [ ] Achievements system

---

## Out of Scope for v1.0
- Multiplayer
- Seasons
- NPCs / quests / story
- Mobile support
- Controller support
- Crafting system

---

## Done
- [x] Full farming loop (plant/water/grow/harvest)
- [x] 10 crops with polyperfect 3D models
- [x] 4×4 grid aligned to flower beds
- [x] One crop per flower bed
- [x] Building and decoration placement system
- [x] Shop UI (B key) with unlock levels
- [x] Inventory UI (Tab key) with sell all
- [x] Build Mode UI (G key)
- [x] HUD (coins, XP, level, notifications)
- [x] XP and 15 level progression
- [x] Economy and coins (150 starting)
- [x] Save/load system (JSON)
- [x] Animated polyperfect character (walk/idle)
- [x] AudioManager with all hooks assigned
- [x] Music and ambience playing
- [x] Main Menu, Pause Menu, Settings UI
- [x] CozyFarm Toolkit editor window
- [x] Hover highlight (basic, needs transparency fix)
- [x] One-click planting (no till step)
- [x] Clean demo scene (383 props removed)
- [x] Flower bed clicking fixed (Ignore Raycast)
- [x] Editor-built panels (HUDBootstrapper removed)
- [x] Grid gizmo aligned to flower beds

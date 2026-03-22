# Jira Update Notes
Generated: 2026-03-22

---

## DEV-57 — Replace placeholder assets with real 3D art
**Status: In Progress**

### Done today:
- Imported Polyperfect Low Poly Ultimate Pack into project
- Integrated polyperfect DEMO_11_Farm scene as the main Farm scene
- Added animated low-poly character (SKM_Boy_Beach_Rig) with walk/idle animations
- Fixed URP materials on character model
- Created CropModelAssigner.cs Editor tool (Tools > CozyFarm > Assign Crop Models)
  - Auto-assigns polyperfect Food_M meshes to all 10 CropData ScriptableObjects
  - Crop mappings: Carrot=SM_Carrot, Tomato=SM_Tomato, Corn=SM_Corn,
    Pumpkin=SM_Melon, Potato=SM_Egg, Strawberry=SM_Apple,
    Grapes=SM_Honey, Chilli=SM_Eggplant, Sunflower=SM_Haystack, Lavender=SM_Hay_Pile

### Still to do:
- Run CropModelAssigner tool in Unity to assign models
- Test crop visuals in game
- Find better model matches for Potato, Strawberry, Grapes, Lavender
- Replace placeholder building visuals with polyperfect models

---

## DEV-26 — Source 3D farm environment assets
**Status: Done**

- Polyperfect Low Poly Ultimate Pack imported
- DEMO_11_Farm scene used as base farm environment
- Scene includes barn, silo, fences, animals, flower beds, trees, props

---

## DEV-52 — Balance progression curve
**Status: Done**

- Starting coins reduced from 500 to 150
- XP thresholds rebalanced for better pacing:
  - Level 1→2: 80 XP (quick win)
  - Level 2→5: steady pace
  - Level 6→15: meaningful grind up to 10000 XP
- Removed all debug logs from PlayerInteraction and EconomyManager

---

## DEV-48 — Player character animations
**Status: In Progress**

- Polyperfect character (SKM_Boy_Beach_Rig) added as player model
- Walk and idle animations working via controller swap
- PlayerController updated to swap between CTL_People_Walk and CTL_People_Idle
- No harvest/water/till animations yet — Mixamo animations could be added later

---

## Bug Fixes Today:
- Fixed FarmingManager using tile.WorldPosition (baked at grid init before Grid Origin set)
  - Now uses grid.GridToWorld(coord) everywhere for correct world positions
  - tileMarkers dictionary changed from Vector3 key to Vector2Int coord key
- Fixed grid alignment with flower beds in polyperfect farm scene
  - Grid Origin set to match flower bed positions
  - Tile size adjusted to 2 units
- Fixed UI panels (Shop, Inventory, Build Mode) not receiving button clicks
  - Panels now get their own Canvas with sort order 50
- Fixed HUDBootstrapper crashing silently — wrapped sections in try/catch
- Fixed TMP font rendering (box characters) — Kenney Future SDF font assigned
- Fixed large NotoEmoji font asset (128MB) blocked git push
  - Removed from git history using git filter-repo
  - Added to .gitignore to prevent recurrence

---

## Git commits today:
- Fix grid world position using GridToWorld instead of stored tile position
- Add CropModelAssigner Editor tool for polyperfect model assignment

---

## Next session priorities:
1. Run CropModelAssigner and test crop visuals (DEV-57)
2. Replace building placeholders with polyperfect models
3. Alpha playtesting with friends (DEV-58)
4. Add Mixamo animations for harvest/water/till actions (DEV-48)

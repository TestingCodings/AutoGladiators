# AutoGladiators Art Style Guide

Version 1.0 — Initial sprite-system foundation

---

## 1. Project Overview and Visual Goals

**AutoGladiators** is a mobile turn-based RPG for Android (and Windows development).  
Target audience: fans of portable pixel-art RPGs (Game Boy Advance / early DS era) who
enjoy robot-themed strategy games on mobile.

The visual goal is **crisp, readable, robot-gladiator pixel art** that works at small
sizes on phone screens, scales well for larger displays, and remains visually coherent
across all game contexts (exploration, dialogue, battle, UI).

---

## 2. Pixel-Art Rules

| Rule | Requirement |
|------|------------|
| Rendering | Always **nearest-neighbour** scaling — no bilinear or bicubic |
| Edges | **Hard pixel edges** — no anti-aliasing |
| Gradients | **Prohibited** — use dithering or banded shading instead |
| Line art | 1-pixel outlines using a **dark-but-coloured** outline, not pure black |
| Palette | Limited per-character palette; shared base palette across the project |
| Consistency | Every asset in the game must feel like it belongs to the same art set |

### Nearest-neighbour enforcement

The current `SpriteSheet` renderer already applies `SKFilterQuality.None` and
`IsAntialias = false`. All new assets must be created or exported at their native
pixel size; the engine handles scaling.

---

## 3. Palette Guidelines

### Shared base palette categories

```
outline_dark      — primary outline (dark steel-blue, not black)
steel_dark        — shadow/underside of metallic surfaces
steel_mid         — primary metallic body colour
steel_light       — highlight/top bevel of metallic surfaces
warm_accent       — warm energy highlight (orange/amber)
cool_accent       — cool energy highlight (cyan/teal)
fire_energy       — elemental fire core
electric_energy   — elemental electric core
ice_energy        — elemental ice core
terrain_green     — grass/outdoor ground
terrain_stone     — pavement/indoor floor
water_deep        — deep water tile
water_shallow     — shallow water/puddle
ui_neutral        — HUD backgrounds
damage_red        — HP loss indicator
heal_green        — HP recovery indicator
status_yellow     — status-condition indicator
```

### Per-character palette rules

Each character sprite should use:
- 1 outline colour
- 2–3 body colours (dark, mid, light)
- 1 main accent colour (faction or role identifier)
- 1 energy/core colour (tied to elemental type where applicable)
- Optionally 1–2 detail colours for accessories, faction markings, etc.

Avoid unique per-character palettes that have nothing in common with the base set.
Characters in the same faction should share accent colours.

---

## 4. Outline and Contrast Guidelines

- Use a **dark steel-blue** outline (e.g., `#0E121A`) rather than pure black.
- Ensure sufficient contrast between the character silhouette and the terrain tiles.
- On dark terrain, add a subtle lighter outline or ground shadow to separate feet.
- Characters must be identifiable as silhouettes at their native sprite size.
- Visor/core elements should use a bright saturated colour for instant readability.

---

## 5. Silhouette Guidelines

- Every character class must have a **unique silhouette**.
- Silhouettes should read clearly at 16×16 **before** colour is considered.
- Avoid symmetrical blobs — use asymmetric antennas, weapons, hats, or bulk differences.
- The head/core area should be the strongest visual focal point.
- Feet must be visually separated (avoid solid-block legs that merge with the ground).

---

## 6. Lighting and Shading Guidelines

- Light source: upper-left at approximately 45 degrees.
- Top/front-facing surfaces: steel-light highlight colour.
- Underside/back surfaces: steel-dark shadow colour.
- No complex subsurface scattering or specular maps.
- For energy cores and visors, apply a simple 2-colour glow (bright centre, dim outer ring).

---

## 7. Readability Guidelines (Mobile)

- Minimum tap target is not a concern for sprites (non-interactive), but sprites must
  remain recognisable at the **48×48 px** tile size used in the current renderer.
- Walk cycles must animate smoothly; avoid identical adjacent frames.
- Important UI icons should be designed at 32×32 and scaled, not forced into 16×16.
- Portrait art for dialogue (64×64 or larger) may use fine detail not possible at 16×16.

---

## 8. Character Design Rules

### Mandatory requirements

- Every character must have an **original design**; no real-person likenesses.
- **Prohibited**: real-person names, recognisable real faces, real-world logos, brand
  trademarks, sports jerseys, or any other personally identifying references.
- **Prohibited**: reproducing any copyrighted character design from another game,
  film, comic, or media property.
- Characters are robots, mechs, and android-adjacent beings — lean into mechanical
  aesthetic rather than human anatomy.
- Characters should have a clear **role** (trainer, scientist, shopkeeper, enemy, boss)
  communicated by silhouette, colour, and accessories.

### Naming in source code and assets

- Asset files must use **neutral IDs only**: `npc_01.png`, `trainer_02.png`, etc.
- Do **not** use real-person first names, surnames, or nicknames in filenames, asset IDs,
  JSON keys, or code symbols.
- Display names shown to the player in-game may be fictional names, but these must be
  stored as data (dialogue files, NPC config), not baked into asset filenames.

---

## 9. Sprite-Sheet Contract

The current renderer assumes the following layout for every character exploration sheet:

| Property | Value |
|----------|-------|
| Frame width | 16 px |
| Frame height | 16 px |
| Columns | 3 |
| Rows | 4 |
| Direction order | Down (row 0), Up (row 1), Left (row 2), Right (row 3) |
| Frames per direction | 3: left-step (col 0), idle/centre (col 1), right-step (col 2) |
| Background | Fully transparent (alpha = 0) |
| Total sheet size | 48 × 64 px |
| Scaling | Nearest-neighbour |

**Do not change this contract without updating `SpriteSheet.cs` and `GameActor.cs`.**

Walk-cycle playback order is `[centre, left, centre, right]` at 8 FPS, driven by
`GameActor.FrameIndex()`.

---

## 10. Asset Types and Size Guidelines

### Exploration sprites (current contract)
48 × 64 px sheets, 3 × 4 frame layout as above.

### Dialogue portraits
- **Minimum**: 64 × 64 px
- **Recommended**: 96 × 96 or 128 × 128 px
- May use more detail than exploration sprites.
- Should clearly match (or complement) the exploration sprite design.
- Separate file from the exploration sheet.

### Battle sprites
- **Minimum**: 64 × 64 px
- **Recommended**: 128 × 128 px
- May include attack, idle, hit, and defeat poses.
- Separate file from the exploration sheet.

### UI icons
- Design at 32 × 32 px native.
- Use the shared base palette.
- Hard edges, no gradients.

### Effects (particles, attacks, status)
- Vary by use; document frame size and count in the manifest.
- Use the base palette's energy colours for consistency.

---

## 11. AI-Assisted Generation Workflow

AI image tools are permitted as **draft generation aids only**.  
AI output must never be shipped directly without human review and manual cleanup.

### Approved pipeline

1. **Reference preparation** — Gather any old sprite as a design reference only.
   Strip personal names and identifying associations before using as a prompt reference.
2. **Character definition** — Write a character-role description, palette notes, and a
   list of "do not include" constraints (no real faces, no logos, transparent background).
3. **Large-canvas generation** — Generate a 256 × 256 or 512 × 512 px image using the
   approved prompt template (see below). Do not attempt to generate a 16 × 16 sheet directly.
4. **Background removal** — Remove the background; verify full transparency.
5. **Nearest-neighbour reduction** — Scale to 16 × 16 using strict nearest-neighbour
   interpolation (Photoshop: Image Size → Resample: Nearest Neighbour; GIMP: Scale → None;
   Aseprite: sprite resize with Nearest-Neighbor).
6. **Manual pixel correction** — Review at 800–1600 % zoom; fix silhouette, hands, feet,
   antennae, and any anti-aliased edge pixels.
7. **Direction set creation** — Create all four directional views, maintaining consistent
   palette and silhouette.
8. **Walk-cycle creation** — Create three frames per direction; test animation in Aseprite
   or a preview tool.
9. **Sheet assembly** — Arrange into the 3 × 4 sprite-sheet contract layout (48 × 64 px).
10. **Validation** — Run `AutoGladiators.AssetValidator` against the new file.
    Correct all reported errors before proceeding.
11. **In-game review** — Load the game and walk around; verify the sprite looks correct
    on device at the 48 px tile size.
12. **Provenance documentation** — Record the tool, generation date, prompt (or reference),
    review status, and any modifications in `AssetManifest.json`.

### Approved prompt template

```
Create an original pixel-art robot character for a mobile turn-based RPG.

Character role: [ROLE — e.g., rival trainer, scientist, shopkeeper]
Visual notes: [BRIEF DESCRIPTION — e.g., heavy-duty chassis, blue faction markings, wrench accessory]

Requirements:
- crisp limited-palette pixel art
- transparent background
- no gradients, no anti-aliasing, hard pixel edges
- strong readable silhouette
- front-facing neutral pose
- suitable for reduction to a 16×16 game sprite
- no text, logos, watermarks, names, or recognisable real-world references
- no real-person likeness
- original character design
- consistent proportions
```

### Consistency rules

- Maintain a **character reference sheet** before generating animation frames.
  Generating each frame independently produces character drift between frames.
- Character reference sheet must include: front view, back view, left view, palette
  swatches, and a list of prohibited changes.
- If a generated image deviates from the reference sheet, regenerate rather than
  accepting inconsistent frames.

### What AI produces reliably
- Broad design exploration and silhouette ideas
- Colour palette suggestions
- Large reference images that can be manually reduced

### What AI produces poorly (always manually correct)
- Hands, feet, small accessories
- Transparent backgrounds (check edge pixels carefully)
- Directional consistency between frames
- Sub-pixel walk cycles
- Palette compliance

---

## 12. Vertical-Slice Process

**Before migrating the entire asset library, complete a vertical slice of approximately
eight representative character roles.**

Recommended roles for the vertical slice:

| Slot | Role | Neutral ID | Notes |
|------|------|------------|-------|
| 1 | Player robot | `player_01` | Procedural (drawn in `CharacterArt.cs`) |
| 2 | Friendly civilian | `npc_01` | Simple design, few accessories |
| 3 | Quest-giver / scientist | `npc_quest_01` | Distinct lab-coat silhouette |
| 4 | Rival trainer | `trainer_rival_01` | Antagonistic colour palette |
| 5 | Repair-bay staff | `npc_repair_01` | Tool-holding accessory |
| 6 | Shopkeeper | `npc_shop_01` | Friendly, merchant visual cues |
| 7 | Robot enemy | `enemy_grunt_01` | Aggressive silhouette |
| 8 | Boss character | `boss_01` | Oversized, distinctive design |

For each slot, produce:
- Exploration sprite sheet (48 × 64 px, 3 × 4 layout)
- Dialogue portrait (96 × 96 px recommended)
- Battle sprite placeholder (64 × 64 px minimum)

### Vertical-slice review gate

Before expanding to the full asset library:
- [ ] All eight exploration sprites display correctly on-device at the 48 px tile size.
- [ ] Walk cycles animate smoothly.
- [ ] Silhouettes are distinct from one another.
- [ ] Portraits match the exploration designs.
- [ ] Automated validator passes for all eight assets.
- [ ] Palette is consistent across all eight assets.

---

## 13. Asset Manifest Schema

The manifest at `AutoGladiators.Client/Config/AssetManifest.json` is the authoritative
registry of all sprite assets. It maps neutral asset IDs to file paths and records
provenance.

### Schema fields

| Field | Type | Required | Description |
|-------|------|----------|-------------|
| `id` | string | **Yes** | Unique neutral identifier (e.g., `npc_01`) |
| `category` | string | **Yes** | One of: `npc`, `trainer`, `player`, `enemy`, `boss`, `item`, `ui`, `effect` |
| `explorationSheet` | string | **Yes** | Relative path from `Resources/Raw/` to the exploration sprite sheet PNG |
| `frameWidth` | int | **Yes** | Frame width in pixels (16 for current contract) |
| `frameHeight` | int | **Yes** | Frame height in pixels (16 for current contract) |
| `columns` | int | **Yes** | Number of columns in the sheet (3 for current contract) |
| `rows` | int | **Yes** | Number of rows in the sheet (4 for current contract) |
| `portrait` | string | No | Relative path to dialogue portrait PNG |
| `battleSprite` | string | No | Relative path to battle sprite PNG |
| `paletteId` | string | No | Identifier for the character's named palette entry |
| `sourceCategory` | string | **Yes** | One of: `original`, `ai-assisted-original`, `ai-assisted-edited`, `licensed-pack`, `placeholder`, `pending-anonymization` |
| `reviewStatus` | string | **Yes** | One of: `approved`, `pending`, `pending-anonymization`, `draft` |
| `licensingNotes` | string | No | Ownership, licensing, or provenance notes |

### Migration approach

1. Add an entry for each existing sprite using its neutral ID.
2. Set `explorationSheet` to the current filename (may temporarily contain a personal
   name while anonymization is in progress — mark `sourceCategory` as `pending-anonymization`).
3. Once the anonymization PR lands and files are renamed, update the path in the manifest.
4. Run the validator to confirm no forbidden names remain.
5. Update `reviewStatus` to `approved` once the art direction review is complete.

---

## 14. Asset Ownership, Licensing, and Provenance Checklist

Before merging any new sprite into the main branch:

- [ ] The asset is original work, AI-assisted original work, or sourced from a
      compatible licence (e.g., CC0, purchased licence).
- [ ] No real-person likeness, name, or identifying reference is present.
- [ ] No third-party copyright is infringed.
- [ ] The `sourceCategory` and `licensingNotes` fields in the manifest are filled in.
- [ ] Human review of the asset has been completed and `reviewStatus` is `approved`.
- [ ] The automated validator passes for this asset.
- [ ] If AI-assisted: the generation prompt and tool are documented in `licensingNotes`.

---

## 15. Release Gates (Remaining Work)

The following work is **not complete** and must be finished before a production release:

| Area | Remaining work |
|------|---------------|
| Art production | Generate, clean, review, and validate all character exploration sheets, portraits, and battle sprites per the vertical-slice process |
| Anonymization | Complete renaming of all personal-name sprite files; update manifest paths |
| Device QA | Test on representative Android hardware (various screen densities and OS versions) |
| Licensing review | Audit every asset in the manifest for ownership and licensing compliance |
| Performance profiling | Profile sprite loading and rendering on low-end Android devices |
| Release signing | Configure Android keystore and signing for release builds |
| Store compliance | Review Google Play policies; confirm no policy violations in content or metadata |
| Save data | Verify save compatibility across app updates |
| Portrait/battle integration | Renderer support for dialogue portraits and battle sprites is not yet implemented |

---

*Last updated: 2026-08-01 — AutoGladiators sprite-system foundation PR.*

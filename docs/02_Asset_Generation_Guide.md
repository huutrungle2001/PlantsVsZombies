# Asset Generation Guide

This document lists every static art asset that needs to be (re)generated using
an image-generation LLM (DALL-E 3, Midjourney, Stable Diffusion, etc.), along
with a ready-to-paste prompt for each one.

---

## Style Anchor

Paste this at the beginning of **every** prompt to keep all assets visually
consistent:

> Flat 2D cartoon vector art in the style of a cheerful mobile tower-defense
> game. Bright, saturated primary colors. Thick (2–4 px at source resolution)
> solid black outlines on all shapes. No gradients on fills — use flat color
> with a single highlight shape for depth. Friendly, slightly exaggerated
> proportions. **Transparent PNG background** unless stated otherwise.

---

## Group 1 — Plant Cards

Displayed at **80 × 110 reference pixels** in the UI (CanvasScaler 1920 × 1080).
Generate at **256 × 360 px** so they stay sharp when the engine scales them down.

Each card looks like a **seed packet**: portrait orientation, rounded border,
a colored header strip at the top with the plant name, a central illustration,
and a plain strip at the bottom where the engine prints the sun cost.

### Shared Card Template

Replace `[PLANT]`, `[DESCRIPTION]`, `[HEADER_COLOR]`, and `[ACCENT_COLOR]`:

> Seed packet card for a tower-defense plant game. Portrait orientation,
> 256 × 360 pixels. Transparent background. The card has a rounded rectangular
> border with a thin black outline. Upper third: a solid `[HEADER_COLOR]` banner
> with the text "`[PLANT]`" in bold cartoon font. Center: a charming cartoon
> illustration of `[DESCRIPTION]`, centered on a light-green panel. Lower fifth:
> a plain light-colored strip (no text — cost is added by the game engine).
> Flat 2D cartoon, bright saturated colors, thick black outlines, friendly and
> cute. No shadows outside the card border. PNG with transparent background.

### Per-Card Values

| # | Plant | `[PLANT]` | `[DESCRIPTION]` | `[HEADER_COLOR]` |
|---|---|---|---|---|
| 1 | Peashooter | Peashooter | a cheerful green pea-shooting plant with a single round mouth like a pea cannon | `#4CAF50` green |
| 2 | Sunflower | Sunflower | a happy bright yellow sunflower with a smiling face and large petals | `#FFC107` amber |
| 3 | Wall-nut | Wall-nut | a tough smiling brown walnut with a determined expression and cracked shell texture | `#8D6E63` brown |
| 4 | Repeater | Repeater | a green plant with two pea-shooter heads side by side, both aimed right, smiling | `#388E3C` dark green |
| 5 | Snow Pea | Snow Pea | a pale blue-green pea-shooter plant with frosty ice crystals around its mouth | `#4FC3F7` sky blue |
| 6 | Cherry Bomb | Cherry Bomb | two bright red cherries with a lit fuse and an excited expression | `#E53935` red |
| 7 | Jalapeno | Jalapeno | a fiery red-orange jalapeño pepper character with angry flames around it | `#FF5722` deep orange |
| 8 | Chomper | Chomper | a purple Venus flytrap with a huge open toothy mouth and eager expression | `#7B1FA2` purple |
| 9 | Potato Mine | Potato Mine | a cute brown potato half-buried in dirt with a small detonator on top, nervous face | `#795548` earthy brown |

### Target Paths

```
Assets/Art/Cards/PeaShooter.png
Assets/Art/Cards/SunFlower.png
Assets/Art/Cards/WallNut.png
Assets/Art/Cards/Repeater.png
Assets/Art/Cards/SnowPea.png
Assets/Art/Cards/CherryBomb.png
Assets/Art/Cards/Jalapeno.png
Assets/Art/Cards/Chomper.png
Assets/Art/Cards/PotatoMine.png
```

---

## Group 2 — Card Tray / Deck Panel

The vertical panel on the left side of the screen that holds all plant cards.

**Path:** `Assets/Art/Cards/card_deck.png`  
**Size:** 200 × 600 px

> A vertical panel for a 2D tower-defense game UI. 200 × 600 pixels. Designed
> as a 9-slice-friendly panel: solid repeating center with decorative top and
> bottom caps. Style: worn wooden plank or mossy stone tablet, dark green tones,
> subtle grass and dirt texture along the edges, thick black cartoon outline on
> all sides. Flat 2D cartoon style, no photorealism. No text. Transparent
> background around the panel shape. PNG.

---

## Group 3 — HUD Icons

Small icons used in the heads-up display.

### Sun Coin (HUD display)

**Path:** `Assets/Art/items/Sun.png`  
**Size:** 128 × 128 px

> A glowing cartoon sun coin, circular, bright yellow-orange with a cheerful
> smiling face, surrounded by short triangular rays. Flat 2D cartoon style,
> thick black outline, vibrant gradient from yellow center to warm orange edge.
> Transparent background. PNG. No text.

### Pea Projectile

**Path:** `Assets/Art/items/Pea.png`  
**Size:** 64 × 64 px

> A single cartoon pea projectile seen from a slight 3/4 angle. Small round
> green sphere with a subtle shine highlight on the top-left. Flat 2D cartoon
> style, bright grass-green color, thick black outline. Transparent background.
> PNG.

### Shovel Tool Icon

**Path:** `Assets/Art/items/Shovel.png`  
**Size:** 128 × 128 px

> A cartoon garden shovel icon. Brown wooden handle with a shiny silver spade
> head, cheerful and rounded. Flat 2D cartoon style, thick black outline.
> Transparent background. PNG.

---

## Group 4 — UI Panels & Buttons

Reusable 9-slice panel textures and button states. Keep the **center area
plain** so Unity can stretch it without distorting the corners.

### Sun Counter Panel

**Path:** `Assets/Art/items/sun_panel.png`  
**Size:** 200 × 60 px

> A small horizontal pill-shaped UI panel for displaying a sun count in a
> tower-defense game. Dark earthy brown with a subtle wood-grain texture and a
> thin golden border. Flat 2D cartoon style, thick black outline, 9-slice
> friendly (plain center, decorative rounded ends). No text. Transparent
> background. PNG.

### Primary Button (normal state)

**Path:** `Assets/Art/items/Button1.png`  
**Size:** 256 × 80 px

> A wide rounded-rectangle button for a 2D cartoon game UI. Bright green with
> a slightly lighter green highlight stripe across the top half. Thick black
> cartoon outline. 9-slice friendly: solid stretchable center, decorative
> rounded corners only at the sides. Flat 2D art. No text. Transparent
> background. PNG.

### Danger Button (red — used for Restart)

**Path:** `Assets/Art/items/Button2.png`  
**Size:** 256 × 80 px

> Same shape as a rounded-rectangle game button but in deep red with a lighter
> red highlight stripe across the top. Thick black outline. 9-slice friendly.
> Flat 2D cartoon. No text. Transparent background. PNG.

---

## Group 5 — Win / Lose Screen Decorations

Layered on top of the result panels built by `UIFactory`.

### Win Banner

**Path:** `Assets/Art/items/win_banner.png`  
**Size:** 800 × 200 px

> A celebratory horizontal banner for a victory screen in a 2D cartoon
> tower-defense game. Bright golden-yellow with colorful pennant flags hanging
> across it, confetti shapes, and stars. Text-free. Flat 2D cartoon style,
> thick black outlines. Transparent background. PNG.

### Lose Banner

**Path:** `Assets/Art/items/lose_banner.png`  
**Size:** 800 × 200 px

> A dramatic horizontal banner for a game-over screen in a 2D cartoon
> tower-defense game. Dark purple-black with cracked earth texture and a few
> ominous green zombie hands reaching up from the bottom edge. Text-free.
> Flat 2D cartoon style, thick black outlines. Transparent background. PNG.

---

## Group 6 — Lawn Mower Idle Sprite

The animated mower (`lawnMower_Active`) already exists. This is the **static
idle** sprite shown before the mower is triggered.

**Path:** `Assets/Art/items/LawnMower.png`  
**Size:** 128 × 96 px

> A small cartoon red push lawn mower seen from a side angle, facing right.
> Red body, silver blades, black rubber wheels, a silver handle at the back.
> Flat 2D cartoon style, thick black outlines, bright cheerful colors.
> Transparent background. PNG.

---

## Recommended Drop-In Workflow

1. Generate each asset at the size listed above.
2. Drop the PNG into the correct `Assets/Art/` path shown — overwrite the
   existing placeholder.
3. **Card art only** — open Unity and run `PvZ > UI > Build HUD` so
   `UIFactory` picks up the new file (it re-reads by path, no GUID change).
4. **Sun.png / Pea.png** — run `make prefabs` to rebuild `SunPickup.prefab`
   and `PeaProjectile.prefab` with the new sprite.
5. **Button1/Button2** — if switching to 9-slice rendering, update the
   `Image.type` and border settings in `UIFactory.BuildCard`.
6. No code changes are required for any asset in this list as long as the
   file is saved at the exact path shown.

---

## Asset Count Summary

| Group | Count | Status |
|---|---|---|
| Plant cards | 9 | Placeholders exist — regenerate |
| Card tray panel | 1 | Placeholder exists — regenerate |
| HUD icons | 3 | Placeholders exist — regenerate |
| UI panels & buttons | 3 | Placeholders exist — regenerate |
| Win / Lose banners | 2 | New — does not exist yet |
| Lawn mower idle | 1 | New — does not exist yet |
| **Total** | **19** | |

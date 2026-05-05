# PvZ Core Unity Concepts

## Core Unity Concepts

### Scene

A level or screen. For this game, one scene might be the main lawn battle.

### GameObject

An object in the scene. Examples: plant, zombie, pea projectile, sun, tile, lawn mower.

### Component

Behavior or data attached to a GameObject. Unity uses composition heavily.

A zombie GameObject might have:

- `SpriteRenderer` to display the zombie image
- `Animator` to play walk, attack, and death animations
- `Collider2D` for hit detection
- `Zombie.cs` script for HP, movement, damage, and death logic

### Prefab

A reusable template. Similar to a component instance or template in web dev.

You make one `ZombiePrefab`, then spawn many zombies from it.

### Script

Usually a C# class attached to a GameObject. This is where movement, attack, HP, cooldowns, targeting, and similar behavior live.

## For A Plants Vs Zombies Style Game

The game can be broken into systems.

### Board / Grid

The lawn is a grid: rows and columns. Plants are placed on tiles.

### Plant

Has HP, cost, cooldown, attack behavior, and maybe sun generation.

Example plant logic:

- Check if a zombie exists in the same row
- Wait for attack cooldown
- Spawn projectile
- Take damage if a zombie attacks
- Die when HP reaches 0

### Zombie

Has HP, speed, damage, and attack interval.

Example zombie logic:

- Walk left
- If touching or blocking a plant, stop
- Attack plant every X seconds
- Die when HP reaches 0

### Projectile

Moves right, checks for zombie hit, applies damage, and disappears.

### GameManager

Controls global state:

- Selected plant
- Sun count
- Wave timing
- Win and lose conditions
- Pause and restart

### Spawner

Creates zombies over time, usually by row and wave data.

## Animations From GIFs

The basic idea is correct:

```text
GIF -> PNG frames -> Unity Sprite animation -> .anim file
```

In Unity, the common flow is:

1. Convert GIF into transparent PNG frames.
2. Import PNGs into Unity.
3. Set them as `Sprite`.
4. Select frames in order.
5. Drag them into the scene or Animation window.
6. Unity creates an `AnimationClip` / `.anim`.
7. Attach it to an `Animator`.

For each character, you usually want separate clips:

- `Idle`
- `Walk`
- `Attack`
- `Death`
- Maybe `Hit`

For a PvZ-style game, animation is mostly visual. The actual logic should be in scripts.

For example, the zombie's walk animation plays while the zombie is moving, but the script decides the real position, HP, attack timing, and death.

## Important Difference From Web Layout

In Unity, visual placement is not CSS layout. You work in **world coordinates**.

A tile might be at:

```text
row 2, column 4
```

but in Unity world space that becomes something like:

```text
x = 1.5
y = -0.8
```

So we usually make helper code that converts grid positions into world positions.

## Collision: Physics Or Manual Logic

Unity has physics and collision tools, but for this type of game, full physics should not control every rule.

For Plants vs Zombies, simpler logic is often better:

- Zombies belong to a row
- Plants belong to a row and column
- Projectiles check zombies in the same row
- Zombies stop when they reach the plant in front of them

Use `Collider2D` where useful, but keep the main rules grid-based.

## Beginner Build Order

Start tiny:

1. One row only.
2. One plant already placed.
3. One zombie walking toward it.
4. Plant shoots peas.
5. Peas damage zombie.
6. Zombie attacks plant.
7. Add death animation.

After that works, expand:

1. Multiple rows.
2. Grid placement.
3. Sun resource.
4. Plant selection UI.
5. Zombie waves.
6. More plant and zombie types.
7. Menus and polish.

That keeps the learning manageable. The first goal should not be "build Plants vs Zombies." It should be: **make one plant fight one zombie correctly**.

Also, if the assets are actual Plants vs Zombies assets, keep it as a private learning project. For anything public, use original or licensed assets.

The next useful discussion would be the architecture: how to design `Plant`, `Zombie`, `Projectile`, `Tile`, and `GameManager` so the project does not become messy as more types are added.

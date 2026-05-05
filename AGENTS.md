# Agent Instructions

This file is the persistent onboarding guide for Codex-style agents working on this repository.

## Project Summary

This is a Unity project for building a private learning version of a Plants vs Zombies-style game.

The project should be designed as an **environment plus agents**:

- Environment: board, tiles, lanes, global game state, resources, waves, win/lose rules.
- Agents: plants, zombies, projectiles, sun pickups, lawn mowers.

Keep the game rules in environment systems. Keep object-specific behavior in agents.

## Current Stack

- Unity version: `6000.4.5f1`
- Main scene: `Assets/Scenes/Main.unity`
- Runtime scripts: `Assets/Scripts`
- Editor scripts: `Assets/Editor`
- Prefabs: `Assets/Prefabs`
- Art and audio: `Assets/Art`, `Assets/Audio`
- Generated animation controllers:
  - `Assets/Art/Animations`
  - `Assets/Resources/Animations`

## Important Docs

Read these before making architecture changes:

- `docs/00_PvZ_Core_Unity_Concepts.md`
- `docs/01_PvZ_Environment_Agents_System_Design.md`
- `TODO.md`

`TODO.md` is the master roadmap. When a task is completed, update the relevant checklist item.

## Architecture Rules

- Prefer Unity scene objects and prefabs over creating the whole world from runtime bootstrap code.
- Keep `Bootstrap` only for minimal global setup if it remains necessary.
- Use the `Main` scene as the primary gameplay scene.
- The board should be a 5-row by 9-column grid for the first playable version.
- Tile occupancy is authoritative: one plant per tile.
- Lane tracking is authoritative: zombies register and unregister through the lane registry.
- Plants should not directly manage zombies.
- Zombies should not directly manage the board.
- Projectiles should only damage valid zombies in their lane.
- UI should display and request state changes, not own gameplay rules.
- Expose tunable gameplay values in Inspector fields when practical.

## Target First Playable Core

Build toward this milestone before expanding:

- Clickable 5 x 9 board.
- Peashooter placement.
- Sunflower placement.
- Sun economy.
- Basic Zombie waves.
- Pea projectile combat.
- Plant/zombie blocking and zombie attacks.
- Win and lose conditions.
- Restart.

Do not prioritize menus, many plant types, many zombie types, save data, or level selection until this core loop works.

## Asset And Animation Pipeline

Source assets include GIFs and PNGs.

The GIF workflow is:

```text
GIF -> sprite sheet PNG + metadata JSON -> Unity sliced sprites -> .anim -> .controller
```

Existing editor tooling:

- `GifSpriteSheetProcessor`
- `AnimationResourceExporter`
- `ProjectSetup`
- `SceneArtSetup`
- `BuildScript`

Reuse existing generated controllers before creating new animation systems.

## Build And Verification

Use these commands from the repository root:

```sh
make build
```

```sh
make run
```

`make run` opens the built macOS app and may require permission in restricted environments.

For code changes, at minimum:

- Check compile/build status when feasible.
- Verify Unity scene references are not broken.
- Verify generated files under `Library/`, `Logs/`, `Builds/`, and `UserSettings/` are not committed.

## Git Rules

- Do not commit unless the user asks.
- When asked to commit, use small logical commits.
- Use Conventional Commits for every commit message: `type(scope): imperative summary`.
- Use lowercase types such as `feat`, `fix`, `docs`, `refactor`, `chore`, `test`, `build`, or `style`.
- Use lowercase scopes such as `repo`, `project`, `assets`, `editor`, `gameplay`, `docs`, `ui`, `audio`, `build`, or `tests`.
- Examples:
  - `docs(architecture): add environment agent system design`
  - `feat(gameplay): add lane registry`
  - `build(editor): add gif sprite sheet processor`
  - `fix(projectile): prevent cross-lane hits`
- Do not commit Unity-generated cache folders.
- Do not revert user changes unless explicitly asked.
- Preserve `.meta` files for Unity assets.

## Coding Style

- Keep C# scripts small and responsibility-focused.
- Prefer clear component names: `BoardGrid`, `Tile`, `LaneRegistry`, `GameManager`, `WaveManager`, `Plant`, `ZombieAgent`, `ProjectileAgent`.
- Use simple Unity `MonoBehaviour` components for now.
- Avoid adding complex frameworks or abstractions before the first playable core works.
- Add comments only when they clarify non-obvious game logic.

## Licensing Note

The current assets appear to be Plants vs Zombies-style assets. Treat this as a private learning project unless assets are replaced with original or properly licensed art.

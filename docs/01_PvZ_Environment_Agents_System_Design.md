# PvZ Environment And Agents System Design

This game should be designed as an **environment** that owns shared rules and state, plus **agents** that act inside that environment.

The environment answers questions like:

- Where can this thing exist?
- What tile is under the mouse?
- Is this tile occupied?
- What zombies are active in this lane?
- Does the player have enough sun?
- Has the player won or lost?

Agents answer questions like:

- What should I do this frame?
- Should I move?
- Should I attack?
- Should I shoot?
- Should I die?

This keeps the game easier to grow. The board and game rules stay in environment systems. Plants, zombies, projectiles, and sun pickups stay focused on their own behavior.

## High-Level Architecture

```text
Environment
- BoardGrid
- Tile
- LaneRegistry
- GameManager
- WaveManager

Agents
- Plant
- PeashooterAgent
- SunflowerAgent
- ZombieAgent
- ProjectileAgent
- SunPickupAgent
```

The environment owns the stable rules. Agents own local behavior inside those rules.

## Environment Systems

### BoardGrid

`BoardGrid` defines the playable lawn.

Responsibilities:

- Store row count and column count.
- Store tile size and board origin.
- Convert `row + column` into Unity world position.
- Convert mouse world position into a tile coordinate.
- Provide helper methods used by placement, spawning, and lane logic.

For the first playable version:

- Use 5 rows.
- Use 9 columns.
- Use fixed tile spacing.
- Keep this visible in the Unity scene while learning.

### Tile

`Tile` represents one board cell.

Responsibilities:

- Store `row` and `column`.
- Store the current plant on the tile.
- Know whether it is empty or occupied.
- Receive click input.
- Forward placement requests to `GameManager`.

Important rule:

- `Tile` is the authority for plant occupancy.
- One tile can have at most one plant.

### LaneRegistry

`LaneRegistry` tracks active agents by lane.

Responsibilities:

- Register zombies when they spawn.
- Unregister zombies when they die or leave the board.
- Let plants ask whether a zombie exists in their row.
- Let projectiles identify valid zombie targets.
- Avoid making every agent search the whole scene.

Example questions it should answer:

```text
Is there any zombie in row 2?
What is the first zombie in row 2 after x = -1.5?
How many zombies are still alive?
```

### GameManager

`GameManager` owns global match state.

Responsibilities:

- Track current sun amount.
- Track selected plant card.
- Validate plant placement.
- Instantiate plant prefabs.
- Spend sun on successful placement.
- Track pause, win, and lose states.
- Receive reports from other systems when the game ends.

Placement should flow through `GameManager`, not directly through plants.

### WaveManager

`WaveManager` owns zombie spawning over time.

Responsibilities:

- Store wave schedule.
- Spawn zombies into specific lanes.
- Track whether all scheduled zombies have spawned.
- Report wave completion to `GameManager`.

For the first playable version, wave data can be simple and hardcoded in the Inspector or script. More advanced level files can come later.

## Agent Systems

### Plant

`Plant` is the base plant component.

Responsibilities:

- Store HP.
- Store sun cost.
- Store tile position.
- Receive damage.
- Die when HP reaches 0.
- Clear its tile when it dies.

`Plant` should not decide Peashooter or Sunflower behavior by itself. Specific behavior belongs in separate agent components.

### PeashooterAgent

`PeashooterAgent` is a plant behavior component.

Responsibilities:

- Check `LaneRegistry` for zombies in the same row.
- Wait for shoot cooldown.
- Spawn a pea projectile.
- Use `ProjectileAgent` to handle projectile behavior.

Important rule:

- Peashooter should shoot only when there is a zombie in its lane.

### SunflowerAgent

`SunflowerAgent` is a plant behavior component.

Responsibilities:

- Wait for sun generation interval.
- Spawn `SunPickupAgent`.
- Let `GameManager` handle adding sun when the pickup is collected.

Sunflower does not need to know about zombies or combat.

### ZombieAgent

`ZombieAgent` owns zombie behavior.

Responsibilities:

- Store HP.
- Store move speed.
- Store attack damage.
- Store attack interval.
- Register with `LaneRegistry` when spawned.
- Walk left through its lane.
- Detect a blocking plant.
- Stop and attack while blocked.
- Resume walking when the plant dies.
- Die and unregister from `LaneRegistry` at 0 HP.
- Trigger lose condition if it reaches the house side.

Important rule:

- Zombie should not manage the board.
- Zombie asks the environment whether it is blocked.

### ProjectileAgent

`ProjectileAgent` owns projectile behavior.

Responsibilities:

- Store speed.
- Store damage.
- Store lane.
- Move right.
- Damage the first valid zombie it hits.
- Destroy itself after hit or max lifetime.

For the first version, projectile hit detection can use `Collider2D` triggers, with lane filtering to avoid cross-lane hits.

### SunPickupAgent

`SunPickupAgent` owns collectible sun behavior.

Responsibilities:

- Display sun sprite.
- Respond to click input.
- Add sun through `GameManager`.
- Destroy itself after collection or timeout.

Sun pickup should not directly modify UI. It should update game state through `GameManager`.

## Data Flow

### Plant Placement

```text
Player selects plant card
Player clicks tile
Tile sends request to GameManager
GameManager validates sun, cooldown, and occupancy
GameManager instantiates plant prefab
Plant registers itself on Tile
GameManager subtracts sun
UI updates sun amount
```

### Zombie Spawning

```text
WaveManager reaches spawn time
WaveManager chooses zombie prefab and lane
WaveManager instantiates zombie
ZombieAgent registers with LaneRegistry
ZombieAgent starts walking
```

### Peashooter Combat

```text
PeashooterAgent checks LaneRegistry
If zombie exists in same row, cooldown starts/continues
When cooldown is ready, PeashooterAgent spawns ProjectileAgent
ProjectileAgent moves right
ProjectileAgent hits ZombieAgent
ZombieAgent takes damage
ProjectileAgent destroys itself
ZombieAgent dies if HP reaches 0
```

### Zombie Attacking Plant

```text
ZombieAgent walks left
ZombieAgent asks environment if a plant blocks its path
If blocked, zombie stops
Zombie attacks plant on cooldown
Plant receives damage
Plant dies if HP reaches 0
Tile clears occupancy
Zombie resumes walking
```

### Win And Lose

```text
Lose:
Zombie reaches the house side
GameManager sets game state to Lost

Win:
WaveManager has spawned every scheduled zombie
LaneRegistry has no living zombies
GameManager sets game state to Won
```

## Build Order

### Phase 1: Environment Foundation

Build the world before adding complex agents.

Tasks:

1. Create editor-visible scene objects: `GameManager`, `Board`, `ZombieSpawner`, `UI`, and `Background`.
2. Configure the orthographic camera and lawn background.
3. Create `BoardGrid`.
4. Create 5 x 9 `Tile` objects.
5. Add mouse-to-tile selection.
6. Add simple tile debug visuals.

Success criteria:

- Clicking a tile logs or displays the correct row and column.
- Every tile has a stable world position.
- The board is visible and aligned with the lawn.

### Phase 2: Passive Plant Agent

Add the first agent without combat.

Tasks:

1. Create `Plant`.
2. Create `Peashooter.prefab`.
3. Allow `GameManager` to place Peashooter on a clicked tile.
4. Enforce one plant per tile.
5. Add plant HP and death.

Success criteria:

- Clicking an empty tile places one Peashooter.
- Clicking an occupied tile does not place another plant.
- A plant can be destroyed and its tile becomes empty again.

### Phase 3: Moving Zombie Agent

Add the first enemy agent.

Tasks:

1. Create `ZombieAgent`.
2. Create `BasicZombie.prefab`.
3. Create `LaneRegistry`.
4. Spawn one zombie in a chosen lane.
5. Register and unregister zombies by lane.
6. Move zombie left until it reaches the house side.

Success criteria:

- Zombie spawns in the correct lane.
- Zombie moves left.
- Zombie is tracked by `LaneRegistry`.
- Reaching the house side triggers lose state.

### Phase 4: Plant And Zombie Interaction

Make agents interact through environment rules.

Tasks:

1. Let zombie detect a blocking plant in its lane.
2. Stop zombie movement while blocked.
3. Attack plant on a cooldown.
4. Damage plant HP.
5. Resume walking after plant dies.

Success criteria:

- Zombie does not walk through a plant.
- Zombie repeatedly damages the plant.
- Plant death clears tile occupancy.
- Zombie resumes walking after plant death.

### Phase 5: Peashooter Combat

Add shooting and projectiles.

Tasks:

1. Create `PeashooterAgent`.
2. Create `PeaProjectile.prefab`.
3. Create `ProjectileAgent`.
4. Peashooter checks `LaneRegistry`.
5. Peashooter shoots only if a zombie exists in the same row.
6. Projectile damages zombie and disappears.

Success criteria:

- Peashooter does not shoot when no zombie exists in lane.
- Peashooter shoots when a zombie enters its lane.
- Projectile damages zombie.
- Zombie dies at 0 HP.

### Phase 6: Sun Economy

Add the first resource loop.

Tasks:

1. Add sun count to `GameManager`.
2. Add simple sun UI.
3. Create `Sunflower.prefab`.
4. Create `SunflowerAgent`.
5. Create `SunPickupAgent`.
6. Add plant card selection for Peashooter and Sunflower.
7. Add placement costs.

Success criteria:

- Sun amount is visible.
- Sunflower creates collectible sun.
- Collecting sun increases sun amount.
- Placing plants spends sun.
- Placement fails if sun is too low.

### Phase 7: Waves And Game Loop

Turn the prototype into a playable level.

Tasks:

1. Create `WaveManager`.
2. Replace endless random spawning with a simple wave schedule.
3. Spawn zombies by time and lane.
4. Track wave completion.
5. Add win condition.
6. Add restart button.

Success criteria:

- Zombies spawn according to the wave schedule.
- Player loses if a zombie reaches the house side.
- Player wins after all scheduled zombies are defeated.
- Player can restart the level.

### Phase 8: Polish And Expansion

Only expand after the core loop works.

Tasks:

1. Add card cooldown visuals.
2. Add plant placement sound.
3. Add projectile hit sound.
4. Add zombie groan sound.
5. Add pause menu.
6. Add shovel/remove plant tool.
7. Add Wall-nut.
8. Add Conehead and Buckethead zombies.
9. Add lawn mower.

Success criteria:

- The core game is understandable and playable.
- New agents can be added without rewriting the environment.
- Existing agents use the same environment APIs.

## Design Rules

- Environment owns shared state.
- Agents own local behavior.
- Agents ask the environment for information instead of searching the whole scene.
- Plants should not directly manage zombies.
- Zombies should not directly manage the board.
- Projectiles should only damage valid zombies in their lane.
- Tile occupancy is authoritative.
- Lane tracking is authoritative.
- UI displays state but does not own gameplay rules.
- Prefabs should expose tunable values in the Inspector.

## First Playable Core

The first complete milestone should include:

- 5 x 9 board.
- Clickable placement tiles.
- Peashooter.
- Sunflower.
- Basic Zombie.
- Pea projectile.
- Sun resource.
- Simple plant cards.
- Basic zombie waves.
- Win and lose condition.
- Restart.

Anything beyond that should wait until the core is stable.

## Assumptions

- The project remains a private learning project unless assets are replaced with original or licensed assets.
- The workflow should be Unity prefab and scene first.
- Existing GIF-generated animation controllers should be reused.
- Existing prototype scripts can be refactored instead of preserved as-is.
- The first zombie type is Basic Zombie.
- The first plant types are Peashooter and Sunflower.

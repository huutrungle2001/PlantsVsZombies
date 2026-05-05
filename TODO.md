# Plants Vs Zombies Unity TODO

This is the master build checklist for finishing the game. The project should be built as an **environment** plus **agents**:

- Environment: board, tiles, lanes, game rules, resources, waves, win/lose state.
- Agents: plants, zombies, projectiles, sun pickups, lawn mowers.

Complete each milestone before moving to the next one.

## 0. Project Foundation

- [x] Confirm Unity version is `6000.4.5f1`.
- [x] Keep the main scene at `Assets/Scenes/Main.unity`.
- [x] Keep game scripts under `Assets/Scripts`.
- [x] Keep prefabs under `Assets/Prefabs`.
- [x] Keep source art under `Assets/Art`.
- [x] Keep generated animation controllers under `Assets/Art/Animations` and `Assets/Resources/Animations`.
- [x] Document important architecture decisions in `docs/`.
- [x] Remove or reduce runtime world creation from `Bootstrap`.
- [x] Use scene objects and prefabs as the main Unity workflow.
- [x] Keep `Bootstrap` only for small global setup if needed.

## 1. Environment Foundation

- [x] Create editor-visible scene root objects:
  - [x] `GameManager`
  - [x] `Board`
  - [x] `LaneRegistry`
  - [x] `WaveManager`
  - [x] `UI`
  - [x] `Background`
- [x] Configure the main camera as orthographic.
- [x] Place and scale the lawn background.
- [x] Create `BoardGrid`.
- [x] Define a 5-row by 9-column board.
- [x] Define tile width, tile height, board origin, and lane spacing.
- [x] Convert row/column coordinates to world positions.
- [x] Convert mouse world position to row/column coordinates.
- [x] Create `Tile` objects for every board cell.
- [x] Store row and column on each `Tile`.
- [x] Add temporary debug visuals for tiles.
- [x] Add tile hover feedback.
- [x] Add tile click detection.
- [x] Verify clicking every tile reports the correct row and column.

## 2. Core Game State

- [ ] Create `GameManager`.
- [ ] Add game states:
  - [ ] NotStarted
  - [ ] Playing
  - [ ] Paused
  - [ ] Won
  - [ ] Lost
- [ ] Track current sun amount.
- [ ] Track selected plant type.
- [ ] Track whether placement mode is active.
- [ ] Add plant placement validation.
- [ ] Reject placement when tile is occupied.
- [ ] Reject placement when sun is too low.
- [ ] Reject placement when game is not playing.
- [ ] Spend sun only after successful placement.
- [ ] Add basic restart flow.
- [ ] Add basic pause and resume flow.

## 3. Plant Base Agent

- [ ] Create base `Plant` component.
- [ ] Add max HP and current HP.
- [ ] Add sun cost.
- [ ] Add row and column.
- [ ] Add reference to occupied `Tile`.
- [ ] Register plant with tile after placement.
- [ ] Clear tile when plant dies.
- [ ] Add `TakeDamage(int amount)`.
- [ ] Add `Die()`.
- [ ] Add optional death sound hook.
- [ ] Create `Peashooter.prefab`.
- [ ] Attach Peashooter animation controller.
- [ ] Add collider if needed for click/debug selection.
- [ ] Tune Peashooter scale and sorting order.
- [ ] Place Peashooter through `GameManager`.
- [ ] Verify one plant can be placed on a tile.
- [ ] Verify a second plant cannot be placed on the same tile.

## 4. Lane Registry

- [ ] Create `LaneRegistry`.
- [ ] Track zombies by row.
- [ ] Register zombies when spawned.
- [ ] Unregister zombies when dead.
- [ ] Unregister zombies when they leave the board.
- [ ] Query whether a row has any zombies.
- [ ] Query the first zombie in a row after a given X position.
- [ ] Query active zombie count.
- [ ] Use `LaneRegistry` instead of broad scene searches.

## 5. Basic Zombie Agent

- [ ] Create `ZombieAgent`.
- [ ] Add max HP and current HP.
- [ ] Add lane row.
- [ ] Add movement speed.
- [ ] Add attack damage.
- [ ] Add attack interval.
- [ ] Add house-side lose X position.
- [ ] Register zombie with `LaneRegistry`.
- [ ] Move zombie left while not blocked.
- [ ] Trigger lose state when zombie reaches the house side.
- [ ] Add `TakeDamage(int amount)`.
- [ ] Add `Die()`.
- [ ] Unregister from `LaneRegistry` on death.
- [ ] Create `BasicZombie.prefab`.
- [ ] Attach NormalZombie animation controller.
- [ ] Add `BoxCollider2D`.
- [ ] Tune zombie scale and sorting order.
- [ ] Spawn one BasicZombie in a chosen lane.
- [ ] Verify zombie moves in the correct lane.
- [ ] Verify zombie death removes it from lane tracking.

## 6. Plant And Zombie Blocking

- [ ] Let `ZombieAgent` ask the environment for the next blocking plant.
- [ ] Detect when zombie reaches a plant in the same lane.
- [ ] Stop zombie movement while blocked.
- [ ] Attack plant on cooldown.
- [ ] Damage plant HP.
- [ ] Resume walking after the plant dies.
- [ ] Prevent zombie from walking through living plants.
- [ ] Trigger attack animation while attacking.
- [ ] Trigger walk animation while walking.
- [ ] Verify zombie attacks plant until it dies.
- [ ] Verify tile occupancy clears after plant death.

## 7. Projectile Combat

- [ ] Create `ProjectileAgent`.
- [ ] Add speed.
- [ ] Add damage.
- [ ] Add lane row.
- [ ] Add max lifetime.
- [ ] Move projectile right.
- [ ] Destroy projectile after max lifetime.
- [ ] Damage only zombies in the same lane.
- [ ] Destroy projectile after hitting a zombie.
- [ ] Create `PeaProjectile.prefab`.
- [ ] Use `Assets/Art/items/Pea.png`.
- [ ] Add `CircleCollider2D` as trigger.
- [ ] Add kinematic `Rigidbody2D` if trigger collision requires it.
- [ ] Tune projectile scale and sorting order.
- [ ] Add projectile hit sound hook.

## 8. Peashooter Agent

- [ ] Create `PeashooterAgent`.
- [ ] Add fire interval.
- [ ] Add projectile spawn point offset.
- [ ] Add projectile prefab reference.
- [ ] Check `LaneRegistry` for zombies in the same row.
- [ ] Shoot only when a zombie exists in the same row.
- [ ] Spawn projectile in the correct lane.
- [ ] Start cooldown after shooting.
- [ ] Stop shooting after game ends.
- [ ] Verify Peashooter does not shoot when lane is empty.
- [ ] Verify Peashooter shoots when zombie enters lane.
- [ ] Verify projectiles damage and kill zombies.

## 9. Sun Economy

- [ ] Add sun amount UI.
- [ ] Add `AddSun(int amount)` to `GameManager`.
- [ ] Add `SpendSun(int amount)` to `GameManager`.
- [ ] Create `SunPickupAgent`.
- [ ] Use `Assets/Art/items/Sun.png`.
- [ ] Add click collection.
- [ ] Add pickup lifetime.
- [ ] Add collection sound hook.
- [ ] Create `Sunflower.prefab`.
- [ ] Attach SunFlower animation controller.
- [ ] Create `SunflowerAgent`.
- [ ] Add sun generation interval.
- [ ] Spawn sun pickup near Sunflower.
- [ ] Verify collecting sun increases sun count.
- [ ] Verify planting spends sun.
- [ ] Verify placement fails when sun is too low.

## 10. Plant Card UI

- [ ] Create card UI area.
- [ ] Add Peashooter card.
- [ ] Add Sunflower card.
- [ ] Display plant cost.
- [ ] Display card cooldown state.
- [ ] Highlight selected card.
- [ ] Disable card when sun is too low.
- [ ] Disable card while on cooldown.
- [ ] Select plant by clicking card.
- [ ] Cancel selection by clicking empty UI/cancel area.
- [ ] Verify selected plant controls tile placement.

## 11. Wave Manager

- [ ] Create `WaveManager`.
- [ ] Define simple wave entry data:
  - [ ] spawn time
  - [ ] lane
  - [ ] zombie prefab
- [ ] Replace endless random spawning.
- [ ] Spawn zombies according to schedule.
- [ ] Track how many scheduled zombies have spawned.
- [ ] Track whether the wave is complete.
- [ ] Notify `GameManager` when all zombies are spawned and defeated.
- [ ] Add pre-wave delay.
- [ ] Add "zombies are coming" audio hook.
- [ ] Verify zombies spawn at expected times and lanes.

## 12. Win, Lose, And Restart

- [ ] Lose when any zombie reaches the house side.
- [ ] Win when all scheduled zombies are spawned and no zombies remain alive.
- [ ] Stop agent updates after win or lose.
- [ ] Show win UI.
- [ ] Show lose UI.
- [ ] Add restart button.
- [ ] Add return-to-menu placeholder button if needed.
- [ ] Verify restart resets board, plants, zombies, projectiles, sun, and wave state.

## 13. Audio

- [ ] Add central audio manager or simple scene audio source.
- [ ] Add plant placement sound.
- [ ] Add projectile hit sound.
- [ ] Add zombie groan sound.
- [ ] Add zombie eating sound.
- [ ] Add Sun collection sound if asset exists.
- [ ] Add lawn mower sound.
- [ ] Add win sound if asset exists.
- [ ] Add lose sound if asset exists.
- [ ] Tune volume levels.
- [ ] Avoid overlapping repeated sounds too aggressively.

## 14. Animation And Visual Polish

- [ ] Confirm every generated animation controller loops correctly.
- [ ] Use walk animation for moving zombies.
- [ ] Use eating animation for attacking zombies when available.
- [ ] Use idle animation for plants.
- [ ] Add plant death visual behavior.
- [ ] Add zombie death visual behavior.
- [ ] Add projectile hit visual effect if available.
- [ ] Add sorting order by row so lower lanes appear in front.
- [ ] Tune sprite scale per plant and zombie.
- [ ] Remove debug tile visuals or make them toggleable.
- [ ] Keep hover/placement indicators readable.

## 15. Lawn Mower

- [ ] Create `LawnMowerAgent`.
- [ ] Create one lawn mower per row.
- [ ] Use idle mower sprite before activation.
- [ ] Activate when a zombie reaches mower trigger area.
- [ ] Move mower right after activation.
- [ ] Kill zombies in the same lane on contact.
- [ ] Play mower sound.
- [ ] Remove mower after it exits the board.
- [ ] Ensure each row has only one mower use.
- [ ] Verify mower prevents immediate lose once per lane.

## 16. Additional Plant Agents

- [ ] Add Wall-nut.
- [ ] Add Wall-nut high HP.
- [ ] Add damaged Wall-nut visual state.
- [ ] Add Repeater.
- [ ] Add Repeater double-shot behavior.
- [ ] Add Snow Pea.
- [ ] Add Snow Pea slow effect.
- [ ] Add Cherry Bomb.
- [ ] Add Cherry Bomb area damage.
- [ ] Add Potato Mine.
- [ ] Add Potato Mine arm delay and trigger.
- [ ] Add Chomper.
- [ ] Add Chomper eat behavior and digest cooldown.
- [ ] Add Jalapeno.
- [ ] Add Jalapeno full-lane damage.
- [ ] Add cards, costs, cooldowns, and prefabs for each plant.
- [ ] Verify each plant works through environment APIs.

## 17. Additional Zombie Agents

- [ ] Add Conehead Zombie.
- [ ] Tune Conehead HP above Basic Zombie.
- [ ] Add Buckethead Zombie.
- [ ] Tune Buckethead HP above Conehead Zombie.
- [ ] Add Flag Zombie.
- [ ] Use Flag Zombie for wave introduction.
- [ ] Add running zombie variant if desired.
- [ ] Add burnt/death variants if useful.
- [ ] Add prefab, HP, speed, damage, and animation controller per zombie type.
- [ ] Verify every zombie registers with `LaneRegistry`.
- [ ] Verify every zombie respects plant blocking.

## 18. Level Progression

- [ ] Create level data structure.
- [ ] Define level number.
- [ ] Define starting sun.
- [ ] Define allowed plant cards.
- [ ] Define wave schedule.
- [ ] Define background if multiple maps are added.
- [ ] Build Level 1 with Peashooter, Sunflower, and Basic Zombie.
- [ ] Build Level 2 with Wall-nut and Conehead.
- [ ] Build Level 3 with more lanes or harder wave timing.
- [ ] Add level completion tracking.
- [ ] Unlock next level after win.

## 19. Menus

- [ ] Create main menu scene or menu panel.
- [ ] Add start game button.
- [ ] Add level select screen.
- [ ] Add pause menu.
- [ ] Add resume button.
- [ ] Add restart button.
- [ ] Add return to main menu button.
- [ ] Add quit button for desktop build.
- [ ] Wire menu buttons to scene/game state.
- [ ] Verify navigation works without broken states.

## 20. Save Data

- [ ] Decide save format.
- [ ] Save completed levels.
- [ ] Save unlocked levels.
- [ ] Save player settings if needed.
- [ ] Load save data on game start.
- [ ] Handle missing save file.
- [ ] Add reset progress option if useful.

## 21. Balance Pass

- [ ] Tune plant costs.
- [ ] Tune plant cooldowns.
- [ ] Tune plant HP.
- [ ] Tune zombie HP.
- [ ] Tune zombie speed.
- [ ] Tune zombie attack damage.
- [ ] Tune wave spawn timing.
- [ ] Tune starting sun.
- [ ] Tune Sunflower generation interval.
- [ ] Playtest Level 1 until it is easy but not empty.
- [ ] Playtest later levels for steady difficulty increase.

## 22. Testing Checklist

- [ ] Test tile selection across all rows and columns.
- [ ] Test placing each plant.
- [ ] Test occupied tile rejection.
- [ ] Test insufficient sun rejection.
- [ ] Test card cooldowns.
- [ ] Test every projectile type.
- [ ] Test every zombie type.
- [ ] Test plant death.
- [ ] Test zombie death.
- [ ] Test sun collection.
- [ ] Test mower activation.
- [ ] Test win state.
- [ ] Test lose state.
- [ ] Test restart after win.
- [ ] Test restart after lose.
- [ ] Test pause and resume.
- [ ] Test level transition.
- [ ] Test built app, not only Unity Editor play mode.

## 23. Build And Release

- [ ] Run Unity build through `make build`.
- [ ] Launch built app.
- [ ] Verify main menu loads.
- [ ] Verify Level 1 is playable.
- [ ] Verify audio works in build.
- [ ] Verify animations work in build.
- [ ] Verify restart and quit work in build.
- [ ] Remove temporary debug objects from release scene.
- [ ] Keep debug helpers available behind a toggle if useful.
- [ ] Confirm no console errors during normal play.
- [ ] Confirm asset licensing before sharing publicly.

## Definition Of Done

- [ ] Player can start the game from a menu.
- [ ] Player can select a level.
- [ ] Player can place plants on a 5 x 9 board.
- [ ] Player can collect and spend sun.
- [ ] Plants and zombies interact correctly.
- [ ] Projectiles damage zombies correctly.
- [ ] Zombies attack plants correctly.
- [ ] Lawn mowers work once per lane.
- [ ] Waves spawn predictable zombie groups.
- [ ] Win and lose states work.
- [ ] Restart works.
- [ ] Multiple plant and zombie types work.
- [ ] The game builds and runs outside the Unity Editor.
- [ ] The project structure is understandable for future changes.

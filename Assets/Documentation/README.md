# Space Explorer – Survival 2D

Space Explorer is a Unity 6.6 URP 2D asteroid-survival shooter for Windows. This upgrade keeps the original MainMenu → Gameplay → EndGame flow and adds denser survival gameplay, procedural sci-fi art, HUD feedback and lightweight VFX.

## Play

1. Open `Assets/Scenes/MainMenu.unity` and press Play.
2. Use Arrow Keys/WASD to move and Space to fire.
3. Survive, destroy asteroids, collect stars and gifts, and maximize score.

## Gameplay systems

- 3 HP, short post-hit invulnerability, shield protection and Health gifts.
- Six configured asteroid types: Small (1 HP/5), Medium (2/10), Large (5/25), Fast (1/15), Armored (8/40), Special (12/75).
- Eight gifts: Green +10, Blue +25, Gold +50, Double Score (10s), Rapid Fire (8s), Shield (6s), Health +1 and Bomb.
- Bomb destroys active asteroids and awards 50% of each asteroid's score once.
- Six time stages at 0/30/60/120/180/300 seconds. Active asteroid caps are 12/20/30/45/60/80 and spawn intervals are .9/.65/.45/.3/.2/.15 seconds.
- Asteroids enter from all four camera edges, aim into the play area, avoid the player safe radius and are destroyed without score when they leave the camera bounds.
- Difficulty weights, speed multipliers and caps are Inspector-configurable. The active cap prevents unbounded object growth; the current implementation uses capped Instantiate/Destroy and off-screen cleanup rather than a pool.
- HUD shows score, survival time, HP, stage, high score and active power-up countdown.

## Visual upgrade

- Procedural assets in `Assets/Sprites/Generated`: sci-fi player ship, laser glow, crystal star, six asteroid silhouettes, eight gift icons, nebula background, dust and shockwave ring.
- The ship prefab keeps its Rigidbody2D/Collider2D and now has cyan/blue/silver layered art plus animated engine thrust.
- Asteroids use distinct silhouettes, rock/metal/purple palettes, craters, hit flash and destruction bursts.
- Stars rotate/twinkle and gifts use outlined box/ribbon/bow artwork, type colors and a glow layer.
- Gameplay uses a dark blue/purple space backdrop with runtime parallax dust/stars, safe-margin sci-fi HUD panels, muzzle flash, pickup FX, shield aura and camera shake.

## Important scripts

- `GameSession`: score, high score and HP state.
- `SpaceExplorerGameManager`: run lifecycle, scoring, damage and power-ups.
- `DifficultyManager`: inspector-configurable stage thresholds, weights and caps.
- `AsteroidController`/`AsteroidSpawner`: data-driven asteroid variants and collision-safe scoring.
- `GiftBox`/`GiftSpawner`: weighted, capped gift spawning and one-shot rewards.
- `ScoreManager`: live Gameplay HUD.
- `SpaceBackgroundController`, `PlayerThrustVfx`, `CameraShake`, `ShockwaveFx` and `PickupFx`: visual feedback and low-cost background motion.

## Editor tools

- `Tools/Space Explorer/Build Survival Upgrade` wires the current Scenes and Prefabs.
- `Tools/Space Explorer/Build Visual Upgrade` regenerates procedural sprites and reconnects visual references.
- `Tools/Space Explorer/Validate Lab 1` checks the original scene flow and required components.
- EditMode tests are in `Assets/Tests/Editor/SpaceExplorerGameplayTests.cs`.

## Verification

- Latest script recompile: 0 errors, 0 warnings.
- Latest filtered EditMode run: 11/11 passed, 0 failed, 0 skipped.
- Latest Play Mode smoke: Gameplay entered/exited, runtime spawner and GiftBox clones were observed, and Console remained empty.
- Physical keyboard/mouse injection, long-duration FPS profiling and every collision/pickup path remain `NOT VERIFIED` through MCP; use the manual checklist in `Assets/Documentation/TEST_REPORT.md`.

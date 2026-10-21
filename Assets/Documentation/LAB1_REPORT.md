# Space Explorer – Lab 1 and Survival Upgrade Report

## Preserved Lab 1 flow

`MainMenu (0) → Gameplay (1) → EndGame (2) → MainMenu` remains registered in Build Settings. Existing SceneLoader, menu buttons, player movement/shooting, stars and EndGame UI were retained and extended.

## Gameplay architecture

`SpaceExplorerGameManager` begins a clean run with 3 HP, resets power-up timers and drives Game Over. `GameSession` persists score/high score across scenes and resets score/HP on a new run. `ScoreManager` reads the manager and session every frame for the HUD.

## Asteroids

One reusable `Asteroid.prefab` is configured by six `AsteroidVariant` records in `AsteroidSpawner`. Each record controls type, HP, score, scale, speed, tint and special direction-change interval. Laser damage is configurable; health reaches zero before score is awarded, and collision/bomb paths guard against duplicate processing.

## Gifts

`GiftSpawner` uses a weighted table and maximum-active cap. `GiftBox` has a trigger collider, unique color/scale, one-shot collection guard and the eight rewards described in README. The scene stores `GiftBox.prefab` as an asset reference and a sprite fallback for safe runtime recovery.

## Difficulty and density upgrade

`DifficultyManager` exposes six Inspector stage records. The survival targets are 12/.9s, 20/.65s, 30/.45s, 45/.3s, 60/.2s and 80/.15s at 0/30/60/120/180/300 seconds. `GetStageIndexForTime` makes boundary checks deterministic and supports fast tests without waiting five minutes. `AsteroidSpawner` reads the current cap, interval, speed multiplier and type weights each frame, spawns from all four camera edges, avoids the player and cleans up off-screen asteroids without awarding score.

## Visual feedback

Procedural PNG sprites now provide a layered sci-fi ship, six asteroid silhouettes, eight outlined gift boxes, crystal stars, laser glow and a blue/purple starfield. Asteroids flash on hit and burst on destruction; the player flashes on damage; shield creates a visible aura; gifts are color-coded and glow; point awards create floating score text; bomb destruction uses a shockwave and camera shake. Runtime background dust/stars provide lightweight parallax motion.

## Verification status

- Unity MCP script recompile: 0 errors, 0 warnings.
- EditMode regression suite: 11/11 filtered tests passed.
- Gameplay Play Mode smoke: runtime scene entered successfully, spawner reported the new cap/edge configuration, GiftBox clone and parallax background controller were active, and produced no Console errors during the observed run.
- Visual asset inspection: ship, asteroid, star, gift and background PNGs were inspected after regeneration; gift sprites visibly contain an outlined box, lid, ribbon and bow.
- Physical keyboard/mouse injection is not available through the current MCP surface; those interactions remain NOT VERIFIED by automation.

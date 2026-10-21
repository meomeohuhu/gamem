# TEST REPORT – Space Explorer Survival

## Environment

- Unity 6.6, URP 2D, Windows target.
- Project: `C:\Users\Thinhpa\SpaceExplore`.
- Unity MCP used for compile, scene inspection, Play Mode and Test Runner.

| ID | Test | Result | Evidence |
|---|---|---|---|
| T01 | Script compilation | PASS | Unity MCP: 0 errors, 0 warnings |
| T02 | Difficulty boundaries 0/30/60/120/180/300 | PASS | EditMode parameterized tests |
| T03 | Six asteroid data records | PASS | EditMode test; scene spawner has 6 variants |
| T04 | Eight gift enum/reward slots | PASS | EditMode test; GiftSpawner weights configured |
| T05 | HP and new-run reset | PASS | EditMode session test |
| T06 | Scene wiring and HUD | PASS | MCP hierarchy: Game Manager, DifficultyManager, spawners and six HUD fields |
| T07 | Runtime spawn smoke | PASS | Play Mode hierarchy: new spawner cap/edge settings, asteroid runtime count, GiftBox clone and background controller; no Console errors |
| T08 | Laser/projectile path | NOT VERIFIED | Components/prefab are present; physical input/collision injection was unavailable through MCP |
| T09 | Gift collection/effect expiry | NOT VERIFIED | Code path and serialized wiring present; timed physical collection not automated |
| T10 | Physical keyboard/mouse | NOT VERIFIED | No input-injection tool available |
| T11 | Windows build | PASS | Unity Editor build log: `Builds/Windows/SpaceExplorer.exe`; total output size 110,891,119 bytes |

## Automated test result

Unity Test Runner reported **11/11 passed, 0 failed, 0 skipped** for `SpaceExplore.Tests.SpaceExplorerGameplayTests`.

## Regression notes

Duplicate asteroid processing is guarded by a destruction flag. Bomb destruction marks asteroids before awarding 50% score. GameSession and all timed effects are reset in `SpaceExplorerGameManager.Start`.

## Density target matrix

| Survival time | Stage | Max active | Spawn interval |
|---:|---:|---:|---:|
| 0–30s | 1 | 12 | 0.90s |
| 30–60s | 2 | 20 | 0.65s |
| 60–120s | 3 | 30 | 0.45s |
| 120–180s | 4 | 45 | 0.30s |
| 180–300s | 5 | 60 | 0.20s |
| 300s+ | 6 | 80 | 0.15s |

## Manual checklist (NOT VERIFIED through MCP input injection)

1. In Gameplay, hold Arrow Keys/WASD and confirm the ship remains inside the camera bounds.
2. Hold Space and confirm laser cooldown, muzzle flash, hit flash, HP damage and asteroid destruction.
3. Collect each of the eight gift types and confirm reward text, timer expiry and restart reset.
4. Run for at least 30/60/120 seconds and count visible density against the matrix; monitor FPS in the Unity Stats overlay.
5. Confirm MainMenu → Gameplay → EndGame → MainMenu and Restart/Quit buttons on Windows.

## Latest build note

The Windows build completed and the output directory contains the executable and data files. Unity emitted a non-fatal `Pipeline: No RuntimePipelineConfig asset found` warning and a build-time “uncompiled code changes” warning while the MCP request timed out; the Editor build log nevertheless reported `Windows build PASS`, and the generated `Assembly-CSharp.dll`/scene data were written at the build timestamp. A clean compile and 11/11 EditMode pass were completed before this build.

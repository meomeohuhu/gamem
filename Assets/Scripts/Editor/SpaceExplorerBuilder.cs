#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using TMPro;
using UnityEditor;
using UnityEditor.Events;
using UnityEditor.SceneManagement;
using UnityEditor.Build.Reporting;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace SpaceExplore.Editor
{
    public static class SpaceExplorerBuilder
    {
        private const string Scenes = "Assets/Scenes";
        private const string Prefabs = "Assets/Prefabs";
        private const string Sprites = "Assets/Sprites";

        private const string MainMenuScene = "Assets/Scenes/MainMenu.unity";
        private const string GameplayScene = "Assets/Scenes/Gameplay.unity";
        private const string EndGameScene = "Assets/Scenes/EndGame.unity";
        private static bool smokeStarted;
        private static bool smokeLaserSpawned;
        private static bool smokeScoreUpdated;

        [MenuItem("Tools/Space Explorer/Build Lab 1")]
        public static void BuildLab1()
        {
            try
            {
                EnsureFolders();
                AssetDatabase.Refresh();

                Sprite background = CreateSpriteIfMissing("Assets/Sprites/space_background.png", 2, 2, DrawBackground, 1f);
                Sprite playerSprite = CreateSpriteIfMissing("Assets/Sprites/player_ship.png", 96, 96, DrawPlayer, 64f);
                Sprite laserSprite = CreateSpriteIfMissing("Assets/Sprites/laser.png", 16, 48, DrawLaser, 64f);
                Sprite asteroidSprite = CreateSpriteIfMissing("Assets/Sprites/asteroid.png", 96, 96, DrawAsteroid, 64f);
                Sprite starSprite = CreateSpriteIfMissing("Assets/Sprites/star.png", 64, 64, DrawStar, 64f);
                AssetDatabase.Refresh();

                LaserProjectile laserPrefab = CreateLaserPrefab(laserSprite);
                AsteroidController asteroidPrefab = CreateAsteroidPrefab(asteroidSprite);
                StarCollectible starPrefab = CreateStarPrefab(starSprite);
                PlayerController playerPrefab = CreatePlayerPrefab(playerSprite, laserPrefab);

                if (!File.Exists(MainMenuScene)) BuildMainMenuScene(background);
                if (!File.Exists(GameplayScene)) BuildGameplayScene(background, playerPrefab, asteroidPrefab, starPrefab);
                if (!File.Exists(EndGameScene)) BuildEndGameScene(background);

                EditorBuildSettings.scenes = new[]
                {
                    new EditorBuildSettingsScene(MainMenuScene, true),
                    new EditorBuildSettingsScene(GameplayScene, true),
                    new EditorBuildSettingsScene(EndGameScene, true)
                };

                UpgradeInputModules(MainMenuScene);
                UpgradeInputModules(GameplayScene);
                UpgradeInputModules(EndGameScene);

                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
                EditorSceneManager.OpenScene(MainMenuScene, OpenSceneMode.Single);
                Debug.Log("Space Explorer Lab 1 build completed: scenes, prefabs, scripts and build settings are ready.");
            }
            catch (Exception exception)
            {
                Debug.LogError("Space Explorer Lab 1 builder failed: " + exception);
                throw;
            }
        }

        [MenuItem("Tools/Space Explorer/Clear Console")]
        public static void ClearConsole()
        {
            Type logEntries = Type.GetType("UnityEditor.LogEntries, UnityEditor");
            MethodInfo clear = logEntries?.GetMethod("Clear", BindingFlags.Static | BindingFlags.Public);
            clear?.Invoke(null, null);
        }

        [MenuItem("Tools/Space Explorer/Validate Lab 1")]
        public static void ValidateLab1()
        {
            string[] scenePaths = { MainMenuScene, GameplayScene, EndGameScene };
            string[] prefabPaths =
            {
                "Assets/Prefabs/Player/PlayerShip.prefab",
                "Assets/Prefabs/Projectiles/Laser.prefab",
                "Assets/Prefabs/Asteroids/Asteroid.prefab",
                "Assets/Prefabs/Collectibles/Star.prefab"
            };
            bool valid = true;
            foreach (string scenePath in scenePaths)
            {
                if (AssetDatabase.LoadAssetAtPath<SceneAsset>(scenePath) == null)
                {
                    Debug.LogError("Lab 1 validation missing Scene: " + scenePath);
                    valid = false;
                }
            }
            foreach (string prefabPath in prefabPaths)
            {
                if (AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath) == null)
                {
                    Debug.LogError("Lab 1 validation missing Prefab: " + prefabPath);
                    valid = false;
                }
            }
            HashSet<string> buildScenes = new();
            foreach (EditorBuildSettingsScene scene in EditorBuildSettings.scenes)
                if (scene.enabled) buildScenes.Add(scene.path);
            foreach (string scenePath in scenePaths)
            {
                if (!buildScenes.Contains(scenePath))
                {
                    Debug.LogError("Lab 1 validation Scene is not in Build Settings: " + scenePath);
                    valid = false;
                }
            }
            GameObject player = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Player/PlayerShip.prefab");
            GameObject laser = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Projectiles/Laser.prefab");
            GameObject asteroid = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Asteroids/Asteroid.prefab");
            GameObject star = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Collectibles/Star.prefab");
            valid &= player != null && player.GetComponent<PlayerController>() != null && player.GetComponent<PlayerShooter>() != null && player.GetComponent<Rigidbody2D>() != null && player.GetComponent<Collider2D>() != null;
            valid &= laser != null && laser.GetComponent<LaserProjectile>() != null && laser.GetComponent<Collider2D>() != null;
            valid &= asteroid != null && asteroid.GetComponent<AsteroidController>() != null && asteroid.GetComponent<Rigidbody2D>() != null && asteroid.GetComponent<Collider2D>() != null;
            valid &= star != null && star.GetComponent<StarCollectible>() != null && star.GetComponent<Collider2D>() != null;

            Scene mainMenu = EditorSceneManager.OpenScene(MainMenuScene, OpenSceneMode.Single);
            GameObject mainMenuCanvas = FindSceneObject(mainMenu, "Main Menu Canvas");
            GameObject playButton = FindSceneObject(mainMenu, "Play Button");
            GameObject instructionsButton = FindSceneObject(mainMenu, "Instructions Button");
            GameObject quitButton = FindSceneObject(mainMenu, "Quit Button");
            valid &= mainMenuCanvas != null && mainMenuCanvas.GetComponent<SceneLoader>() != null && mainMenuCanvas.GetComponent<MainMenuUI>() != null;
            valid &= HasPersistentListener(playButton, "LoadGameplay");
            valid &= HasPersistentListener(instructionsButton, "ShowInstructions");
            valid &= HasPersistentListener(quitButton, "QuitGame");

            Scene endGame = EditorSceneManager.OpenScene(EndGameScene, OpenSceneMode.Single);
            GameObject endCanvas = FindSceneObject(endGame, "End Game Canvas");
            valid &= endCanvas != null && endCanvas.GetComponent<SceneLoader>() != null && endCanvas.GetComponent<EndGameUI>() != null;
            valid &= HasPersistentListener(FindSceneObject(endGame, "Main Menu Button"), "LoadMainMenu");
            valid &= HasPersistentListener(FindSceneObject(endGame, "Quit Button"), "QuitGame");
            if (valid) Debug.Log("Lab 1 validation PASS: Scenes, Build Settings and required Prefab components are present.");
            else Debug.LogError("Lab 1 validation FAIL: one or more required assets/components are missing.");
        }

        [MenuItem("Tools/Space Explorer/Run Play Mode Smoke")]
        public static void RunPlayModeSmoke()
        {
            if (EditorApplication.isPlaying)
            {
                Debug.LogError("Stop Play Mode before starting the Space Explorer smoke test.");
                return;
            }
            EditorSceneManager.OpenScene(GameplayScene, OpenSceneMode.Single);
            SpaceExplorerSmokeHooks.Request();
        }

        [MenuItem("Tools/Space Explorer/Build Windows")]
        public static void BuildWindows()
        {
            string buildDirectory = Path.Combine(Application.dataPath, "..", "Builds", "Windows");
            Directory.CreateDirectory(buildDirectory);
            List<string> enabledScenes = new();
            foreach (EditorBuildSettingsScene scene in EditorBuildSettings.scenes)
                if (scene.enabled) enabledScenes.Add(scene.path);
            if (enabledScenes.Count == 0)
            {
                Debug.LogError("Windows build skipped: no enabled scenes in Build Settings.");
                return;
            }
            BuildPlayerOptions options = new()
            {
                scenes = enabledScenes.ToArray(),
                locationPathName = Path.Combine(buildDirectory, "SpaceExplorer.exe"),
                target = BuildTarget.StandaloneWindows64,
                options = BuildOptions.None
            };
            BuildReport report = BuildPipeline.BuildPlayer(options);
            if (report.summary.result == BuildResult.Succeeded)
                Debug.Log($"Windows build PASS: {report.summary.outputPath}, size={report.summary.totalSize} bytes");
            else
                Debug.LogError($"Windows build FAIL: {report.summary.result}, errors={report.summary.totalErrors}, warnings={report.summary.totalWarnings}");
        }

        private static GameObject FindSceneObject(Scene scene, string objectName)
        {
            foreach (GameObject root in scene.GetRootGameObjects())
            {
                Transform match = FindChild(root.transform, objectName);
                if (match != null) return match.gameObject;
            }
            return null;
        }

        private static Transform FindChild(Transform parent, string objectName)
        {
            if (parent.name == objectName) return parent;
            foreach (Transform child in parent)
            {
                Transform match = FindChild(child, objectName);
                if (match != null) return match;
            }
            return null;
        }

        private static bool HasPersistentListener(GameObject gameObject, string methodName)
        {
            Button button = gameObject != null ? gameObject.GetComponent<Button>() : null;
            if (button == null || button.onClick.GetPersistentEventCount() == 0)
            {
                Debug.LogError("Lab 1 validation missing Button listener: " + methodName);
                return false;
            }
            for (int i = 0; i < button.onClick.GetPersistentEventCount(); i++)
                if (button.onClick.GetPersistentMethodName(i) == methodName) return true;
            Debug.LogError("Lab 1 validation listener method mismatch: expected " + methodName);
            return false;
        }

        private static void EnsureFolders()
        {
            EnsureFolder("Assets/Scripts");
            EnsureFolder("Assets/Scripts/Core");
            EnsureFolder("Assets/Scripts/Player");
            EnsureFolder("Assets/Scripts/Gameplay");
            EnsureFolder("Assets/Scripts/UI");
            EnsureFolder("Assets/Scripts/Editor");
            EnsureFolder(Prefabs);
            EnsureFolder("Assets/Prefabs/Player");
            EnsureFolder("Assets/Prefabs/Asteroids");
            EnsureFolder("Assets/Prefabs/Collectibles");
            EnsureFolder("Assets/Prefabs/Projectiles");
            EnsureFolder(Sprites);
            EnsureFolder("Assets/Materials");
            EnsureFolder("Assets/Audio");
            EnsureFolder("Assets/Documentation");
        }

        private static void UpgradeInputModules(string scenePath)
        {
            if (AssetDatabase.LoadAssetAtPath<SceneAsset>(scenePath) == null) return;
            Scene scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);
            foreach (GameObject root in scene.GetRootGameObjects())
            {
                EventSystem eventSystem = root.GetComponent<EventSystem>();
                if (eventSystem == null) continue;
                StandaloneInputModule legacy = root.GetComponent<StandaloneInputModule>();
                if (legacy != null) UnityEngine.Object.DestroyImmediate(legacy, true);
                if (root.GetComponent<InputSystemUIInputModule>() == null) root.AddComponent<InputSystemUIInputModule>();
            }
            EditorSceneManager.SaveScene(scene, scenePath);
        }

        private static void EnsureFolder(string path)
        {
            if (AssetDatabase.IsValidFolder(path)) return;
            string parent = Path.GetDirectoryName(path)?.Replace('\\', '/');
            string name = Path.GetFileName(path);
            if (!string.IsNullOrEmpty(parent) && !AssetDatabase.IsValidFolder(parent)) EnsureFolder(parent);
            AssetDatabase.CreateFolder(parent, name);
        }

        private static Sprite CreateSpriteIfMissing(string assetPath, int width, int height, Action<Texture2D> draw, float pixelsPerUnit)
        {
            Sprite existing = AssetDatabase.LoadAssetAtPath<Sprite>(assetPath);
            if (existing != null) return existing;

            string absolutePath = Path.Combine(Application.dataPath, assetPath.Substring("Assets/".Length));
            Directory.CreateDirectory(Path.GetDirectoryName(absolutePath));
            Texture2D texture = new(width, height, TextureFormat.RGBA32, false);
            Color[] clear = new Color[width * height];
            for (int i = 0; i < clear.Length; i++) clear[i] = Color.clear;
            texture.SetPixels(clear);
            draw(texture);
            texture.Apply();
            File.WriteAllBytes(absolutePath, texture.EncodeToPNG());
            UnityEngine.Object.DestroyImmediate(texture);
            AssetDatabase.ImportAsset(assetPath, ImportAssetOptions.ForceUpdate);
            TextureImporter importer = AssetImporter.GetAtPath(assetPath) as TextureImporter;
            if (importer != null)
            {
                importer.textureType = TextureImporterType.Sprite;
                importer.spritePixelsPerUnit = pixelsPerUnit;
                importer.alphaIsTransparency = true;
                importer.filterMode = FilterMode.Bilinear;
                importer.SaveAndReimport();
            }
            return AssetDatabase.LoadAssetAtPath<Sprite>(assetPath);
        }

        private static LaserProjectile CreateLaserPrefab(Sprite sprite)
        {
            const string path = "Assets/Prefabs/Projectiles/Laser.prefab";
            LaserProjectile existing = AssetDatabase.LoadAssetAtPath<LaserProjectile>(path);
            if (existing != null) return existing;
            GameObject root = new("Laser");
            SpriteRenderer renderer = root.AddComponent<SpriteRenderer>();
            renderer.sprite = sprite;
            renderer.color = new Color(0.3f, 0.95f, 1f, 1f);
            Rigidbody2D body = root.AddComponent<Rigidbody2D>();
            body.bodyType = RigidbodyType2D.Kinematic;
            body.gravityScale = 0f;
            BoxCollider2D collider = root.AddComponent<BoxCollider2D>();
            collider.isTrigger = true;
            collider.size = new Vector2(0.12f, 0.7f);
            LaserProjectile projectile = root.AddComponent<LaserProjectile>();
            LaserProjectile prefab = SavePrefab(root, path, projectile);
            UnityEngine.Object.DestroyImmediate(root);
            return prefab;
        }

        private static AsteroidController CreateAsteroidPrefab(Sprite sprite)
        {
            const string path = "Assets/Prefabs/Asteroids/Asteroid.prefab";
            AsteroidController existing = AssetDatabase.LoadAssetAtPath<AsteroidController>(path);
            if (existing != null) return existing;
            GameObject root = new("Asteroid");
            SpriteRenderer renderer = root.AddComponent<SpriteRenderer>();
            renderer.sprite = sprite;
            renderer.color = new Color(0.72f, 0.7f, 0.82f, 1f);
            Rigidbody2D body = root.AddComponent<Rigidbody2D>();
            body.bodyType = RigidbodyType2D.Dynamic;
            body.gravityScale = 0f;
            body.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
            CircleCollider2D collider = root.AddComponent<CircleCollider2D>();
            collider.radius = 0.68f;
            AsteroidController asteroid = root.AddComponent<AsteroidController>();
            AsteroidController prefab = SavePrefab(root, path, asteroid);
            UnityEngine.Object.DestroyImmediate(root);
            return prefab;
        }

        private static StarCollectible CreateStarPrefab(Sprite sprite)
        {
            const string path = "Assets/Prefabs/Collectibles/Star.prefab";
            StarCollectible existing = AssetDatabase.LoadAssetAtPath<StarCollectible>(path);
            if (existing != null) return existing;
            GameObject root = new("Star");
            SpriteRenderer renderer = root.AddComponent<SpriteRenderer>();
            renderer.sprite = sprite;
            renderer.color = new Color(1f, 0.88f, 0.2f, 1f);
            CircleCollider2D collider = root.AddComponent<CircleCollider2D>();
            collider.isTrigger = true;
            collider.radius = 0.45f;
            StarCollectible star = root.AddComponent<StarCollectible>();
            StarCollectible prefab = SavePrefab(root, path, star);
            UnityEngine.Object.DestroyImmediate(root);
            return prefab;
        }

        private static PlayerController CreatePlayerPrefab(Sprite sprite, LaserProjectile laserPrefab)
        {
            const string path = "Assets/Prefabs/Player/PlayerShip.prefab";
            PlayerController existing = AssetDatabase.LoadAssetAtPath<PlayerController>(path);
            if (existing != null) return existing;
            GameObject root = new("Player Ship");
            SpriteRenderer renderer = root.AddComponent<SpriteRenderer>();
            renderer.sprite = sprite;
            renderer.color = new Color(0.35f, 0.85f, 1f, 1f);
            Rigidbody2D body = root.AddComponent<Rigidbody2D>();
            body.bodyType = RigidbodyType2D.Dynamic;
            body.gravityScale = 0f;
            body.freezeRotation = true;
            body.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
            CircleCollider2D collider = root.AddComponent<CircleCollider2D>();
            collider.radius = 0.55f;
            PlayerController controller = root.AddComponent<PlayerController>();
            PlayerShooter shooter = root.AddComponent<PlayerShooter>();
            GameObject firePointObject = new("Fire Point");
            firePointObject.transform.SetParent(root.transform, false);
            firePointObject.transform.localPosition = new Vector3(0f, 0.72f, 0f);
            Transform firePoint = firePointObject.transform;

            SerializedObject serializedShooter = new(shooter);
            serializedShooter.FindProperty("laserPrefab").objectReferenceValue = laserPrefab;
            serializedShooter.FindProperty("firePoint").objectReferenceValue = firePoint;
            serializedShooter.ApplyModifiedPropertiesWithoutUndo();

            PlayerController prefab = SavePrefab(root, path, controller);
            UnityEngine.Object.DestroyImmediate(root);
            return prefab;
        }

        private static T SavePrefab<T>(GameObject root, string path, T component) where T : Component
        {
            PrefabUtility.SaveAsPrefabAsset(root, path);
            AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);
            return AssetDatabase.LoadAssetAtPath<T>(path);
        }

        private static void BuildMainMenuScene(Sprite background)
        {
            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            CreateCamera("Main Camera", new Color(0.015f, 0.02f, 0.08f, 1f));
            CreateBackground(background);
            Canvas canvas = CreateCanvas("Main Menu Canvas");
            SceneLoader loader = canvas.gameObject.AddComponent<SceneLoader>();
            MainMenuUI menu = canvas.gameObject.AddComponent<MainMenuUI>();

            CreateText(canvas.transform, "Title", "SPACE EXPLORER", 64, new Vector2(0f, 290f), new Vector2(900f, 110f), new Color(0.55f, 0.9f, 1f, 1f));
            CreateText(canvas.transform, "Subtitle", "A 2D ARCADE SPACE ADVENTURE", 22, new Vector2(0f, 215f), new Vector2(800f, 50f), new Color(0.68f, 0.72f, 0.95f, 1f));
            Button play = CreateButton(canvas.transform, "Play Button", "PLAY", new Vector2(0f, 80f));
            Button instructions = CreateButton(canvas.transform, "Instructions Button", "INSTRUCTIONS", new Vector2(0f, -20f));
            Button quit = CreateButton(canvas.transform, "Quit Button", "QUIT", new Vector2(0f, -120f));

            GameObject panel = CreatePanel(canvas.transform, "Instructions Panel", new Vector2(0f, 10f), new Vector2(820f, 430f));
            CreateText(panel.transform, "Instructions Title", "HOW TO PLAY", 38, new Vector2(0f, 130f), new Vector2(700f, 70f), Color.white);
            CreateText(panel.transform, "Instructions Body", "ARROW KEYS / WASD  -  MOVE\nSPACE  -  FIRE LASERS\n\nCollect stars for points. Avoid asteroids!", 25, new Vector2(0f, 10f), new Vector2(700f, 180f), new Color(0.85f, 0.9f, 1f, 1f));
            Button close = CreateButton(panel.transform, "Close Instructions", "CLOSE", new Vector2(0f, -140f));
            panel.SetActive(false);
            SerializedObject menuSerialized = new(menu);
            menuSerialized.FindProperty("instructionsPanel").objectReferenceValue = panel;
            menuSerialized.ApplyModifiedPropertiesWithoutUndo();

            AddPersistentListener(play, loader.LoadGameplay);
            AddPersistentListener(instructions, menu.ShowInstructions);
            AddPersistentListener(quit, loader.QuitGame);
            AddPersistentListener(close, menu.HideInstructions);
            EditorSceneManager.SaveScene(scene, MainMenuScene);
        }

        private static void BuildGameplayScene(Sprite background, PlayerController playerPrefab, AsteroidController asteroidPrefab, StarCollectible starPrefab)
        {
            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            CreateCamera("Main Camera", new Color(0.008f, 0.01f, 0.04f, 1f));
            CreateGlobalLight();
            CreateBackground(background);
            Canvas canvas = CreateCanvas("Gameplay Canvas");
            TMP_Text scoreText = CreateText(canvas.transform, "Score", "SCORE: 0", 30, new Vector2(-780f, 465f), new Vector2(430f, 70f), Color.white, TextAlignmentOptions.Left);
            CreateText(canvas.transform, "Controls", "ARROWS / WASD: MOVE     SPACE: FIRE", 20, new Vector2(0f, -495f), new Vector2(1000f, 45f), new Color(0.7f, 0.78f, 0.95f, 1f));
            ScoreManager scoreManager = canvas.gameObject.AddComponent<ScoreManager>();
            SerializedObject scoreSerialized = new(scoreManager);
            scoreSerialized.FindProperty("scoreText").objectReferenceValue = scoreText;
            scoreSerialized.ApplyModifiedPropertiesWithoutUndo();

            GameObject managerObject = new("Game Manager");
            managerObject.AddComponent<SpaceExplorerGameManager>();
            GameObject playerAsset = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Player/PlayerShip.prefab");
            if (playerAsset == null) throw new InvalidOperationException("PlayerShip prefab asset could not be loaded.");
            GameObject player = PrefabUtility.InstantiatePrefab(playerAsset) as GameObject;
            if (player == null) throw new InvalidOperationException("PlayerShip prefab could not be instantiated.");
            player.name = "Player Ship";
            player.transform.position = new Vector3(0f, -3.2f, 0f);
            GameObject asteroidSpawnerObject = new("Asteroid Spawner");
            AsteroidSpawner asteroidSpawner = asteroidSpawnerObject.AddComponent<AsteroidSpawner>();
            SerializedObject asteroidSerialized = new(asteroidSpawner);
            asteroidSerialized.FindProperty("asteroidPrefab").objectReferenceValue = AssetDatabase.LoadAssetAtPath<AsteroidController>("Assets/Prefabs/Asteroids/Asteroid.prefab");
            asteroidSerialized.ApplyModifiedPropertiesWithoutUndo();
            GameObject starSpawnerObject = new("Star Spawner");
            StarSpawner starSpawner = starSpawnerObject.AddComponent<StarSpawner>();
            SerializedObject starSerialized = new(starSpawner);
            starSerialized.FindProperty("starPrefab").objectReferenceValue = AssetDatabase.LoadAssetAtPath<StarCollectible>("Assets/Prefabs/Collectibles/Star.prefab");
            starSerialized.ApplyModifiedPropertiesWithoutUndo();
            EditorSceneManager.SaveScene(scene, GameplayScene);
        }

        private static void BuildEndGameScene(Sprite background)
        {
            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            CreateCamera("Main Camera", new Color(0.02f, 0.01f, 0.08f, 1f));
            CreateBackground(background);
            Canvas canvas = CreateCanvas("End Game Canvas");
            SceneLoader loader = canvas.gameObject.AddComponent<SceneLoader>();
            EndGameUI endGame = canvas.gameObject.AddComponent<EndGameUI>();
            CreateText(canvas.transform, "Game Over", "GAME OVER", 70, new Vector2(0f, 260f), new Vector2(900f, 110f), new Color(1f, 0.45f, 0.65f, 1f));
            TMP_Text finalScore = CreateText(canvas.transform, "Final Score", "FINAL SCORE: 0", 42, new Vector2(0f, 120f), new Vector2(700f, 80f), Color.white);
            Button mainMenu = CreateButton(canvas.transform, "Main Menu Button", "MAIN MENU", new Vector2(0f, -30f));
            Button quit = CreateButton(canvas.transform, "Quit Button", "QUIT", new Vector2(0f, -140f));
            SerializedObject endSerialized = new(endGame);
            endSerialized.FindProperty("finalScoreText").objectReferenceValue = finalScore;
            endSerialized.ApplyModifiedPropertiesWithoutUndo();
            AddPersistentListener(mainMenu, loader.LoadMainMenu);
            AddPersistentListener(quit, loader.QuitGame);
            EditorSceneManager.SaveScene(scene, EndGameScene);
        }

        private static Camera CreateCamera(string name, Color backgroundColor)
        {
            GameObject cameraObject = new(name);
            cameraObject.tag = "MainCamera";
            cameraObject.transform.position = new Vector3(0f, 0f, -10f);
            Camera camera = cameraObject.AddComponent<Camera>();
            camera.orthographic = true;
            camera.orthographicSize = 5.625f;
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = backgroundColor;
            cameraObject.AddComponent<AudioListener>();
            cameraObject.AddComponent<UniversalAdditionalCameraData>();
            return camera;
        }

        private static void CreateGlobalLight()
        {
            GameObject lightObject = new("Global Light 2D");
            Light2D light = lightObject.AddComponent<Light2D>();
            light.lightType = Light2D.LightType.Global;
            light.intensity = 1f;
        }

        private static void CreateBackground(Sprite sprite)
        {
            GameObject background = new("Space Background");
            SpriteRenderer renderer = background.AddComponent<SpriteRenderer>();
            renderer.sprite = sprite;
            renderer.sortingOrder = -100;
            background.transform.position = new Vector3(0f, 0f, 5f);
            background.transform.localScale = new Vector3(10f, 5.625f, 1f);
        }

        private static Canvas CreateCanvas(string name)
        {
            GameObject canvasObject = new(name);
            Canvas canvas = canvasObject.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            CanvasScaler scaler = canvasObject.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            scaler.matchWidthOrHeight = 0.5f;
            canvasObject.AddComponent<GraphicRaycaster>();
            GameObject eventSystem = new("EventSystem");
            eventSystem.AddComponent<EventSystem>();
            eventSystem.AddComponent<InputSystemUIInputModule>();
            return canvas;
        }

        private static TMP_Text CreateText(Transform parent, string name, string text, int fontSize, Vector2 position, Vector2 size, Color color, TextAlignmentOptions alignment = TextAlignmentOptions.Center)
        {
            GameObject textObject = new(name);
            textObject.transform.SetParent(parent, false);
            RectTransform rect = textObject.AddComponent<RectTransform>();
            SetRect(rect, position, size);
            TextMeshProUGUI textComponent = textObject.AddComponent<TextMeshProUGUI>();
            textComponent.text = text;
            textComponent.fontSize = fontSize;
            textComponent.color = color;
            textComponent.alignment = alignment;
            textComponent.textWrappingMode = TextWrappingModes.Normal;
            textComponent.font = Resources.Load<TMP_FontAsset>("Fonts & Materials/LiberationSans SDF");
            return textComponent;
        }

        private static GameObject CreatePanel(Transform parent, string name, Vector2 position, Vector2 size)
        {
            GameObject panel = new(name);
            panel.transform.SetParent(parent, false);
            RectTransform rect = panel.AddComponent<RectTransform>();
            SetRect(rect, position, size);
            Image image = panel.AddComponent<Image>();
            image.color = new Color(0.04f, 0.06f, 0.18f, 0.97f);
            return panel;
        }

        private static Button CreateButton(Transform parent, string name, string label, Vector2 position)
        {
            GameObject buttonObject = new(name);
            buttonObject.transform.SetParent(parent, false);
            RectTransform rect = buttonObject.AddComponent<RectTransform>();
            SetRect(rect, position, new Vector2(430f, 78f));
            Image image = buttonObject.AddComponent<Image>();
            image.color = new Color(0.1f, 0.25f, 0.5f, 0.95f);
            Button button = buttonObject.AddComponent<Button>();
            ColorBlock colors = button.colors;
            colors.highlightedColor = new Color(0.2f, 0.5f, 0.85f, 1f);
            colors.pressedColor = new Color(0.08f, 0.15f, 0.35f, 1f);
            button.colors = colors;
            CreateText(buttonObject.transform, "Label", label, 27, Vector2.zero, new Vector2(410f, 70f), Color.white);
            return button;
        }

        private static void SetRect(RectTransform rect, Vector2 position, Vector2 size)
        {
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = position;
            rect.sizeDelta = size;
        }

        private static void AddPersistentListener(Button button, UnityEngine.Events.UnityAction action)
        {
            UnityEventTools.AddPersistentListener(button.onClick, action);
        }

        private static void DrawBackground(Texture2D texture)
        {
            texture.SetPixel(0, 0, new Color(0.008f, 0.015f, 0.07f, 1f));
            texture.SetPixel(1, 0, new Color(0.03f, 0.01f, 0.12f, 1f));
            texture.SetPixel(0, 1, new Color(0.015f, 0.04f, 0.12f, 1f));
            texture.SetPixel(1, 1, new Color(0.05f, 0.015f, 0.14f, 1f));
        }

        private static void DrawPlayer(Texture2D texture)
        {
            int center = texture.width / 2;
            for (int y = 6; y < 88; y++)
            {
                float normalized = y / 82f;
                int halfWidth = Mathf.RoundToInt(Mathf.Lerp(8f, 31f, normalized));
                for (int x = center - halfWidth; x <= center + halfWidth; x++)
                {
                    if (x < 0 || x >= texture.width) continue;
                    Color color = y > 54 && Mathf.Abs(x - center) < 11 ? new Color(0.8f, 0.95f, 1f, 1f) : new Color(0.2f, 0.65f, 1f, 1f);
                    texture.SetPixel(x, y, color);
                }
            }
            DrawCircle(texture, center, 55, 9, new Color(0.8f, 0.95f, 1f, 1f));
        }

        private static void DrawLaser(Texture2D texture)
        {
            for (int y = 2; y < texture.height - 2; y++)
            for (int x = 3; x < texture.width - 3; x++)
                texture.SetPixel(x, y, new Color(0.3f, 0.95f, 1f, 1f));
        }

        private static void DrawAsteroid(Texture2D texture)
        {
            int center = texture.width / 2;
            for (int y = 6; y < texture.height - 6; y++)
            for (int x = 6; x < texture.width - 6; x++)
            {
                float dx = x - center;
                float dy = y - center;
                float radius = 36f + 5f * Mathf.Sin((x + y) * 0.17f);
                if (dx * dx + dy * dy <= radius * radius)
                    texture.SetPixel(x, y, new Color(0.32f + y / 600f, 0.3f + x / 700f, 0.42f, 1f));
            }
            DrawCircle(texture, 32, 59, 5, new Color(0.18f, 0.16f, 0.25f, 1f));
            DrawCircle(texture, 63, 35, 4, new Color(0.18f, 0.16f, 0.25f, 1f));
        }

        private static void DrawStar(Texture2D texture)
        {
            int center = texture.width / 2;
            for (int y = 0; y < texture.height; y++)
            for (int x = 0; x < texture.width; x++)
            {
                float dx = x - center;
                float dy = y - center;
                float angle = Mathf.Atan2(dy, dx) + Mathf.PI / 2f;
                float radius = Mathf.Sqrt(dx * dx + dy * dy);
                float sector = Mathf.Repeat(angle, Mathf.PI / 5f);
                float edge = 27f * Mathf.Cos(sector - Mathf.PI / 10f);
                if (radius < edge) texture.SetPixel(x, y, new Color(1f, 0.88f, 0.2f, 1f));
            }
        }

        private static void DrawCircle(Texture2D texture, int centerX, int centerY, int radius, Color color)
        {
            for (int y = centerY - radius; y <= centerY + radius; y++)
            for (int x = centerX - radius; x <= centerX + radius; x++)
            {
                if (x < 0 || y < 0 || x >= texture.width || y >= texture.height) continue;
                int dx = x - centerX;
                int dy = y - centerY;
                if (dx * dx + dy * dy <= radius * radius) texture.SetPixel(x, y, color);
            }
        }
    }

    [InitializeOnLoad]
    internal static class SpaceExplorerSmokeHooks
    {
        private const string RequestKey = "SpaceExplore.PlayModeSmoke.Requested";
        private static bool running;
        private static bool triggered;
        private static bool laserSpawned;
        private static bool scoreUpdated;
        private static double smokeDeadline;
        private static int smokeFrames;

        static SpaceExplorerSmokeHooks()
        {
            EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
        }

        public static void Request()
        {
            SessionState.SetBool(RequestKey, true);
            EditorApplication.isPlaying = true;
        }

        private static void OnPlayModeStateChanged(PlayModeStateChange state)
        {
            if (state == PlayModeStateChange.EnteredPlayMode && SessionState.GetBool(RequestKey, false))
            {
                SessionState.EraseBool(RequestKey);
                running = true;
                triggered = false;
                laserSpawned = false;
                scoreUpdated = false;
                smokeFrames = 0;
                EditorApplication.update += Tick;
            }
            else if (state == PlayModeStateChange.ExitingPlayMode)
            {
                running = false;
                EditorApplication.update -= Tick;
            }
        }

        private static void Tick()
        {
            if (!running || !EditorApplication.isPlaying)
            {
                EditorApplication.update -= Tick;
                return;
            }
            smokeFrames++;
            if (smokeFrames < 10) return;
            if (!triggered && SceneManager.GetActiveScene().name == "Gameplay")
            {
                SpaceExplorerGameManager manager = UnityEngine.Object.FindAnyObjectByType<SpaceExplorerGameManager>();
                PlayerShooter shooter = UnityEngine.Object.FindAnyObjectByType<PlayerShooter>();
                if (manager == null || shooter == null) return;
                shooter.Fire();
                laserSpawned = UnityEngine.Object.FindAnyObjectByType<LaserProjectile>() != null;
                foreach (AsteroidController asteroid in UnityEngine.Object.FindObjectsByType<AsteroidController>())
                {
                    Collider2D collider = asteroid.GetComponent<Collider2D>();
                    if (collider != null) collider.enabled = false;
                    asteroid.enabled = false;
                }
                manager.CollectStar();
                scoreUpdated = GameSession.CurrentScore == 10;
                manager.HandleAsteroidCollision();
                smokeDeadline = EditorApplication.timeSinceStartup + 3d;
                triggered = true;
                Debug.Log($"Space Explorer smoke setup: laserSpawned={laserSpawned}, starScore={GameSession.CurrentScore}, scoreCheck={scoreUpdated}, gameOverRequested=true");
                return;
            }
            if (triggered && SceneManager.GetActiveScene().name == "EndGame")
            {
                bool finalScoreCorrect = GameSession.FinalScore == -15;
                if (laserSpawned && scoreUpdated && finalScoreCorrect)
                    Debug.Log("Space Explorer Play Mode smoke PASS: laser, star scoring, asteroid penalty and EndGame transition verified.");
                else
                    Debug.LogError($"Space Explorer Play Mode smoke FAIL: laser={laserSpawned}, starScore={scoreUpdated}, finalScore={GameSession.FinalScore}");
                running = false;
                EditorApplication.update -= Tick;
                EditorApplication.isPlaying = false;
            }
            else if (triggered && EditorApplication.timeSinceStartup > smokeDeadline)
            {
                Debug.LogError("Space Explorer Play Mode smoke FAIL: EndGame transition timed out.");
                running = false;
                EditorApplication.update -= Tick;
                EditorApplication.isPlaying = false;
            }
        }
    }
}
#endif

#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

namespace SpaceExplore.Editor
{
    public static class SurvivalUpgradeBuilder
    {
        private const string GameplayPath = "Assets/Scenes/Gameplay.unity";
        private const string MainMenuPath = "Assets/Scenes/MainMenu.unity";
        private const string EndGamePath = "Assets/Scenes/EndGame.unity";
        private const string GiftPath = "Assets/Prefabs/Collectibles/GiftBox.prefab";

        [MenuItem("Tools/Space Explorer/Build Survival Upgrade")]
        public static void BuildSurvivalUpgrade()
        {
            AssetDatabase.Refresh();
            GiftBox giftPrefab = CreateGiftPrefab();
            UpgradeGameplay(giftPrefab);
            UpgradeMainMenu();
            UpgradeEndGame();
            EditorBuildSettings.scenes = new[]
            {
                new EditorBuildSettingsScene(MainMenuPath, true),
                new EditorBuildSettingsScene(GameplayPath, true),
                new EditorBuildSettingsScene(EndGamePath, true)
            };
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            EditorSceneManager.OpenScene(MainMenuPath, OpenSceneMode.Single);
            Debug.Log("Space Explorer Survival upgrade applied: asteroid variants, gifts, difficulty, HP, HUD and regression hooks are connected.");
        }

        private static GiftBox CreateGiftPrefab()
        {
            GameObject existingAsset = AssetDatabase.LoadAssetAtPath<GameObject>(GiftPath);
            GiftBox existing = existingAsset != null ? existingAsset.GetComponent<GiftBox>() : null;
            if (existing != null) return existing;
            Sprite sprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/star.png");
            GameObject root = new("Gift Box");
            SpriteRenderer renderer = root.AddComponent<SpriteRenderer>();
            renderer.sprite = sprite;
            renderer.color = new Color(.2f, 1f, .35f);
            renderer.sortingOrder = 3;
            CircleCollider2D collider = root.AddComponent<CircleCollider2D>();
            collider.isTrigger = true;
            GiftBox gift = root.AddComponent<GiftBox>();
            PrefabUtility.SaveAsPrefabAsset(root, GiftPath);
            Object.DestroyImmediate(root);
            AssetDatabase.ImportAsset(GiftPath, ImportAssetOptions.ForceUpdate);
            GameObject asset = AssetDatabase.LoadAssetAtPath<GameObject>(GiftPath);
            return asset != null ? asset.GetComponent<GiftBox>() : null;
        }

        private static void UpgradeGameplay(GiftBox giftPrefab)
        {
            Scene scene = EditorSceneManager.OpenScene(GameplayPath, OpenSceneMode.Single);
            GameObject managerObject = FindSceneObject(scene, "Game Manager");
            if (managerObject == null) managerObject = new GameObject("Game Manager");
            DifficultyManager difficulty = GetOrAdd<DifficultyManager>(managerObject);
            difficulty.EnsureDefaults();

            AsteroidSpawner asteroidSpawner = GetOrAdd<AsteroidSpawner>(FindSceneObject(scene, "Asteroid Spawner"));
            SerializedObject asteroidSerialized = new(asteroidSpawner);
            asteroidSerialized.FindProperty("difficultyManager").objectReferenceValue = difficulty;
            asteroidSerialized.FindProperty("asteroidPrefab").objectReferenceValue = AssetDatabase.LoadAssetAtPath<AsteroidController>("Assets/Prefabs/Asteroids/Asteroid.prefab");
            WriteVariants(asteroidSerialized.FindProperty("asteroidVariants"));
            asteroidSerialized.FindProperty("initialCount").intValue = 8;
            asteroidSerialized.ApplyModifiedPropertiesWithoutUndo();

            GameObject giftObject = FindSceneObject(scene, "Gift Spawner");
            if (giftObject == null) giftObject = new GameObject("Gift Spawner");
            GiftSpawner giftSpawner = GetOrAdd<GiftSpawner>(giftObject);
            SerializedObject giftSerialized = new(giftSpawner);
            giftSerialized.FindProperty("giftPrefab").objectReferenceValue = giftPrefab;
            giftSerialized.FindProperty("giftPrefabAsset").objectReferenceValue = AssetDatabase.LoadAssetAtPath<GameObject>(GiftPath);
            giftSerialized.FindProperty("fallbackSprite").objectReferenceValue = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/star.png");
            giftSerialized.ApplyModifiedPropertiesWithoutUndo();

            Canvas canvas = FindSceneObject(scene, "Gameplay Canvas")?.GetComponent<Canvas>();
            if (canvas != null) ConfigureHud(canvas);
            EditorSceneManager.SaveScene(scene);
        }

        private static void ConfigureHud(Canvas canvas)
        {
            CreateHudPanel(canvas.transform, "HUD Panel Left", new Vector2(-760f, 435f), new Vector2(390f, 145f));
            CreateHudPanel(canvas.transform, "HUD Panel Right", new Vector2(760f, 435f), new Vector2(390f, 145f));
            CreateHudPanel(canvas.transform, "HUD Panel Center", new Vector2(0f, 435f), new Vector2(420f, 145f));
            TMP_Text score = FindOrCreateText(canvas.transform, "Score", "SCORE  0", new Vector2(-760f, 465f), new Vector2(330f, 50f), 28, TextAlignmentOptions.Left);
            TMP_Text survival = FindOrCreateText(canvas.transform, "Survival Time", "SURVIVAL  00:00", new Vector2(760f, 465f), new Vector2(330f, 50f), 28, TextAlignmentOptions.Right);
            TMP_Text hp = FindOrCreateText(canvas.transform, "HP", "HP  3/3", new Vector2(-760f, 415f), new Vector2(300f, 45f), 24, TextAlignmentOptions.Left);
            TMP_Text high = FindOrCreateText(canvas.transform, "High Score", "HIGH  0", new Vector2(760f, 415f), new Vector2(300f, 45f), 24, TextAlignmentOptions.Right);
            TMP_Text stage = FindOrCreateText(canvas.transform, "Difficulty Stage", "1 / 6", new Vector2(0f, 465f), new Vector2(260f, 50f), 27, TextAlignmentOptions.Center);
            TMP_Text power = FindOrCreateText(canvas.transform, "Power Up", "POWER  —", new Vector2(0f, 415f), new Vector2(380f, 45f), 22, TextAlignmentOptions.Center);
            ScoreManager scoreManager = GetOrAdd<ScoreManager>(canvas.gameObject);
            SerializedObject serialized = new(scoreManager);
            serialized.FindProperty("scoreText").objectReferenceValue = score;
            serialized.FindProperty("survivalTimeText").objectReferenceValue = survival;
            serialized.FindProperty("hpText").objectReferenceValue = hp;
            serialized.FindProperty("stageText").objectReferenceValue = stage;
            serialized.FindProperty("highScoreText").objectReferenceValue = high;
            serialized.FindProperty("powerUpText").objectReferenceValue = power;
            serialized.ApplyModifiedPropertiesWithoutUndo();
            TextMeshProUGUI controls = FindSceneObject(canvas.gameObject.scene, "Controls")?.GetComponent<TextMeshProUGUI>();
            if (controls != null) controls.text = "ARROWS / WASD: MOVE     SPACE: FIRE     SURVIVE, SHOOT, COLLECT";
        }

        private static void UpgradeMainMenu()
        {
            Scene scene = EditorSceneManager.OpenScene(MainMenuPath, OpenSceneMode.Single);
            TMP_Text body = FindSceneObject(scene, "Instructions Body")?.GetComponent<TMP_Text>();
            if (body != null) body.text = "ARROW KEYS / WASD  -  MOVE\nSPACE  -  FIRE LASERS\n\nDestroy six asteroid types, collect gifts and survive as long as possible!";
            EditorSceneManager.SaveScene(scene);
        }

        private static void UpgradeEndGame()
        {
            Scene scene = EditorSceneManager.OpenScene(EndGamePath, OpenSceneMode.Single);
            GameObject canvasObject = FindSceneObject(scene, "End Game Canvas");
            if (canvasObject != null)
            {
                Canvas canvas = canvasObject.GetComponent<Canvas>();
                TMP_Text high = FindOrCreateText(canvas.transform, "High Score", "HIGH SCORE: 0", new Vector2(0f, 45f), new Vector2(700f, 70f), 30, TextAlignmentOptions.Center);
                EndGameUI endGame = GetOrAdd<EndGameUI>(canvasObject);
                SerializedObject serialized = new(endGame);
                serialized.FindProperty("highScoreText").objectReferenceValue = high;
                serialized.ApplyModifiedPropertiesWithoutUndo();
            }
            EditorSceneManager.SaveScene(scene);
        }

        private static void WriteVariants(SerializedProperty array)
        {
            array.arraySize = 6;
            AsteroidVariant[] values =
            {
                new() { type = AsteroidType.Small, maxHealth = 1, score = 5, size = .55f, minimumSpeed = 1.5f, maximumSpeed = 2.6f, tint = new Color(.65f,.7f,.82f) },
                new() { type = AsteroidType.Medium, maxHealth = 2, score = 10, size = .8f, minimumSpeed = 1.25f, maximumSpeed = 2.2f, tint = new Color(.55f,.6f,.75f) },
                new() { type = AsteroidType.Large, maxHealth = 5, score = 25, size = 1.2f, minimumSpeed = .8f, maximumSpeed = 1.45f, tint = new Color(.48f,.5f,.66f) },
                new() { type = AsteroidType.Fast, maxHealth = 1, score = 15, size = .6f, minimumSpeed = 3.1f, maximumSpeed = 4.4f, tint = new Color(1f,.5f,.28f) },
                new() { type = AsteroidType.Armored, maxHealth = 8, score = 40, size = 1.05f, minimumSpeed = .7f, maximumSpeed = 1.15f, tint = new Color(.42f,.55f,.72f) },
                new() { type = AsteroidType.Special, maxHealth = 12, score = 75, size = 1.25f, minimumSpeed = 1.2f, maximumSpeed = 2f, tint = new Color(.85f,.25f,1f), directionChangeInterval = 1.2f }
            };
            for (int i = 0; i < values.Length; i++)
            {
                SerializedProperty item = array.GetArrayElementAtIndex(i);
                item.FindPropertyRelative("type").enumValueIndex = (int)values[i].type;
                item.FindPropertyRelative("maxHealth").intValue = values[i].maxHealth;
                item.FindPropertyRelative("score").intValue = values[i].score;
                item.FindPropertyRelative("size").floatValue = values[i].size;
                item.FindPropertyRelative("minimumSpeed").floatValue = values[i].minimumSpeed;
                item.FindPropertyRelative("maximumSpeed").floatValue = values[i].maximumSpeed;
                item.FindPropertyRelative("tint").colorValue = values[i].tint;
                item.FindPropertyRelative("directionChangeInterval").floatValue = values[i].directionChangeInterval;
                item.FindPropertyRelative("sprite").objectReferenceValue = AssetDatabase.LoadAssetAtPath<Sprite>($"Assets/Sprites/Generated/asteroid_{i}.png");
            }
        }

        private static T GetOrAdd<T>(GameObject gameObject) where T : Component
        {
            if (gameObject == null) return null;
            T component = gameObject.GetComponent<T>();
            return component != null ? component : gameObject.AddComponent<T>();
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

        private static TMP_Text FindOrCreateText(Transform parent, string name, string text, Vector2 position, Vector2 size, int fontSize, TextAlignmentOptions alignment)
        {
            Transform existing = FindChild(parent, name);
            TextMeshProUGUI component = existing != null ? existing.GetComponent<TextMeshProUGUI>() : null;
            GameObject textObject = component != null ? component.gameObject : new GameObject(name);
            if (component == null) textObject.transform.SetParent(parent, false);
            RectTransform rect = component != null ? component.rectTransform : textObject.AddComponent<RectTransform>();
            rect.anchorMin = rect.anchorMax = new Vector2(.5f, .5f);
            rect.pivot = new Vector2(.5f, .5f);
            rect.anchoredPosition = position;
            rect.sizeDelta = size;
            component ??= textObject.AddComponent<TextMeshProUGUI>();
            component.text = text;
            component.fontSize = fontSize;
            component.color = Color.white;
            component.alignment = alignment;
            component.font = Resources.Load<TMP_FontAsset>("Fonts & Materials/LiberationSans SDF");
            return component;
        }

        private static void CreateHudPanel(Transform parent, string name, Vector2 position, Vector2 size)
        {
            Transform existing = FindChild(parent, name);
            if (existing != null && existing.GetComponent<RectTransform>() == null)
            {
                Object.DestroyImmediate(existing.gameObject);
                existing = null;
            }
            GameObject panel = existing != null ? existing.gameObject : new GameObject(name, typeof(RectTransform), typeof(Image));
            if (existing == null) panel.transform.SetParent(parent, false);
            RectTransform rect = panel.GetComponent<RectTransform>();
            rect.anchorMin = rect.anchorMax = new Vector2(.5f, .5f);
            rect.pivot = new Vector2(.5f, .5f);
            rect.anchoredPosition = position;
            rect.sizeDelta = size;
            Image image = panel.GetComponent<Image>() ?? panel.AddComponent<Image>();
            image.color = new Color(.015f, .07f, .16f, .72f);
            panel.transform.SetAsFirstSibling();
        }
    }
}
#endif

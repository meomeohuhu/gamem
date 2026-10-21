#if UNITY_EDITOR
using System;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace SpaceExplore.Editor
{
    public static class UserUpgradeBuilder
    {
        private const string GameplayScene = "Assets/Scenes/Gameplay.unity";
        private const string MainMenuScene = "Assets/Scenes/MainMenu.unity";
        private const string PlayerPrefab = "Assets/Prefabs/Player/PlayerShip.prefab";
        private const string MainBackground = "Assets/Sprites/Imported/AnimePack/anime_space_background_01.png";
        private const string CombatOverlay = "Assets/Sprites/Imported/AnimePack/anime_space_background_02.png";
        private const string NewPlayerSprite = "Assets/Sprites/Imported/AnimePack/anime_spaceship_pincher.png";
        private const string StarCrystal = "Assets/Sprites/Generated/star_crystal.png";
        private const string UpgradePrefabPath = "Assets/Prefabs/Collectibles/WeaponUpgrade.prefab";

        [MenuItem("Tools/Space Explorer/Apply User Assets and Weapon Upgrades")]
        public static void ApplyAll()
        {
            AssetDatabase.Refresh();
            ConfigureTexture(MainBackground, 100f, false);
            ConfigureTexture(CombatOverlay, 100f, false);
            ConfigureTexture(NewPlayerSprite, 64f, true);
            WeaponUpgradePickup upgradePrefab = CreateUpgradePrefab();
            ConfigurePlayerPrefab();
            ConfigureGameplayScene(upgradePrefab);
            ConfigureMainMenuScene();
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("Space Explorer user assets and weapon upgrades applied.");
        }

        [MenuItem("Tools/Space Explorer/Run Weapon Upgrade Smoke")]
        public static void RunWeaponUpgradeSmoke()
        {
            if (!Application.isPlaying)
            {
                Debug.LogError("Weapon upgrade smoke requires Gameplay to be in Play Mode.");
                return;
            }

            PlayerShooter shooter = UnityEngine.Object.FindAnyObjectByType<PlayerShooter>();
            if (shooter == null)
            {
                Debug.LogError("Weapon upgrade smoke failed: PlayerShooter not found.");
                return;
            }

            bool passed = true;
            shooter.ResetWeaponUpgrades();
            float normalCooldown = shooter.CurrentFireCooldown;
            shooter.ActivateWeaponUpgrade(WeaponUpgradeType.RapidFire, 30f);
            passed &= shooter.RapidFireActive && shooter.CurrentFireCooldown < normalCooldown;
            shooter.ResetWeaponUpgrades();

            int before = UnityEngine.Object.FindObjectsByType<LaserProjectile>().Length;
            shooter.ActivateWeaponUpgrade(WeaponUpgradeType.DoubleShot, 30f);
            shooter.Fire();
            int doubleCount = UnityEngine.Object.FindObjectsByType<LaserProjectile>().Length - before;
            passed &= doubleCount == 2;

            before = UnityEngine.Object.FindObjectsByType<LaserProjectile>().Length;
            shooter.ResetWeaponUpgrades();
            shooter.ActivateWeaponUpgrade(WeaponUpgradeType.SpreadShot, 30f);
            shooter.Fire();
            int spreadCount = UnityEngine.Object.FindObjectsByType<LaserProjectile>().Length - before;
            passed &= spreadCount == 3;

            shooter.ResetWeaponUpgrades();
            shooter.ActivateWeaponUpgrade(WeaponUpgradeType.PiercingShot, 30f);
            shooter.ActivateWeaponUpgrade(WeaponUpgradeType.DamageUp, 30f);
            shooter.Fire();
            LaserProjectile[] lasers = UnityEngine.Object.FindObjectsByType<LaserProjectile>();
            bool foundEnhancedLaser = false;
            foreach (LaserProjectile laser in lasers)
                if (laser.IsPiercing && laser.DamageValue >= 2) foundEnhancedLaser = true;
            passed &= foundEnhancedLaser;

            foreach (LaserProjectile laser in lasers) UnityEngine.Object.Destroy(laser.gameObject);
            shooter.ResetWeaponUpgrades();
            if (passed)
                Debug.Log($"Weapon upgrade smoke PASS: rapid cooldown, double={doubleCount}, spread={spreadCount}, piercing+damage verified.");
            else
                Debug.LogError($"Weapon upgrade smoke FAIL: rapid={shooter.RapidFireActive}, double={doubleCount}, spread={spreadCount}.");
        }

        private static void ConfigureTexture(string path, float pixelsPerUnit, bool preserveTransparency)
        {
            TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;
            if (importer == null) return;
            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Single;
            importer.spritePixelsPerUnit = pixelsPerUnit;
            importer.filterMode = FilterMode.Bilinear;
            importer.wrapMode = TextureWrapMode.Clamp;
            importer.alphaIsTransparency = preserveTransparency;
            importer.mipmapEnabled = false;
            importer.SaveAndReimport();
        }

        private static WeaponUpgradePickup CreateUpgradePrefab()
        {
            EnsureFolder("Assets/Prefabs/Collectibles");
            GameObject existing = AssetDatabase.LoadAssetAtPath<GameObject>(UpgradePrefabPath);
            if (existing != null) return existing.GetComponent<WeaponUpgradePickup>();

            GameObject root = new("Weapon Upgrade");
            root.SetActive(false);
            SpriteRenderer renderer = root.AddComponent<SpriteRenderer>();
            renderer.sprite = LoadSprite(StarCrystal);
            renderer.sortingOrder = 4;
            CircleCollider2D collider = root.AddComponent<CircleCollider2D>();
            collider.isTrigger = true;
            collider.radius = .52f;
            root.AddComponent<WeaponUpgradePickup>();
            GameObject saved = PrefabUtility.SaveAsPrefabAsset(root, UpgradePrefabPath);
            UnityEngine.Object.DestroyImmediate(root);
            return saved != null ? saved.GetComponent<WeaponUpgradePickup>() : null;
        }

        private static void ConfigurePlayerPrefab()
        {
            GameObject root = PrefabUtility.LoadPrefabContents(PlayerPrefab);
            if (root == null) return;
            SpriteRenderer renderer = root.GetComponent<SpriteRenderer>();
            if (renderer != null)
            {
                renderer.sprite = LoadSprite(NewPlayerSprite);
                renderer.sortingOrder = 5;
            }
            CircleCollider2D collider = root.GetComponent<CircleCollider2D>();
            if (collider != null)
            {
                collider.radius = .48f;
                collider.offset = new Vector2(0f, .03f);
            }
            Transform firePoint = root.transform.Find("Fire Point");
            if (firePoint != null) firePoint.localPosition = new Vector3(0f, 1.45f, 0f);
            PrefabUtility.SaveAsPrefabAsset(root, PlayerPrefab);
            PrefabUtility.UnloadPrefabContents(root);
        }

        private static void ConfigureGameplayScene(WeaponUpgradePickup upgradePrefab)
        {
            Scene scene = EditorSceneManager.OpenScene(GameplayScene, OpenSceneMode.Single);
            GameObject background = FindSceneObject(scene, "Space Background");
            if (background != null)
            {
                Transform transform = background.transform;
                transform.localScale = Vector3.one * .82f;
                transform.position = new Vector3(0f, 0f, 5f);
                SpriteRenderer renderer = background.GetComponent<SpriteRenderer>();
                if (renderer != null)
                {
                    renderer.sprite = LoadSprite(MainBackground);
                    renderer.color = Color.white;
                    renderer.sortingOrder = -100;
                }

                SpaceBackgroundController controller = background.GetComponent<SpaceBackgroundController>();
                if (controller != null)
                {
                    SerializedObject serialized = new(controller);
                    serialized.FindProperty("dustSprite").objectReferenceValue = LoadSprite("Assets/Sprites/Generated/space_dust.png");
                    serialized.FindProperty("starSprite").objectReferenceValue = LoadSprite("Assets/Sprites/Generated/star_crystal.png");
                    serialized.ApplyModifiedPropertiesWithoutUndo();
                }
            }

            GameObject overlay = FindSceneObject(scene, "Space Combat Overlay");
            if (overlay == null)
            {
                overlay = new GameObject("Space Combat Overlay");
                SceneManager.MoveGameObjectToScene(overlay, scene);
            }
            overlay.transform.position = new Vector3(0f, 0f, 4.8f);
            overlay.transform.localScale = Vector3.one * 4.65f;
            SpriteRenderer overlayRenderer = overlay.GetComponent<SpriteRenderer>();
            if (overlayRenderer == null) overlayRenderer = overlay.AddComponent<SpriteRenderer>();
            overlayRenderer.sprite = LoadSprite(CombatOverlay);
            overlayRenderer.color = new Color(1f, 1f, 1f, .08f);
            overlayRenderer.sortingOrder = -95;

            GameObject player = FindSceneObject(scene, "Player Ship");
            if (player != null)
            {
                SpriteRenderer renderer = player.GetComponent<SpriteRenderer>();
                if (renderer != null) renderer.sprite = LoadSprite(NewPlayerSprite);
                CircleCollider2D collider = player.GetComponent<CircleCollider2D>();
                if (collider != null)
                {
                    collider.radius = .48f;
                    collider.offset = new Vector2(0f, .03f);
                }
                Transform firePoint = player.transform.Find("Fire Point");
                if (firePoint != null) firePoint.localPosition = new Vector3(0f, 1.45f, 0f);
            }

            GameObject spawnerObject = FindSceneObject(scene, "Weapon Upgrade Spawner");
            if (spawnerObject == null)
            {
                spawnerObject = new GameObject("Weapon Upgrade Spawner");
                SceneManager.MoveGameObjectToScene(spawnerObject, scene);
            }
            WeaponUpgradeSpawner spawner = spawnerObject.GetComponent<WeaponUpgradeSpawner>() ?? spawnerObject.AddComponent<WeaponUpgradeSpawner>();
            SerializedObject spawnerSerialized = new(spawner);
            spawnerSerialized.FindProperty("upgradePrefab").objectReferenceValue = upgradePrefab;
            spawnerSerialized.FindProperty("upgradePrefabAsset").objectReferenceValue = AssetDatabase.LoadAssetAtPath<GameObject>(UpgradePrefabPath);
            spawnerSerialized.FindProperty("fallbackSprite").objectReferenceValue = LoadSprite(StarCrystal);
            spawnerSerialized.FindProperty("maximumActive").intValue = 2;
            spawnerSerialized.FindProperty("spawnInterval").floatValue = 15f;
            spawnerSerialized.FindProperty("firstSpawnDelay").floatValue = 6f;
            spawnerSerialized.ApplyModifiedPropertiesWithoutUndo();

            EditorSceneManager.SaveScene(scene);
        }

        private static void ConfigureMainMenuScene()
        {
            if (AssetDatabase.LoadAssetAtPath<SceneAsset>(MainMenuScene) == null) return;
            Scene scene = EditorSceneManager.OpenScene(MainMenuScene, OpenSceneMode.Single);
            GameObject background = FindSceneObject(scene, "Space Background");
            if (background != null)
            {
                background.transform.localScale = Vector3.one * .82f;
                SpriteRenderer renderer = background.GetComponent<SpriteRenderer>();
                if (renderer != null)
                {
                    renderer.sprite = LoadSprite(MainBackground);
                    renderer.color = Color.white;
                    renderer.sortingOrder = -100;
                }
            }
            EditorSceneManager.SaveScene(scene);
        }

        private static Sprite LoadSprite(string path) => AssetDatabase.LoadAssetAtPath<Sprite>(path);

        private static void EnsureFolder(string path)
        {
            if (AssetDatabase.IsValidFolder(path)) return;
            string parent = System.IO.Path.GetDirectoryName(path)?.Replace('\\', '/');
            string name = System.IO.Path.GetFileName(path);
            if (!string.IsNullOrEmpty(parent) && !AssetDatabase.IsValidFolder(parent)) EnsureFolder(parent);
            AssetDatabase.CreateFolder(parent ?? "Assets", name);
        }

        private static GameObject FindSceneObject(Scene scene, string name)
        {
            foreach (GameObject root in scene.GetRootGameObjects())
            {
                Transform found = FindChild(root.transform, name);
                if (found != null) return found.gameObject;
            }
            return null;
        }

        private static Transform FindChild(Transform parent, string name)
        {
            if (parent.name == name) return parent;
            foreach (Transform child in parent)
            {
                Transform found = FindChild(child, name);
                if (found != null) return found;
            }
            return null;
        }
    }
}
#endif

#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace SpaceExplore.Editor
{
    public static class VisualUpgradeBuilder
    {
        private const string SpriteFolder = "Assets/Sprites/Generated";
        private const string PlayerSprite = SpriteFolder + "/player_ship_sci_fi.png";
        private const string LaserSprite = SpriteFolder + "/laser_glow.png";
        private const string StarSprite = SpriteFolder + "/star_crystal.png";
        private const string BackgroundSprite = SpriteFolder + "/space_nebula.png";
        private const string DustSprite = SpriteFolder + "/space_dust.png";
        private const string ShockwaveSprite = SpriteFolder + "/shockwave_ring.png";

        [MenuItem("Tools/Space Explorer/Build Visual Upgrade")]
        public static void BuildVisualUpgrade()
        {
            EnsureFolder();
            GenerateAssets();
            AssetDatabase.Refresh();
            ConfigurePrefabs();
            SurvivalUpgradeBuilder.BuildSurvivalUpgrade();
            ConfigureGameplayVisuals();
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            EditorSceneManager.OpenScene("Assets/Scenes/MainMenu.unity", OpenSceneMode.Single);
            Debug.Log("Space Explorer visual upgrade applied: procedural sci-fi sprites, nebula background, parallax stars and HUD safe margins connected.");
        }

        private static void EnsureFolder()
        {
            if (!AssetDatabase.IsValidFolder("Assets/Sprites/Generated")) AssetDatabase.CreateFolder("Assets/Sprites", "Generated");
        }

        private static void GenerateAssets()
        {
            SaveSprite(PlayerSprite, DrawPlayer, 128, 128, 64f);
            SaveSprite(LaserSprite, DrawLaser, 32, 96, 64f);
            SaveSprite(StarSprite, DrawStar, 96, 96, 64f);
            SaveSprite(BackgroundSprite, DrawBackground, 512, 512, 256f);
            SaveSprite(DustSprite, DrawDust, 32, 32, 32f);
            SaveSprite(ShockwaveSprite, DrawShockwave, 128, 128, 64f);
            for (int i = 0; i < 6; i++) SaveSprite($"{SpriteFolder}/asteroid_{i}.png", texture => DrawAsteroid(texture, i), 128, 128, 64f);
            for (int i = 0; i < 8; i++) SaveSprite($"{SpriteFolder}/gift_{i}.png", texture => DrawGift(texture, i), 96, 96, 64f);
        }

        private static void ConfigurePrefabs()
        {
            ConfigurePrefab("Assets/Prefabs/Player/PlayerShip.prefab", root =>
            {
                SpriteRenderer renderer = root.GetComponent<SpriteRenderer>();
                renderer.sprite = LoadSprite(PlayerSprite);
                renderer.color = Color.white;
                renderer.sortingOrder = 5;
                PlayerThrustVfx thrust = root.GetComponent<PlayerThrustVfx>() ?? root.AddComponent<PlayerThrustVfx>();
                SerializedObject serialized = new(thrust);
                serialized.FindProperty("flameSprite").objectReferenceValue = LoadSprite(LaserSprite);
                serialized.ApplyModifiedPropertiesWithoutUndo();
            });
            ConfigurePrefab("Assets/Prefabs/Projectiles/Laser.prefab", root =>
            {
                SpriteRenderer renderer = root.GetComponent<SpriteRenderer>();
                renderer.sprite = LoadSprite(LaserSprite);
                renderer.color = Color.white;
                renderer.sortingOrder = 7;
            });
            ConfigurePrefab("Assets/Prefabs/Asteroids/Asteroid.prefab", root =>
            {
                SpriteRenderer renderer = root.GetComponent<SpriteRenderer>();
                renderer.sprite = LoadSprite($"{SpriteFolder}/asteroid_0.png");
                renderer.color = Color.white;
                renderer.sortingOrder = 2;
                AsteroidController controller = root.GetComponent<AsteroidController>();
                SerializedObject serialized = new(controller);
                SerializedProperty variant = serialized.FindProperty("defaultVariant");
                variant.FindPropertyRelative("sprite").objectReferenceValue = LoadSprite($"{SpriteFolder}/asteroid_0.png");
                serialized.ApplyModifiedPropertiesWithoutUndo();
            });
            ConfigurePrefab("Assets/Prefabs/Collectibles/Star.prefab", root =>
            {
                SpriteRenderer renderer = root.GetComponent<SpriteRenderer>();
                renderer.sprite = LoadSprite(StarSprite);
                renderer.color = Color.white;
                renderer.sortingOrder = 4;
            });
            ConfigurePrefab("Assets/Prefabs/Collectibles/GiftBox.prefab", root =>
            {
                SpriteRenderer renderer = root.GetComponent<SpriteRenderer>();
                renderer.sprite = LoadSprite($"{SpriteFolder}/gift_0.png");
                renderer.color = Color.white;
                renderer.sortingOrder = 4;
                GiftBox gift = root.GetComponent<GiftBox>();
                SerializedObject serialized = new(gift);
                SerializedProperty icons = serialized.FindProperty("icons");
                icons.arraySize = 8;
                for (int i = 0; i < 8; i++) icons.GetArrayElementAtIndex(i).objectReferenceValue = LoadSprite($"{SpriteFolder}/gift_{i}.png");
                serialized.ApplyModifiedPropertiesWithoutUndo();
            });
        }

        private static void ConfigureGameplayVisuals()
        {
            Scene scene = EditorSceneManager.OpenScene("Assets/Scenes/Gameplay.unity", OpenSceneMode.Single);
            GameObject background = FindSceneObject(scene, "Space Background");
            if (background != null)
            {
                SpriteRenderer renderer = background.GetComponent<SpriteRenderer>();
                renderer.sprite = LoadSprite(BackgroundSprite);
                renderer.color = Color.white;
                SpaceBackgroundController controller = background.GetComponent<SpaceBackgroundController>() ?? background.AddComponent<SpaceBackgroundController>();
                SerializedObject serialized = new(controller);
                serialized.FindProperty("dustSprite").objectReferenceValue = LoadSprite(DustSprite);
                serialized.FindProperty("starSprite").objectReferenceValue = LoadSprite(StarSprite);
                serialized.ApplyModifiedPropertiesWithoutUndo();
            }
            Camera camera = FindSceneObject(scene, "Main Camera")?.GetComponent<Camera>();
            if (camera != null)
            {
                camera.backgroundColor = new Color(.004f, .006f, .025f, 1f);
                if (camera.GetComponent<CameraShake>() == null) camera.gameObject.AddComponent<CameraShake>();
            }
            GameObject managerObject = FindSceneObject(scene, "Game Manager");
            SpaceExplorerGameManager manager = managerObject != null ? managerObject.GetComponent<SpaceExplorerGameManager>() : null;
            if (manager != null)
            {
                SerializedObject serialized = new(manager);
                serialized.FindProperty("shockwaveSprite").objectReferenceValue = LoadSprite(ShockwaveSprite);
                serialized.ApplyModifiedPropertiesWithoutUndo();
            }
            EditorSceneManager.SaveScene(scene);
        }

        private static void ConfigurePrefab(string path, Action<GameObject> configure)
        {
            GameObject root = PrefabUtility.LoadPrefabContents(path);
            if (root == null) return;
            configure(root);
            PrefabUtility.SaveAsPrefabAsset(root, path);
            PrefabUtility.UnloadPrefabContents(root);
        }

        private static Sprite LoadSprite(string path) => AssetDatabase.LoadAssetAtPath<Sprite>(path);

        private static void SaveSprite(string path, Action<Texture2D> draw, int width, int height, float pixelsPerUnit)
        {
            Texture2D texture = NewTexture(width, height);
            draw(texture);
            string absolute = Path.Combine(Application.dataPath, path.Substring("Assets/".Length).Replace('/', Path.DirectorySeparatorChar));
            File.WriteAllBytes(absolute, texture.EncodeToPNG());
            UnityEngine.Object.DestroyImmediate(texture);
            AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);
            TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;
            if (importer == null) return;
            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Single;
            importer.alphaIsTransparency = true;
            importer.filterMode = FilterMode.Bilinear;
            importer.wrapMode = TextureWrapMode.Clamp;
            importer.spritePixelsPerUnit = pixelsPerUnit;
            importer.SaveAndReimport();
        }

        private static Texture2D NewTexture(int width, int height)
        {
            Texture2D texture = new(width, height, TextureFormat.RGBA32, false);
            Color[] pixels = new Color[width * height];
            for (int i = 0; i < pixels.Length; i++) pixels[i] = Color.clear;
            texture.SetPixels(pixels);
            return texture;
        }

        private static void DrawPlayer(Texture2D t)
        {
            DrawPolygon(t, new[] { new Vector2(64, 8), new Vector2(84, 46), new Vector2(86, 92), new Vector2(64, 121), new Vector2(42, 92), new Vector2(44, 46) }, new Color(.08f, .55f, .9f));
            DrawPolygon(t, new[] { new Vector2(45, 49), new Vector2(8, 83), new Vector2(31, 78), new Vector2(48, 65) }, new Color(.12f, .78f, .95f));
            DrawPolygon(t, new[] { new Vector2(83, 49), new Vector2(120, 83), new Vector2(97, 78), new Vector2(80, 65) }, new Color(.12f, .78f, .95f));
            DrawEllipse(t, 64, 57, 15, 23, new Color(.72f, .92f, 1f));
            DrawEllipse(t, 64, 58, 10, 16, new Color(.05f, .18f, .38f));
            DrawCircle(t, 51, 103, 7, new Color(1f, .42f, .12f, .9f));
            DrawCircle(t, 77, 103, 7, new Color(1f, .42f, .12f, .9f));
            DrawLine(t, new Vector2(52, 104), new Vector2(52, 125), new Color(1f, .7f, .25f, .55f), 4);
            DrawLine(t, new Vector2(76, 104), new Vector2(76, 125), new Color(1f, .7f, .25f, .55f), 4);
            DrawLine(t, new Vector2(64, 15), new Vector2(64, 40), new Color(.75f, 1f, 1f), 3);
        }

        private static void DrawLaser(Texture2D t)
        {
            DrawEllipse(t, 16, 48, 7, 45, new Color(.1f, .7f, 1f, .22f));
            DrawEllipse(t, 16, 48, 3, 44, new Color(.75f, 1f, 1f));
            DrawCircle(t, 16, 8, 5, new Color(.2f, .85f, 1f, .7f));
        }

        private static void DrawStar(Texture2D t)
        {
            DrawCircle(t, 48, 48, 34, new Color(.2f, .75f, 1f, .16f));
            Vector2[] points = new Vector2[10];
            for (int i = 0; i < 10; i++)
            {
                float angle = Mathf.PI / 2f + i * Mathf.PI / 5f;
                float radius = i % 2 == 0 ? 39f : 17f;
                points[i] = new Vector2(48 + Mathf.Cos(angle) * radius, 48 + Mathf.Sin(angle) * radius);
            }
            DrawPolygon(t, points, new Color(1f, .88f, .25f));
            DrawCircle(t, 48, 48, 7, new Color(1f, 1f, .8f));
        }

        private static void DrawBackground(Texture2D t)
        {
            for (int y = 0; y < t.height; y++)
            for (int x = 0; x < t.width; x++)
            {
                float nx = x / (float)t.width - .5f;
                float ny = y / (float)t.height - .5f;
                float nebula = Mathf.Exp(-((nx + .2f) * (nx + .2f) * 7f + (ny - .15f) * (ny - .15f) * 10f));
                float violet = Mathf.Exp(-((nx - .25f) * (nx - .25f) * 14f + (ny + .2f) * (ny + .2f) * 8f));
                Put(t, x, y, new Color(.008f + nebula * .08f + violet * .035f, .006f + nebula * .018f + violet * .01f, .035f + nebula * .2f + violet * .22f, 1f));
            }
            System.Random random = new(17);
            for (int i = 0; i < 150; i++)
            {
                int x = random.Next(8, t.width - 8); int y = random.Next(8, t.height - 8);
                float size = (float)random.NextDouble() * 1.8f + .5f;
                DrawCircle(t, x, y, Mathf.CeilToInt(size), new Color(.45f, .7f, 1f, .35f));
            }
        }

        private static void DrawDust(Texture2D t)
        {
            DrawCircle(t, 16, 16, 3, new Color(.5f, .75f, 1f, .9f));
            DrawLine(t, new Vector2(16, 7), new Vector2(16, 25), new Color(.4f, .65f, 1f, .4f), 1);
            DrawLine(t, new Vector2(7, 16), new Vector2(25, 16), new Color(.4f, .65f, 1f, .4f), 1);
        }

        private static void DrawShockwave(Texture2D t)
        {
            for (int y = 0; y < t.height; y++)
            for (int x = 0; x < t.width; x++)
            {
                float distance = Vector2.Distance(new Vector2(x, y), new Vector2(64, 64));
                if (distance > 48f && distance < 54f) Put(t, x, y, new Color(.25f, .85f, 1f, .8f));
                else if (distance > 38f && distance < 41f) Put(t, x, y, new Color(1f, .55f, .2f, .45f));
            }
        }

        private static void DrawAsteroid(Texture2D t, int index)
        {
            Color[] colors = { new Color(.32f, .37f, .46f), new Color(.45f, .28f, .18f), new Color(.38f, .34f, .42f), new Color(.3f, .45f, .55f), new Color(.38f, .48f, .58f), new Color(.5f, .18f, .72f) };
            Color baseColor = colors[index];
            Vector2[] points = new Vector2[12];
            float seed = index * .73f;
            for (int i = 0; i < points.Length; i++)
            {
                float angle = i * Mathf.PI * 2f / points.Length;
                float radius = 42f + Mathf.Sin(i * 2.1f + seed) * 8f + Mathf.Cos(i * 1.3f + seed) * 4f;
                points[i] = new Vector2(64 + Mathf.Cos(angle) * radius, 64 + Mathf.Sin(angle) * radius);
            }
            DrawPolygon(t, points, baseColor);
            DrawPolygon(t, new[] { new Vector2(30, 45), new Vector2(68, 25), new Vector2(96, 51), new Vector2(80, 88), new Vector2(43, 96), new Vector2(23, 72) }, Color.Lerp(baseColor, Color.white, .14f));
            DrawCircle(t, 45 + index * 2, 55, 8, new Color(.08f, .09f, .13f, .55f));
            DrawCircle(t, 42 + index * 2, 52, 4, new Color(.65f, .7f, .78f, .2f));
            DrawCircle(t, 77, 77 - index, 5, new Color(.08f, .09f, .13f, .5f));
            if (index == 3) DrawLine(t, new Vector2(14, 103), new Vector2(42, 83), new Color(.3f, .85f, 1f, .55f), 4);
            if (index == 4)
            {
                DrawLine(t, new Vector2(23, 35), new Vector2(98, 35), new Color(.75f, .9f, 1f, .7f), 3);
                DrawLine(t, new Vector2(23, 91), new Vector2(98, 91), new Color(.2f, .35f, .48f, .8f), 3);
            }
            if (index == 5)
            {
                DrawCircle(t, 64, 64, 30, new Color(.8f, .3f, 1f, .12f));
                DrawLine(t, new Vector2(25, 64), new Vector2(103, 64), new Color(1f, .5f, 1f, .35f), 2);
            }
        }

        private static void DrawGift(Texture2D t, int index)
        {
            Color[] colors = { new Color(.15f, .9f, .35f), new Color(.15f, .5f, 1f), new Color(1f, .72f, .12f), new Color(.72f, .2f, 1f), new Color(1f, .25f, .18f), new Color(.15f, .85f, 1f), new Color(1f, .3f, .45f), new Color(1f, .45f, .12f) };
            Color color = colors[index];
            // A readable sci-fi present silhouette: glow, outlined box, lid, ribbon and bow.
            DrawCircle(t, 48, 51, 43, new Color(color.r, color.g, color.b, .11f));
            DrawRect(t, 15, 30, 81, 80, new Color(.015f, .025f, .07f, .95f));
            DrawRect(t, 19, 34, 77, 76, color);
            DrawRect(t, 19, 34, 77, 45, Color.Lerp(color, Color.white, .2f));
            DrawRect(t, 44, 34, 52, 76, new Color(1f, .88f, .35f, .95f));
            DrawRect(t, 11, 25, 85, 40, new Color(.015f, .025f, .07f, .95f));
            DrawRect(t, 15, 28, 81, 36, color);
            DrawRect(t, 44, 28, 52, 36, new Color(1f, .88f, .35f, .95f));
            DrawCircle(t, 35, 20, 10, color);
            DrawCircle(t, 61, 20, 10, color);
            DrawCircle(t, 48, 25, 7, new Color(1f, .9f, .4f));
            DrawLine(t, new Vector2(21, 82), new Vector2(75, 82), new Color(1f, 1f, 1f, .38f), 2);
        }

        private static void DrawRect(Texture2D t, int x0, int y0, int x1, int y1, Color color)
        {
            for (int y = y0; y <= y1; y++) for (int x = x0; x <= x1; x++) Put(t, x, y, color);
        }

        private static void DrawCircle(Texture2D t, float cx, float cy, float radius, Color color)
        {
            int minX = Mathf.FloorToInt(cx - radius); int maxX = Mathf.CeilToInt(cx + radius);
            int minY = Mathf.FloorToInt(cy - radius); int maxY = Mathf.CeilToInt(cy + radius);
            for (int y = minY; y <= maxY; y++) for (int x = minX; x <= maxX; x++) if ((x - cx) * (x - cx) + (y - cy) * (y - cy) <= radius * radius) Put(t, x, y, color);
        }

        private static void DrawEllipse(Texture2D t, float cx, float cy, float rx, float ry, Color color)
        {
            for (int y = Mathf.FloorToInt(cy - ry); y <= Mathf.CeilToInt(cy + ry); y++) for (int x = Mathf.FloorToInt(cx - rx); x <= Mathf.CeilToInt(cx + rx); x++) if (((x - cx) * (x - cx)) / (rx * rx) + ((y - cy) * (y - cy)) / (ry * ry) <= 1f) Put(t, x, y, color);
        }

        private static void DrawLine(Texture2D t, Vector2 from, Vector2 to, Color color, int width)
        {
            int steps = Mathf.CeilToInt(Vector2.Distance(from, to) * 2f);
            for (int i = 0; i <= steps; i++) DrawCircle(t, Mathf.Lerp(from.x, to.x, i / (float)Mathf.Max(1, steps)), Mathf.Lerp(from.y, to.y, i / (float)Mathf.Max(1, steps)), width, color);
        }

        private static void DrawPolygon(Texture2D t, IReadOnlyList<Vector2> points, Color color)
        {
            float minX = float.MaxValue, maxX = float.MinValue, minY = float.MaxValue, maxY = float.MinValue;
            foreach (Vector2 point in points) { minX = Mathf.Min(minX, point.x); maxX = Mathf.Max(maxX, point.x); minY = Mathf.Min(minY, point.y); maxY = Mathf.Max(maxY, point.y); }
            for (int y = Mathf.FloorToInt(minY); y <= Mathf.CeilToInt(maxY); y++) for (int x = Mathf.FloorToInt(minX); x <= Mathf.CeilToInt(maxX); x++) if (InsidePolygon(new Vector2(x, y), points)) Put(t, x, y, color);
        }

        private static bool InsidePolygon(Vector2 point, IReadOnlyList<Vector2> polygon)
        {
            bool inside = false;
            for (int i = 0, j = polygon.Count - 1; i < polygon.Count; j = i++)
            {
                if (((polygon[i].y > point.y) != (polygon[j].y > point.y)) && point.x < (polygon[j].x - polygon[i].x) * (point.y - polygon[i].y) / (polygon[j].y - polygon[i].y) + polygon[i].x) inside = !inside;
            }
            return inside;
        }

        private static void Put(Texture2D t, int x, int y, Color color)
        {
            if (x < 0 || y < 0 || x >= t.width || y >= t.height || color.a <= 0f) return;
            Color existing = t.GetPixel(x, y);
            float alpha = color.a + existing.a * (1f - color.a);
            if (alpha <= 0f) return;
            t.SetPixel(x, y, new Color((color.r * color.a + existing.r * existing.a * (1f - color.a)) / alpha, (color.g * color.a + existing.g * existing.a * (1f - color.a)) / alpha, (color.b * color.a + existing.b * existing.a * (1f - color.a)) / alpha, alpha));
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

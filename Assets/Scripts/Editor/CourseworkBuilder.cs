using System.Collections.Generic;
using System.IO;
using SpaceExplore.Coursework;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.SceneManagement;
using UnityEngine.Tilemaps;
using UnityEngine.UI;

namespace SpaceExplore.EditorTools
{
    public static class CourseworkBuilder
    {
        private const string SceneFolder = "Assets/Scenes/Coursework";

        [MenuItem("SpaceExplore/Coursework/Build all practices")]
        public static void BuildAll()
        {
            EnsureFolder("Assets/Scenes", "Coursework");
            string week1 = BuildTransformPractice();
            string week2 = BuildTilemapPractice();
            string week3 = BuildPhysicsPractice();
            string week7 = BuildAnimationPractice();
            string week8 = BuildUiPractice();
            AddBuildScenes(week1, week2, week3, week7, week8);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("Coursework practices generated under Assets/Scenes/Coursework.");
        }

        private static string BuildTransformPractice()
        {
            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            CreateCamera(new Vector3(0f, 5f, -15f), new Vector3(12f, 0f, 0f), true);
            GameObject light = new GameObject("Directional Light");
            Light directional = light.AddComponent<Light>();
            directional.type = LightType.Directional;
            directional.intensity = 1.2f;
            light.transform.rotation = Quaternion.Euler(35f, -25f, 0f);

            GameObject airplane = new GameObject("AirPlane");
            GameObject body = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            body.name = "AirplaneBody";
            body.transform.SetParent(airplane.transform, false);
            body.transform.localRotation = Quaternion.Euler(-90f, 0f, 0f);
            body.transform.localScale = new Vector3(1f, 4.2f, 1.1f);
            GameObject wing = GameObject.CreatePrimitive(PrimitiveType.Cube);
            wing.name = "AirplaneWing";
            wing.transform.SetParent(airplane.transform, false);
            wing.transform.localPosition = new Vector3(0f, 0f, 0.58f);
            wing.transform.localScale = new Vector3(6.8f, 0.18f, 1.3f);
            GameObject tail = GameObject.CreatePrimitive(PrimitiveType.Cube);
            tail.name = "TailPlane";
            tail.transform.SetParent(airplane.transform, false);
            tail.transform.localPosition = new Vector3(0f, 0f, 3.23f);
            tail.transform.localScale = new Vector3(1.93f, 0.2f, 0.87f);
            GameObject fin = GameObject.CreatePrimitive(PrimitiveType.Cube);
            fin.name = "AirplaneFin";
            fin.transform.SetParent(airplane.transform, false);
            fin.transform.localPosition = new Vector3(-0.1f, 0.5f, 3.6f);
            fin.transform.localRotation = Quaternion.Euler(18f, 0f, 0f);
            fin.transform.localScale = new Vector3(1.93f, 0.2f, 0.87f);

            return SaveScene(scene, "Week01_TransformPractice");
        }

        private static string BuildTilemapPractice()
        {
            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            GameObject grid = InstantiatePrefab("Assets/CourseLabs/SlideAssets/Slide1_2/Prefabs/Tilemap Grid.prefab", "Tilemap Grid");
            if (grid == null) grid = new GameObject("Tilemap Grid");
            InstantiatePrefab("Assets/CourseLabs/SlideAssets/Slide1_2/Prefabs/1.2_Basic-Game-Object/Background Tilemap.prefab", "Background Tilemap");
            InstantiatePrefab("Assets/CourseLabs/SlideAssets/Slide1_2/Prefabs/1.2_Basic-Game-Object/Platforms Tilemap.prefab", "Platforms Tilemap");
            InstantiatePrefab("Assets/CourseLabs/SlideAssets/Slide1_2/Prefabs/1.2_Basic-Game-Object/Hazards Tilemap.prefab", "Hazards Tilemap");
            GameObject player = InstantiatePrefab("Assets/CourseLabs/SlideAssets/Slide1_2/Prefabs/1.2_Basic-Game-Object/Ginger (Player).prefab", "Ginger (Player)");
            if (player != null)
            {
                player.transform.position = new Vector3(-3f, 2f, 0f);
                AddPlatformerComponents(player);
            }
            InstantiatePrefab("Assets/CourseLabs/SlideAssets/Slide1_2/Prefabs/1.2_Basic-Game-Object/Cameras.prefab", "Cameras");
            return SaveScene(scene, "Week02_TilemapPractice");
        }

        private static string BuildPhysicsPractice()
        {
            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            GameObject ground = InstantiatePrefab("Assets/CourseLabs/SlideAssets/Slide1_5/Prefabs/Tilemap Grid.prefab", "Physics Ground");
            if (ground == null)
            {
                ground = new GameObject("Physics Ground");
                BoxCollider2D box = ground.AddComponent<BoxCollider2D>();
                box.size = new Vector2(18f, 1f);
                ground.transform.position = new Vector3(0f, -3f, 0f);
            }
            TilemapCollider2D tilemapCollider = ground.GetComponentInChildren<TilemapCollider2D>();
            if (tilemapCollider != null)
            {
                CompositeCollider2D composite = ground.GetComponentInChildren<CompositeCollider2D>();
                if (composite == null) composite = ground.AddComponent<CompositeCollider2D>();
                Rigidbody2D body = ground.GetComponent<Rigidbody2D>();
                if (body == null) body = ground.AddComponent<Rigidbody2D>();
                body.bodyType = RigidbodyType2D.Static;
                tilemapCollider.usedByComposite = true;
            }
            GameObject player = InstantiatePrefab("Assets/CourseLabs/SlideAssets/Slide1_5/Prefabs/Ginger (Player).prefab", "Ginger (Player)");
            if (player == null) player = CreatePhysicsFallbackPlayer();
            player.transform.position = new Vector3(0f, 2f, 0f);
            AddPhysicsComponents(player);
            CreateCamera(new Vector3(0f, 0f, -10f), Vector3.zero, false);
            return SaveScene(scene, "Week03_PhysicsPractice");
        }

        private static string BuildAnimationPractice()
        {
            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            GameObject hero = InstantiatePrefab("Assets/CourseLabs/LegacyAssets/HeroKnight/Demo/HeroKnight.prefab", "HeroKnight");
            if (hero == null) hero = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            Animator animator = hero.GetComponent<Animator>();
            if (animator == null) animator = hero.AddComponent<Animator>();
            RuntimeAnimatorController controller = AssetDatabase.LoadAssetAtPath<RuntimeAnimatorController>("Assets/CourseLabs/LegacyAssets/HeroKnight/Animations/HeroKnight_AnimController.controller");
            if (controller != null) animator.runtimeAnimatorController = controller;
            hero.AddComponent<CourseAnimationEventDemo>();
            hero.transform.position = Vector3.zero;
            CreateCamera(new Vector3(0f, 2f, -12f), new Vector3(8f, 0f, 0f), true);
            return SaveScene(scene, "Week07_AnimationPractice");
        }

        private static string BuildUiPractice()
        {
            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            CreateCamera(new Vector3(0f, 0f, -10f), Vector3.zero, false);
            GameObject eventSystem = new GameObject("EventSystem");
            eventSystem.AddComponent<EventSystem>();
            eventSystem.AddComponent<InputSystemUIInputModule>();
            GameObject canvasObject = new GameObject("Course UI Canvas");
            Canvas canvas = canvasObject.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvasObject.AddComponent<CanvasScaler>();
            canvasObject.AddComponent<GraphicRaycaster>();
            GameObject panel = CreateUiObject("Popup Panel", canvasObject.transform);
            Image panelImage = panel.AddComponent<Image>();
            panelImage.color = new Color(0.06f, 0.1f, 0.2f, 0.95f);
            RectTransform panelRect = panel.GetComponent<RectTransform>();
            panelRect.sizeDelta = new Vector2(760f, 420f);
            GameObject title = CreateUiObject("Title", panel.transform);
            TextMeshProUGUI titleText = title.AddComponent<TextMeshProUGUI>();
            titleText.text = "Space Explorer - Lab Practice";
            titleText.fontSize = 34f;
            titleText.alignment = TextAlignmentOptions.Center;
            title.GetComponent<RectTransform>().anchoredPosition = new Vector2(0f, 125f);
            title.GetComponent<RectTransform>().sizeDelta = new Vector2(700f, 60f);
            GameObject message = CreateUiObject("Message", panel.transform);
            TextMeshProUGUI messageText = message.AddComponent<TextMeshProUGUI>();
            messageText.text = "UI practice: Canvas, Panel, TextMeshPro and Buttons.";
            messageText.fontSize = 24f;
            messageText.alignment = TextAlignmentOptions.Center;
            message.GetComponent<RectTransform>().anchoredPosition = new Vector2(0f, 35f);
            message.GetComponent<RectTransform>().sizeDelta = new Vector2(680f, 100f);
            Button continueButton = CreateButton("Continue", panel.transform, new Vector2(-150f, -115f));
            Button restartButton = CreateButton("Restart", panel.transform, new Vector2(150f, -115f));
            CoursePopupController popup = canvasObject.AddComponent<CoursePopupController>();
            SerializedObject serialized = new SerializedObject(popup);
            serialized.FindProperty("popup").objectReferenceValue = panel;
            serialized.FindProperty("continueButton").objectReferenceValue = continueButton;
            serialized.FindProperty("restartButton").objectReferenceValue = restartButton;
            serialized.ApplyModifiedPropertiesWithoutUndo();
            return SaveScene(scene, "Week08_UIPractice");
        }

        private static void AddBuildScenes(params string[] generatedScenes)
        {
            List<EditorBuildSettingsScene> scenes = new List<EditorBuildSettingsScene>();
            foreach (EditorBuildSettingsScene current in EditorBuildSettings.scenes)
                scenes.Add(current);
            foreach (string path in generatedScenes)
            {
                if (!scenes.Exists(scene => scene.path == path))
                    scenes.Add(new EditorBuildSettingsScene(path, true));
            }
            EditorBuildSettings.scenes = scenes.ToArray();
        }

        private static void AddPlatformerComponents(GameObject player)
        {
            if (player.GetComponent<Rigidbody2D>() == null) player.AddComponent<Rigidbody2D>();
            if (player.GetComponent<CapsuleCollider2D>() == null) player.AddComponent<CapsuleCollider2D>();
            if (player.GetComponent<CoursePlatformerController>() == null) player.AddComponent<CoursePlatformerController>();
        }

        private static void AddPhysicsComponents(GameObject player)
        {
            Rigidbody2D body = player.GetComponent<Rigidbody2D>();
            if (body == null) body = player.AddComponent<Rigidbody2D>();
            body.bodyType = RigidbodyType2D.Dynamic;
            body.freezeRotation = true;
            if (player.GetComponent<CapsuleCollider2D>() == null) player.AddComponent<CapsuleCollider2D>();
            if (player.GetComponent<CoursePhysicsShowcase>() == null) player.AddComponent<CoursePhysicsShowcase>();
        }

        private static GameObject CreatePhysicsFallbackPlayer()
        {
            GameObject player = new GameObject("Ginger (Player)");
            SpriteRenderer renderer = player.AddComponent<SpriteRenderer>();
            renderer.sprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/CourseLabs/SlideAssets/Slide1_2/Sprites/SPA_Player_Ginger.png");
            return player;
        }

        private static GameObject CreateUiObject(string name, Transform parent)
        {
            GameObject result = new GameObject(name, typeof(RectTransform));
            result.transform.SetParent(parent, false);
            return result;
        }

        private static Button CreateButton(string label, Transform parent, Vector2 position)
        {
            GameObject buttonObject = CreateUiObject(label + " Button", parent);
            Image image = buttonObject.AddComponent<Image>();
            image.color = new Color(0.15f, 0.45f, 0.85f, 1f);
            Button button = buttonObject.AddComponent<Button>();
            RectTransform rect = buttonObject.GetComponent<RectTransform>();
            rect.anchoredPosition = position;
            rect.sizeDelta = new Vector2(220f, 70f);
            GameObject textObject = CreateUiObject("Text", buttonObject.transform);
            TextMeshProUGUI text = textObject.AddComponent<TextMeshProUGUI>();
            text.text = label;
            text.fontSize = 26f;
            text.alignment = TextAlignmentOptions.Center;
            RectTransform textRect = textObject.GetComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.offsetMin = Vector2.zero;
            textRect.offsetMax = Vector2.zero;
            return button;
        }

        private static GameObject InstantiatePrefab(string path, string name)
        {
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (prefab == null) return null;
            GameObject instance = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
            instance.name = name;
            return instance;
        }

        private static void CreateCamera(Vector3 position, Vector3 rotation, bool perspective)
        {
            GameObject cameraObject = new GameObject("Main Camera");
            Camera camera = cameraObject.AddComponent<Camera>();
            cameraObject.tag = "MainCamera";
            cameraObject.transform.position = position;
            cameraObject.transform.rotation = Quaternion.Euler(rotation);
            camera.orthographic = !perspective;
            camera.orthographicSize = 6f;
        }

        private static string SaveScene(Scene scene, string fileName)
        {
            string path = SceneFolder + "/" + fileName + ".unity";
            EditorSceneManager.SaveScene(scene, path);
            return path;
        }

        private static void EnsureFolder(string parent, string child)
        {
            if (!AssetDatabase.IsValidFolder(parent + "/" + child))
                AssetDatabase.CreateFolder(parent, child);
        }
    }
}

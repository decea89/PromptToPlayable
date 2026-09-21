using Unity.AI.Navigation;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.SceneManagement;

public static class ArenaSceneSetup
{
    private const string ScenePath = "Assets/Scenes/Arena.unity";

    [MenuItem("Tools/Prompt To Playable/Build Minimal Arena")]
    public static void BuildMinimalArena()
    {
        if (!EditorUtility.DisplayDialog(
                "Build Minimal Arena",
                "This replaces the contents of Assets/Scenes/Arena.unity with the prototype arena and bakes its NavMesh. Continue?",
                "Build Arena",
                "Cancel"))
        {
            return;
        }

        Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

        Material arenaMaterial = CreateLitMaterial("Arena Dark", new Color(0.025f, 0.055f, 0.085f));
        Material accentMaterial = CreateLitMaterial("Cyan Accent", new Color(0.01f, 0.25f, 0.38f), new Color(0f, 1.5f, 2.2f));
        Material playerMaterial = CreateLitMaterial("Player", new Color(0.7f, 0.9f, 1f), new Color(0.03f, 0.22f, 0.4f));
        Material goalMaterial = CreateLitMaterial("Extraction", new Color(0.05f, 0.75f, 0.28f), new Color(0.02f, 2.5f, 0.3f));
        Material warningMaterial = CreateLitMaterial("Spawn Marker", new Color(0.85f, 0.55f, 0.02f), new Color(1.2f, 0.4f, 0f));

        GameObject navigationRoot = new GameObject("Navigation");
        NavMeshSurface surface = navigationRoot.AddComponent<NavMeshSurface>();
        surface.collectObjects = CollectObjects.Children;
        surface.useGeometry = NavMeshCollectGeometry.PhysicsColliders;

        CreatePrimitive(PrimitiveType.Cube, "Arena Floor", new Vector3(0f, -0.2f, 0f), new Vector3(20f, 0.4f, 15f), arenaMaterial, navigationRoot.transform);

        GameObject arenaRoot = new GameObject("Arena");
        CreatePrimitive(PrimitiveType.Cube, "North Wall", new Vector3(0f, 1.5f, 7.6f), new Vector3(20.8f, 3f, 0.5f), arenaMaterial, arenaRoot.transform);
        CreatePrimitive(PrimitiveType.Cube, "South Wall", new Vector3(0f, 1.5f, -7.6f), new Vector3(20.8f, 3f, 0.5f), arenaMaterial, arenaRoot.transform);
        CreatePrimitive(PrimitiveType.Cube, "East Wall", new Vector3(10.1f, 1.5f, 0f), new Vector3(0.5f, 3f, 15f), arenaMaterial, arenaRoot.transform);
        CreatePrimitive(PrimitiveType.Cube, "West Wall", new Vector3(-10.1f, 1.5f, 0f), new Vector3(0.5f, 3f, 15f), arenaMaterial, arenaRoot.transform);

        CreatePrimitive(PrimitiveType.Cube, "Accent North", new Vector3(0f, 0.03f, 5.8f), new Vector3(15f, 0.04f, 0.16f), accentMaterial, arenaRoot.transform);
        CreatePrimitive(PrimitiveType.Cube, "Accent South", new Vector3(0f, 0.03f, -5.8f), new Vector3(15f, 0.04f, 0.16f), accentMaterial, arenaRoot.transform);
        CreatePrimitive(PrimitiveType.Cube, "Accent Center", new Vector3(0f, 0.03f, 0f), new Vector3(0.16f, 0.04f, 11.5f), accentMaterial, arenaRoot.transform);
        CreatePrimitive(PrimitiveType.Cube, "Cover A", new Vector3(-3.5f, 0.75f, 1.6f), new Vector3(2.3f, 1.5f, 1.1f), arenaMaterial, arenaRoot.transform);
        CreatePrimitive(PrimitiveType.Cube, "Cover B", new Vector3(3.8f, 0.75f, -1.9f), new Vector3(2.3f, 1.5f, 1.1f), arenaMaterial, arenaRoot.transform);

        GameObject player = CreatePrimitive(PrimitiveType.Capsule, "PlayerPlaceholder", new Vector3(0f, 1f, -4.7f), Vector3.one, playerMaterial, null);
        player.tag = "Player";

        GameObject extraction = CreatePrimitive(PrimitiveType.Cylinder, "ExtractionPoint", new Vector3(0f, 0.35f, 5f), new Vector3(1.1f, 0.35f, 1.1f), goalMaterial, null);
        CreatePrimitive(PrimitiveType.Cylinder, "ExtractionBeacon", new Vector3(0f, 2f, 5f), new Vector3(0.12f, 1.5f, 0.12f), goalMaterial, extraction.transform);

        GameObject spawnRoot = new GameObject("EnemySpawnPoints");
        CreateSpawnPoint("SpawnPoint_A", new Vector3(-6.5f, 0.06f, 3.7f), warningMaterial, spawnRoot.transform);
        CreateSpawnPoint("SpawnPoint_B", new Vector3(6.5f, 0.06f, 3.7f), warningMaterial, spawnRoot.transform);
        CreateSpawnPoint("SpawnPoint_C", new Vector3(0f, 0.06f, 1.8f), warningMaterial, spawnRoot.transform);

        CreateLighting();
        CreateCamera();

        surface.BuildNavMesh();
        EditorSceneManager.SaveScene(scene, ScenePath);
        Selection.activeGameObject = navigationRoot;
        Debug.Log("Minimal Arena built and NavMesh baked.");
    }

    private static GameObject CreatePrimitive(PrimitiveType type, string objectName, Vector3 position, Vector3 scale, Material material, Transform parent)
    {
        GameObject instance = GameObject.CreatePrimitive(type);
        instance.name = objectName;
        instance.transform.SetParent(parent);
        instance.transform.position = position;
        instance.transform.localScale = scale;
        instance.GetComponent<Renderer>().sharedMaterial = material;
        return instance;
    }

    private static void CreateSpawnPoint(string objectName, Vector3 position, Material material, Transform parent)
    {
        GameObject marker = CreatePrimitive(PrimitiveType.Cylinder, objectName, position, new Vector3(0.45f, 0.05f, 0.45f), material, parent);
        marker.GetComponent<Collider>().enabled = false;
    }

    private static Material CreateLitMaterial(string materialName, Color baseColor, Color emissionColor = default)
    {
        Shader shader = Shader.Find("Universal Render Pipeline/Lit");
        Material material = new Material(shader) { name = materialName };
        material.SetColor("_BaseColor", baseColor);

        if (emissionColor.maxColorComponent > 0f)
        {
            material.EnableKeyword("_EMISSION");
            material.SetColor("_EmissionColor", emissionColor);
        }

        return material;
    }

    private static void CreateLighting()
    {
        RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Flat;
        RenderSettings.ambientSkyColor = new Color(0.015f, 0.03f, 0.06f);

        GameObject directional = new GameObject("Key Light");
        Light keyLight = directional.AddComponent<Light>();
        keyLight.type = LightType.Directional;
        keyLight.color = new Color(0.38f, 0.72f, 1f);
        keyLight.intensity = 1.1f;
        directional.transform.rotation = Quaternion.Euler(48f, -28f, 0f);

        CreatePointLight("Cyan Fill", new Vector3(-7f, 3.5f, -3f), new Color(0f, 0.75f, 1f), 5f, 8f);
        CreatePointLight("Goal Glow", new Vector3(0f, 2.6f, 5f), new Color(0.05f, 1f, 0.3f), 5f, 6f);
    }

    private static void CreatePointLight(string objectName, Vector3 position, Color color, float intensity, float range)
    {
        GameObject lightObject = new GameObject(objectName);
        lightObject.transform.position = position;
        Light light = lightObject.AddComponent<Light>();
        light.type = LightType.Point;
        light.color = color;
        light.intensity = intensity;
        light.range = range;
    }

    private static void CreateCamera()
    {
        GameObject cameraObject = new GameObject("Main Camera");
        cameraObject.tag = "MainCamera";
        Camera camera = cameraObject.AddComponent<Camera>();
        cameraObject.AddComponent<AudioListener>();
        camera.transform.position = new Vector3(13.5f, 15.5f, -17.5f);
        camera.transform.rotation = Quaternion.LookRotation(new Vector3(0f, 0.7f, 0f) - camera.transform.position);
        camera.fieldOfView = 52f;
        camera.clearFlags = CameraClearFlags.SolidColor;
        camera.backgroundColor = new Color(0.005f, 0.012f, 0.025f);
    }
}

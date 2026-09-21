using UnityEditor;
using UnityEngine;

public static class EnemyPlaceholderPrefabUtility
{
    private const string PrefabPath = "Assets/Prefabs/EnemyPlaceholder.prefab";

    [MenuItem("Tools/Prompt To Playable/Fix Enemy Placeholder Ground Pivot")]
    public static void FixGroundPivot()
    {
        GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(PrefabPath);
        if (prefab == null)
        {
            Debug.LogError($"Enemy placeholder prefab was not found at {PrefabPath}.");
            return;
        }

        GameObject prefabRoot = PrefabUtility.LoadPrefabContents(PrefabPath);
        try
        {
            MeshFilter rootMeshFilter = prefabRoot.GetComponent<MeshFilter>();
            MeshRenderer rootMeshRenderer = prefabRoot.GetComponent<MeshRenderer>();
            CapsuleCollider rootCollider = prefabRoot.GetComponent<CapsuleCollider>();

            if (rootMeshFilter == null || rootMeshRenderer == null || rootCollider == null)
            {
                Debug.LogError("EnemyPlaceholder must have a MeshFilter, MeshRenderer, and CapsuleCollider on its root before it can be fixed.");
                return;
            }

            GameObject visual = new GameObject("Visual");
            visual.transform.SetParent(prefabRoot.transform);
            visual.transform.localPosition = Vector3.up;
            visual.transform.localRotation = Quaternion.identity;
            visual.transform.localScale = Vector3.one;

            MeshFilter visualMeshFilter = visual.AddComponent<MeshFilter>();
            visualMeshFilter.sharedMesh = rootMeshFilter.sharedMesh;

            MeshRenderer visualMeshRenderer = visual.AddComponent<MeshRenderer>();
            visualMeshRenderer.sharedMaterials = rootMeshRenderer.sharedMaterials;
            visualMeshRenderer.shadowCastingMode = rootMeshRenderer.shadowCastingMode;
            visualMeshRenderer.receiveShadows = rootMeshRenderer.receiveShadows;

            CapsuleCollider visualCollider = visual.AddComponent<CapsuleCollider>();
            visualCollider.center = rootCollider.center;
            visualCollider.radius = rootCollider.radius;
            visualCollider.height = rootCollider.height;
            visualCollider.direction = rootCollider.direction;
            visualCollider.isTrigger = rootCollider.isTrigger;
            visualCollider.sharedMaterial = rootCollider.sharedMaterial;

            Object.DestroyImmediate(rootMeshFilter);
            Object.DestroyImmediate(rootMeshRenderer);
            Object.DestroyImmediate(rootCollider);
            prefabRoot.transform.localPosition = Vector3.zero;

            PrefabUtility.SaveAsPrefabAsset(prefabRoot, PrefabPath);
            Debug.Log("EnemyPlaceholder pivot aligned to the ground.");
        }
        finally
        {
            PrefabUtility.UnloadPrefabContents(prefabRoot);
        }
    }
}

using System;
using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
public class FloorTileGenerator : MonoBehaviour
{
    private const string FloorRootName = "FloorRoot";

    [Tooltip("Number of tiles on the X axis.")]
    [Min(1)]
    public int width = 1;

    [Tooltip("Number of tiles on the Y axis.")]
    [Min(1)]
    public int height = 1;

    // Local start position relative to FloorRoot.
    [Tooltip("Local start position relative to the generated FloorRoot object.")]
    public Vector3 origin = Vector3.zero;

    [Tooltip("Distance between tiles in Unity units.")]
    [Min(0f)]
    public float tileSpacing = 1f;

    // Register prefabs that contain a SpriteRenderer.
    [Tooltip("Prefab assets used when placing floor tiles.")]
    public GameObject[] floorTilePrefabs = new GameObject[0];

    [SerializeField]
    [HideInInspector]
    private Transform floorRoot;

    public Transform FloorRoot => floorRoot;

#if UNITY_EDITOR
    public void GenerateTiles()
    {
        if (Application.isPlaying)
        {
            Debug.LogWarning("FloorTileGenerator is intended to run in Edit Mode only.", this);
            return;
        }

        if (!gameObject.scene.IsValid())
        {
            Debug.LogWarning("FloorTileGenerator can only be used on a scene object.", this);
            return;
        }

        if (!TryGetValidPrefabs(out List<GameObject> validPrefabs))
        {
            return;
        }

        int undoGroup = UnityEditor.Undo.GetCurrentGroup();
        UnityEditor.Undo.SetCurrentGroupName("Generate Floor Tiles");

        Transform root = EnsureFloorRoot();
        ClearTilesInternal(root);

        System.Random random = new System.Random();
        int safeWidth = Mathf.Max(1, width);
        int safeHeight = Mathf.Max(1, height);

        for (int y = 0; y < safeHeight; y++)
        {
            for (int x = 0; x < safeWidth; x++)
            {
                GameObject selectedPrefab = validPrefabs[random.Next(validPrefabs.Count)];
                GameObject createdTile = UnityEditor.PrefabUtility.InstantiatePrefab(selectedPrefab, gameObject.scene) as GameObject;

                if (createdTile == null)
                {
                    continue;
                }

                UnityEditor.Undo.RegisterCreatedObjectUndo(createdTile, "Create Floor Tile");
                UnityEditor.Undo.SetTransformParent(createdTile.transform, root, "Parent Floor Tile");

                createdTile.transform.localPosition = origin + new Vector3(x * tileSpacing, y * tileSpacing, 0f);
                createdTile.name = $"FloorTile_{x}_{y}";
            }
        }

        MarkSceneDirty();
        UnityEditor.Undo.CollapseUndoOperations(undoGroup);
    }

    public void ClearTiles()
    {
        if (Application.isPlaying)
        {
            Debug.LogWarning("FloorTileGenerator is intended to run in Edit Mode only.", this);
            return;
        }

        if (!gameObject.scene.IsValid())
        {
            return;
        }

        Transform root = GetExistingFloorRoot();
        if (root == null)
        {
            return;
        }

        int undoGroup = UnityEditor.Undo.GetCurrentGroup();
        UnityEditor.Undo.SetCurrentGroupName("Clear Floor Tiles");

        ClearTilesInternal(root);
        MarkSceneDirty();

        UnityEditor.Undo.CollapseUndoOperations(undoGroup);
    }

    private bool TryGetValidPrefabs(out List<GameObject> validPrefabs)
    {
        validPrefabs = new List<GameObject>();

        if (floorTilePrefabs == null || floorTilePrefabs.Length == 0)
        {
            Debug.LogWarning("Add at least one floor tile prefab to floorTilePrefabs.", this);
            return false;
        }

        foreach (GameObject prefab in floorTilePrefabs)
        {
            if (prefab == null)
            {
                continue;
            }

            if (!UnityEditor.PrefabUtility.IsPartOfPrefabAsset(prefab))
            {
                Debug.LogWarning($"'{prefab.name}' is not a prefab asset, so it will be skipped.", prefab);
                continue;
            }

            if (prefab.GetComponentInChildren<SpriteRenderer>(true) == null)
            {
                Debug.LogWarning($"'{prefab.name}' does not contain a SpriteRenderer, so it will be skipped.", prefab);
                continue;
            }

            validPrefabs.Add(prefab);
        }

        if (validPrefabs.Count == 0)
        {
            Debug.LogWarning("No valid floor tile prefabs were found. Register prefab assets that contain a SpriteRenderer.", this);
            return false;
        }

        return true;
    }

    private Transform EnsureFloorRoot()
    {
        Transform root = GetExistingFloorRoot();
        if (root != null)
        {
            return root;
        }

        GameObject rootObject = new GameObject(FloorRootName);
        UnityEditor.Undo.RegisterCreatedObjectUndo(rootObject, "Create Floor Root");
        UnityEditor.Undo.SetTransformParent(rootObject.transform, transform, "Parent Floor Root");

        rootObject.transform.localPosition = Vector3.zero;
        rootObject.transform.localRotation = Quaternion.identity;
        rootObject.transform.localScale = Vector3.one;

        UnityEditor.Undo.RecordObject(this, "Assign Floor Root");
        floorRoot = rootObject.transform;
        UnityEditor.EditorUtility.SetDirty(this);

        return floorRoot;
    }

    private Transform GetExistingFloorRoot()
    {
        if (floorRoot != null)
        {
            return floorRoot;
        }

        Transform foundRoot = transform.Find(FloorRootName);
        if (foundRoot == null)
        {
            return null;
        }

        UnityEditor.Undo.RecordObject(this, "Assign Floor Root");
        floorRoot = foundRoot;
        UnityEditor.EditorUtility.SetDirty(this);

        return floorRoot;
    }

    private void ClearTilesInternal(Transform root)
    {
        for (int i = root.childCount - 1; i >= 0; i--)
        {
            UnityEditor.Undo.DestroyObjectImmediate(root.GetChild(i).gameObject);
        }
    }

    private void MarkSceneDirty()
    {
        UnityEditor.EditorUtility.SetDirty(this);

        if (floorRoot != null)
        {
            UnityEditor.EditorUtility.SetDirty(floorRoot.gameObject);
        }

        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(gameObject.scene);
    }
#endif
}

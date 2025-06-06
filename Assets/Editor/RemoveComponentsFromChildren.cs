using UnityEngine;
using UnityEditor;

public static class ToolsComponentRemover
{
    static readonly string[] targetNames = {
        "N 201.001", "N 202.001", "N 203.001", "N 204.001", "N 205.001", "N 206.001",
        "N 207.001", "N 208.001", "N 209.001", "N 210.001", "N 211.001", "N 212 A.001", "N 212 B.001",
        "N 213.001", "N 214 A.001", "N 214 B.001", "N 215.001", "N 216.001", "N 217.001", "N 218.001", "N 219.001",
        "N 220.001", "N 221.001", "N 230.001", "N 231.001", "S 222.001", "S 223.001", "S 224.001", "S 225.001", "S 226.001",
        "S 227.001", "S 228.001", "S 229 A.001", "S 229 B.001"
    };

    [MenuItem("Tools/Remove Specific Components From Targeted Children")]
    public static void RemoveComponentsFromChildren()
    {
        foreach (GameObject parent in Selection.gameObjects)
        {
            RemoveFromChildrenRecursive(parent.transform);
        }

        AssetDatabase.SaveAssets();
    }

    private static void RemoveFromChildrenRecursive(Transform parent)
    {
        foreach (Transform child in parent)
        {
            if (System.Array.Exists(targetNames, name => name == child.name))
            {
                GameObject go = child.gameObject;
                RemoveComponent<MeshCollider>(go);
                RemoveComponent<HoverTip>(go);
                RemoveComponent<HoverOutline>(go);
                RemoveComponent<OutlineGenerator>(go);
            }

            RemoveFromChildrenRecursive(child);
        }
    }

    private static void RemoveComponent<T>(GameObject go) where T : Component
    {
        T comp = go.GetComponent<T>();
        if (comp != null)
        {
            Object.DestroyImmediate(comp);
            EditorUtility.SetDirty(go);
            Debug.Log($"{typeof(T).Name} supprimé de: {go.name}");
        }
    }
}
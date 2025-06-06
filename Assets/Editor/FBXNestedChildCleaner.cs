using UnityEngine;
using UnityEditor;

public static class FBXNestedChildCleaner
{
    [MenuItem("Tools/Clean Components From FBX Nested Children (Level 2+)")]
    public static void CleanNestedChildrenComponents()
    {
        foreach (GameObject selected in Selection.gameObjects)
        {
            foreach (Transform child in selected.transform)
            {
                // Niveau 1 — enfant direct (on ignore)
                // Niveau 2+ — enfants des enfants
                foreach (Transform grandChild in child)
                {
                    CleanRecursively(grandChild); // Supprimer à partir du niveau 2
                }
            }
        }

        AssetDatabase.SaveAssets();
        Debug.Log("Composants supprimés des enfants des enfants.");
    }

    private static void CleanRecursively(Transform t)
    {
        GameObject go = t.gameObject;

        RemoveComponent<MeshCollider>(go);
        RemoveComponent<HoverTip>(go);
        RemoveComponent<HoverOutline>(go);
        RemoveComponent<OutlineGenerator>(go);

        foreach (Transform child in t)
        {
            CleanRecursively(child); // Récursion pour niveaux plus profonds
        }
    }

    private static void RemoveComponent<T>(GameObject go) where T : Component
    {
        T comp = go.GetComponent<T>();
        if (comp != null)
        {
            Object.DestroyImmediate(comp);
            EditorUtility.SetDirty(go);
            Debug.Log($"Composant {typeof(T).Name} supprimé de: {go.name}");
        }
    }
}

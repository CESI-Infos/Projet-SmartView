using UnityEngine;
using UnityEditor;

public static class ToolsForChildren
{
    // Paramètres personnalisables
    const float defaultLiftAmount = 0.75f;
    const float defaultOutlineScale = 1.1f;

    // Liste des noms d'objets à exclure
    static readonly string[] exclusions = {
        "2e étage", "Objet vide", "Ascenseur", "local",
        "Etage 1", "terrasse", "WC", "WC.001", "WC.002", "WC.003", "WC.004", "RDC"
    };

    [MenuItem("Tools/Add All Components To Children")]
    static void AddAllComponents()
    {
        foreach (GameObject parent in Selection.gameObjects)
        {
            ProcessChildrenRecursive(parent.transform);
        }
    }

    static void ProcessChildrenRecursive(Transform parent)
    {
        foreach (Transform child in parent)
        {
            if (IsExcluded(child.name)) continue;

            GameObject go = child.gameObject;

            // Ajout de CubeColor
            if (go.GetComponent<CubeColor>() == null)
            {
                go.AddComponent<CubeColor>();
                Debug.Log($"CubeColor ajouté à: {child.name}");
            }

            // Ajout ou mise à jour de HoverOutline
            HoverOutline hover = go.GetComponent<HoverOutline>();
            if (hover == null)
            {
                hover = go.AddComponent<HoverOutline>();
                Debug.Log($"HoverOutline ajouté à: {child.name}");
            }
            hover.liftAmount = defaultLiftAmount;

            // Ajout ou mise à jour de OutlineGenerator
            OutlineGenerator outlineGen = go.GetComponent<OutlineGenerator>();
            if (outlineGen == null)
            {
                outlineGen = go.AddComponent<OutlineGenerator>();
                Debug.Log($"OutlineGenerator ajouté à: {child.name}");
            }
            outlineGen.outlineScale = defaultOutlineScale;

            // Ajout de MeshCollider si nécessaire
            if (go.GetComponent<MeshFilter>() != null && go.GetComponent<MeshCollider>() == null)
            {
                go.AddComponent<MeshCollider>();
                Debug.Log($"MeshCollider ajouté à: {child.name}");
            }

            // Traitement récursif des enfants
            ProcessChildrenRecursive(child);
        }
    }

    static bool IsExcluded(string name)
    {
        if (System.Array.Exists(exclusions, n => n == name))
        {
            Debug.Log($"Exclusion: {name}");
            return true;
        }
        return false;
    }
}

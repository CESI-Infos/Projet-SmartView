using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.IO;

public static class ToolsForChildren
{
    const float defaultLiftAmount = 0.50f;
    const float defaultOutlineScale = 1.1f;

    static readonly string[] exclusions = {
        "2e étage", "Objet vide", "Ascenseur", "local",
        "Etage 1", "terrasse", "WC", "WC.001", "WC.002",
        "WC.003", "WC.004", "RDC", "Cylindre", "Cylindre.001",
        "Cylindre.002", "Cylindre.003", "Cylindre.004", "Cylindre.005",
        "Cylindre.006", "Plan.024", "Plan.025", "Plan.026", "Plan.027", "Cube",
        "N 201.001", "N 202.001", "N 203.001", "N 204.001", "N 205.001", "N 206.001",
        "N 207.001", "N 208.001", "N 209.001", "N 210.001", "N 211.001", "N 212 A.001", "N 212 B.001",
        "N 213.001","N 214 A.001", "N 214 B.001", "N 215.001", "N 216.001", "N 217.001", "N 218.001", "N 219.001",
        "N 220.001", "N 221.001", "N 230.001", "N 231.001", "S 222.001", "S 223.001", "S 224.001", "S 225.001", "S 226.001",
        "S 227.001", "S 228.001", "S 229 A.001", "S 229 B.001"
    };

    [MenuItem("Tools/Add All Components To Children")]
    static void AddAllComponents()
    {
        foreach (GameObject parent in Selection.gameObjects)
        {
            ProcessChildrenRecursive(parent.transform);
        }

        AssetDatabase.SaveAssets();
    }

    static void ProcessChildrenRecursive(Transform parent)
    {
        foreach (Transform child in parent)
        {
            if (IsExcluded(child.name)) continue;

            GameObject go = child.gameObject;
            string salleName = child.name;

            // Lecture CSV InfosSalles.csv
            string csvPath = Path.Combine(Application.streamingAssetsPath, "InfosSalles.csv");
            string capacity = "N/A";
            string libelle = "Inconnu";

            if (File.Exists(csvPath))
            {
                string[] lines = File.ReadAllLines(csvPath);

                foreach (string line in lines)
                {
                    string[] parts = line.Split(';');
                    if (parts.Length >= 3 && parts[0].Trim() == salleName)
                    {
                        if (!string.IsNullOrWhiteSpace(parts[1]))
                            capacity = parts[1].Trim();
                        libelle = parts[2].Trim();
                        break;
                    }
                }
            }

            // CubeColor
            if (go.GetComponent<CubeColor>() == null)
            {
                go.AddComponent<CubeColor>();
                Debug.Log($"CubeColor ajouté à: {salleName}");
            }

            // HoverOutline
            HoverOutline hover = go.GetComponent<HoverOutline>();
            if (hover == null)
            {
                hover = go.AddComponent<HoverOutline>();
                Debug.Log($"HoverOutline ajouté à: {salleName}");
            }
            hover.liftAmount = defaultLiftAmount;
            EditorUtility.SetDirty(hover);

            // OutlineGenerator
            OutlineGenerator outlineGen = go.GetComponent<OutlineGenerator>();
            if (outlineGen == null)
            {
                outlineGen = go.AddComponent<OutlineGenerator>();
                Debug.Log($"OutlineGenerator ajouté à: {salleName}");
            }
            outlineGen.outlineScale = defaultOutlineScale;
            EditorUtility.SetDirty(outlineGen);

            // HoverTip
            HoverTip tip = go.GetComponent<HoverTip>();
            if (tip == null)
            {
                tip = go.AddComponent<HoverTip>();
                Debug.Log($"HoverTip ajouté à: {salleName}");
            }
            tip.tipToShow = $"NomSalle : {salleName}\nCapacité : {capacity}\nType : {libelle}";
            EditorUtility.SetDirty(tip);

            // MeshCollider
            if (go.GetComponent<MeshFilter>() != null && go.GetComponent<MeshCollider>() == null)
            {
                go.AddComponent<MeshCollider>();
                Debug.Log($"MeshCollider ajouté à: {salleName}");
            }

            // Récursivité
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

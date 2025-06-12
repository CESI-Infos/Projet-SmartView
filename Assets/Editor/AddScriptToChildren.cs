using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.IO;

public static class ToolsForChildren
{
    const float defaultLiftAmount = 0.50f;
    const float defaultOutlineScale = 1.1f;

    static readonly string[] exclusions = { 
        // Bâtiment en général
        "2e étage", "Objet vide", "Ascenseur", "local", "Etage 1", "terrasse", "WC", "WC.001", "WC.002", "WC.003", "WC.004", "WC.005", "WC.006", "WC.007", "RDC", "Cylindre", "Cylindre.001", "Cylindre.002", "Cylindre.003", "Cylindre.004", "Cylindre.005", "Cylindre.006", "Plan.024", "Plan.025", "Plan.026", "Plan.027", "Cube",  

        // Deuxième étage 
        "N 201.001", "N 202.001", "N 203.001", "N 204.001", "N 205.001", "N 206.001", "N 207.001", "N 208.001", "N 209.001", "N 210.001", "N 211.001", "N 212 A.001", "N 212 B.001", "N 213.001","N 214 A.001", "N 214 B.001", "N 215.001", "N 216.001", "N 217.001", "N 218.001", "N 219.001", "N 220.001", "N 221.001", "N 230.001", "N 231.001", 

        "S 222.001", "S 223.001", "S 224.001", "S 225.001", "S 226.001", "S 227.001", "S 228.001", "S 229 A.001", "S 229 B.001",
        
        // Premier étage 
        "S157.001", "S158.001", "S159.001", "S160.001", "S161.001", "S162.001", "S163.001",
        "N113.001", "N114.001", "N115.001", "N116.001", "N117.001", "N118.001", "N119.001", "N120.001", "N121.001", "N122.001", "N123.001", "N124.001", "N125.001", "N126.001", "N153.001", "N154.001", "N155.001", "N156.001",  "OASIS.001", "OPIBUS.001", "S157.001", "E141.001", "E142.001", "E143.001", "E144.001", "E145.001", "E146.001", "E147.001", "E148.001", "E149.001", "E150.001", "E151.001", 
        "N101A.001", "N102.001", "N103B.001", "N104A.001", "N104B.001", "N105.001", "N106.001", "N107.001", "N108.001", "N109.001", "N110.001", "N111.001", "N112.001",

        // RDC
        "E020.001", "E028.001", "E033A.001", "E033B.001", "E035.001",
        "N001.001", "N002.001", "N003.001", "N005.001", "N006.001", "N007.001", "N008.001", "N009.001", "N010.001", "N011.001", "N012.001", "N013.001", "N014.001", "N015.001", "N016.001", "N017.001", "N018.001", "N019A.001", "N019B.001",

        "E033 A.001", "E033 B.001", "N010.001", "N018.001", "N019 A.001", "N019 B.001", 

        "S036.001", "S037.001", "S038.001", "S039.001", "S040.001", "S041.001", "S042.001", "S043.001", "S044.001", "S045.001", "S046.001", "S047.001", "S048.001"
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

            if (go.GetComponent<CubeColor>() == null)
            {
                go.AddComponent<CubeColor>();
                Debug.Log($"CubeColor ajouté à: {salleName}");
            }

            HoverOutline hover = go.GetComponent<HoverOutline>();
            if (hover == null)
            {
                hover = go.AddComponent<HoverOutline>();
                Debug.Log($"HoverOutline ajouté à: {salleName}");
            }
            hover.liftAmount = defaultLiftAmount;
            EditorUtility.SetDirty(hover);

            OutlineGenerator outlineGen = go.GetComponent<OutlineGenerator>();
            if (outlineGen == null)
            {
                outlineGen = go.AddComponent<OutlineGenerator>();
                Debug.Log($"OutlineGenerator ajouté à: {salleName}");
            }
            outlineGen.outlineScale = defaultOutlineScale;
            EditorUtility.SetDirty(outlineGen);

            HoverTip tip = go.GetComponent<HoverTip>();
            if (tip == null)
            {
                tip = go.AddComponent<HoverTip>();
                Debug.Log($"HoverTip ajouté à: {salleName}");
            }

            tip.tipToShow = $"Salle : {salleName}";

            EditorUtility.SetDirty(tip);

            if (go.GetComponent<MeshFilter>() != null && go.GetComponent<MeshCollider>() == null)
            {
                go.AddComponent<MeshCollider>();
                Debug.Log($"MeshCollider ajouté à: {salleName}");
            }

            // Traitement récursif
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

    // Ajout d'une fonction helper pour formater le texte du ratio
    private static string GetRatioText() 
    {
        return "calculé dynamiquement"; // Le ratio réel sera calculé et affiché par CubeColor
    }
}

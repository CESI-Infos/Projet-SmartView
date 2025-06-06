using UnityEngine;
using UnityEditor;

public class AddMaterialToChildrenOfChildren
{
    [MenuItem("Tools/Add Material To Children of Children")]
    static void AddMaterial()
    {
        Material mat = AssetDatabase.LoadAssetAtPath<Material>("Assets/Materials/white.mat");

        if (mat == null)
        {
            Debug.LogError("Matériau non trouvé. Vérifie le chemin.");
            return;
        }

        string[] exclure = { "2e étage", "Objet vide", "Ascenseur", "box", "OASIS", "local", "Etage 1", "terasse", "terrasse", "WC", "WC.001", "WC.002", "WC.003", "WC.004", "WC.005", "WC.006", "WC.007", "RDC", "plan.092", "plan.100", "plan.102", "plan.103", "plan.104", "plan.106", "plan.107"};

        foreach (GameObject parent in Selection.gameObjects)
        {
            foreach (Transform child in parent.transform)
            {
                if (System.Array.Exists(exclure, nom => nom == child.name))
                {
                    Debug.Log($"Exclusion: {child.name}");
                    continue;
                }

                foreach (Transform grandChild in child)
                {
                    Renderer rend = grandChild.GetComponent<Renderer>();
                    if (rend != null)
                    {
                        rend.sharedMaterial = mat;
                        Debug.Log($"Matériau ajouté à : {grandChild.name}");
                    }
                    else
                    {
                        Debug.LogWarning($"Aucun Renderer sur : {grandChild.name}");
                    }
                }
            }
        }
    }
}

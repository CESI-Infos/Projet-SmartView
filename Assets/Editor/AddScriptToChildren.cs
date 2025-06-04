using UnityEngine;
using UnityEditor;

public class AddScriptToChildren
{
    [MenuItem("Tools/Add Script To Selected Parent's Children")]
    static void AddScript()
    {
        string[] exclure = { "2e étage", "Objet vide", "Ascenseur", "local", "Etage 1", "terasse", "WC", "WC.001", "WC.002", "WC.003", "WC.004", "RDC"};

        foreach (GameObject parent in Selection.gameObjects)
        {
            foreach (Transform child in parent.transform)
            {
                if (System.Array.Exists(exclure, nom => nom == child.name))
                {
                    Debug.Log($"Exclusion: {child.name}");
                    continue;
                }
                if (child.GetComponent<CubeColor>() == null)
                {
                    child.gameObject.AddComponent<CubeColor>();
                    Debug.Log($"Script ajouté à: {child.name}");
                }
            }
        }
    }
}

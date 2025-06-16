using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

#if UNITY_EDITOR
public class ClickableRoomAutomator : EditorWindow
{
    [Header("Configuration")]
    private GameObject targetFbxObject;
    private CsvDynamicReader csvReader;
    private bool addCollidersAutomatically = true;
    private bool removeExistingScripts = false;
    private bool showExcludedNames = false;
    
    // Statistiques
    private int processedCount = 0;
    private int skippedCount = 0;
    private int errorCount = 0;
    
    // Liste des noms à exclure
    private readonly HashSet<string> excludedNames = new HashSet<string>
    {
        // Éléments structurels
        "2e étage", "Objet vide", "Ascenseur", "local", "Etage 1", "terrasse",
        "WC", "WC.001", "WC.002", "WC.003", "WC.004", "WC.005", "WC.006", "WC.007",
        "RDC", "Cylindre", "Cylindre.001", "Cylindre.002", "Cylindre.003", "Cylindre.004", 
        "Cylindre.005", "Cylindre.006", "Plan.024", "Plan.025", "Plan.026", "Plan.027", "Cube",
        
        // Deuxième étage
        "N 201.001", "N 202.001", "N 203.001", "N 204.001", "N 205.001", "N 206.001", 
        "N 207.001", "N 208.001", "N 209.001", "N 210.001", "N 211.001", "N 212 A.001", 
        "N 212 B.001", "N 213.001", "N 214 A.001", "N 214 B.001", "N 215.001", "N 216.001", 
        "N 217.001", "N 218.001", "N 219.001", "N 220.001", "N 221.001", "N 230.001", "N 231.001",
        "S 222.001", "S 223.001", "S 224.001", "S 225.001", "S 226.001", "S 227.001", 
        "S 228.001", "S 229 A.001", "S 229 B.001",
        
        // Premier étage
        "S157.001", "S158.001", "S159.001", "S160.001", "S161.001", "S162.001", "S163.001",
        "N113.001", "N114.001", "N115.001", "N116.001", "N117.001", "N118.001", "N119.001", 
        "N120.001", "N121.001", "N122.001", "N123.001", "N124.001", "N125.001", "N126.001", 
        "N153.001", "N154.001", "N155.001", "N156.001", "OASIS.001", "OPIBUS.001", "S157.001",
        "E141.001", "E142.001", "E143.001", "E144.001", "E145.001", "E146.001", "E147.001", 
        "E148.001", "E149.001", "E150.001", "E151.001", "N101A.001", "N102.001", "N103B.001", 
        "N104A.001", "N104B.001", "N105.001", "N106.001", "N107.001", "N108.001", "N109.001", 
        "N110.001", "N111.001", "N112.001",
        
        // RDC
        "E020.001", "E028.001", "E033A.001", "E033B.001", "E035.001", "N001.001", "N002.001", 
        "N003.001", "N005.001", "N006.001", "N007.001", "N008.001", "N009.001", "N010.001", 
        "N011.001", "N012.001", "N013.001", "N014.001", "N015.001", "N016.001", "N017.001", 
        "N018.001", "N019A.001", "N019B.001", "E033 A.001", "E033 B.001", "N010.001", 
        "N018.001", "N019 A.001", "N019 B.001", "S036.001", "S037.001", "S038.001", 
        "S039.001", "S040.001", "S041.001", "S042.001", "S043.001", "S044.001", "S045.001", 
        "S046.001", "S047.001", "S048.001"
    };
    
    [MenuItem("Tools/Clickable Room Automator")]
    public static void ShowWindow()
    {
        GetWindow<ClickableRoomAutomator>("Clickable Room Automator");
    }
    
    private void OnGUI()
    {
        EditorGUILayout.Space(10);
        EditorGUILayout.LabelField("Clickable Room Automator", EditorStyles.boldLabel);
        EditorGUILayout.HelpBox("Cet outil applique automatiquement le script ClickableRoom à tous les enfants d'un objet .fbx, " +
                               "en excluant certains noms prédéfinis.", MessageType.Info);
        
        EditorGUILayout.Space(10);
        
        // Configuration principale
        EditorGUILayout.LabelField("Configuration", EditorStyles.boldLabel);
        
        targetFbxObject = (GameObject)EditorGUILayout.ObjectField(
            new GUIContent("Objet FBX racine", "L'objet parent contenant tous les enfants à traiter"),
            targetFbxObject, typeof(GameObject), true);
        
        csvReader = (CsvDynamicReader)EditorGUILayout.ObjectField(
            new GUIContent("CSV Dynamic Reader", "Référence vers le script CsvDynamicReader"),
            csvReader, typeof(CsvDynamicReader), true);
        
        EditorGUILayout.Space(5);
        
        // Options
        EditorGUILayout.LabelField("Options", EditorStyles.boldLabel);
        
        addCollidersAutomatically = EditorGUILayout.Toggle(
            new GUIContent("Ajouter colliders automatiquement", "Ajoute un BoxCollider si aucun collider n'est présent"),
            addCollidersAutomatically);
        
        removeExistingScripts = EditorGUILayout.Toggle(
            new GUIContent("Supprimer scripts existants", "Supprime les scripts ClickableRoom existants avant d'en ajouter de nouveaux"),
            removeExistingScripts);
        
        EditorGUILayout.Space(10);
        
        // Boutons d'action
        EditorGUILayout.BeginHorizontal();
        
        GUI.enabled = targetFbxObject != null;
        if (GUILayout.Button("Analyser l'objet", GUILayout.Height(30)))
        {
            AnalyzeObject();
        }
        
        GUI.enabled = targetFbxObject != null && csvReader != null;
        if (GUILayout.Button("Appliquer les scripts", GUILayout.Height(30)))
        {
            ApplyScripts();
        }
        
        GUI.enabled = true;
        EditorGUILayout.EndHorizontal();
        
        if (targetFbxObject != null)
        {
            EditorGUILayout.Space(5);
            if (GUILayout.Button("Supprimer tous les ClickableRoom", GUILayout.Height(25)))
            {
                RemoveAllClickableRoomScripts();
            }
        }
        
        EditorGUILayout.Space(10);
        
        // Statistiques
        if (processedCount > 0 || skippedCount > 0 || errorCount > 0)
        {
            EditorGUILayout.LabelField("Dernière opération :", EditorStyles.boldLabel);
            EditorGUILayout.LabelField($"• Objets traités : {processedCount}");
            EditorGUILayout.LabelField($"• Objets ignorés : {skippedCount}");
            if (errorCount > 0)
            {
                GUI.color = Color.red;
                EditorGUILayout.LabelField($"• Erreurs : {errorCount}");
                GUI.color = Color.white;
            }
        }
        
        EditorGUILayout.Space(10);
        
        // Section des noms exclus (repliable)
        showExcludedNames = EditorGUILayout.Foldout(showExcludedNames, $"Noms exclus ({excludedNames.Count})");
        if (showExcludedNames)
        {
            EditorGUILayout.BeginVertical("box");
            EditorGUILayout.HelpBox("Les objets avec ces noms ne recevront pas le script ClickableRoom :", MessageType.None);
            
            EditorGUILayout.BeginVertical();
            foreach (string name in excludedNames)
            {
                EditorGUILayout.LabelField($"• {name}", EditorStyles.miniLabel);
            }
            EditorGUILayout.EndVertical();
            
            EditorGUILayout.EndVertical();
        }
    }
    
    private void AnalyzeObject()
    {
        if (targetFbxObject == null)
        {
            Debug.LogError("Aucun objet FBX sélectionné !");
            return;
        }
        
        List<Transform> allChildren = GetAllChildren(targetFbxObject.transform);
        int validChildren = 0;
        int excludedChildren = 0;
        int alreadyHasScript = 0;
        
        foreach (Transform child in allChildren)
        {
            if (excludedNames.Contains(child.name))
            {
                excludedChildren++;
            }
            else if (child.GetComponent<ClickableRoom>() != null)
            {
                alreadyHasScript++;
                validChildren++;
            }
            else
            {
                validChildren++;
            }
        }
        
        string message = $"Analyse de '{targetFbxObject.name}' :\n" +
                        $"• Total d'enfants : {allChildren.Count}\n" +
                        $"• Objets valides : {validChildren}\n" +
                        $"• Objets exclus : {excludedChildren}\n" +
                        $"• Ont déjà le script : {alreadyHasScript}";
        
        Debug.Log(message);
        EditorUtility.DisplayDialog("Analyse terminée", message, "OK");
    }
    
    private void ApplyScripts()
    {
        if (targetFbxObject == null || csvReader == null)
        {
            Debug.LogError("Objet FBX ou CsvDynamicReader manquant !");
            return;
        }
        
        processedCount = 0;
        skippedCount = 0;
        errorCount = 0;
        
        List<Transform> allChildren = GetAllChildren(targetFbxObject.transform);
        
        // Démarrer le progress bar
        int totalChildren = allChildren.Count;
        
        for (int i = 0; i < totalChildren; i++)
        {
            Transform child = allChildren[i];
            
            // Mettre à jour la progress bar
            if (EditorUtility.DisplayCancelableProgressBar(
                "Application des scripts ClickableRoom", 
                $"Traitement de {child.name}...", 
                (float)i / totalChildren))
            {
                break; // L'utilisateur a annulé
            }
            
            try
            {
                if (excludedNames.Contains(child.name))
                {
                    skippedCount++;
                    Debug.Log($"Objet ignoré (nom exclu) : {child.name}");
                    continue;
                }
                
                // Supprimer le script existant si demandé
                if (removeExistingScripts)
                {
                    ClickableRoom existingScript = child.GetComponent<ClickableRoom>();
                    if (existingScript != null)
                    {
                        DestroyImmediate(existingScript);
                    }
                }
                
                // Ajouter le script ClickableRoom s'il n'existe pas déjà
                ClickableRoom clickableRoom = child.GetComponent<ClickableRoom>();
                if (clickableRoom == null)
                {
                    clickableRoom = child.gameObject.AddComponent<ClickableRoom>();
                }
                
                // Configurer le script
                clickableRoom.csvReader = csvReader;
                
                // Nettoyer le nom pour l'affichage (enlever .001, .002, etc.)
                string cleanName = CleanRoomName(child.name);
                clickableRoom.customRoomName = cleanName;
                
                // Ajouter un collider si nécessaire
                if (addCollidersAutomatically && child.GetComponent<Collider>() == null)
                {
                    BoxCollider boxCollider = child.gameObject.AddComponent<BoxCollider>();
                    
                    // Tenter d'ajuster la taille du collider basé sur le renderer
                    Renderer renderer = child.GetComponent<Renderer>();
                    if (renderer != null)
                    {
                        boxCollider.size = renderer.bounds.size;
                        boxCollider.center = renderer.bounds.center - child.transform.position;
                    }
                }
                
                processedCount++;
                Debug.Log($"Script appliqué à : {child.name} (nom affiché: {cleanName})");
            }
            catch (System.Exception e)
            {
                errorCount++;
                Debug.LogError($"Erreur lors du traitement de {child.name} : {e.Message}");
            }
        }
        
        // Fermer la progress bar
        EditorUtility.ClearProgressBar();
        
        // Marquer la scène comme modifiée
        EditorUtility.SetDirty(targetFbxObject);
        
        // Afficher le résumé
        string summary = $"Opération terminée !\n" +
                        $"• Scripts appliqués : {processedCount}\n" +
                        $"• Objets ignorés : {skippedCount}";
        
        if (errorCount > 0)
        {
            summary += $"\n• Erreurs : {errorCount}";
        }
        
        Debug.Log(summary);
        EditorUtility.DisplayDialog("Opération terminée", summary, "OK");
    }
    
    private void RemoveAllClickableRoomScripts()
    {
        if (targetFbxObject == null)
        {
            Debug.LogError("Aucun objet FBX sélectionné !");
            return;
        }
        
        if (!EditorUtility.DisplayDialog("Confirmation", 
            "Êtes-vous sûr de vouloir supprimer tous les scripts ClickableRoom de cet objet et ses enfants ?", 
            "Oui", "Annuler"))
        {
            return;
        }
        
        List<Transform> allChildren = GetAllChildren(targetFbxObject.transform);
        int removedCount = 0;
        
        foreach (Transform child in allChildren)
        {
            ClickableRoom[] scripts = child.GetComponents<ClickableRoom>();
            foreach (ClickableRoom script in scripts)
            {
                DestroyImmediate(script);
                removedCount++;
            }
        }
        
        EditorUtility.SetDirty(targetFbxObject);
        
        string message = $"Suppression terminée !\n{removedCount} script(s) ClickableRoom supprimé(s).";
        Debug.Log(message);
        EditorUtility.DisplayDialog("Suppression terminée", message, "OK");
    }
    
    private List<Transform> GetAllChildren(Transform parent)
    {
        List<Transform> children = new List<Transform>();
        GetAllChildrenRecursive(parent, children);
        return children;
    }
    
    private void GetAllChildrenRecursive(Transform parent, List<Transform> children)
    {
        for (int i = 0; i < parent.childCount; i++)
        {
            Transform child = parent.GetChild(i);
            children.Add(child);
            GetAllChildrenRecursive(child, children);
        }
    }
    
    private string CleanRoomName(string originalName)
    {
        // Enlever les suffixes .001, .002, etc. ajoutés par Blender/Unity
        if (originalName.Contains("."))
        {
            string[] parts = originalName.Split('.');
            if (parts.Length >= 2)
            {
                string suffix = parts[parts.Length - 1];
                // Vérifier si le suffixe est numérique
                if (int.TryParse(suffix, out _))
                {
                    return string.Join(".", parts, 0, parts.Length - 1);
                }
            }
        }
        
        return originalName;
    }
}
#endif
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using System.IO;
using System.Globalization;
using System.Text;

public class CsvDynamicReader : MonoBehaviour
{
    [Header("UI References")]
    public TMP_InputField dateInputField;
    public TMP_Text displayText;
    public GameObject displayPanel;
    public Button closeButton;
    public Canvas canvasToHide;

    [Header("Configuration")]
    public string csvFileName = "ConvertedData.csv";

    [Header("Debug Options")]
    [Tooltip("Désactiver les logs de debug dans la console")]
    public bool disableDebugLogs = true;

    private string csvPath;
    private string[] csvLines;
    private char separator;
    private string currentTargetRoom = "";
    private DateTime currentTargetDate;
    private bool currentIsMorning = true;
    private bool pendingRoomClick = false;

    void Start()
    {
        // Initialiser le chemin du fichier CSV
        csvPath = Path.Combine(Application.streamingAssetsPath, csvFileName);

        // Vérifier si le fichier existe
        if (!File.Exists(csvPath))
        {
            Debug.LogError("CSV file not found at: " + csvPath);
            return;
        }

        // Charger le fichier CSV une seule fois
        LoadCsvFile();

        // Configurer l'InputField
        if (dateInputField != null)
        {
            // Ajouter un listener pour détecter les changements
            dateInputField.onValueChanged.AddListener(OnDateInputChanged);

            // Optionnel : définir une date par défaut
            if (string.IsNullOrEmpty(dateInputField.text))
            {
                dateInputField.text = DateTime.Now.ToString("dd/MM/yyyy") + " AM";
            }

            // Traiter la valeur initiale
            OnDateInputChanged(dateInputField.text);
        }
        else
        {
            Debug.LogError("DateInputField n'est pas assigné dans l'inspecteur!");
        }

        // Initialiser le panel comme masqué
        if (displayPanel != null)
        {
            displayPanel.SetActive(false);
        }

        // Configurer le bouton de fermeture
        if (closeButton != null)
        {
            closeButton.onClick.AddListener(ClosePanel);
        }
    }

    void LoadCsvFile()
    {
        try
        {
            csvLines = File.ReadAllLines(csvPath);

            if (csvLines.Length < 2)
            {
                Debug.LogWarning("CSV file is empty or missing data.");
                return;
            }

            // Détecter le séparateur
            separator = csvLines[0].Contains(";") ? ';' : ',';

            if (!disableDebugLogs)
                Debug.Log($"CSV loaded successfully. {csvLines.Length} lines found.");
        }
        catch (Exception e)
        {
            Debug.LogError($"Error loading CSV file: {e.Message}");
        }
    }

    public void OnRoomClicked(string roomName)
    {
        if (!disableDebugLogs)
            Debug.Log($"Salle cliquée : {roomName}");
            
        currentTargetRoom = roomName;
        pendingRoomClick = true; // ← on vient d'un clic

        if (!string.IsNullOrEmpty(dateInputField.text))
        {
            OnDateInputChanged(dateInputField.text);
        }
    }

    void OnDateInputChanged(string inputValue)
    {
        if (string.IsNullOrEmpty(inputValue) || csvLines == null)
            return;

        if (!ParseDateInput(inputValue, out DateTime targetDate, out bool isMorning))
        {
            Debug.LogWarning($"Format de date invalide: {inputValue}.");
            return;
        }

        currentTargetDate = targetDate;
        currentIsMorning = isMorning;

        // Affiche les données si une salle est sélectionnée ET (qu'on vient d'un clic OU que le panel est déjà ouvert)
        if (!string.IsNullOrEmpty(currentTargetRoom) && (pendingRoomClick || (displayPanel != null && displayPanel.activeSelf)))
        {
            SearchAndDisplayData(currentTargetDate, currentIsMorning, currentTargetRoom);
            pendingRoomClick = false; // reset après affichage
        }
    }

    bool ParseDateInput(string input, out DateTime targetDate, out bool isMorning)
    {
        targetDate = DateTime.MinValue;
        isMorning = true;

        // Nettoyer l'input
        input = input.Trim().ToUpper();

        // Vérifier si l'input contient AM ou PM
        bool hasAM = input.EndsWith(" AM");
        bool hasPM = input.EndsWith(" PM");

        if (!hasAM && !hasPM)
        {
            return false;
        }

        // Extraire la partie date
        string dateStr = hasAM ? input.Substring(0, input.Length - 3) : input.Substring(0, input.Length - 3);

        // Parser la date
        if (DateTime.TryParseExact(dateStr.Trim(), "dd/MM/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out targetDate))
        {
            isMorning = hasAM;
            return true;
        }

        return false;
    }

    void SearchAndDisplayData(DateTime targetDate, bool isMorning, string targetRoom)
    {
        if (!disableDebugLogs)
            Debug.Log($"=== Recherche pour {targetDate:dd/MM/yyyy} {(isMorning ? "AM" : "PM")} dans la salle {targetRoom} ===");

        int matchCount = 0;
        StringBuilder displayContent = new StringBuilder();

        // Récupérer les informations de la salle depuis CubeColor
        CubeColor cubeColor = FindCubeColorByRoomName(targetRoom);
        string typeSalle = cubeColor != null ? cubeColor.Infos.ContainsKey("LibelleTypeSalle") ? cubeColor.Infos["LibelleTypeSalle"].ToString() : "Non défini" : "Non défini";
        float ratio = cubeColor != null ? cubeColor.ratio : -1f;

        // En-tête du panel avec le nouveau format
        displayContent.AppendLine($"<size=18><b>Salle : {targetRoom}, Type Salle : {typeSalle}</b></size>");
        displayContent.AppendLine($"<size=14>Date : {targetDate:dd/MM/yyyy} {(isMorning ? "AM" : "PM")}</size>");
        displayContent.AppendLine("─────────────────────────────────");

        for (int i = 1; i < csvLines.Length; i++)
        {
            string[] cols = csvLines[i].Split(separator);

            // Vérifier qu'on a assez de colonnes (au moins 15 pour l'index 14)
            if (cols.Length < 15) continue;

            // Récupérer les colonnes nécessaires
            string debutSeanceStr = cols[1].Trim('\uFEFF').Trim();  // "DebutSeance" [1]
            string nomSalle = cols[6].Trim();                       // "NomSalle" [6]

            // Parser la date de début de séance
            if (DateTime.TryParseExact(debutSeanceStr, "dd/MM/yyyy HH:mm:ss:fff", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime debutDate))
            {
                bool sameDay = debutDate.Date == targetDate.Date;
                bool isAM = debutDate.Hour < 12;

                // Vérifier si les critères correspondent
                if (sameDay && isAM == isMorning && nomSalle == targetRoom)
                {
                    matchCount++;

                    // Limiter l'affichage détaillé aux 2 premières séances
                    if (matchCount <= 2)
                    {
                        // Extraire les colonnes nécessaires
                        string debutHeure = cols[4].Trim();         // "Début" [4]
                        string finHeure = cols[5].Trim();           // "Fin" [5]
                        string nombreInscrit = cols[10].Trim();     // "NombreInscrit" [10]
                        string capaciteSalle = cols[7].Trim();      // "CapaciteSalle" [7]
                        string codeAnalytique = cols[9].Trim();     // "CodeAnalytique" [9]
                        string nomSession = cols[8].Trim();         // "NomSession" [8]
                        string intervenant = cols[11].Trim();       // "Intervenant" [11]
                        string assistanteAdmin = cols[14].Trim();   // "AssistanteAdministrative" [14]

                        displayContent.AppendLine($"<b>Séance {matchCount} - Heure de {debutHeure} - {finHeure}</b>");
                        
                        // Afficher le ratio d'occupation avec la couleur correspondante
                        string ratioText = ratio >= 0 ? $"{ratio.ToString("P1", CultureInfo.InvariantCulture)}" : "N/A";
                        string ratioColor = GetRatioColor(ratio);
                        displayContent.AppendLine($"<color={ratioColor}>Ratio d'occupation : {ratioText}</color>");
                        
                        // Informations avec charte graphique cohérente
                        displayContent.AppendLine($"• Etudiants inscrits : <color=#FFD700>{nombreInscrit}</color>");
                        displayContent.AppendLine($"• Capacité Salle : <color=#FFA500>{capaciteSalle}</color>");
                        displayContent.AppendLine($"• Code analytique : <color=#FFBF00>{codeAnalytique}</color>");
                        displayContent.AppendLine($"• Produit de la promotion : <color=#DAA520>{nomSession}</color>");
                        displayContent.AppendLine($"• Intervenant : <color=#F4A460>\"{intervenant}\"</color>");
                        displayContent.AppendLine($"• Assistant Administratif : <color=#CD853F>\"{assistanteAdmin}\"</color>");
                        displayContent.AppendLine("─────────────────────────────────");
                    }
                    else if (matchCount == 3)
                    {
                        // À partir de la séance 3, afficher le message de référence à Bora
                        displayContent.AppendLine("<color=#FF6B6B><b>Pour visualiser le reste, veuillez vous référer à la requête Bora</b></color>");
                        break; // Sortir de la boucle
                    }
                }
            }
        }

        // Afficher les résultats dans le panel
        if (matchCount == 0)
        {
            displayContent.AppendLine("<color=red>Aucune donnée trouvée pour ces critères.</color>");

            if (!disableDebugLogs)
                Debug.LogWarning($"Aucune donnée trouvée pour la salle {targetRoom} le {targetDate:dd/MM/yyyy} {(isMorning ? "AM" : "PM")}.");
        }
        else
        {
            if (!disableDebugLogs)
                Debug.Log($"→ {matchCount} ligne(s) trouvée(s) pour {targetRoom} le {targetDate:dd/MM/yyyy} {(isMorning ? "AM" : "PM")}");
        }

        // Mettre à jour l'affichage UI
        UpdateDisplayPanel(displayContent.ToString());
    }

    // Méthode pour obtenir la couleur hexadécimale du ratio basée sur la logique de CubeColor
    private string GetRatioColor(float ratio)
    {
        if (ratio == -1.0f)
        {
            // neutralRatioColor = new Color(254.0f / 255.0f, 225.0f / 255.0f, 49.0f / 255.0f, 1.0f);
            return "#FEE131"; // Jaune neutre
        }
        else if (ratio > 1.0f)
        {
            // bordeauRatioColor = new Color(109f / 255f, 7.0f / 255f, 26.0f / 255.0f, 1.0f);
            return "#6D071A"; // Bordeaux
        }
        else if (ratio == 0.0f)
        {
            // zeroRatioColor = new Color(1.0f, 0.0f, 185.0f / 255.0f, 1.0f);
            return "#FF00B9"; // Rose/Magenta
        }
        else if (ratio > 0.0f && ratio < 1.0f / 3.0f)
        {
            // redRatioColor = new Color(1.0f, 0.0f, 0.0f, 1.0f);
            return "#FF0000"; // Rouge
        }
        else if (ratio >= 1.0f / 3.0f && ratio < 2.0f / 3.0f)
        {
            // orangeRatioColor = new Color(1.0f, 0.5f, 0.0f, 1.0f);
            return "#FF8000"; // Orange
        }
        else if (ratio >= 2.0f / 3.0f && ratio < 1.0f)
        {
            // greenRatioColor = new Color(0.0f, 1.0f, 0.0f, 1.0f);
            return "#00FF00"; // Vert
        }
        
        // Par défaut, retourner la couleur neutre
        return "#FEE131";
    }

    // Méthode pour trouver le CubeColor correspondant à une salle
    private CubeColor FindCubeColorByRoomName(string roomName)
    {
        GameObject roomObject = GameObject.Find(roomName);
        if (roomObject != null)
        {
            return roomObject.GetComponent<CubeColor>();
        }
        return null;
    }

    void UpdateDisplayPanel(string content)
    {
        // Masquer le canvas si assigné
        if (canvasToHide != null)
        {
            canvasToHide.gameObject.SetActive(false);
        }

        if (displayText != null)
        {
            displayText.text = content;
        }

        if (displayPanel != null)
        {
            displayPanel.SetActive(true);
        }
    }

    public void ClosePanel()
    {
        if (displayPanel != null)
        {
            displayPanel.SetActive(false);
        }

        // Réafficher le canvas si assigné
        if (canvasToHide != null)
        {
            canvasToHide.gameObject.SetActive(true);
        }
    }

    void OnDestroy()
    {
        // Nettoyer les listeners
        if (dateInputField != null)
        {
            dateInputField.onValueChanged.RemoveListener(OnDateInputChanged);
        }

        if (closeButton != null)
        {
            closeButton.onClick.RemoveListener(ClosePanel);
        }
    }
}
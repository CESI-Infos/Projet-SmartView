using UnityEngine;
using System.Collections;

public class ClickableRoom : MonoBehaviour
{
    [Header("Configuration")]
    [Tooltip("Référence vers le CsvDynamicReader - glissez le GameObject qui contient le script")]
    public CsvDynamicReader csvReader;
    
    [Header("Optionnel - Nom personnalisé")]
    [Tooltip("Laissez vide pour utiliser le nom du GameObject")]
    public string customRoomName;
    
    [Header("Animation Settings")]
    [Tooltip("Facteur d'agrandissement au survol")]
    public float hoverScale = 1.05f;
    
    [Tooltip("Facteur d'agrandissement au clic")]
    public float clickScale = 0.95f;
    
    [Tooltip("Durée des animations")]
    public float animationDuration = 0.1f;
    
    [Tooltip("Couleur de surbrillance au survol")]
    public Color hoverColor = Color.white;
    
    [Tooltip("Désactiver les animations de couleur pour préserver les matériaux")]
    public bool disableColorAnimation = true;
    
    private Vector3 originalScale;
    private Color originalColor;
    private Renderer objectRenderer;
    private Material originalMaterial;
    private bool isAnimating = false;
    private bool isHovering = false;
    
    void Start()
    {
        // Auto-trouver le CsvDynamicReader si pas assigné
        if (csvReader == null)
        {
            csvReader = FindFirstObjectByType<CsvDynamicReader>();
            if (csvReader == null)
            {
                Debug.LogError($"CsvDynamicReader not found in scene for {gameObject.name}");
            }
        }
        
        // Vérifier qu'il y a un collider pour les clics
        if (GetComponent<Collider>() == null)
        {
            Debug.LogWarning($"Aucun Collider trouvé sur {gameObject.name}. Ajoutez un Collider pour rendre l'objet cliquable.");
        }
        
        // Sauvegarder les valeurs originales
        originalScale = transform.localScale;
        
        // Récupérer le renderer pour les effets de couleur
        objectRenderer = GetComponent<Renderer>();
        if (objectRenderer != null && !disableColorAnimation)
        {
            // Créer une copie du matériau pour éviter de modifier l'original
            originalMaterial = objectRenderer.material;
            objectRenderer.material = new Material(originalMaterial);
            originalColor = objectRenderer.material.color;
        }
    }
    
    private void OnMouseDown()
    {
        // Vérifier si le csvReader est assigné
        if (csvReader != null && !isAnimating)
        {
            // Utiliser le nom personnalisé ou le nom du GameObject
            string roomName = string.IsNullOrEmpty(customRoomName) ? gameObject.name : customRoomName;
            csvReader.OnRoomClicked(roomName);
            
            // Animation de clic
            StartCoroutine(ClickAnimation());
        }
        else if (csvReader == null)
        {
            Debug.LogError($"CsvReader not assigned to {gameObject.name}");
        }
    }
    
    private void OnMouseEnter()
    {
        if (!isAnimating)
        {
            isHovering = true;
            StartCoroutine(HoverEnterAnimation());
        }
    }
    
    private void OnMouseExit()
    {
        if (!isAnimating)
        {
            isHovering = false;
            StartCoroutine(HoverExitAnimation());
        }
    }
    
    private IEnumerator HoverEnterAnimation()
    {
        isAnimating = true;
        
        Vector3 targetScale = originalScale * hoverScale;
        Vector3 startScale = transform.localScale;
        
        float elapsedTime = 0f;
        
        while (elapsedTime < animationDuration)
        {
            elapsedTime += Time.deltaTime;
            float t = elapsedTime / animationDuration;
            
            // Animation de scale avec courbe smooth
            transform.localScale = Vector3.Lerp(startScale, targetScale, EaseOutQuart(t));
            
            // Animation de couleur uniquement si activée
            if (objectRenderer != null && !disableColorAnimation)
            {
                Color currentColor = Color.Lerp(originalColor, hoverColor, EaseOutQuart(t));
                objectRenderer.material.color = currentColor;
            }
            
            yield return null;
        }
        
        transform.localScale = targetScale;
        if (objectRenderer != null && !disableColorAnimation)
        {
            objectRenderer.material.color = hoverColor;
        }
        
        isAnimating = false;
    }
    
    private IEnumerator HoverExitAnimation()
    {
        isAnimating = true;
        
        Vector3 startScale = transform.localScale;
        Color startColor = (objectRenderer != null && !disableColorAnimation) ? objectRenderer.material.color : originalColor;
        
        float elapsedTime = 0f;
        
        while (elapsedTime < animationDuration)
        {
            elapsedTime += Time.deltaTime;
            float t = elapsedTime / animationDuration;
            
            // Animation de scale avec courbe smooth
            transform.localScale = Vector3.Lerp(startScale, originalScale, EaseOutQuart(t));
            
            // Animation de couleur uniquement si activée
            if (objectRenderer != null && !disableColorAnimation)
            {
                Color currentColor = Color.Lerp(startColor, originalColor, EaseOutQuart(t));
                objectRenderer.material.color = currentColor;
            }
            
            yield return null;
        }
        
        transform.localScale = originalScale;
        if (objectRenderer != null && !disableColorAnimation)
        {
            objectRenderer.material.color = originalColor;
        }
        
        isAnimating = false;
    }
    
    private IEnumerator ClickAnimation()
    {
        isAnimating = true;
        
        Vector3 startScale = transform.localScale;
        Vector3 clickTargetScale = originalScale * clickScale;
        Vector3 finalScale = isHovering ? originalScale * hoverScale : originalScale;
        
        // Phase 1: Rétrécir
        float elapsedTime = 0f;
        while (elapsedTime < animationDuration / 2)
        {
            elapsedTime += Time.deltaTime;
            float t = elapsedTime / (animationDuration / 2);
            
            transform.localScale = Vector3.Lerp(startScale, clickTargetScale, EaseOutQuart(t));
            
            yield return null;
        }
        
        // Phase 2: Revenir à la taille normale (ou hover si toujours en survol)
        elapsedTime = 0f;
        while (elapsedTime < animationDuration / 2)
        {
            elapsedTime += Time.deltaTime;
            float t = elapsedTime / (animationDuration / 2);
            
            transform.localScale = Vector3.Lerp(clickTargetScale, finalScale, EaseOutQuart(t));
            
            yield return null;
        }
        
        transform.localScale = finalScale;
        isAnimating = false;
    }
    
    // Fonction d'easing pour des animations plus fluides
    private float EaseOutQuart(float t)
    {
        return 1 - Mathf.Pow(1 - t, 4);
    }
    
    // Méthode pour réinitialiser l'objet (utile pour le debug)
    [ContextMenu("Reset to Original State")]
    public void ResetToOriginalState()
    {
        StopAllCoroutines();
        transform.localScale = originalScale;
        
        if (objectRenderer != null && !disableColorAnimation)
        {
            objectRenderer.material.color = originalColor;
        }
        
        isAnimating = false;
        isHovering = false;
    }
    
    void OnDestroy()
    {
        // Restaurer le matériau original si on en a créé une copie
        if (objectRenderer != null && originalMaterial != null && !disableColorAnimation)
        {
            objectRenderer.material = originalMaterial;
        }
    }
}
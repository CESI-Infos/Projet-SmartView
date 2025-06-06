using UnityEngine;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class OutlineGenerator : MonoBehaviour
{
    [Tooltip("Matériau avec un shader URP Unlit noir + Culling = Front")]
    public Material outlineMaterial;

    [Tooltip("Facteur d'agrandissement de l'outline")]
    public float outlineScale = 1.03f;

    private GameObject outlineObject;

    void Start()
    {
        if (outlineMaterial == null)
        {
            Debug.LogError("Aucun matériau d'outline assigné !");
            return;
        }

        // Créer l'objet enfant
        outlineObject = new GameObject("OutlineObject");
        outlineObject.transform.SetParent(transform);
        outlineObject.transform.localPosition = Vector3.zero;
        outlineObject.transform.localRotation = Quaternion.identity;
        outlineObject.transform.localScale = Vector3.one * outlineScale;

        // Copier MeshFilter et MeshRenderer
        MeshFilter originalFilter = GetComponent<MeshFilter>();
        MeshRenderer originalRenderer = GetComponent<MeshRenderer>();

        MeshFilter outlineFilter = outlineObject.AddComponent<MeshFilter>();
        outlineFilter.sharedMesh = originalFilter.sharedMesh;

        MeshRenderer outlineRenderer = outlineObject.AddComponent<MeshRenderer>();
        outlineRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        outlineRenderer.receiveShadows = false;
        outlineRenderer.material = outlineMaterial;

        // (Optionnel) désactiver au démarrage
        outlineObject.SetActive(false);
    }

    public void EnableOutline(bool enabled)
    {
        if (outlineObject != null)
        {
            outlineObject.SetActive(enabled);
        }
    }
}


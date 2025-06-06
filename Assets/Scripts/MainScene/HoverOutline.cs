using UnityEngine;
using UnityEngine.EventSystems;

[RequireComponent(typeof(OutlineGenerator))]
public class HoverOutline : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public float liftAmount = 1f;
    public float liftSpeed = 2f;

    private OutlineGenerator outline;

        private Vector3 originalLocalPosition;
private Vector3 targetLocalPosition;

void Start()
{
    originalLocalPosition = transform.localPosition;
    targetLocalPosition = originalLocalPosition;

    outline = GetComponent<OutlineGenerator>();
    outline.EnableOutline(false);
}

void Update()
{
    transform.localPosition = Vector3.Lerp(transform.localPosition, targetLocalPosition, Time.deltaTime * liftSpeed);
}

public void OnPointerEnter(PointerEventData eventData)
{
    targetLocalPosition = originalLocalPosition + Vector3.up * liftAmount;
    outline.EnableOutline(true);
}

public void OnPointerExit(PointerEventData eventData)
{
    targetLocalPosition = originalLocalPosition;
    outline.EnableOutline(false);
}

}

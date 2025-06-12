using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using System.Globalization;
public class HoverTip : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public string tipToShow;
    private float timetoWait = 0.5f;
    public void OnPointerEnter(PointerEventData eventData)
    {
        StopAllCoroutines();
        StartCoroutine(StartTimer());
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        StopAllCoroutines();
        HoverTipManager.OnMouseLoseFocus();
    }

    private void ShowMessage()
    {
        var cubeColor = GetComponent<CubeColor>();
        if (cubeColor != null)
        {
            string ratioText = cubeColor.ratio >= 0 ? $"{cubeColor.ratio.ToString("P1", CultureInfo.InvariantCulture)}" : "N/A";
            string message = $"NomSalle : {gameObject.name}\nRatio d'occupation : {ratioText}";
            HoverTipManager.OnMouseHover(message, Input.mousePosition);
        }
        else
        {
            HoverTipManager.OnMouseHover(tipToShow, Input.mousePosition);
        }
    }

    private IEnumerator StartTimer()
    {
        yield return new WaitForSeconds(timetoWait);
        ShowMessage();
    }
}

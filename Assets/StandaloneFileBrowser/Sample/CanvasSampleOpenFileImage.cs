using System.Text;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using SFB;
using UnityEngine.Networking;

[RequireComponent(typeof(Button))]
public class CanvasSampleOpenFileImage : MonoBehaviour, IPointerDownHandler {
    public RawImage output;

#if UNITY_WEBGL && !UNITY_EDITOR
    //
    // WebGL
    //
    [DllImport("__Internal")]
    private static extern void UploadFile(string gameObjectName, string methodName, string filter, bool multiple);

    public void OnPointerDown(PointerEventData eventData) {
        UploadFile(gameObject.name, "OnFileUpload", ".png, .jpg", false);
    }

    // Called from browser
    public void OnFileUpload(string url) {
        StartCoroutine(OutputRoutine(url));
    }
#else
    //
    // Standalone platforms & editor
    //
    public void OnPointerDown(PointerEventData eventData) { }

    void Start() {
        var button = GetComponent<Button>();
        button.onClick.AddListener(OnClick);
    }

    private void OnClick() {
        var paths = StandaloneFileBrowser.OpenFilePanel("Title", "", ".png", false);
        if (paths.Length > 0) {
            StartCoroutine(OutputRoutine(new System.Uri(paths[0]).AbsoluteUri));
        }
    }
#endif

    private IEnumerator OutputRoutine(string url) {
        UnityWebRequest loader = UnityWebRequestTexture.GetTexture(url);
        yield return loader.SendWebRequest();

        #if UNITY_2020_1_OR_NEWER
        if (loader.result != UnityWebRequest.Result.Success)
        #else
        if (loader.isNetworkError || loader.isHttpError)
        #endif
        {
            Debug.LogError("Erreur lors du chargement de l'image : " + loader.error);
        }
        else
        {
            Texture2D texture = DownloadHandlerTexture.GetContent(loader);
            output.texture = texture;
        }
    }
}
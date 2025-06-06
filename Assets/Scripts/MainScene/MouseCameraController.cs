using UnityEngine;

public class MouseCameraController : MonoBehaviour
{
    public float rotationSensitivity = 3f;
    public float zoomSpeed = 50f;

    private Vector3 lastMousePosition;

    void Update()
    {
        HandleRotation();
    }

    void HandleRotation()
    {
        if (Input.GetMouseButton(1)) // Clic droit maintenuu
        {
            Vector3 delta = Input.mousePosition - lastMousePosition;
            float rotationX = -delta.y * rotationSensitivity * Time.deltaTime;
            float rotationY = delta.x * rotationSensitivity * Time.deltaTime;
            transform.eulerAngles += new Vector3(rotationX, rotationY, 0f);
        }

        lastMousePosition = Input.mousePosition;
    }
}

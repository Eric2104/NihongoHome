using UnityEngine;
using UnityEngine.InputSystem;

public class FirstPersonLook : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform eyePoint;

    [Header("Look Settings")]
    [SerializeField] private float lookSensitivity = 0.15f;
    [SerializeField] private float minPitch = -80f;
    [SerializeField] private float maxPitch = 80f;
    [SerializeField] private float dragThreshold = 10f; 

    [Header("Camera Follow")]
    [SerializeField] private float positionSmoothSpeed = 15f;

    private float yaw;
    private float pitch;
    private Vector2 lastPos;
    private Vector2 pressPos;
    private bool isDragging;

    private void Start()
    {
        if (eyePoint != null)
        {
            yaw = transform.eulerAngles.y;
            pitch = transform.eulerAngles.x;
        }
    }

    private void LateUpdate()
    {
        UpdateLook();
        UpdatePosition();
    }

    private void UpdateLook()
    {
        if (Pointer.current == null) return;

        var pointer = Pointer.current;
        Vector2 currentPos = pointer.position.ReadValue();

        if (pointer.press.wasPressedThisFrame)
        {
            pressPos = currentPos;
            lastPos = currentPos;
            isDragging = false;
        }

        if (pointer.press.isPressed)
        {
            if (!isDragging && Vector2.Distance(pressPos, currentPos) > dragThreshold)
            {
                isDragging = true;
                lastPos = currentPos;
            }

            if (isDragging)
            {
                Vector2 delta = currentPos - lastPos;
                lastPos = currentPos;

                yaw += delta.x * lookSensitivity;
                pitch -= delta.y * lookSensitivity;
                pitch = Mathf.Clamp(pitch, minPitch, maxPitch);

                transform.rotation = Quaternion.Euler(pitch, yaw, 0f);
            }
        }

        if (pointer.press.wasReleasedThisFrame)
        {
            isDragging = false;
        }
    }

    private void UpdatePosition()
    {
        if (eyePoint != null)
        {
            transform.position = Vector3.Lerp(transform.position, eyePoint.position, positionSmoothSpeed * Time.deltaTime);
        }
    }
}
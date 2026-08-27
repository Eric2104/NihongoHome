using System.Collections;
using UnityEngine;

public class InteractionManager : MonoBehaviour
{
    [Header("Referencias")]
    [SerializeField] private Camera mainCamera;
    
    [Header("Configuración de Interacción")]
    [SerializeField] private float interactDistance = 5f;
    [SerializeField] private LayerMask interactableLayer;

    [Header("Configuración de Outline (Suavizado)")]
    [SerializeField] private float maxOutlineWidth = 5f;
    [SerializeField] private float fadeSpeed = 10f;

    private Outline currentOutline;
    private Coroutine fadeCoroutine; // Esta variable evita el bug de que se quede encendido

    private void Update()
    {
        DetectInteractables();
    }

    private void DetectInteractables()
    {
        // 1. Lanza el rayo estrictamente desde el centro físico de la cámara hacia el frente
        Ray ray = new Ray(mainCamera.transform.position, mainCamera.transform.forward);
        
        // 2. Línea de depuración: Te dibujará una línea roja en la pestaña "Scene" para que veas el rayo
        Debug.DrawRay(ray.origin, ray.direction * interactDistance, Color.red);

        // 3. Detecta colisiones
        if (Physics.Raycast(ray, out RaycastHit hit, interactDistance, interactableLayer))
        {
            Outline outline = hit.collider.GetComponent<Outline>();

            if (outline != null)
            {
                // Si estamos mirando a un objeto interactivo NUEVO
                if (currentOutline != outline)
                {
                    // Si ya había otro objeto encendido, lo apagamos de golpe para evitar errores
                    if (currentOutline != null)
                    {
                        if (fadeCoroutine != null) StopCoroutine(fadeCoroutine);
                        currentOutline.OutlineWidth = 0f;
                        currentOutline.enabled = false;
                    }

                    // Encendemos suavemente el objeto actual
                    currentOutline = outline;
                    currentOutline.enabled = true; 
                    
                    if (fadeCoroutine != null) StopCoroutine(fadeCoroutine);
                    fadeCoroutine = StartCoroutine(FadeOutline(currentOutline, maxOutlineWidth));
                }
            }
        }
        else
        {
            // Si nuestro rayo deja de tocar el objeto (miramos al vacío o a otro lado)
            if (currentOutline != null)
            {
                // Detenemos el encendido e iniciamos el apagado suave
                if (fadeCoroutine != null) StopCoroutine(fadeCoroutine);
                fadeCoroutine = StartCoroutine(FadeOutline(currentOutline, 0f));
                
                // Vaciamos la variable para que el sistema sepa que ya no miramos nada
                currentOutline = null; 
            }
        }
    }

    private IEnumerator FadeOutline(Outline targetOutline, float targetWidth)
    {
        if (targetOutline == null) yield break;

        // Suavizado matemático hasta llegar al valor objetivo
        while (Mathf.Abs(targetOutline.OutlineWidth - targetWidth) > 0.05f)
        {
            targetOutline.OutlineWidth = Mathf.Lerp(targetOutline.OutlineWidth, targetWidth, Time.deltaTime * fadeSpeed);
            yield return null;
        }

        // Fijar el valor final exacto
        targetOutline.OutlineWidth = targetWidth;

        // Desactivar el componente si se apagó del todo para ahorrar memoria
        if (targetWidth == 0f)
        {
            targetOutline.enabled = false;
        }
    }
}
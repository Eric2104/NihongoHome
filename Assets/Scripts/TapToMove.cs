using UnityEngine;
using UnityEngine.AI;
using UnityEngine.InputSystem;

public class TapToMove : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private NavMeshAgent player;
    [SerializeField] private Camera mainCamera;
    [SerializeField] private GameObject destinationIndicatorPrefab;

    [Header("Settings")]
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private float tapThreshold = 20f;
    [SerializeField] private float indicatorHeight = 0.03f;
    [SerializeField] private float tapTimeLimit = 0.3f;

    private GameObject indicatorInstance;
    private Vector2 pressPos;
    private float pressTime;
    private bool isPressing;

    private void Start()
    {
        if (destinationIndicatorPrefab != null)
        {
            indicatorInstance = Instantiate(destinationIndicatorPrefab);
            indicatorInstance.name = "DestinationIndicator_Runtime";
            indicatorInstance.SetActive(false);
        }
    }

    private void Update()
    {
        HandleInput();
        CheckDestinationReached();
    }

    private void HandleInput()
    {
        if (Pointer.current == null) return;

        var pointer = Pointer.current;

        if (pointer.press.wasPressedThisFrame)
        {
            pressPos = pointer.position.ReadValue();
            pressTime = Time.time;
            isPressing = true;
        }
        else if (pointer.press.wasReleasedThisFrame && isPressing)
        {
            Vector2 releasePos = pointer.position.ReadValue();
            isPressing = false;

            if (Time.time - pressTime <= tapTimeLimit && Vector2.Distance(pressPos, releasePos) <= tapThreshold)
            {
                MoveTo(releasePos);
            }
        }
    }

    private void MoveTo(Vector2 screenPos)
    {
        Ray ray = mainCamera.ScreenPointToRay(screenPos);
        if (Physics.Raycast(ray, out RaycastHit hit, 1000f, groundLayer))
        {
            if (NavMesh.SamplePosition(hit.point, out NavMeshHit navHit, 2f, NavMesh.AllAreas))
            {
                player.SetDestination(navHit.position);
                if (indicatorInstance != null)
                {
                    indicatorInstance.transform.position = navHit.position + Vector3.up * indicatorHeight;
                    indicatorInstance.SetActive(true);
                }
            }
        }
    }

    private void CheckDestinationReached()
    {
        if (indicatorInstance != null && indicatorInstance.activeSelf)
        {
            if (!player.pathPending)
            {
                if (player.remainingDistance <= player.stoppingDistance)
                {
                    if (!player.hasPath || player.velocity.sqrMagnitude == 0f)
                    {
                        indicatorInstance.SetActive(false);
                    }
                }
            }
        }
    }
}
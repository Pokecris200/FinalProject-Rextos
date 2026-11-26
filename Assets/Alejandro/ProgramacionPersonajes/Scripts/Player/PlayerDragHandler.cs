using UnityEngine;

/// <summary>
/// Finds a nearby IDraggable and moves it along with the player,
/// keeping the offset (direction and distance) it had when the drag started.
/// </summary>
[RequireComponent(typeof(Rigidbody))]
public class PlayerDragHandler : MonoBehaviour
{
    [SerializeField] private float _detectionRadius = 2f;
    [SerializeField] private LayerMask _draggableLayers = ~0;

    private Rigidbody _playerRigidbody;
    private IDraggable _currentDraggable;
    private Vector3 _dragOffset; // World-space offset from the player to the object at grab time.

    public bool IsDragging => _currentDraggable != null;

    private void Awake()
    {
        _playerRigidbody = GetComponent<Rigidbody>();
    }

    private void OnValidate()
    {
        _detectionRadius = Mathf.Max(0f, _detectionRadius);
    }

    /// <summary>Grabs the closest draggable in range. Returns true if one was grabbed.</summary>
    public bool TryStartDrag()
    {
        if (IsDragging)
        {
            return false;
        }

        Collider[] hits = Physics.OverlapSphere(
            transform.position, _detectionRadius, _draggableLayers, QueryTriggerInteraction.Ignore);

        IDraggable closest = null;
        float closestSqrDistance = float.MaxValue;

        foreach (Collider hit in hits)
        {
            IDraggable candidate = hit.GetComponentInParent<IDraggable>();
            if (candidate == null)
            {
                continue;
            }

            float sqrDistance = (candidate.DragRigidbody.position - transform.position).sqrMagnitude;
            if (sqrDistance < closestSqrDistance)
            {
                closestSqrDistance = sqrDistance;
                closest = candidate;
            }
        }

        if (closest == null)
        {
            return false;
        }

        // Store the offset once; it is never recalculated while dragging.
        _dragOffset = closest.DragRigidbody.position - transform.position;
        _currentDraggable = closest;
        _currentDraggable.OnDragStarted();
        return true;
    }

    /// <summary>Releases the current object (it stays frozen where it is).</summary>
    public void StopDrag()
    {
        if (!IsDragging)
        {
            return;
        }

        _currentDraggable.OnDragEnded();
        _currentDraggable = null;
    }

    private void FixedUpdate()
    {
        if (!IsDragging)
        {
            return;
        }

        Rigidbody draggedBody = _currentDraggable.DragRigidbody;
        Vector3 targetPosition = transform.position + _dragOffset;

        // Match the player's velocity and add a correction that closes any gap to the target.
        Vector3 correction = (targetPosition - draggedBody.position) / Time.fixedDeltaTime;
        draggedBody.linearVelocity = _playerRigidbody.linearVelocity + correction;
    }
}

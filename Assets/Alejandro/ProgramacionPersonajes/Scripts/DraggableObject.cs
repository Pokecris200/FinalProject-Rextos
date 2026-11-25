using UnityEngine;

/// <summary>
/// Basic draggable object. Its Rigidbody is "disabled" (kinematic) until the
/// player starts dragging it, and frozen again as soon as it is released.
/// </summary>
[RequireComponent(typeof(Rigidbody))]
public class DraggableObject : MonoBehaviour, IDraggable
{
    private Rigidbody _rigidbody;

    public Rigidbody DragRigidbody => _rigidbody;

    private void Awake()
    {
        _rigidbody = GetComponent<Rigidbody>();

        // The Rigidbody has no "enabled" flag, so kinematic is used to make it ignore physics.
        _rigidbody.isKinematic = true;
    }

    /// <summary>Enables physics so the player can move the object through its velocity.</summary>
    public void OnDragStarted()
    {
        _rigidbody.isKinematic = false;
    }

    /// <summary>Stops the object and freezes it in place.</summary>
    public void OnDragEnded()
    {
        // Velocity can only be reset while the body is non-kinematic, so do it first.
        _rigidbody.linearVelocity = Vector3.zero;
        _rigidbody.angularVelocity = Vector3.zero;
        _rigidbody.isKinematic = true;
    }
}

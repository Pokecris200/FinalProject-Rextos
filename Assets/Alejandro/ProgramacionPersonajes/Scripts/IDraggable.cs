using UnityEngine;

/// <summary>
/// Contract for any object the player can drag.
/// The player only depends on this interface, never on a concrete draggable type.
/// </summary>
public interface IDraggable
{
    /// <summary>Rigidbody used by the player to move the object while it is dragged.</summary>
    Rigidbody DragRigidbody { get; }

    /// <summary>Called by the player when the drag begins.</summary>
    void OnDragStarted();

    /// <summary>Called by the player when the drag ends.</summary>
    void OnDragEnded();
}

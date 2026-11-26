using UnityEngine;

/// <summary>
/// Contract for anything the player can pick up (body parts, keys, etc.).
/// The player only knows this interface, never the concrete collectible type.
/// </summary>
public interface ICollectible
{
    /// <summary>Called by the player when it touches the collectible.</summary>
    /// <param name="collector">The GameObject that picked it up.</param>
    void Collect(GameObject collector);
}

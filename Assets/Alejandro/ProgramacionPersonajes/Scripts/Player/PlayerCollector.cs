using UnityEngine;

/// <summary>
/// Detects collectibles touched by the player and tells them they were collected.
/// It knows nothing about what each collectible does.
/// </summary>
public class PlayerCollector : MonoBehaviour
{
    // Requires the collectible's collider to be a trigger and the player to have a Rigidbody.
    private void OnTriggerEnter(Collider other)
    {
        ICollectible collectible = other.GetComponentInParent<ICollectible>();

        if (collectible != null)
        {
            collectible.Collect(gameObject);
        }
    }
}
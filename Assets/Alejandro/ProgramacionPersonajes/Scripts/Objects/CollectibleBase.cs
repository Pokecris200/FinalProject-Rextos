using UnityEngine;

/// <summary>
/// Base class for every collectible. It handles the shared flow:
/// disappear if already collected, apply the specific effect, record it and destroy itself.
/// </summary>
public abstract class CollectibleBase : MonoBehaviour, ICollectible
{
    // Must be unique across all scenes (for example "torso_scene1").
    [SerializeField] private string _collectibleId;

    private bool _isCollected;

    protected virtual void Start()
    {
        // If it was collected in a previous visit to this scene, it must not reappear.
        bool alreadyCollected = !string.IsNullOrEmpty(_collectibleId)
            && GameManager.Instance != null
            && GameManager.Instance.IsCollected(_collectibleId);

        if (alreadyCollected)
        {
            Destroy(gameObject);
        }
    }

    private void OnValidate()
    {
        // An empty ID would make this collectible impossible to track between scenes.
        if (string.IsNullOrEmpty(_collectibleId))
        {
            Debug.LogWarning($"{name}: collectible ID is empty.", this);
        }
    }

    /// <summary>Shared pickup flow. Subclasses only implement OnCollected.</summary>
    public void Collect(GameObject collector)
    {
        // Guards against two colliders triggering the pickup in the same frame.
        if (_isCollected)
        {
            return;
        }

        // The subclass decides whether the pickup actually happened.
        if (!OnCollected(collector))
        {
            return;
        }

        _isCollected = true;

        if (GameManager.Instance != null && !string.IsNullOrEmpty(_collectibleId))
        {
            GameManager.Instance.MarkCollected(_collectibleId);
        }

        Destroy(gameObject);
    }

    /// <summary>Applies the collectible's effect. Returns true if it was consumed.</summary>
    protected abstract bool OnCollected(GameObject collector);
}
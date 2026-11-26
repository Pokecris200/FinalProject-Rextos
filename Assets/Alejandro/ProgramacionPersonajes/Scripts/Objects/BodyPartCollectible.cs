using UnityEngine;

/// <summary>
/// Collectible that gives the player a body part, which changes the player's model.
/// </summary>
public class BodyPartCollectible : CollectibleBase
{
    [SerializeField] private BodyPartType _bodyPartType;

    protected override bool OnCollected(GameObject collector)
    {
        // Only a collector with a model switcher can use a body part.
        if (!collector.TryGetComponent(out PlayerModelSwitcher modelSwitcher))
        {
            return false;
        }

        // Returns false if the part was already owned: nothing happens and the object stays.
        return modelSwitcher.TryCollectBodyPart(_bodyPartType);
    }
}

using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Persistent singleton that keeps the game progress between scenes:
/// which body parts were collected and which collectibles were already picked up.
/// Place one in every scene; duplicates destroy themselves, so any scene can be played on its own.
/// </summary>
public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    // One bool per BodyPartType value, indexed by the enum's integer value.
    private bool[] _hasBodyPart;

    // IDs of every collectible already picked up, so it doesn't reappear in revisited scenes.
    private readonly HashSet<string> _collectedIds = new HashSet<string>();

    private void Awake()
    {
        // Keep only the first instance alive.
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        // Sized from the enum itself, so no edits are needed if it grows.
        _hasBodyPart = new bool[Enum.GetValues(typeof(BodyPartType)).Length];

        // Head is the default model, so the player always owns it.
        _hasBodyPart[(int)BodyPartType.Head] = true;
    }

    /// <summary>Returns true if the given body part was already collected.</summary>
    public bool HasBodyPart(BodyPartType partType)
    {
        return _hasBodyPart[(int)partType];
    }

    /// <summary>Marks a body part as collected. Returns false if it was already collected.</summary>
    public bool TryCollectBodyPart(BodyPartType partType)
    {
        int index = (int)partType;
        if (_hasBodyPart[index])
        {
            return false;
        }

        _hasBodyPart[index] = true;
        return true;
    }

    /// <summary>Finds the most advanced collected part (highest enum value). False if none.</summary>
    public bool TryGetMostAdvancedBodyPart(out BodyPartType partType)
    {
        for (int i = _hasBodyPart.Length - 1; i >= 0; i--)
        {
            if (_hasBodyPart[i])
            {
                partType = (BodyPartType)i;
                return true;
            }
        }

        partType = default;
        return false;
    }

    /// <summary>Returns true if the collectible with this ID was already picked up.</summary>
    public bool IsCollected(string collectibleId)
    {
        return _collectedIds.Contains(collectibleId);
    }

    /// <summary>Records a collectible as picked up.</summary>
    public void MarkCollected(string collectibleId)
    {
        _collectedIds.Add(collectibleId);
    }
}
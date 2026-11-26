using System;
using UnityEngine;

/// <summary>
/// Swaps the player's visual model according to the collected body parts.
/// The model of the most advanced collected part is the one shown.
/// </summary>
public class PlayerModelSwitcher : MonoBehaviour
{
    /// <summary>Links a body part with the prefab used as the player's model.</summary>
    [Serializable]
    private struct BodyPartModel
    {
        [SerializeField] private BodyPartType _partType;
        [SerializeField] private GameObject _prefab;

        public BodyPartType PartType => _partType;
        public GameObject Prefab => _prefab;
    }

    [Tooltip("Child of the player where the model is placed.")]
    [SerializeField] private Transform _modelHolder;

    [Tooltip("Default model already placed under the holder. It is replaced on the first swap.")]
    [SerializeField] private GameObject _currentModel;

    [Tooltip("One entry per body part (max 5).")]
    [SerializeField] private BodyPartModel[] _bodyPartModels;

    [SerializeField] private PlayerColorChanger _colorChanger;

    private const int MaxModels = 5;

    private void OnValidate()
    {
        // Keeps the list within the design limit.
        if (_bodyPartModels != null && _bodyPartModels.Length > MaxModels)
        {
            Array.Resize(ref _bodyPartModels, MaxModels);
        }
    }

    private void Start()
    {
        // Restore the model from the progress kept between scenes.
        RefreshModel();
    }

    /// <summary>
    /// Registers a new body part and updates the model.
    /// Returns false (and does nothing) if the part was already collected.
    /// </summary>
    public bool TryCollectBodyPart(BodyPartType partType)
    {
        if (GameManager.Instance == null || !GameManager.Instance.TryCollectBodyPart(partType))
        {
            return false;
        }

        RefreshModel();
        return true;
    }

    /// <summary>Shows the model of the most advanced collected part. Keeps the default if none.</summary>
    private void RefreshModel()
    {
        if (GameManager.Instance == null
            || !GameManager.Instance.TryGetMostAdvancedBodyPart(out BodyPartType partType))
        {
            return;
        }

        GameObject prefab = FindPrefab(partType);
        if (prefab == null)
        {
            Debug.LogWarning($"No prefab assigned for {partType}.", this);
            return;
        }

        // Replace the previous model with the new one at the holder's origin.
        if (_currentModel != null)
        {
            Destroy(_currentModel);
        }

        _currentModel = Instantiate(prefab, _modelHolder);
        _currentModel.transform.localPosition = Vector3.zero;
        _currentModel.transform.localRotation = Quaternion.identity;

        // The color changer must tint the new model; it re-applies the tint if it is active.
        if (_colorChanger != null)
        {
            _colorChanger.SetTargetRenderer(_currentModel.GetComponentInChildren<Renderer>());
        }
    }

    private GameObject FindPrefab(BodyPartType partType)
    {
        foreach (BodyPartModel entry in _bodyPartModels)
        {
            if (entry.PartType == partType)
            {
                return entry.Prefab;
            }
        }

        return null;
    }
}

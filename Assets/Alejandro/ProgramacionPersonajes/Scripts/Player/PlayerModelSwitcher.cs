using System;
using UnityEngine;

/// <summary>
/// Swaps the player's visual model according to the collected body parts by enabling
/// and disabling models that already exist as children of the ModelHolder.
/// The model of the most advanced collected part is the one shown (Head is the default).
/// </summary>
public class PlayerModelSwitcher : MonoBehaviour
{
    /// <summary>Links a body part with the prefab used as the player's model.</summary>
    [Serializable]
    private struct BodyPartModel
    {
        [SerializeField] private BodyPartType _partType;
        [SerializeField] private GameObject _model;

        public BodyPartType PartType => _partType;
        public GameObject Model => _model;
    }

    [Tooltip("Child of the player where the model is placed.")]
    [SerializeField] private Transform _modelHolder; 

    [Tooltip("One entry per body part (max 5).")]
    [SerializeField] private BodyPartModel[] _bodyPartModels;

    [SerializeField] private PlayerColorChanger _colorChanger;

    private GameObject _currentModel;

    private const int MaxModels = 5;
    private const float PositionTolerance = 0.001f; // Meters.
    private const float RotationTolerance = 0.1f;   // Degrees.

    private void OnValidate()
    {
        // Keeps the list within the design limit.
        if (_bodyPartModels != null && _bodyPartModels.Length > MaxModels)
        {
            Array.Resize(ref _bodyPartModels, MaxModels);
        }
    }


    private void Awake()
    {
        if (_modelHolder == null)
        {
            Debug.LogError("Model holder is not assigned.", this);
            return;
        }

        // Start with every model disabled so only one has an active collider at a time.
        foreach (BodyPartModel entry in _bodyPartModels)
        {
            GameObject model = entry.Model;

            if (!IsUsable(model))
            {
                // Never touch an object that is not under the holder (it could be a prefab asset).
                Debug.LogError($"Model for {entry.PartType} is missing or is not a child of the holder.", this);
                continue;
            }

            WarnIfMisplaced(model);
            model.SetActive(false);
        }
    }


    private void Start()
    {
        // Show the model matching the progress kept between scenes.
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

        GameObject nextModel = FindModel(partType);
        if (nextModel == null)
        {
            Debug.LogWarning($"No valid model assigned for {partType}.", this);
            return;
        }

        // Disable the previous model first: its colliders leave physics immediately.
        if (_currentModel != null)
        {
            _currentModel.SetActive(false);
        }

        nextModel.SetActive(true);
        _currentModel = nextModel;

        // The color changer must tint the new model; it re-applies the tint if it is active.
        if (_colorChanger != null)
        {
            _colorChanger.SetTargetRenderer(nextModel.GetComponentInChildren<Renderer>());
        }
    }


    private GameObject FindModel(BodyPartType partType)
    {
        foreach (BodyPartModel entry in _bodyPartModels)
        {
            if (entry.PartType == partType && IsUsable(entry.Model))
            {
                return entry.Model;
            }
        }

        return null;
    }


    /// <summary>A model is usable only if it exists and is a child of the holder.</summary>
    private bool IsUsable(GameObject model)
    {
        return model != null && model.transform.IsChildOf(_modelHolder);
    }


    /// <summary>Warns (without changing anything) if a model is not at the holder's origin.</summary>
    private void WarnIfMisplaced(GameObject model)
    {
        Transform modelTransform = model.transform;

        bool hasPositionOffset = modelTransform.localPosition.sqrMagnitude > PositionTolerance * PositionTolerance;
        bool hasRotationOffset = Quaternion.Angle(modelTransform.localRotation, Quaternion.identity) > RotationTolerance;

        if (hasPositionOffset || hasRotationOffset)
        {
            Debug.LogWarning(
                $"{model.name} is not at the holder's origin " +
                $"(local position {modelTransform.localPosition}, local rotation {modelTransform.localEulerAngles}).",
                model);
        }
    }

    /// <summary>
    /// Executes model switch logic by input. 
    /// For testing purposes only. 
    /// Returns to initial model after reaching "complete" model in switch logic
    ///</summary>
    public void DebugModelSwitch()
    {
        BodyPartType partType = BodyPartType.Torso;
        

        GameObject prefab = FindModel(partType);
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
        switch (partType) {
            case BodyPartType.Head:
                partType = BodyPartType.Torso;
                break;


            case BodyPartType.Torso:
                partType = BodyPartType.Arms;
                break;

            case BodyPartType.Arms:
                partType = BodyPartType.Legs;
                break;

            case BodyPartType.Legs:
                partType = BodyPartType.Head;
                break;

            default:
                break;
        }


        // The color changer must tint the new model; it re-applies the tint if it is active.
        if (_colorChanger != null)
        {
            _colorChanger.SetTargetRenderer(_currentModel.GetComponentInChildren<Renderer>());
        }
    }

}

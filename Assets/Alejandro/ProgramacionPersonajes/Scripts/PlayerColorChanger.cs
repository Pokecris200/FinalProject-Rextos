using UnityEngine;

/// <summary>
/// Changes the character's material color while the color input is held.
/// If another input interrupts it, the color is restored and the action is locked for a while.
/// </summary>
public class PlayerColorChanger : MonoBehaviour
{
    private const string ActiveColorHex = "#34dbeb";

    [SerializeField] private Renderer _targetRenderer;
    [SerializeField] private float _cooldownDuration = 1f;

    private Color _baseColor;
    private Color _activeColor;
    private bool _isColorActive;
    private float _cooldownTimer;

    public bool isColorActive
    {
        get => _isColorActive;
    }

    /// <summary>True as long as the cooldown time has not passed.</summary>
    public bool IsOnCooldown { get; private set; }

    private void Awake()
    {
        if (_targetRenderer == null)
        {
            _targetRenderer = GetComponentInChildren<Renderer>();
        }

        // Cache the original color so it can be restored later.
        _baseColor = _targetRenderer.material.color;

        if (!ColorUtility.TryParseHtmlString(ActiveColorHex, out _activeColor))
        {
            Debug.LogError($"Invalid color hex: {ActiveColorHex}", this);
        }
    }

    private void OnValidate()
    {
        _cooldownDuration = Mathf.Max(0f, _cooldownDuration);
    }

    private void Update()
    {
        if (!IsOnCooldown)
        {
            return;
        }

        // Count down; the flag stays true until the time has passed.
        _cooldownTimer -= Time.deltaTime;
        if (_cooldownTimer <= 0f)
        {
            IsOnCooldown = false;
        }
    }

    /// <summary>Applies the active color. Ignored while on cooldown.</summary>
    public void Activate()
    {
        if (IsOnCooldown)
        {
            return;
        }

        _isColorActive = true;
        _targetRenderer.material.color = _activeColor;
    }

    /// <summary>Restores the base color without triggering the cooldown (input released).</summary>
    public void Deactivate()
    {
        _isColorActive = false;
        _targetRenderer.material.color = _baseColor;
    }

    /// <summary>
    /// Call when a different input happens. If the color is active, it is
    /// restored and the cooldown starts.
    /// </summary>
    public void NotifyOtherInput()
    {
        if (!_isColorActive)
        {
            return;
        }

        Deactivate();
        IsOnCooldown = true;
        _cooldownTimer = _cooldownDuration;
    }
}
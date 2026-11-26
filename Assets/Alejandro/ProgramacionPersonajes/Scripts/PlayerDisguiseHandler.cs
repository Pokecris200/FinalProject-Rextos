using UnityEngine;

public class PlayerDisguiseHandler : MonoBehaviour
{
    //Pending: animator use.
    //[SerializeField] private Animator _targetAnimator;
    [SerializeField] private float _cooldownDuration = 1f;

    private bool _isDisguiseActive;
    private float _cooldownTimer;

    public bool isDisguiseActive
    {
        get => _isDisguiseActive;
    }

    /// <summary>True as long as the cooldown time has not passed.</summary>
    public bool IsOnCooldown { get; private set; }

    //Commented while animator starts being implemented
    private void Awake()
    {
        //if (_targetAnimator == null)
        //{
        //    _targetAnimator = GetComponentInChildren<Animator>();
        //}
        //else
        //{
        //    Debug.LogError("No Animator asigned.");
        //}    
        
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

    /// <summary>Applies the active disguise. Ignored while on cooldown.</summary>
    public void Activate()
    {
        if (IsOnCooldown)
        {
            return;
        }

        _isDisguiseActive = true;
    }

    /// <summary>Restores the base disguise without triggering the cooldown (input released).</summary>
    public void Deactivate()
    {
        _isDisguiseActive = false;
        IsOnCooldown = true;
        _cooldownTimer = _cooldownDuration;
    }

    /// <summary>
    /// Call when an input different from move happens. If the disguise is active, it is
    /// removed and the cooldown starts.
    /// </summary>
    public void NotifyOtherInput()
    {
        if (!_isDisguiseActive)
        {
            return;
        }

        Deactivate();
    }
}
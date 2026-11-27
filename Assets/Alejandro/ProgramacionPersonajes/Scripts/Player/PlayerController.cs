using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Reads the InputSystem actions and forwards them to the player components.
/// It contains no gameplay logic of its own.
/// </summary>
[RequireComponent(typeof(PlayerMovement))]
[RequireComponent(typeof(PlayerDragHandler))]
[RequireComponent(typeof(PlayerDisguiseHandler))]
[RequireComponent(typeof(PlayerModelSwitcher))]
public class PlayerController : MonoBehaviour
{
    [Header("Input Actions")]
    [SerializeField] private InputActionReference _moveAction;   // Vector2
    [SerializeField] private InputActionReference _dragAction;   // Button (hold to drag)
    [SerializeField] private InputActionReference _disguiseAction;  // Button (hold to change disguise)
    [SerializeField] private InputActionReference _switchAction;  // Button (press to change model)

    private PlayerMovement _movement;
    private PlayerDragHandler _dragHandler;
    private PlayerDisguiseHandler _disguiseHandler;
    private PlayerModelSwitcher _modelSwitcher;



    private void Awake()
    {
        _movement = GetComponent<PlayerMovement>();
        _dragHandler = GetComponent<PlayerDragHandler>();
        _disguiseHandler = GetComponent<PlayerDisguiseHandler>();
        _modelSwitcher = GetComponent<PlayerModelSwitcher>();
    }

    private void OnEnable()
    {
        _moveAction.action.performed += OnMovePerformed;
        _moveAction.action.canceled += OnMoveCanceled;
        _dragAction.action.performed += OnDragPerformed;
        _dragAction.action.canceled += OnDragCanceled;
        _disguiseAction.action.performed += OnDisguisePerformed;
        _disguiseAction.action.canceled += OnDisguiseCanceled;
        _switchAction.action.performed += OnSwitchPerformed;
        _switchAction.action.canceled += OnSwitchCancelled;

        _moveAction.action.Enable();
        _dragAction.action.Enable();
        _disguiseAction.action.Enable();
        _switchAction.action.Enable();
    }

    private void OnDisable()
    {
        _moveAction.action.performed -= OnMovePerformed;
        _moveAction.action.canceled -= OnMoveCanceled;
        _dragAction.action.performed -= OnDragPerformed;
        _dragAction.action.canceled -= OnDragCanceled;
        _disguiseAction.action.performed -= OnDisguisePerformed;
        _disguiseAction.action.canceled -= OnDisguiseCanceled;
        _switchAction.action.performed -= OnSwitchPerformed;
        _switchAction.action.canceled -= OnSwitchCancelled;

        _moveAction.action.Disable();
        _dragAction.action.Disable();
        _disguiseAction.action.Disable();

    }

    private void OnMovePerformed(InputAction.CallbackContext context)
    {
        if (!_disguiseHandler.isDisguiseActive)
        {
            Vector2 direction = context.ReadValue<Vector2>();
            _movement.SetDirection(direction);
        }
    }

    private void OnMoveCanceled(InputAction.CallbackContext context)
    {
        _movement.SetDirection(Vector2.zero);
    }

    private void OnDragPerformed(InputAction.CallbackContext context)
    {
        // Starting a drag counts as a different input for the disguise action.
        _disguiseHandler.NotifyOtherInput();
        _dragHandler.TryStartDrag();
    }

    private void OnDragCanceled(InputAction.CallbackContext context)
    {
        _dragHandler.StopDrag();
    }

    private void OnDisguisePerformed(InputAction.CallbackContext context)
    {
        _disguiseHandler.Activate();
    }

    private void OnDisguiseCanceled(InputAction.CallbackContext context)
    {
        _disguiseHandler.Deactivate();
    }

    private void OnSwitchPerformed(InputAction.CallbackContext context)
    {
        Debug.Log("Switch Performed");
        _modelSwitcher.DebugModelSwitch();
    }

    private void OnSwitchCancelled(InputAction.CallbackContext context)
    {
        Debug.Log("SwitchFinished");
    }
}

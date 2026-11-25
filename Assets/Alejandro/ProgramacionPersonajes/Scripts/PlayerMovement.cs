using UnityEngine;

/// <summary>
/// Moves the character on the horizontal plane by setting the Rigidbody's linear velocity.
/// </summary>
[RequireComponent(typeof(Rigidbody))]
public class PlayerMovement : MonoBehaviour
{
    [SerializeField] private float _speed = 5f;

    private Rigidbody _rigidbody;
    private Vector2 _direction;

    /// <summary>Movement speed. Negative values are clamped to zero.</summary>
    public float Speed
    {
        get => _speed;
        set => _speed = Mathf.Max(0f, value);
    }

    /// <summary>True while the movement input is not zero.</summary>
    public bool IsMoving => _direction.sqrMagnitude > 0f;

    private void Awake()
    {
        _rigidbody = GetComponent<Rigidbody>();

        // The character never moves vertically.
        _rigidbody.constraints |= RigidbodyConstraints.FreezePositionY;
    }

    // Keeps the value valid when it is edited in the Inspector.
    private void OnValidate()
    {
        _speed = Mathf.Max(0f, _speed);
    }

    /// <summary>Sets the movement direction. X maps to world X, Y maps to world Z.</summary>
    public void SetDirection(Vector2 direction)
    {
        // Clamp so diagonal or oversized input never exceeds the configured speed.
        _direction = Vector2.ClampMagnitude(direction, 1f);
    }

    private void FixedUpdate()
    {
        // Convert the 2D input to the horizontal plane (Y stays at 0).
        Vector3 velocity = new Vector3(_direction.x, 0f, _direction.y) * Speed;
        _rigidbody.linearVelocity = velocity;
    }
}

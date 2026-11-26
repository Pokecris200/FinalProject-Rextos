using UnityEngine;

[RequireComponent(typeof(PlayerController))]
[RequireComponent(typeof(PlayerMovement))]
[RequireComponent(typeof(Rigidbody))]

public class PlayerCapture : MonoBehaviour
{
    [Header("Estado de captura - Debug")]
    [SerializeField] private bool isCaptured;

    private PlayerController playerController;
    private PlayerMovement playerMovement;
    private Rigidbody playerRigidbody;

    public bool IsCaptured => isCaptured;

    private void Awake()
    {
        playerController = GetComponent<PlayerController>();
        playerMovement = GetComponent<PlayerMovement>();
        playerRigidbody = GetComponent<Rigidbody>();
    }

    // Bloquear al jugador cuando un enemigo lo detecta
    public void Capture()
    {
        // Evitar capturas duplicadas
        if (isCaptured)
            return;

        isCaptured = true;

        // Desactivar las entradas del jugador
        playerController.enabled = false;

        // Limpiar la ultima direccion de movimiento
        playerMovement.SetDirection(Vector2.zero);

        // Desactivar el componente de movimiento
        playerMovement.enabled = false;

        // Detener completamente el Rigidbody
        playerRigidbody.linearVelocity = Vector3.zero;
        playerRigidbody.angularVelocity = Vector3.zero;

        // Impedir desplazamientos y rotaciones externas
        playerRigidbody.constraints =
            RigidbodyConstraints.FreezeAll;

        Debug.Log("Jugador capturado", this);
    }
}

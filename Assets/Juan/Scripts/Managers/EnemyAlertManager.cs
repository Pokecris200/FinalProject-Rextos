using UnityEngine;

public class EnemyAlertManager : MonoBehaviour
{
    public static EnemyAlertManager Instance { get; private set; }

    [Header("Estado actual - Debug")]

    [SerializeField]
    private EnemyAI assignedGuard;

    [SerializeField]
    private bool alertActive;

    public bool AlertActive => alertActive;

    private void Awake()
    {
        // Garantizar un unico gestor de alertas por escena
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    // Recibir una alerta desde una camara de seguridad
    public bool RaiseAlert(Transform target)
    {
        if (target == null || alertActive)
            return false;

        // Evitar alertas si la partida ya termino
        if (GameOverManager.Instance != null &&
            GameOverManager.Instance.IsGameOver)
        {
            return false;
        }

        // Buscar todos los guardias activos de la escena
        EnemyAI[] guards = FindObjectsByType<EnemyAI>(
            FindObjectsSortMode.None);

        EnemyAI closestGuard = null;
        float shortestDistance = Mathf.Infinity;

        // Comparar las rutas navegables de los guardias
        foreach (EnemyAI guard in guards)
        {
            if (!guard.TryGetAlertPathDistance(
                target,
                out float pathDistance))
            {
                continue;
            }

            if (pathDistance < shortestDistance)
            {
                shortestDistance = pathDistance;
                closestGuard = guard;
            }
        }

        // Comprobar si existe un guardia disponible
        if (closestGuard == null)
        {
            Debug.LogWarning(
                "Alerta: no hay guardias disponibles " +
                "con una ruta valida hacia el jugador.",
                this);

            return false;
        }

        // Enviar la alerta al guardia seleccionado
        if (!closestGuard.ReceiveSecurityAlert(target))
        {
            Debug.LogWarning(
                "El guardia seleccionado no pudo recibir la alerta.",
                this);

            return false;
        }

        assignedGuard = closestGuard;
        alertActive = true;

        Debug.Log(
            "Alerta de seguridad asignada a: " +
            assignedGuard.name +
            " | Distancia de recorrido: " +
            shortestDistance.ToString("F2"),
            this);

        return true;
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }
}
using UnityEngine;

public class EnemyVision : MonoBehaviour
{
    [Header("Referencias")]
    [SerializeField] private Transform eyePoint;
    [SerializeField] private Transform player;

    [Header("Configuracion de vision")]
    [SerializeField] private float viewDistance = 5f;
    [SerializeField] private bool use3DVision = false;

    [Range(0f, 360f)]
    [SerializeField] private float viewAngle = 90f;

    [Header("Vision vertical de guardias")]
    [Range(0f, 180f)]
    [SerializeField] private float verticalViewAngle = 120f;

    [Min(0f)]
    [SerializeField] private float maxHeightDifference = 5f;

    [Header("Obstaculos")]
    [SerializeField] private LayerMask obstacleMask;

    public bool CanSeePlayer { get; private set; }
    public float ViewDistance => viewDistance;
    public float ViewAngle => viewAngle;
    public Transform EyePoint => eyePoint;
    public LayerMask ObstacleMask => obstacleMask;
    public Transform Player => player;
    public bool Use3DVision => use3DVision;

    public float VerticalViewAngle => verticalViewAngle;
    public float MaxHeightDifference => maxHeightDifference;

    private void Update()
    {
        CanSeePlayer = DetectPlayer();
    }

    private bool DetectPlayer()
    {
        if (player == null || eyePoint == null)
            return false;

        if (!player.gameObject.activeInHierarchy)
            return false;

        Vector3 directionToPlayer =
            player.position - eyePoint.position;

        float distanceToPlayer =
            directionToPlayer.magnitude;

        // Vision tridimensional para camaras de seguridad
        if (use3DVision)
        {
            if (distanceToPlayer > viewDistance)
                return false;

            if (Vector3.Angle(
                eyePoint.forward,
                directionToPlayer) > viewAngle / 2f)
            {
                return false;
            }
        }

        // Vision horizontal y vertical para guardias
        else
        {
            Vector3 horizontalDirection =
                Vector3.ProjectOnPlane(
                    directionToPlayer, Vector3.up);

            float horizontalDistance =
                horizontalDirection.magnitude;

            // La altura no reduce el alcance horizontal
            if (horizontalDistance > viewDistance)
                return false;

            // Limitar la diferencia de altura
            if (Mathf.Abs(directionToPlayer.y)
                > maxHeightDifference)
            {
                return false;
            }

            Vector3 horizontalForward =
                Vector3.ProjectOnPlane(
                    eyePoint.forward, Vector3.up);

            // Comprobar el angulo horizontal
            if (horizontalDistance > 0.001f &&
                Vector3.Angle(
                    horizontalForward,
                    horizontalDirection) > viewAngle / 2f)
            {
                return false;
            }

            // Calcular el angulo vertical hacia el jugador
            float verticalAngle = Mathf.Atan2(
                directionToPlayer.y,
                horizontalDistance) * Mathf.Rad2Deg;

            if (Mathf.Abs(verticalAngle)
                > verticalViewAngle / 2f)
            {
                return false;
            }
        }

        // Comprobar obstaculos mediante un Raycast 3D
        if (Physics.Raycast(
            eyePoint.position,
            directionToPlayer.normalized,
            distanceToPlayer,
            obstacleMask,
            QueryTriggerInteraction.Ignore))
        {
            return false;
        }

        return true;
    }

    private void OnDrawGizmos()
    {
        if (eyePoint == null)
            return;

        Gizmos.color = CanSeePlayer
            ? Color.red
            : Color.green;

        Vector3 origin = eyePoint.position;

        Vector3 forward = Vector3.ProjectOnPlane(
            eyePoint.forward,
            Vector3.up).normalized;

        int segments = 40;
        Vector3 previousPoint = origin;

        // Dibujar los limites horizontales de vision
        for (int i = 0; i <= segments; i++)
        {
            float angle = Mathf.Lerp(
                -viewAngle / 2f,
                viewAngle / 2f,
                (float)i / segments);

            Vector3 direction =
                Quaternion.AngleAxis(
                    angle, Vector3.up) * forward;

            Vector3 point =
                origin + direction * viewDistance;

            Gizmos.DrawLine(previousPoint, point);

            previousPoint = point;
        }

        Gizmos.DrawLine(previousPoint, origin);

        // Dibujar los limites verticales de los guardias
        if (!use3DVision)
        {
            float upperHeight = Mathf.Min(
                maxHeightDifference,
                viewDistance * Mathf.Tan(
                    verticalViewAngle / 2f * Mathf.Deg2Rad));

            Vector3 upperPoint =
                origin + forward * viewDistance
                + Vector3.up * upperHeight;

            Vector3 lowerPoint =
                origin + forward * viewDistance
                - Vector3.up * upperHeight;

            Gizmos.DrawLine(origin, upperPoint);
            Gizmos.DrawLine(origin, lowerPoint);
            Gizmos.DrawLine(upperPoint, lowerPoint);
        }

        // Dibujar una linea hacia el jugador detectado
        if (CanSeePlayer && player != null)
        {
            Gizmos.DrawLine(origin, player.position);
        }
    }

    [ContextMenu("Diagnosticar vision")]
    private void DiagnoseVision()
    {
        if (player == null || eyePoint == null)
        {
            Debug.LogWarning("Faltan referencias", this);
            return;
        }

        Vector3 direction = player.position - eyePoint.position;

        float distance = direction.magnitude;

        float angle = Vector3.Angle(
            eyePoint.forward, direction);

        bool blocked = Physics.Raycast(
            eyePoint.position,
            direction.normalized,
            out RaycastHit hit,
            distance,
            obstacleMask,
            QueryTriggerInteraction.Ignore);

        Debug.Log(
            $"Jugador: {player.name}\n" +
            $"Activo: {player.gameObject.activeInHierarchy}\n" +
            $"Vision 3D: {use3DVision}\n" +
            $"Distancia: {distance:F2} / {viewDistance}\n" +
            $"Angulo: {angle:F2} / {viewAngle / 2f}\n" +
            $"Obstaculo: {(blocked ? hit.collider.name : "Ninguno")}\n" +
            $"Detectado: {DetectPlayer()}",
            this);
    }
}

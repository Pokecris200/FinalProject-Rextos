
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

    [SerializeField] private LayerMask obstacleMask;

    public bool CanSeePlayer { get; private set; }
    public float ViewDistance => viewDistance;
    public float ViewAngle => viewAngle;
    public Transform EyePoint => eyePoint;
    public LayerMask ObstacleMask => obstacleMask;
    public Transform Player => player;
    public bool Use3DVision => use3DVision;

    private void Update()
    {
        CanSeePlayer = DetectPlayer();
    }

    
    private bool DetectPlayer()
    {
        if (player == null || eyePoint == null)
            return false;

        Vector3 directionToPlayer =
            player.position - eyePoint.position;

        float distanceToPlayer = directionToPlayer.magnitude;

        // Comprobar distancia de deteccion
        if (distanceToPlayer > viewDistance)
            return false;

        Vector3 forward = eyePoint.forward;
        Vector3 detectionDirection = directionToPlayer;

        // Vision horizontal para guardias moviles
        if (!use3DVision)
        {
            forward = Vector3.ProjectOnPlane(
                forward, Vector3.up);

            detectionDirection = Vector3.ProjectOnPlane(
                detectionDirection, Vector3.up);
        }

        // Comprobar el angulo de vision
        if (Vector3.Angle(forward, detectionDirection)
            > viewAngle / 2f)
            return false;

        // Comprobar si existen obstaculos
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

        // Verde: patrullaje
        // Rojo: jugador detectado
        Gizmos.color = CanSeePlayer
            ? Color.red
            : Color.green;

        Vector3 origin = eyePoint.position;

        Vector3 forward = Vector3.ProjectOnPlane(
            eyePoint.forward,
            Vector3.up).normalized;

        int segments = 40;
        Vector3 previousPoint = origin;

        // Dibujar los limites del campo de vision
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

        // Dibujar una linea hacia el jugador detectado
        if (CanSeePlayer && player != null)
        {
            Gizmos.DrawLine(origin, player.position);
        }
    }
}


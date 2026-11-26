using UnityEngine;

[RequireComponent(typeof(EnemyVision))]
public class StaticEnemyAI : MonoBehaviour
{
    [Header("Referencias")]
    [SerializeField] private Transform rotationPivot;

    [Header("Configuracion de vigilancia")]
    [SerializeField] private float scanAngle = 60f;
    [SerializeField] private float scanSpeed = 30f;

    [Header("Configuracion de sospecha")]
    [SerializeField, Min(0.1f)]
    private float suspicionDuration = 1f;

    [Header("Estado actual - Debug")]
    [SerializeField]
    private EnemyState currentState = EnemyState.Patrol;

    [SerializeField]
    private float suspicionTimer;

    [SerializeField]
    private Vector3 lastKnownPosition;

    private EnemyVision vision;

    private Quaternion initialRotation;
    private float currentAngle;
    private int direction = 1;

    public EnemyState CurrentState => currentState;

    public float SuspicionProgress =>
        Mathf.Clamp01(suspicionTimer / suspicionDuration);

    private void Awake()
    {
        vision = GetComponent<EnemyVision>();
    }

    private void Start()
    {
        initialRotation = rotationPivot.localRotation;
        currentState = EnemyState.Patrol;
    }

    private void Update()
    {
        switch (currentState)
        {
            case EnemyState.Patrol:
                UpdatePatrol();
                break;

            case EnemyState.Suspicious:
                UpdateSuspicious();
                break;

            case EnemyState.Chase:
                UpdateAlert();
                break;
        }
    }

    // Vigilancia normal de la camara
    private void UpdatePatrol()
    {
        if (vision.CanSeePlayer && vision.Player != null)
        {
            lastKnownPosition = vision.Player.position;

            ChangeState(EnemyState.Suspicious);
            return;
        }

        ScanArea();
    }

    // Confirmar la presencia del jugador
    private void UpdateSuspicious()
    {
        if (!vision.CanSeePlayer || vision.Player == null)
        {
            ChangeState(EnemyState.Patrol);
            return;
        }

        lastKnownPosition = vision.Player.position;

        suspicionTimer = Mathf.Min(
            suspicionTimer + Time.deltaTime,
            suspicionDuration);

        if (suspicionTimer >= suspicionDuration)
        {
            ChangeState(EnemyState.Chase);
        }
    }

    // Alerta confirmada de la camara
    private void UpdateAlert()
    {
        if (!vision.CanSeePlayer || vision.Player == null)
        {
            ChangeState(EnemyState.Patrol);
            return;
        }

        lastKnownPosition = vision.Player.position;
    }

    // Administrar las transiciones entre estados
    private void ChangeState(EnemyState newState)
    {
        if (currentState == newState)
            return;

        currentState = newState;

        switch (currentState)
        {
            case EnemyState.Patrol:

                suspicionTimer = 0f;

                break;

            case EnemyState.Suspicious:

                suspicionTimer = 0f;

                break;

            case EnemyState.Chase:

                Debug.Log(
                    "Camara: alerta confirmada",
                    this);

                break;
        }

        Debug.Log(
            gameObject.name + " - Estado: " +
            currentState,
            this);
    }

    // Inspeccionar la zona mediante rotacion horizontal
    private void ScanArea()
    {
        currentAngle +=
            direction * scanSpeed * Time.deltaTime;

        if (currentAngle >= scanAngle)
        {
            currentAngle = scanAngle;
            direction = -1;
        }
        else if (currentAngle <= -scanAngle)
        {
            currentAngle = -scanAngle;
            direction = 1;
        }

        rotationPivot.localRotation =
            initialRotation *
            Quaternion.Euler(0f, currentAngle, 0f);
    }
}

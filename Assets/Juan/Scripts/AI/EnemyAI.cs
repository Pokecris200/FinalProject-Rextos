
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
[RequireComponent(typeof(EnemyPatrol))]
[RequireComponent(typeof(EnemyVision))]
public class EnemyAI : MonoBehaviour
{
    [Header("Configuracion de sospecha")]

    [SerializeField, Min(0.1f)]
    private float suspicionDuration = 1f;

    [SerializeField]
    private float rotationSpeed = 360f;

    [Header("Configuracion de persecucion")]

    [SerializeField, Min(0.1f)]
    private float chaseSpeed = 3.5f;

    [SerializeField, Min(0.1f)]
    private float chaseStoppingDistance = 0.8f;

    [SerializeField, Min(0.1f)]
    private float captureDistance = 1f;

    [SerializeField, Min(0.05f)]
    private float repathInterval = 0.2f;

    private float repathTimer;

    private float patrolStoppingDistance;

    [Header("Estado actual - Debug")]

    [SerializeField]
    private EnemyState currentState = EnemyState.Patrol;

    [SerializeField]
    private float suspicionTimer;

    [SerializeField]
    private Vector3 lastKnownPosition;

    private NavMeshAgent agent;
    private EnemyPatrol patrol;
    private EnemyVision vision;

    public EnemyState CurrentState => currentState;

    public float SuspicionProgress =>
        Mathf.Clamp01(suspicionTimer / suspicionDuration);

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        patrolStoppingDistance = agent.stoppingDistance;
        patrol = GetComponent<EnemyPatrol>();
        vision = GetComponent<EnemyVision>();

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
               UpdateChase();
               break;
        }
   }

    // Comportamiento durante el patrullaje
    private void UpdatePatrol()
    {
        if (vision.CanSeePlayer && vision.Player != null)
        {
            lastKnownPosition = vision.Player.position;

            ChangeState(EnemyState.Suspicious);
        }
    }

    // Comportamiento durante la sospecha
    private void UpdateSuspicious()
{
        if (!vision.CanSeePlayer || vision.Player == null)
        {
            ChangeState(EnemyState.Patrol);
            return;
        }

        lastKnownPosition = vision.Player.position;

        LookAtPosition(lastKnownPosition);

        suspicionTimer = Mathf.Min(
            suspicionTimer + Time.deltaTime,
            suspicionDuration);

       // Iniciar persecucion al confirmar la sospecha
        if (suspicionTimer >= suspicionDuration)
        {
            ChangeState(EnemyState.Chase);
        }
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

            agent.updateRotation = true;

            agent.stoppingDistance =
                patrolStoppingDistance;

            patrol.StartPatrol();

            break;

        case EnemyState.Suspicious:

            patrol.StopPatrol();

            suspicionTimer = 0f;

            agent.updateRotation = false;

            break;

        
        case EnemyState.Chase:

            patrol.StopPatrol();

            agent.speed = chaseSpeed;

            agent.stoppingDistance =
                chaseStoppingDistance;

            agent.updateRotation = false;

            repathTimer = 0f;

        if (agent.isOnNavMesh)
        {
            agent.isStopped = false;
        }

        // Capturar al jugador al confirmar la deteccion
        if (vision.Player != null)
        {
            PlayerCapture playerCapture =
                vision.Player.GetComponentInParent<PlayerCapture>();

            if (playerCapture != null)
            {
                playerCapture.Capture();
            }
            else
            {
                Debug.LogWarning(
                    "No se encontro PlayerCapture en el jugador.");
            }
        }

        break;

    }

    Debug.Log(
        gameObject.name + " - Estado: " +
        currentState,
        this);
}

    // Rotar horizontalmente hacia una posicion
    private void LookAtPosition(Vector3 position)
    {
        Vector3 direction =
            position - transform.position;

        direction.y = 0f;

        if (direction.sqrMagnitude < 0.001f)
            return;

        Quaternion targetRotation =
            Quaternion.LookRotation(direction);

        transform.rotation =
            Quaternion.RotateTowards(
                transform.rotation,
                targetRotation,
                rotationSpeed * Time.deltaTime);
    }


    
    
    private void UpdateChase()
    {
        if (!agent.isOnNavMesh)
            return;

        if (GameOverManager.Instance != null &&
            GameOverManager.Instance.IsGameOver)
            return;

        // Comprobar si el guardia alcanzo al jugador
        if (CheckCapture())
            return;

        // Si pierde al jugador, volver temporalmente a patrullar
        if (!vision.CanSeePlayer || vision.Player == null)
        {
            ChangeState(EnemyState.Patrol);
            return;
        }

        // Actualizar la ultima posicion conocida
        lastKnownPosition = vision.Player.position;

        // Mantener la mirada hacia el jugador
        LookAtPosition(lastKnownPosition);

        repathTimer -= Time.deltaTime;

        if (repathTimer > 0f)
            return;

        repathTimer = repathInterval;

        NavMeshQueryFilter filter = new NavMeshQueryFilter
        {
            agentTypeID = agent.agentTypeID,
            areaMask = agent.areaMask
        };

        // Proyectar la posicion del jugador sobre el NavMesh
        Vector3 groundTarget = lastKnownPosition;
        groundTarget.y = agent.nextPosition.y;

        if (NavMesh.SamplePosition(
            groundTarget,
            out NavMeshHit hit,
            2f,
            filter))
       {
           agent.SetDestination(hit.position);
        }
    }



    private bool CheckCapture()
    {
        if (vision.Player == null)
            return false;

        PlayerCapture playerCapture =
            vision.Player.GetComponentInParent<PlayerCapture>();

        if (playerCapture == null || !playerCapture.IsCaptured)
            return false;

        // Calcular la distancia horizontal al jugador
        Vector3 direction =
            vision.Player.position - transform.position;

        direction.y = 0f;

        float distance = direction.magnitude;

        // Comprobar si el guardia ha alcanzado al jugador
        if (distance > captureDistance)
            return false;

        if (GameOverManager.Instance == null)
            return false;

        // Detener al guardia
        agent.ResetPath();
        agent.isStopped = true;

        // Finalizar la partida
        GameOverManager.Instance.TriggerGameOver();

        return true;
    }



}

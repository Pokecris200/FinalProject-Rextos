
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

    [Header("Alertas de seguridad")]

    [SerializeField]
    private bool respondingToAlert;

    private Transform alertTarget;

    public bool IsRespondingToAlert => respondingToAlert;

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
            
            respondingToAlert = false;
            alertTarget = null;
            
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

    // Calcular la longitud del recorrido hacia una alerta
    // sin modificar el estado actual del enemigo
    public bool TryGetAlertPathDistance(
        Transform target,
        out float pathDistance)
    {
        pathDistance = Mathf.Infinity;

        // Comprobar disponibilidad del guardia
        if (target == null ||
            !isActiveAndEnabled ||
            !agent.isOnNavMesh ||
            currentState == EnemyState.Chase ||
            respondingToAlert)
        {
            return false;
        }

        // Verificar que sea el jugador correspondiente
        if (target != vision.Player)
            return false;

        // Proyectar el objetivo sobre el NavMesh
        Vector3 groundTarget = target.position;
        groundTarget.y = agent.nextPosition.y;

        NavMeshQueryFilter filter = new NavMeshQueryFilter
        {
            agentTypeID = agent.agentTypeID,
            areaMask = agent.areaMask
        };

        if (!NavMesh.SamplePosition(
            groundTarget,
            out NavMeshHit hit,
            2f,
            filter))
        {
            return false;
        }

        // Calcular una ruta sin modificar el destino actual
        NavMeshPath alertPath = new NavMeshPath();

        if (!agent.CalculatePath(hit.position, alertPath) ||
            alertPath.status != NavMeshPathStatus.PathComplete)
        {
            return false;
        }

        // Sumar las distancias entre los puntos del recorrido
        pathDistance = 0f;

        Vector3[] corners = alertPath.corners;

       for (int i = 1; i < corners.Length; i++)
       {
            pathDistance += Vector3.Distance(
                corners[i - 1],
                corners[i]);
       }

       return true;
    }

    // Recibir una alerta externa de una camara de seguridad
    public bool ReceiveSecurityAlert(Transform target)
    {
        if (target == null ||
            !isActiveAndEnabled ||
            !agent.isOnNavMesh)
        {
            return false;
        }

        // Evitar interrumpir una persecucion existente
        if (currentState == EnemyState.Chase)
            return false;

        // Comprobar que el objetivo corresponde al jugador
        if (target != vision.Player)
            return false;

        // Proyectar la posicion del jugador sobre el NavMesh
        Vector3 groundTarget = target.position;
        groundTarget.y = agent.nextPosition.y;

        NavMeshQueryFilter filter = new NavMeshQueryFilter
        {
            agentTypeID = agent.agentTypeID,
            areaMask = agent.areaMask
        };

        if (!NavMesh.SamplePosition(
            groundTarget,
            out NavMeshHit hit,
            2f,
            filter))
        {
            return false;
        }

        // Verificar que exista una ruta completa hacia el jugador
        NavMeshPath path = new NavMeshPath();

        if (!agent.CalculatePath(hit.position, path) ||
            path.status != NavMeshPathStatus.PathComplete)
        {
            return false;
        }

        // Registrar la alerta
        respondingToAlert = true;
        alertTarget = target;
        lastKnownPosition = target.position;

        // Iniciar la persecucion sin deteccion visual directa
        ChangeState(EnemyState.Chase);

        agent.SetPath(path);

        Debug.Log(
            gameObject.name + " - Alerta de seguridad recibida",
            this);

        return true;
    }


    
    
    private void UpdateChase()
    {
        if (!agent.isOnNavMesh)
            return;

        if (GameOverManager.Instance != null &&
            GameOverManager.Instance.IsGameOver)
            return;

        Transform target = respondingToAlert
            ? alertTarget
            : vision.Player;

        if (target == null)
        {
            ChangeState(EnemyState.Patrol);
            return;
        }

        // Comprobar si el jugador ya esta inmovilizado
        PlayerCapture playerCapture =
            target.GetComponentInParent<PlayerCapture>();

        bool playerCaptured =
            playerCapture != null && playerCapture.IsCaptured;

        // Comprobar si el guardia alcanzo al jugador
        if (CheckCapture())
            return;

        // Solo abandonar la persecucion si no existe una
        // alerta externa y el jugador no ha sido capturado
        if (!respondingToAlert &&
            !playerCaptured &&
            !vision.CanSeePlayer)
        {
            ChangeState(EnemyState.Patrol);
            return;
        }

        // Actualizar la posicion del objetivo
        lastKnownPosition = target.position;

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

        // Proyectar el objetivo sobre el NavMesh
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

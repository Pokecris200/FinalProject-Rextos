
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class EnemyPatrol : MonoBehaviour
{
    [Header("Referencias")]
    [SerializeField] private PatrolNetwork patrolNetwork;

    [Header("Modo de patrullaje")]
    [SerializeField] private PatrolMode patrolMode = PatrolMode.Random;

    [SerializeField] private PatrolRoute fixedRoute;

    [Header("Configuracion de patrullaje")]
    [SerializeField] private float patrolSpeed = 2f;

    [SerializeField, Min(0.1f)]
    private float nodeSampleDistance = 0.5f;

    [Header("Estado actual - Debug")]
    [SerializeField] private PatrolRoute currentRoute;
    [SerializeField] private PatrolNode currentNode;
    [SerializeField] private PatrolNode targetNode;
    [SerializeField] private int currentNodeIndex;
    [SerializeField] private bool reverseRoute;
    [SerializeField] private bool isWaiting;
    [SerializeField] private bool isPatrolling;

    private NavMeshAgent agent;
    private NavMeshPath path;
    private float waitTimer;

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        path = new NavMeshPath();
    }

    private void Start()
    {
        StartPatrol();
    }

    private void Update()
    {
        if (!isPatrolling || !agent.isOnNavMesh)
            return;

        if (isWaiting)
        {
            waitTimer -= Time.deltaTime;

            if (waitTimer <= 0f)
            {
                isWaiting = false;
                AdvanceToNextNode();
            }

            return;
        }

        if (agent.pathPending)
            return;

        // Detener el patrullaje si la ruta deja de ser valida
        if (agent.pathStatus != NavMeshPathStatus.PathComplete)
        {
            StopPatrol();

            Debug.LogWarning(
                "El enemigo perdio una ruta valida.",
                this);

            return;
        }

        // Comprobar si el enemigo llego al nodo
        if (targetNode != null &&
            agent.remainingDistance <= agent.stoppingDistance &&
            agent.velocity.sqrMagnitude < 0.01f)
        {
            ArriveAtNode();
        }
    }

    // Iniciar el patrullaje
    // Iniciar el patrullaje segun el modo seleccionado
    public void StartPatrol()
    {
        if (!agent.isOnNavMesh)
        {
            Debug.LogWarning(
                "El enemigo no esta sobre el NavMesh.",
                this);

            return;
        }

        // Seleccionar la ruta segun el modo de patrullaje
        switch (patrolMode)
        {
            case PatrolMode.Random:

                if (patrolNetwork == null)
                {
                    Debug.LogError(
                        "No se ha asignado PatrolNetwork.",
                        this);

                    return;
                }

                currentRoute = patrolNetwork.GetRandomRoute();

                break;

            case PatrolMode.FixedRoute:

                currentRoute = fixedRoute;

                break;
        }

        // Validar la ruta seleccionada
        if (currentRoute == null || !currentRoute.IsValid())
        {
            Debug.LogWarning(
                "No existe una ruta de patrullaje valida.",
                this);

            StopPatrol();
            return;
        }

        agent.speed = patrolSpeed;
        agent.isStopped = false;

        isPatrolling = true;
        isWaiting = false;
        currentNode = null;

        // Configurar el sentido inicial del recorrido
        if (patrolMode == PatrolMode.FixedRoute)
        {
            // Calcular la distancia a los extremos de la ruta
            float startDistance = Vector3.Distance(
                transform.position,
                currentRoute.StartNode.Position);

            float endDistance = Vector3.Distance(
                transform.position,
                currentRoute.EndNode.Position);

            // Comenzar por el extremo mas cercano
            reverseRoute = currentRoute.AllowReverse &&
                           endDistance < startDistance;
        }
        else
        {
            // Mantener el comportamiento aleatorio original
            reverseRoute = currentRoute.AllowReverse &&
                           Random.value < 0.5f;
        }

        currentNodeIndex = reverseRoute
            ? currentRoute.NodeCount - 1
            : 0;

        MoveToCurrentTarget();
    }

    // Desplazarse hacia el nodo objetivo
    private void MoveToCurrentTarget()
    {
        targetNode = currentRoute.GetNode(currentNodeIndex);

        if (targetNode == null)
        {
            StopPatrol();
            return;
        }

        NavMeshQueryFilter filter = new NavMeshQueryFilter
        {
            agentTypeID = agent.agentTypeID,
            areaMask = agent.areaMask
        };

        // Buscar una posicion navegable cercana al nodo
        if (!NavMesh.SamplePosition(
            targetNode.Position,
            out NavMeshHit hit,
            nodeSampleDistance,
            filter))
        {
            Debug.LogWarning(
                "Nodo fuera del NavMesh: " +
                targetNode.name,
                targetNode);

            StopPatrol();
            return;
        }

        // Verificar que el destino sea alcanzable
        if (!agent.CalculatePath(hit.position, path) ||
            path.status != NavMeshPathStatus.PathComplete)
        {
            Debug.LogWarning(
                "Nodo inaccesible: " + targetNode.name,
                targetNode);

            StopPatrol();
            return;
        }

        agent.isStopped = false;

        if (!agent.SetPath(path))
        {
            Debug.LogWarning(
                "No se pudo asignar la ruta al agente.",
                this);

            StopPatrol();
        }
    }

    // Registrar la llegada al nodo
    private void ArriveAtNode()
    {
        currentNode = targetNode;

        agent.ResetPath();
        agent.isStopped = true;

        isWaiting = true;
        waitTimer = currentNode.WaitTime;
    }

    // Avanzar dentro de la ruta
    private void AdvanceToNextNode()
    {
        currentNodeIndex += reverseRoute ? -1 : 1;

        // Comprobar si termino el recorrido
        if (currentNodeIndex < 0 ||
            currentNodeIndex >= currentRoute.NodeCount)
        {
            SelectNextRoute();
            return;
        }

        MoveToCurrentTarget();
    }

    // Seleccionar una ruta conectada
    // Seleccionar el siguiente recorrido de patrullaje
    private void SelectNextRoute()
    {
        // Mantener al enemigo dentro de su ruta asignada
        if (patrolMode == PatrolMode.FixedRoute)
        {
            if (currentRoute.AllowReverse)
            {
                // Invertir el sentido del recorrido
                reverseRoute = !reverseRoute;

                currentNodeIndex = reverseRoute
                    ? currentRoute.NodeCount - 2
                    : 1;
            }
            else
            {
                // Regresar al inicio de la misma ruta
                reverseRoute = false;
                currentNodeIndex = 0;
            }

            MoveToCurrentTarget();
            return;
        }

        // Comportamiento original del patrullaje aleatorio
        PatrolRoute nextRoute =
            patrolNetwork.GetConnectedRoute(
                currentNode,
                currentRoute,
                out bool nextReverse);

        if (nextRoute == null)
        {
            Debug.LogWarning(
                "No hay rutas conectadas desde " +
                currentNode.name,
                this);

            StopPatrol();
            return;
        }

        currentRoute = nextRoute;
        reverseRoute = nextReverse;

        // Saltar el nodo inicial porque ya estamos en el
        currentNodeIndex = reverseRoute
            ? currentRoute.NodeCount - 2
            : 1;

        MoveToCurrentTarget();
    }

    // Detener el patrullaje
    public void StopPatrol()
    {
        isPatrolling = false;
        isWaiting = false;
        waitTimer = 0f;

        if (agent.isOnNavMesh)
        {
            agent.isStopped = true;
            agent.ResetPath();
        }
    }
}

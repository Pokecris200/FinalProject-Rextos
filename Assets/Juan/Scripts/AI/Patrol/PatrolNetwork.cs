
using UnityEngine;
using System.Collections.Generic;

public class PatrolNetwork : MonoBehaviour
{
    [Header("Contenedores de la red")]

    [SerializeField] private Transform nodesContainer;
    [SerializeField] private Transform routesContainer;

    [Header("Elementos registrados")]

    [SerializeField] private List<PatrolNode> nodes =
        new List<PatrolNode>();

    [SerializeField] private List<PatrolRoute> routes =
        new List<PatrolRoute>();

    private readonly List<PatrolRoute> validRoutes =
        new List<PatrolRoute>();

    public IReadOnlyList<PatrolNode> Nodes => nodes;

    public IReadOnlyList<PatrolRoute> Routes => validRoutes;

    private void Awake()
    {
        RefreshNetwork();
    }

    // Registrar automaticamente los nodos y rutas
    [ContextMenu("Refresh Network")]
    public void RefreshNetwork()
    {
        nodes.Clear();
        routes.Clear();
        validRoutes.Clear();

        if (nodesContainer == null ||
            routesContainer == null)
        {
            Debug.LogError(
                "PatrolNetwork: faltan los contenedores.",
                this);

            return;
        }

        nodes.AddRange(
            nodesContainer
                .GetComponentsInChildren<PatrolNode>());

        routes.AddRange(
            routesContainer
                .GetComponentsInChildren<PatrolRoute>());

        foreach (PatrolRoute route in routes)
        {
            if (route.IsValid())
            {
                validRoutes.Add(route);
            }
            else
            {
                Debug.LogWarning(
                    "Ruta invalida: " + route.RouteName,
                    route);
            }
        }

        Debug.Log(
            "PatrolNetwork: " +
            nodes.Count + " nodos registrados, " +
            validRoutes.Count + " rutas validas de " +
            routes.Count + " rutas totales.",
            this);
    }

    // Encontrar el nodo mas cercano a una posicion
    public PatrolNode GetClosestNode(Vector3 position)
    {
        PatrolNode closestNode = null;

        float closestDistance = float.MaxValue;

        foreach (PatrolNode node in nodes)
        {
            if (node == null)
                continue;

            float distance =
                (node.Position - position).sqrMagnitude;

            if (distance < closestDistance)
            {
                closestDistance = distance;
                closestNode = node;
            }
        }

        return closestNode;
    }

    // Seleccionar una ruta inicial aleatoria
    public PatrolRoute GetRandomRoute()
    {
        if (validRoutes.Count == 0)
            return null;

        int index = Random.Range(0, validRoutes.Count);

        return validRoutes[index];
    }

    // Obtener una ruta que continue desde el nodo actual
    public PatrolRoute GetConnectedRoute(
        PatrolNode currentNode,
        PatrolRoute previousRoute,
        out bool reverse)
    {
        reverse = false;

        if (currentNode == null)
            return null;

        List<PatrolRoute> candidates =
            new List<PatrolRoute>();

        foreach (PatrolRoute route in validRoutes)
        {
            if (route == previousRoute)
                continue;

            if (route.StartNode == currentNode ||
                (route.AllowReverse &&
                 route.EndNode == currentNode))
            {
                candidates.Add(route);
            }
        }

        // Seleccionar una ruta conectada diferente
        if (candidates.Count > 0)
        {
            PatrolRoute selectedRoute =
                candidates[Random.Range(
                    0, candidates.Count)];

            reverse =
                selectedRoute.StartNode != currentNode;

            return selectedRoute;
        }

        // Si no existen alternativas,
        // intentar reutilizar la ruta anterior
        if (previousRoute != null &&
            validRoutes.Contains(previousRoute))
        {
            if (previousRoute.StartNode == currentNode)
            {
                return previousRoute;
            }

            if (previousRoute.AllowReverse &&
                previousRoute.EndNode == currentNode)
            {
                reverse = true;

                return previousRoute;
            }
        }

        return null;
    }
}

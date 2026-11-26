
using UnityEngine;
using System.Collections.Generic;

public class PatrolRoute : MonoBehaviour
{
    [Header("Configuracion de la ruta")]

    [SerializeField]
    private string routeName = "Patrol Route";

    [SerializeField]
    private List<PatrolNode> nodes =
        new List<PatrolNode>();

    [SerializeField]
    private bool allowReverse = true;

    [Header("Visualizacion")]

    [SerializeField]
    private Color routeColor = Color.green;

    // Propiedades de acceso
    public string RouteName => routeName;

    public IReadOnlyList<PatrolNode> Nodes => nodes;

    public bool AllowReverse => allowReverse;

    public int NodeCount => nodes.Count;

    public PatrolNode StartNode =>
        nodes.Count > 0 ? nodes[0] : null;

    public PatrolNode EndNode =>
        nodes.Count > 0 ? nodes[nodes.Count - 1] : null;

    // Obtener un nodo mediante su indice
    public PatrolNode GetNode(int index)
    {
        if (index < 0 || index >= nodes.Count)
            return null;

        return nodes[index];
    }

    // Validar las conexiones de la ruta
    public bool IsValid()
    {
        if (nodes == null || nodes.Count < 2)
            return false;

        for (int i = 0; i < nodes.Count; i++)
        {
            if (nodes[i] == null)
                return false;

            if (i == nodes.Count - 1)
                continue;

            PatrolNode current = nodes[i];
            PatrolNode next = nodes[i + 1];

            if (next == null || current == next)
                return false;

            if (!HasConnection(current, next))
                return false;

            // Validar conexiones en sentido inverso
            if (allowReverse &&
                !HasConnection(next, current))
            {
                return false;
            }
        }

        return true;
    }

    private bool HasConnection(
        PatrolNode from, PatrolNode to)
    {
        foreach (PatrolNode connected in from.ConnectedNodes)
        {
            if (connected == to)
                return true;
        }

        return false;
    }

    private void OnDrawGizmosSelected()
    {
        if (nodes == null || nodes.Count < 2)
            return;

        // Rojo si la ruta tiene conexiones invalidas
        Gizmos.color = IsValid()
            ? routeColor
            : Color.red;

        for (int i = 0; i < nodes.Count; i++)
        {
            if (nodes[i] == null)
                continue;

            Vector3 position =
                nodes[i].Position + Vector3.up * 0.15f;

            Gizmos.DrawWireSphere(position, 0.35f);

            if (i < nodes.Count - 1 &&
                nodes[i + 1] != null)
            {
                Vector3 nextPosition =
                    nodes[i + 1].Position +
                    Vector3.up * 0.15f;

                Gizmos.DrawLine(position, nextPosition);
            }
        }
    }
}

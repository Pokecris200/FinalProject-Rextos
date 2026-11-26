
using UnityEngine;
using System.Collections.Generic;

public class PatrolNode : MonoBehaviour
{
    [Header("Configuracion del nodo")]

    [SerializeField, Min(0f)]
    private float waitTime = 1f;

    [Header("Nodos conectados")]

    [SerializeField]
    private List<PatrolNode> connectedNodes =
        new List<PatrolNode>();

    [Header("Visualizacion")]

    [SerializeField]
    private Color nodeColor = Color.cyan;

    [SerializeField]
    private Color connectionColor = Color.yellow;

    [SerializeField, Min(0.05f)]
    private float nodeRadius = 0.25f;

    // Propiedades de acceso
    public float WaitTime => waitTime;

    public Vector3 Position => transform.position;

    public IReadOnlyList<PatrolNode> ConnectedNodes
        => connectedNodes;

    private void OnDrawGizmos()
    {
        // Dibujar el nodo
        Gizmos.color = nodeColor;

        Gizmos.DrawSphere(
            transform.position, nodeRadius);

        if (connectedNodes == null)
            return;

        // Dibujar las conexiones
        Gizmos.color = connectionColor;

        foreach (PatrolNode node in connectedNodes)
        {
            if (node == null || node == this)
                continue;

            Gizmos.DrawLine(
                transform.position,
                node.transform.position);
        }
    }

    private void OnDrawGizmosSelected()
    {
        // Resaltar el nodo seleccionado
        Gizmos.color = Color.white;

        Gizmos.DrawWireSphere(
            transform.position,
            nodeRadius * 1.5f);
    }
}

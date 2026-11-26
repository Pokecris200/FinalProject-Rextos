using UnityEngine;
using UnityEngine.Rendering;
using System.Collections.Generic;

[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
public class EnemyVisionMesh : MonoBehaviour
{
    [Header("Referencias")]
    [SerializeField] private EnemyVision enemyVision;

    [Header("Configuracion de la malla")]
    [Range(10, 150)]
    [SerializeField] private int resolution = 60;

    [Header("Proyeccion de camaras")]

    [SerializeField] private LayerMask groundMask;

    [SerializeField] private float groundOffset = 0.03f;

    [Range(5, 40)]
    [SerializeField] private int groundResolution = 20;

    [Range(16, 120)]
    [SerializeField] private int radialResolution = 60;

    [Range(2, 20)]
    [SerializeField] private int radialRings = 8;

    private Mesh groundVisionMesh;

    [Header("Colores del campo de vision")]
    [SerializeField] private Color patrolColor =
        new Color(0f, 1f, 0f, 0.25f);

    [SerializeField] private Color detectedColor =
        new Color(1f, 0f, 0f, 0.35f);

    [SerializeField] private Color suspiciousColor =
    new Color(1f, 1f, 0f, 0.35f);

    private EnemyAI enemyAI;

    private Mesh visionMesh;
    private MeshRenderer meshRenderer;
    private MaterialPropertyBlock propertyBlock;

    private Vector3[] vertices;
    private int[] triangles;

    private void Awake()
    {
        // Obtener los componentes necesarios
        meshRenderer = GetComponent<MeshRenderer>();
        enemyAI = GetComponentInParent<EnemyAI>();

        if (enemyVision == null)
            enemyVision = GetComponentInParent<EnemyVision>();

        // Crear una malla dinamica
        visionMesh = new Mesh();
        visionMesh.name = "Enemy Vision Mesh";
        visionMesh.MarkDynamic();

        GetComponent<MeshFilter>().mesh = visionMesh;

        // Desactivar las sombras
        meshRenderer.shadowCastingMode =
            ShadowCastingMode.Off;

        meshRenderer.receiveShadows = false;

        propertyBlock = new MaterialPropertyBlock();

        InitializeMesh();
        
    }

    private void InitializeMesh()
    {
        vertices = new Vector3[resolution + 2];
        triangles = new int[resolution * 3];

        // Construir los triangulos de la malla
        for (int i = 0; i < resolution; i++)
        {
            int index = i * 3;

            triangles[index] = 0;
            triangles[index + 1] = i + 1;
            triangles[index + 2] = i + 2;
        }
    }

    private void LateUpdate()
{
        if (enemyVision == null ||
            enemyVision.EyePoint == null)
            return;

        // Seleccionar el tipo de representacion visual
        if (enemyVision.Use3DVision)
        {
            DrawGroundVisionMesh();
        }
        else
        {
            if (vertices.Length != resolution + 2)
                InitializeMesh();

            GetComponent<MeshFilter>().sharedMesh = visionMesh;

            DrawVisionMesh();
        }

        UpdateColor();
    }

    private void DrawVisionMesh()
    {
        Transform eyePoint = enemyVision.EyePoint;

        Vector3 origin = eyePoint.position;

        Vector3 forward = Vector3.ProjectOnPlane(
            eyePoint.forward,
            Vector3.up).normalized;

        float viewAngle = enemyVision.ViewAngle;
        float viewDistance = enemyVision.ViewDistance;

        // Origen de la malla
        vertices[0] = Vector3.zero;

        for (int i = 0; i <= resolution; i++)
        {
            float angle = Mathf.Lerp(
                -viewAngle / 2f,
                viewAngle / 2f,
                (float)i / resolution);

            Vector3 direction =
                Quaternion.AngleAxis(
                    angle, Vector3.up) * forward;

            Vector3 endPoint =
                origin + direction * viewDistance;

            // Recortar la vision contra los obstaculos
            if (Physics.Raycast(
                origin,
                direction,
                out RaycastHit hit,
                viewDistance,
                enemyVision.ObstacleMask,
                QueryTriggerInteraction.Ignore))
            {
                endPoint = hit.point;
            }

            // Proyectar el punto sobre el plano del cono
            endPoint.y = transform.position.y;

            // Convertir coordenadas globales a locales
            vertices[i + 1] =
                transform.InverseTransformPoint(endPoint);
        }

        // Actualizar la geometria
        visionMesh.Clear();
        visionMesh.vertices = vertices;
        visionMesh.triangles = triangles;

        visionMesh.RecalculateNormals();
        visionMesh.RecalculateBounds();
    }


    
    private void DrawGroundVisionMesh()
    {
        if (groundVisionMesh == null)
        {
            groundVisionMesh = new Mesh();
            groundVisionMesh.name = "Ground Vision Mesh";
            groundVisionMesh.MarkDynamic();
        }

        Transform eye = enemyVision.EyePoint;

        float distance = enemyVision.ViewDistance;
        float halfAngle = enemyVision.ViewAngle / 2f;

        int segments = radialResolution;
        int rings = radialRings;

        int vertexCount = 1 + rings * segments;

        Vector3[] points = new Vector3[vertexCount];
        bool[] visible = new bool[vertexCount];

        List<int> indices = new List<int>();

        // Comprobar el centro del campo visual
        visible[0] = SampleGroundPoint(
           eye.forward, out points[0]);

        // Generar anillos alrededor de la direccion central
        for (int ring = 1; ring <= rings; ring++)
        {
            float angle = halfAngle * ring / rings;

            for (int i = 0; i < segments; i++)
            {
                float azimuth = i * 360f / segments;

                Vector3 axis =
                    Quaternion.AngleAxis(
                        azimuth, eye.forward) * eye.up;

                Vector3 direction =
                    Quaternion.AngleAxis(
                        angle, axis) * eye.forward;

                int index = 1 + (ring - 1) * segments + i;

                visible[index] = SampleGroundPoint(
                    direction, out points[index]);
            }
        }

        // Construir los triangulos centrales
        for (int i = 0; i < segments; i++)
        {
            int a = 1 + i;
            int b = 1 + (i + 1) % segments;

            if (visible[0] && visible[a] && visible[b])
            {
                indices.Add(0);
                indices.Add(a);
                indices.Add(b);
            }
        }

        // Conectar los anillos exteriores
        for (int ring = 1; ring < rings; ring++)
        {
            for (int i = 0; i < segments; i++)
        {
                int next = (i + 1) % segments;

                int a = 1 + (ring - 1) * segments + i;
                int b = 1 + (ring - 1) * segments + next;

                int c = 1 + ring * segments + i;
                int d = 1 + ring * segments + next;

                if (visible[a] && visible[c] && visible[b])
                {
                    indices.Add(a);
                    indices.Add(c);
                    indices.Add(b);
                }

                if (visible[b] && visible[c] && visible[d])
                {
                    indices.Add(b);
                    indices.Add(c);
                    indices.Add(d);
                }
            }
        }

        groundVisionMesh.Clear();
        groundVisionMesh.vertices = points;
        groundVisionMesh.SetTriangles(indices, 0);

        groundVisionMesh.RecalculateNormals();
        groundVisionMesh.RecalculateBounds();

        GetComponent<MeshFilter>().sharedMesh =
            groundVisionMesh;
    }

    private bool SampleGroundPoint(
        Vector3 direction,
        out Vector3 point)
    {
        point = Vector3.zero;

        Transform eye = enemyVision.EyePoint;

        int combinedMask =
            groundMask.value |
            enemyVision.ObstacleMask.value;

        if (!Physics.Raycast(
            eye.position,
            direction.normalized,
            out RaycastHit hit,
            enemyVision.ViewDistance,
            combinedMask,
            QueryTriggerInteraction.Ignore))
         {
            return false;
        }

        // Comprobar que la superficie alcanzada sea suelo
        int hitLayer = 1 << hit.collider.gameObject.layer;

        if ((groundMask.value & hitLayer) == 0)
            return false;

        Vector3 groundPoint =
            hit.point + Vector3.up * groundOffset;

        point = transform.InverseTransformPoint(
            groundPoint);

        return true;
    }

    private void OnDestroy()
    {
        if (visionMesh != null)
            Destroy(visionMesh);

        if (groundVisionMesh != null)
            Destroy(groundVisionMesh);
    }


    
    private void UpdateColor()
    {
        Color currentColor = patrolColor;

        if (enemyAI != null)
        {
            switch (enemyAI.CurrentState)
            {
                case EnemyState.Patrol:
                    currentColor = patrolColor;
                    break;

                case EnemyState.Suspicious:
                    currentColor = suspiciousColor;
                    break;

                case EnemyState.Chase:
                    currentColor = detectedColor;
                    break;
            }
        }
        else
        {
             currentColor = enemyVision.CanSeePlayer
                ? detectedColor
                : patrolColor;
        }

        meshRenderer.GetPropertyBlock(propertyBlock);

        propertyBlock.SetColor(
            "_BaseColor", currentColor);

        meshRenderer.SetPropertyBlock(propertyBlock);
    }
}


using UnityEngine;

[RequireComponent(typeof(EnemyVision))]
public class StaticEnemyAI : MonoBehaviour
{
    [Header("Referencias")]
    [SerializeField] private Transform rotationPivot;

    [Header("Configuracion de vigilancia")]
    [SerializeField] private float scanAngle = 60f;
    [SerializeField] private float scanSpeed = 30f;

    private Quaternion initialRotation;
    private float currentAngle;
    private int direction = 1;

    private void Start()
    {
        initialRotation = rotationPivot.localRotation;
    }

    private void Update()
    {
        ScanArea();
    }

    // Inspeccionar la zona mediante rotacion horizontal
    private void ScanArea()
    {
        currentAngle += direction * scanSpeed * Time.deltaTime;

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

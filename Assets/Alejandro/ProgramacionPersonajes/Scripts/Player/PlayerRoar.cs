using UnityEngine;

public class PlayerRoar : MonoBehaviour
{
    [SerializeField] private float _cooldownDuration = 1f;
    [SerializeField] private float _detectionRadius = 2f;
    [SerializeField] private LayerMask _roarLayers = ~0;
    private float _cooldownTimer;

    private bool _isRoaring;

    public bool IsRoaring
    {
        get => _isRoaring;
    }

    public bool IsOnCooldown { get; private set; }

    private void OnValidate()
    {
        _detectionRadius = Mathf.Max(0f, _detectionRadius);
        _cooldownDuration = Mathf.Max(0f, _cooldownDuration);
    }

    private void Update()
    {
        if (!IsOnCooldown)
        {
            return;
        }

        // Count down; the flag stays true until the time has passed.
        _cooldownTimer -= Time.deltaTime;
        if (_cooldownTimer <= 0f)
        {
            IsOnCooldown = false;
        }
    }

    /// <summary>Stuns enemies in range. Returns true if one was grabbed.</summary>
    public bool TryRoar()
    {
        if (IsRoaring)
        {
            return false;
        }

        Collider[] hits = Physics.OverlapSphere(
            transform.position, _detectionRadius, _roarLayers, QueryTriggerInteraction.Ignore);

        ///Confirm enemies reached, exits early if none
        if (hits == null || hits.Length == 0)
        {
            Debug.Log("No enemy in range for roar");
            return false;
        }

        ///Stuns each enemy in range of the roar
        foreach (Collider hit in hits)
        {
            
            EnemyAI candidate = hit.GetComponentInParent<EnemyAI>();
            if (candidate == null)
            {
                continue;
            }
            Debug.Log("Enviando susto");
            //Nota: Remover comentario luego de implementar GetScared() en EnemyAI
            //candidate.GetScared();
            
        }
        _isRoaring = true;
        return true;
    }

    /// <summary>Releases the current object (it stays frozen where it is).</summary>
    public void StopRoaring()
    {
        if (!IsRoaring)
        {
            return;
        }

        _isRoaring = false;
    }

    
}

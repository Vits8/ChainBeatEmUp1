using System.Collections.Generic;
using UnityEngine;
using System;


[RequireComponent(typeof(ComboSystem))]
public class ChainAttack : MonoBehaviour
{
    [Header("Attack")]
    [SerializeField] private float lightRadius = 2.5f;
    [SerializeField] private float heavyRadius = 3.5f;
    [SerializeField] private float angle = 120f;
    [SerializeField] private int lightDamage = 10;
    [SerializeField] private int heavyDamage = 20;
    [SerializeField] private float knockbackForce = 7f;
    [SerializeField] private LayerMask targetMask;

    public AttackRangeIndicator rangeIndicator;
    private AttackFeedback feedback;


    private ComboSystem combo;
    public event Action<Vector3> OnAnyHit;


    private void Awake()
    {
        feedback = GetComponent<AttackFeedback>();
        combo = GetComponent<ComboSystem>();
    }

    public void DoSwingAttack(Vector2 facingDir, bool heavy)
    {
        float radius = heavy ? heavyRadius : lightRadius;
        int dmg = heavy ? heavyDamage : lightDamage;

        // показати візуальний круг
        if (rangeIndicator != null)
            rangeIndicator.Show(radius, angle, facingDir);

        if (facingDir == Vector2.zero) facingDir = Vector2.right;
        facingDir.Normalize();

        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, radius, targetMask);

        foreach (var hit in hits)
        {
            Vector2 dirToTarget = ((Vector2)hit.transform.position - (Vector2)transform.position).normalized;
            if (Vector2.Angle(facingDir, dirToTarget) <= angle / 2f)
            {
                if (hit.TryGetComponent<IDamageable>(out var dmgComp))
                {
                    int finalDmg = Mathf.RoundToInt(dmg * combo.CurrentBonuses.damageMult);
                    dmgComp.TakeDamage(finalDmg, dirToTarget * knockbackForce);

                    combo.RegisterSuccessfulHit();
                    OnAnyHit?.Invoke(hit.transform.position);
                    feedback.PlayHitEffect(hit.transform.position);
                }
            }
        }
    }




    //private void OnDrawGizmosSelected()
    //{
    //    float radius = heavy ? heavyRadius : lightRadius;
    //    Gizmos.color = Color.red;
    //    Gizmos.DrawWireSphere(transform.position, radius);


    //    Vector3 forward = Vector3.right;
    //    Vector3 left = Quaternion.Euler(0, 0, angle / 2) * forward;
    //    Vector3 right = Quaternion.Euler(0, 0, -angle / 2) * forward;
    //    Gizmos.DrawLine(transform.position, transform.position + left * radius);
    //    Gizmos.DrawLine(transform.position, transform.position + right * radius);
    //}
}
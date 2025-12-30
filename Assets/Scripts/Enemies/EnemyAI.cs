using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Rigidbody2D))]
public class EnemyAI : MonoBehaviour, IDamageable
{
    [Header("Stats")]
    public int maxHealth = 40;
    public float moveSpeed = 2f;
    public float attackRange = 1.2f;
    public float stopDistance = 0.5f; 
    public int attackDamage = 10;
    public float attackCooldown = 1.2f;
    public float staggerDuration = 0.3f;

    [Header("Attack Feedback")]
    public float attackKnockback = 3f;

    public int CurrentHealth => currentHealth;

    public int currentHealth;
    private Rigidbody2D rb;
    private Transform player;

    private bool isStaggered = false;
    private float attackTimer = 0f;
    private Vector2 moveDirection = Vector2.zero;

    private enum State { Idle, Chasing, Attacking, Staggered }
    private State currentState = State.Idle;

    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.constraints = RigidbodyConstraints2D.FreezeRotation; 
        rb.interpolation = RigidbodyInterpolation2D.Interpolate; 

        currentHealth = maxHealth;

        GameObject pl = GameObject.FindGameObjectWithTag("Player");
        if (pl != null) player = pl.transform;
    }

    private void Update()
    {
        if (player == null) return;

        
        if (attackTimer > 0f) attackTimer -= Time.deltaTime;

       
        Vector3 diff = player.position - transform.position;
        if (diff.x > 0.01f)
            transform.localScale = new Vector3(1f, 1f, 1f);
        else if (diff.x < -0.01f)
            transform.localScale = new Vector3(-1f, 1f, 1f);

        // FSM 
        switch (currentState)
        {
            case State.Idle:
                if (Vector2.Distance(transform.position, player.position) <= 5f)
                    currentState = State.Chasing;
                moveDirection = Vector2.zero;
                break;

            case State.Chasing:
                HandleMovement();
                break;

            case State.Attacking:
                // PerformAttack
                moveDirection = Vector2.zero;
                break;

            case State.Staggered:
                moveDirection = Vector2.zero;
                break;
        }
    }

    private void FixedUpdate()
    {
        if (moveDirection != Vector2.zero)
        {
            rb.MovePosition(rb.position + moveDirection * moveSpeed * Time.fixedDeltaTime);
        }
    }

    private void HandleMovement()
    {
        if (isStaggered) return;

        float dist = Vector2.Distance(transform.position, player.position);

        if (dist < stopDistance)
        {
            moveDirection = (transform.position - player.position).normalized;
        }
        else if (dist <= attackRange)
        {
            if (attackTimer <= 0f)
            {
                currentState = State.Attacking;
                StartCoroutine(PerformAttack());
            }
            moveDirection = Vector2.zero;
        }
        else
        {
            moveDirection = (player.position - transform.position).normalized;
        }
    }

    private IEnumerator PerformAttack()
    {
        if (player.TryGetComponent<IDamageable>(out var dmg))
        {
            Vector2 dir = (player.position - transform.position).normalized;
            dmg.TakeDamage(attackDamage, dir * attackKnockback); // knockback 
        }

        attackTimer = attackCooldown;

        yield return new WaitForSeconds(0.2f); 

        if (currentHealth > 0)
            currentState = State.Chasing;
    }

    public void TakeDamage(int damage, Vector2 knockback)
    {
        currentHealth -= damage;

        if (rb != null)
            rb.AddForce(knockback, ForceMode2D.Impulse);

        if (currentHealth <= 0)
        {
            Die();
        }
        else
        {
            StartCoroutine(Stagger());
        }
    }

    private IEnumerator Stagger()
    {
        isStaggered = true;
        State prevState = currentState;
        currentState = State.Staggered;

        yield return new WaitForSeconds(staggerDuration);

        isStaggered = false;
        if (currentHealth > 0)
            currentState = State.Chasing;
    }

    private void Die()
    {
        Destroy(gameObject);
    }
}

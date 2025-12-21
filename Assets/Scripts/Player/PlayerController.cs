using UnityEngine;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(ComboSystem))]
[RequireComponent(typeof(Health))]
[RequireComponent(typeof(ChainAttack))]
public class PlayerController : MonoBehaviour, IDamageable
{
    [Header("Movement")]
    [SerializeField] private float baseMoveSpeed = 4.5f;
    [SerializeField] private float acceleration = 20f;        
    [SerializeField] private float deceleration = 25f;        
    [SerializeField] private float comboIceMultiplier = 0.3f; 
    [SerializeField] private float postDashSpeedBonus = 1.5f; 
    [SerializeField] private float speedBonusDecay = 0.8f;   

    [Header("Dash")]
    [SerializeField] private float dashSpeed = 15f;          
    [SerializeField] private float dashDuration = 0.15f;      
    [SerializeField] private float dashCooldown = 0.8f;      
    [SerializeField] private float doubleTapWindow = 0.3f;    

    [Header("Slide")]
    [SerializeField] private float slideDuration = 0.6f;     
    [SerializeField] private float slideDecayRate = 8f;      

    [Header("Attacks")]
    [SerializeField] private float lightCooldown = 0.25f;
    [SerializeField] private float heavyCooldown = 0.6f;
    [SerializeField] private float lightWindup = 0.04f;
    [SerializeField] private float heavyWindup = 0.12f;

    [Header("Hit reaction")]
    [SerializeField] private float hitKnockback = 3f;
    [SerializeField] private float staggerOnHit = 0.18f;

    // Компоненты
    private Rigidbody2D rb;
    public ComboSystem combo;
    private ChainAttack chain;
    private Health health;
    private AttackFeedback feedback;

    private Vector2 input;
    private Vector2 facing = Vector2.right;
    private Vector2 currentVelocity = Vector2.zero; 
    private float currentSpeedMultiplier = 1f;      

    private float nextLightTime;
    private float nextHeavyTime;
    private bool isStaggered = false;

    // Dash 
    private bool isDashing = false;
    private bool isSliding = false;
    private float dashTimer = 0f;
    private float slideTimer = 0f;
    private float nextDashTime = 0f;
    private Vector2 dashDirection;
    private float slideCurrentSpeed;
    private float idleTimer = 0f;                    

    
    private float lastShiftPressTime = -999f;
    private bool wasShiftPressed = false;

    public int maxHealth = 100;
    public int CurrentHealth { get; set; }

    private void Awake()
    {
        CurrentHealth = maxHealth;
        rb = GetComponent<Rigidbody2D>();
        combo = GetComponent<ComboSystem>();
        chain = GetComponent<ChainAttack>();
        health = GetComponent<Health>();
        feedback = GetComponent<AttackFeedback>();

        gameObject.tag = "Player";

        if (health != null)
            health.OnDamaged += OnReceivedDamage;

        if (chain != null && feedback != null)
            chain.OnAnyHit += feedback.PlayHitFeedbackAt;
    }

    private void OnDestroy()
    {
        if (health != null)
            health.OnDamaged -= OnReceivedDamage;

        if (chain != null && feedback != null)
            chain.OnAnyHit -= feedback.PlayHitFeedbackAt;
    }

    private void Update()
    {
        
        input.x = Input.GetAxisRaw("Horizontal");
        input.y = Input.GetAxisRaw("Vertical");
        input = input.normalized;

        
        if (input.sqrMagnitude > 0.001f && !isDashing && !isSliding)
            facing = input;

        
        if (isStaggered) return;

        // ========== DASH ==========
        bool shiftPressed = Input.GetKeyDown(KeyCode.LeftShift) || Input.GetKeyDown(KeyCode.RightShift);
        
        if (shiftPressed && Time.time >= nextDashTime && !isDashing && !isSliding)
        {
            
            if (Time.time - lastShiftPressTime < doubleTapWindow)
            {
                
                StartSlide();
            }
            else
            {
                
                StartDash();
            }
            
            lastShiftPressTime = Time.time;
        }

        
        if (isDashing)
        {
            dashTimer -= Time.deltaTime;
            if (dashTimer <= 0f)
            {
                isDashing = false;
            }
        }

        if (isSliding)
        {
            slideTimer -= Time.deltaTime;
            if (slideTimer <= 0f)
            {
                isSliding = false;
            }
        }

        
        if (isDashing) return;

        float atkSpeed = combo != null ? combo.CurrentBonuses.attackSpeedMult : 1f;

        
        if (Input.GetKeyDown(KeyCode.J) && Time.time >= nextLightTime)
        {
            if (isSliding)
            {
                isSliding = false;
                slideTimer = 0f;
            }
            
            nextLightTime = Time.time + (lightCooldown / atkSpeed);
            StartCoroutine(PerformSwing(false, lightWindup));
        }

        
        if (Input.GetKeyDown(KeyCode.K) && Time.time >= nextHeavyTime)
        {
            if (isSliding)
            {
                isSliding = false;
                slideTimer = 0f;
            }
            
            nextHeavyTime = Time.time + (heavyCooldown / atkSpeed);
            StartCoroutine(PerformSwing(true, heavyWindup));
        }

        // Quick save/load
        if (Input.GetKeyDown(KeyCode.F5)) GameManager.Instance?.Save();
        if (Input.GetKeyDown(KeyCode.F9)) GameManager.Instance?.Load();
    }

    private void FixedUpdate()
    {
        if (isStaggered)
        {
            rb.velocity = Vector2.zero;
            currentVelocity = Vector2.zero;
            currentSpeedMultiplier = 1f;
            idleTimer = 0f;
            return;
        }

        
        if (isDashing)
        {
            rb.velocity = dashDirection * dashSpeed;
            return;
        }

       
        if (isSliding)
        {
           
            slideCurrentSpeed = Mathf.Max(0f, slideCurrentSpeed - slideDecayRate * Time.fixedDeltaTime);
            rb.velocity = dashDirection * slideCurrentSpeed;
            return;
        }

        
        
        
        float moveMult = combo != null ? combo.CurrentBonuses.moveSpeedMult : 1f;
        
        
        float finalSpeedMult = moveMult * currentSpeedMultiplier;
        float targetSpeed = baseMoveSpeed * finalSpeedMult;
        
        
        Vector2 targetVelocity = input * targetSpeed;
        
        
        float iceEffect = Mathf.Max(0f, (moveMult - 1f) * comboIceMultiplier);
        float currentAcceleration = acceleration / (1f + iceEffect);
        float currentDeceleration = deceleration / (1f + iceEffect);
        
        
        if (input.sqrMagnitude > 0.001f)
        {
            
            currentVelocity = Vector2.MoveTowards(currentVelocity, targetVelocity, 
                currentAcceleration * Time.fixedDeltaTime);
            
            
            idleTimer = 0f;
        }
        else
        {
            
            currentVelocity = Vector2.MoveTowards(currentVelocity, Vector2.zero, 
                currentDeceleration * Time.fixedDeltaTime);
            
            
            idleTimer += Time.fixedDeltaTime;
        }
        
        if (idleTimer > 0f || input.sqrMagnitude < 0.001f)
        {
            currentSpeedMultiplier = Mathf.MoveTowards(currentSpeedMultiplier, 1f, 
                (speedBonusDecay / postDashSpeedBonus) * Time.fixedDeltaTime);
        }
        
        rb.velocity = currentVelocity;

        
        if (input.x > 0.01f)
            transform.localScale = new Vector3(1f, 1f, 1f);
        else if (input.x < -0.01f)
            transform.localScale = new Vector3(-1f, 1f, 1f);
    }

    private void StartDash()
    {
        isDashing = true;
        dashTimer = dashDuration;
        nextDashTime = Time.time + dashCooldown;
        
        
        dashDirection = input.sqrMagnitude > 0.001f ? input : facing;
        dashDirection.Normalize();
        
        
        currentSpeedMultiplier = postDashSpeedBonus;
        idleTimer = 0f;
    }

    private void StartSlide()
    {
        isSliding = true;
        slideTimer = slideDuration;
        nextDashTime = Time.time + dashCooldown;
        
        
        dashDirection = input.sqrMagnitude > 0.001f ? input : facing;
        dashDirection.Normalize();
        
        slideCurrentSpeed = dashSpeed;
        
        
        currentSpeedMultiplier = postDashSpeedBonus;
        idleTimer = 0f;
    }

    private System.Collections.IEnumerator PerformSwing(bool heavy, float windup)
    {
        if (windup > 0f)
            yield return new WaitForSeconds(windup);

        if (chain != null)
            chain.DoSwingAttack(facing, heavy);

        yield return null;
    }

    private void OnReceivedDamage()
    {
        combo?.InterruptCombo();
        
        isDashing = false;
        isSliding = false;
        dashTimer = 0f;
        slideTimer = 0f;

        StopAllCoroutines();
        StartCoroutine(DoStaggerReaction());
    }

    private System.Collections.IEnumerator DoStaggerReaction()
    {
        isStaggered = true;
        yield return new WaitForSeconds(staggerOnHit);
        isStaggered = false;
    }

    public void TakeDamage(int amount, Vector2 fromDirection)
    {
        health?.TakeDamage(amount, fromDirection);

        if (rb != null)
        {
            Vector2 kb = fromDirection.normalized * hitKnockback;
            rb.AddForce(kb, ForceMode2D.Impulse);
        }

        if (health.GetCurrent() <= 0)
        {
            Die();
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }
    }

    public bool IsAlive => health != null ? health.IsAlive : true;

    public void ApplyLoadedHP(int hp)
    {
        health?.InitFromSave(hp);
    }

    public void Die()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public int GetHP() => health != null ? health.GetCurrent() : 0;
    public int GetMaxHP() => health != null ? health.GetMax() : 1;
}
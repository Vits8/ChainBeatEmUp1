using UnityEngine;


public class Health : MonoBehaviour, IDamageable
{
    [SerializeField] private int maxHealth = 100;
    [SerializeField] private int currentHealth;


    public bool IsAlive => currentHealth > 0;


    public System.Action<int, int> OnHealthChanged; // current, max
    public System.Action OnDeath;
    public System.Action OnDamaged;


    private void Awake()
    {
        currentHealth = maxHealth;
    }


    public void InitFromSave(int hp)
    {
        currentHealth = Mathf.Clamp(hp, 0, maxHealth);
        OnHealthChanged?.Invoke(currentHealth, maxHealth);
    }


    public int GetCurrent() => currentHealth;
    public int GetMax() => maxHealth;


    public void TakeDamage(int amount, Vector2 fromDirection)
    {
        if (!IsAlive) return;
        amount = Mathf.Max(0, amount);
        currentHealth = Mathf.Max(0, currentHealth - amount);
        OnHealthChanged?.Invoke(currentHealth, maxHealth);
        OnDamaged?.Invoke();


        if (currentHealth <= 0)
            OnDeath?.Invoke();
    }


    public void Heal(int amount)
    {
        currentHealth = Mathf.Min(maxHealth, currentHealth + Mathf.Max(0, amount));
        OnHealthChanged?.Invoke(currentHealth, maxHealth);
    }
}
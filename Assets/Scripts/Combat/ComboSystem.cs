using UnityEngine;


[System.Serializable]
public class ComboBonuses
{
    public float attackSpeedMult = 1f;
    public float moveSpeedMult = 1f;
    public float damageMult = 1f;
}


public class ComboSystem : MonoBehaviour
{
    [Header("Combo")]
    [SerializeField] private float comboTimeout = 1.2f; // секунда між ударами
    [SerializeField] private int maxCombo = 5;
    [SerializeField] public int currentCombo = 0;


    private float lastHitTime = -999f;
    public ComboBonuses CurrentBonuses { get; private set; } = new ComboBonuses();


    public System.Action<int, ComboBonuses> OnComboChanged;


    public int GetComboLevel() => currentCombo;


    public void LoadComboLevel(int level)
    {
        currentCombo = Mathf.Clamp(level, 0, maxCombo);
        RecalcBonuses();
        OnComboChanged?.Invoke(currentCombo, CurrentBonuses);
    }


    public void RegisterSuccessfulHit()
    {
        if (Time.time - lastHitTime <= comboTimeout || currentCombo == 0)
            currentCombo = Mathf.Min(maxCombo, currentCombo + 1);
        else
            currentCombo = 1;


        lastHitTime = Time.time;
        RecalcBonuses();
        OnComboChanged?.Invoke(currentCombo, CurrentBonuses);
    }


    public void InterruptCombo()
    {
        currentCombo = 0;
        lastHitTime = -999f;
        RecalcBonuses();
        OnComboChanged?.Invoke(currentCombo, CurrentBonuses);
    }


    private void Update()
    {
        if (currentCombo > 0 && Time.time - lastHitTime > comboTimeout)
        {
            InterruptCombo();
        }
    }


    private void RecalcBonuses()
    {
        float atk = 1f, move = 1f, dmg = 1f;
        switch (currentCombo)
        {
            case 0: atk = 1f; move = 1f; dmg = 1f; break;
            case 1: atk = 1.05f; move = 1.00f; dmg = 1.00f; break;
            case 2: atk = 1.10f; move = 1.05f; dmg = 1.00f; break;
            case 3: atk = 1.15f; move = 1.10f; dmg = 1.10f; break;
            case 4: atk = 1.20f; move = 1.15f; dmg = 1.15f; break;
            default: atk = 1.25f; move = 1.20f; dmg = 1.20f; break; // 5+
        }
        CurrentBonuses.attackSpeedMult = atk;
        CurrentBonuses.moveSpeedMult = move;
        CurrentBonuses.damageMult = dmg;
    }
}
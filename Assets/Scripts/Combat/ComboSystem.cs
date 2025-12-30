using UnityEngine;

[System.Serializable]
public class ComboBonuses
{
    public float attackSpeedMult = 1f;
    public float moveSpeedMult = 1f;
    public float damageMult = 1f;
}

public enum ComboRank
{
    None,   // Нет ранга
    D,      // 5 ударов
    C,      // 10 ударов
    B,      // 15 ударов
    A,      // 20 ударов
    S       // 25+ ударов
}

public class ComboSystem : MonoBehaviour
{
    [Header("Combo")]
    [SerializeField] private float comboTimeout = 1.2f;
    [SerializeField] private int hitsPerRank = 5;              // Ударов для получения ранга
    [SerializeField] private float baseMultiplierIncrease = 0.25f; // Базовое увеличение (25%)

    private int currentCombo = 0;
    private int totalHitsInSession = 0;                        // Всего ударов в текущей сессии
    private ComboRank currentRank = ComboRank.None;
    private ComboRank previousRank = ComboRank.None;
    
    private float lastHitTime = -999f;
    
    // Сохраненный множитель от предыдущих рангов
    private float savedAttackMult = 1f;
    private float savedMoveMult = 1f;
    private float savedDamageMult = 1f;
    
    // Текущий максимальный бонус для этого ранга
    private float currentMaxBonus = 0.25f; // Начинаем с 25%

    public ComboBonuses CurrentBonuses { get; private set; } = new ComboBonuses();

    public System.Action<int, ComboBonuses, ComboRank> OnComboChanged;
    public System.Action<ComboRank> OnRankAchieved;

    public int GetComboLevel() => currentCombo;
    public int GetTotalHits() => totalHitsInSession;
    public ComboRank GetCurrentRank() => currentRank;

    public void LoadComboLevel(int level)
    {
        currentCombo = Mathf.Max(0, level);
        totalHitsInSession = currentCombo;
        RecalculateRankAndBonuses();
        OnComboChanged?.Invoke(currentCombo, CurrentBonuses, currentRank);
    }

    public void RegisterSuccessfulHit()
    {
        // Проверяем таймаут
        if (Time.time - lastHitTime > comboTimeout && currentCombo > 0)
        {
            // Таймаут истек, но не сбрасываем ранги - только текущее комбо
            currentCombo = 0;
        }

        currentCombo++;
        totalHitsInSession++;
        lastHitTime = Time.time;

        // Проверяем достижение нового ранга
        CheckRankProgression();
        
        RecalcBonuses();
        OnComboChanged?.Invoke(currentCombo, CurrentBonuses, currentRank);
    }

    public void InterruptCombo()
    {
        // При получении урона сбрасываем ВСЁ
        currentCombo = 0;
        totalHitsInSession = 0;
        currentRank = ComboRank.None;
        previousRank = ComboRank.None;
        
        lastHitTime = -999f;
        
        // Сбрасываем сохраненные множители
        savedAttackMult = 1f;
        savedMoveMult = 1f;
        savedDamageMult = 1f;
        
        // Сбрасываем максимальный бонус до начального значения
        currentMaxBonus = baseMultiplierIncrease;
        
        RecalcBonuses();
        OnComboChanged?.Invoke(currentCombo, CurrentBonuses, currentRank);
    }

    private void Update()
    {
        if (currentCombo > 0 && Time.time - lastHitTime > comboTimeout)
        {
            // Таймаут - сбрасываем только текущее комбо, но НЕ ранги
            currentCombo = 0;
            RecalcBonuses();
            OnComboChanged?.Invoke(currentCombo, CurrentBonuses, currentRank);
        }
    }

    private void CheckRankProgression()
    {
        ComboRank newRank = CalculateRank(totalHitsInSession);
        
        // Если достигли нового ранга
        if (newRank > currentRank)
        {
            // Сохраняем текущие множители как базовые для следующего ранга
            savedAttackMult = CurrentBonuses.attackSpeedMult;
            savedMoveMult = CurrentBonuses.moveSpeedMult;
            savedDamageMult = CurrentBonuses.damageMult;
            
            previousRank = currentRank;
            currentRank = newRank;
            
            // Увеличиваем максимальный бонус на 25%
            currentMaxBonus += baseMultiplierIncrease;
            
            OnRankAchieved?.Invoke(currentRank);
            
            Debug.Log($"Rank achieved: {currentRank}! Max bonus now: +{(currentMaxBonus * 100):F0}%");
        }
    }

    private ComboRank CalculateRank(int hits)
    {
        if (hits >= hitsPerRank * 5) return ComboRank.S;  // 25+ ударов
        if (hits >= hitsPerRank * 4) return ComboRank.A;  // 20+ ударов
        if (hits >= hitsPerRank * 3) return ComboRank.B;  // 15+ ударов
        if (hits >= hitsPerRank * 2) return ComboRank.C;  // 10+ ударов
        if (hits >= hitsPerRank) return ComboRank.D;      // 5+ ударов
        return ComboRank.None;
    }

    private void RecalculateRankAndBonuses()
    {
        // Пересчитываем ранг на основе общего количества ударов
        ComboRank calculatedRank = CalculateRank(totalHitsInSession);
        
        if (calculatedRank != currentRank)
        {
            currentRank = calculatedRank;
            
            // Пересчитываем максимальный бонус на основе ранга
            int rankLevel = (int)currentRank;
            currentMaxBonus = baseMultiplierIncrease * (rankLevel > 0 ? rankLevel : 1);
        }
        
        RecalcBonuses();
    }

    private void RecalcBonuses()
    {
        // Базовые бонусы для текущего комбо (прогресс внутри ранга)
        float comboProgress = currentCombo / (float)hitsPerRank;
        comboProgress = Mathf.Clamp01(comboProgress); // 0-1 от 0 до 5 ударов
        
        // Рассчитываем бонус для текущего комбо
        float currentComboBonus = currentMaxBonus * comboProgress;
        
        // Применяем бонус ПОВЕРХ сохраненных множителей от предыдущих рангов
        float atk = savedAttackMult * (1f + currentComboBonus);
        float move = savedMoveMult * (1f + currentComboBonus * 0.8f);  // Скорость растет чуть медленнее
        float dmg = savedDamageMult * (1f + currentComboBonus);
        
        CurrentBonuses.attackSpeedMult = atk;
        CurrentBonuses.moveSpeedMult = move;
        CurrentBonuses.damageMult = dmg;
    }

    // Для отображения в UI
    public string GetRankString()
    {
        return currentRank == ComboRank.None ? "-" : currentRank.ToString();
    }

    public float GetProgressToNextRank()
    {
        int currentRankHits = (int)currentRank * hitsPerRank;
        int nextRankHits = ((int)currentRank + 1) * hitsPerRank;
        
        if (currentRank == ComboRank.S)
            return 1f; // Максимальный ранг
        
        float progress = (totalHitsInSession - currentRankHits) / (float)(nextRankHits - currentRankHits);
        return Mathf.Clamp01(progress);
    }

    public int GetHitsToNextRank()
    {
        if (currentRank == ComboRank.S)
            return 0; // Уже максимальный ранг
        
        int nextRankHits = ((int)currentRank + 1) * hitsPerRank;
        return nextRankHits - totalHitsInSession;
    }
}
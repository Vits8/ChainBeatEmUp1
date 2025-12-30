using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class PlayerUI : MonoBehaviour
{
    [Header("UI References")]
    public Image hpFill;
    public TextMeshProUGUI comboText;
    public TextMeshProUGUI rankText;
    public Image rankProgressBar;
    public TextMeshProUGUI hitsToNextRankText;
    public GameObject saveIcon;

    [Header("Rank Colors")]
    public Color colorNone = Color.gray;
    public Color colorD = new Color(0.8f, 0.5f, 0.3f);     // Коричневый
    public Color colorC = new Color(0.3f, 0.7f, 1f);       // Голубой
    public Color colorB = new Color(0.3f, 1f, 0.3f);       // Зеленый
    public Color colorA = new Color(1f, 0.8f, 0.2f);       // Золотой
    public Color colorS = new Color(1f, 0.2f, 0.2f);       // Красный

    [Header("Effects")]
    public float shakeIntensityBase = 2f;                  // Базовая интенсивность тряски
    public float shakeSpeed = 20f;                         // Скорость тряски
    public float rankScalePulse = 1.2f;                    // Масштаб при получении ранга
    public float rankScaleDuration = 0.5f;                 // Длительность эффекта масштаба

    [Header("Rank Animation")]
    public AnimationCurve rankScaleCurve = AnimationCurve.EaseInOut(0, 1, 1, 1);

    private PlayerController player;
    private ComboSystem combo;
    private Health health;

    private Vector3 rankOriginalPosition;
    private Vector3 rankOriginalScale;
    private ComboRank lastRank = ComboRank.None;
    private bool isAnimatingRank = false;

    void Start()
    {
        player = FindObjectOfType<PlayerController>();
        if (player != null)
        {
            combo = player.GetComponent<ComboSystem>();
            health = player.GetComponent<Health>();

            // Подписываемся на события комбо
            if (combo != null)
            {
                combo.OnRankAchieved += OnRankAchieved;
            }
        }

        if (rankText != null)
        {
            rankOriginalPosition = rankText.transform.localPosition;
            rankOriginalScale = rankText.transform.localScale;
        }

        if (saveIcon != null)
            saveIcon.SetActive(false);
    }

    void OnDestroy()
    {
        if (combo != null)
        {
            combo.OnRankAchieved -= OnRankAchieved;
        }
    }

    void Update()
    {
        if (player == null) return;

        UpdateHealthBar();
        UpdateComboDisplay();
        UpdateRankDisplay();
    }

    private void UpdateHealthBar()
    {
        if (health != null && hpFill != null)
        {
            float ratio = (float)health.GetCurrent() / health.GetMax();
            hpFill.fillAmount = ratio;

            // Меняем цвет HP бара в зависимости от здоровья
            if (ratio > 0.6f)
                hpFill.color = Color.Lerp(Color.yellow, Color.green, (ratio - 0.6f) / 0.4f);
            else if (ratio > 0.3f)
                hpFill.color = Color.Lerp(Color.red, Color.yellow, (ratio - 0.3f) / 0.3f);
            else
                hpFill.color = Color.red;
        }
    }

    private void UpdateComboDisplay()
    {
        if (combo == null || comboText == null) return;

        int comboLevel = combo.GetComboLevel();
        if (comboLevel > 0)
        {
            comboText.text = $"<b>COMBO x{comboLevel}</b>\n" +
                             $"<size=80%>ATK: <color=#FF6B6B>{combo.CurrentBonuses.attackSpeedMult:F2}×</color>\n" +
                             $"SPD: <color=#4ECDC4>{combo.CurrentBonuses.moveSpeedMult:F2}×</color>\n" +
                             $"DMG: <color=#FFE66D>{combo.CurrentBonuses.damageMult:F2}×</color></size>";
        }
        else
        {
            comboText.text = "";
        }
    }

    private void UpdateRankDisplay()
    {
        if (combo == null) return;

        ComboRank currentRank = combo.GetCurrentRank();

        // --- Обновление текста ранга ---
        if (rankText != null)
        {
            string rankString = combo.GetRankString();
            
            if (currentRank == ComboRank.None)
            {
                rankText.text = "";
            }
            else
            {
                int totalHits = combo.GetTotalHits();
                rankText.text = $"<size=120%><b>RANK {rankString}</b></size>\n<size=70%>{totalHits} hits</size>";
                
                // Применяем цвет
                rankText.color = GetRankColor(currentRank);
                
                // Применяем тряску если не анимируем ранг
                if (!isAnimatingRank)
                {
                    ApplyRankShake(currentRank);
                }
            }
        }

        // --- Обновление прогресс-бара ---
        if (rankProgressBar != null)
        {
            if (currentRank == ComboRank.S)
            {
                rankProgressBar.fillAmount = 1f;
            }
            else
            {
                rankProgressBar.fillAmount = combo.GetProgressToNextRank();
            }
            
            rankProgressBar.color = GetRankColor(currentRank);
        }

        // --- Обновление текста до следующего ранга ---
        if (hitsToNextRankText != null)
        {
            if (currentRank == ComboRank.S)
            {
                hitsToNextRankText.text = "MAX RANK!";
                hitsToNextRankText.color = colorS;
            }
            else if (currentRank == ComboRank.None)
            {
                hitsToNextRankText.text = $"{combo.GetHitsToNextRank()} hits to Rank D";
                hitsToNextRankText.color = colorNone;
            }
            else
            {
                ComboRank nextRank = (ComboRank)((int)currentRank + 1);
                hitsToNextRankText.text = $"{combo.GetHitsToNextRank()} to Rank {nextRank}";
                hitsToNextRankText.color = GetRankColor(nextRank);
            }
        }
    }

    private void ApplyRankShake(ComboRank rank)
    {
        if (rankText == null || rank == ComboRank.None) return;

        // Интенсивность тряски зависит от ранга
        float intensity = shakeIntensityBase * (1 + (int)rank * 0.3f);
        
        // Создаем эффект тряски
        float shakeX = Mathf.Sin(Time.time * shakeSpeed) * intensity;
        float shakeY = Mathf.Cos(Time.time * shakeSpeed * 1.3f) * intensity;
        
        rankText.transform.localPosition = rankOriginalPosition + new Vector3(shakeX, shakeY, 0);
    }

    private Color GetRankColor(ComboRank rank)
    {
        switch (rank)
        {
            case ComboRank.D: return colorD;
            case ComboRank.C: return colorC;
            case ComboRank.B: return colorB;
            case ComboRank.A: return colorA;
            case ComboRank.S: return colorS;
            default: return colorNone;
        }
    }

    private void OnRankAchieved(ComboRank newRank)
    {
        Debug.Log($"UI: Rank {newRank} achieved!");
        
        // Запускаем анимацию ранга
        if (rankText != null)
        {
            StopAllCoroutines();
            StartCoroutine(AnimateRankAchievement(newRank));
        }
    }

    private IEnumerator AnimateRankAchievement(ComboRank rank)
    {
        isAnimatingRank = true;
        float elapsed = 0f;

        // Сохраняем начальные значения
        Vector3 startScale = rankOriginalScale;
        Vector3 targetScale = rankOriginalScale * rankScalePulse;

        // Анимация увеличения и возврата
        while (elapsed < rankScaleDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / rankScaleDuration;
            
            // Используем анимационную кривую для плавности
            float curveValue = rankScaleCurve.Evaluate(t);
            
            // Масштаб: увеличиваем до пика, потом возвращаем
            float scaleProgress = Mathf.Sin(t * Mathf.PI);
            rankText.transform.localScale = Vector3.Lerp(startScale, targetScale, scaleProgress);
            
            // Интенсивная тряска во время анимации
            float shakeIntensity = shakeIntensityBase * 5f * scaleProgress;
            float shakeX = Random.Range(-shakeIntensity, shakeIntensity);
            float shakeY = Random.Range(-shakeIntensity, shakeIntensity);
            rankText.transform.localPosition = rankOriginalPosition + new Vector3(shakeX, shakeY, 0);
            
            yield return null;
        }

        // Возвращаем к исходным значениям
        rankText.transform.localScale = rankOriginalScale;
        rankText.transform.localPosition = rankOriginalPosition;
        
        isAnimatingRank = false;
    }

    public void ShowSaveIcon()
    {
        if (saveIcon != null)
            StartCoroutine(FlashSaveIcon());
    }

    private IEnumerator FlashSaveIcon()
    {
        saveIcon.SetActive(true);
        yield return new WaitForSeconds(1f);
        saveIcon.SetActive(false);
    }
}
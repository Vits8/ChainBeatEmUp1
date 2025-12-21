using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PlayerUI : MonoBehaviour
{
    [Header("UI References")]
    public Image hpFill;
    public TextMeshProUGUI comboText;
    public GameObject saveIcon;

    private PlayerController player;
    private ComboSystem combo;
    private Health health;

    void Start()
    {
        player = FindObjectOfType<PlayerController>();
        if (player != null)
        {
            combo = player.GetComponent<ComboSystem>();
            health = player.GetComponent<Health>();
        }

        if (saveIcon != null)
            saveIcon.SetActive(false);
    }

    void Update()
    {
        if (player == null) return;

        // --- HP ---
        if (health != null && hpFill != null)
        {
            float ratio = (float)health.GetCurrent() / health.GetMax();
            hpFill.fillAmount = ratio;
        }

        // --- Combo ---
        if (combo != null && comboText != null)
        {
            if (combo.GetComboLevel() > 0)
            {
                comboText.text = $"Combo x{combo.GetComboLevel()}\n" +
                                 $"ATK {combo.CurrentBonuses.attackSpeedMult:F2}×\n" +
                                 $"SPD {combo.CurrentBonuses.moveSpeedMult:F2}×\n" +
                                 $"DMG {combo.CurrentBonuses.damageMult:F2}×";
            }
            else
            {
                comboText.text = "";
            }
        }
    }

    
    public void ShowSaveIcon()
    {
        if (saveIcon != null)
            StartCoroutine(FlashSaveIcon());
    }

    private System.Collections.IEnumerator FlashSaveIcon()
    {
        saveIcon.SetActive(true);
        yield return new WaitForSeconds(1f);
        saveIcon.SetActive(false);
    }
}

using UnityEngine;
using UnityEngine.UI;

public class AttackRangeIndicator : MonoBehaviour
{
    public Image sectorImage; 
    private RectTransform rt;

    void Awake()
    {
        if (sectorImage == null)
        {
            Debug.LogError("Sector Image �� ���������� � ���������!");
            return;
        }
        rt = sectorImage.GetComponent<RectTransform>();
        sectorImage.enabled = false;
    }

    
    /// <param name="radius">����� �����</param>
    /// <param name="angle">��� �����</param>
    /// <param name="direction">������ �����</param>
    public void Show(float radius, float angle, Vector2 direction)
    {
        if (sectorImage == null || rt == null) return;

        sectorImage.enabled = true;

        
        rt.localScale = new Vector3(radius * 2f, radius * 2f, 1f);

        
        sectorImage.fillAmount = angle / 360f;

        
        float angleZ = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        rt.rotation = Quaternion.Euler(0, 0, angleZ - angle / 2f);

        
        sectorImage.color = new Color(1f, 0f, 0f, 0.4f);

        
        CancelInvoke(nameof(Hide));
        Invoke(nameof(Hide), 0.2f);
    }

    private void Hide()
    {
        if (sectorImage != null)
            sectorImage.enabled = false;
    }
}

using UnityEngine;
using UnityEngine.UI;

public class EnemyHealthBar : MonoBehaviour
{
    public Image fill;
    public EnemyAI enemyScript; 
    private Camera cam;

    void Start()
    {
        cam = Camera.main;
    }

    void Update()
    {
        if (enemyScript == null || fill == null) return;

        int maxHealth = enemyScript.maxHealth;
        int health = enemyScript.CurrentHealth; 

      
        if (cam != null)
            transform.rotation = Quaternion.LookRotation(Vector3.forward, cam.transform.up);

       
        float ratio = (float)health / (float)maxHealth;
        fill.fillAmount = ratio;
    }
}

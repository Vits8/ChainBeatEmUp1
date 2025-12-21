using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    private string saveKey = "SaveData";
    private SaveData loadedData;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        DontDestroyOnLoad(gameObject);
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    // ------------------- SAVE -------------------
    public void Save()
    {
        SaveData data = new SaveData();

        
        PlayerController player = FindObjectOfType<PlayerController>();
        if (player != null)
        {
            data.playerHealth = player.GetHP();
            data.comboLevel = player.combo.GetComboLevel();
            data.playerPosX = player.transform.position.x;
            data.playerPosY = player.transform.position.y;
        }

        
        data.currentScene = SceneManager.GetActiveScene().buildIndex;

        
        EnemyAI[] enemies = FindObjectsOfType<EnemyAI>();
        foreach (var enemy in enemies)
        {
            EnemySaveData esd = new EnemySaveData
            {
                enemyID = enemy.gameObject.name + "_" + enemy.GetInstanceID(),
                currentHealth = enemy.CurrentHealth,
                posX = enemy.transform.position.x,
                posY = enemy.transform.position.y
            };
            data.enemies.Add(esd);
        }

        string json = JsonUtility.ToJson(data);
        PlayerPrefs.SetString(saveKey, json);
        PlayerPrefs.Save();

        Debug.Log("Game Saved");
        FindObjectOfType<PlayerUI>()?.ShowSaveIcon();
    }

    // ------------------- LOAD -------------------
    public void Load()
    {
        if (!PlayerPrefs.HasKey(saveKey))
        {
            Debug.Log("No save data found.");
            return;
        }

        string json = PlayerPrefs.GetString(saveKey);
        loadedData = JsonUtility.FromJson<SaveData>(json);

       
        if (SceneManager.GetActiveScene().buildIndex != loadedData.currentScene)
        {
            SceneManager.LoadScene(loadedData.currentScene);
        }
        else
        {
            OnSceneLoaded(SceneManager.GetActiveScene(), LoadSceneMode.Single);
        }
    }

    // ------------------- Scene Loaded -------------------
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (loadedData == null) return;

        
        PlayerController player = FindObjectOfType<PlayerController>();
        if (player != null)
        {
            player.ApplyLoadedHP(loadedData.playerHealth);
            player.combo.LoadComboLevel(loadedData.comboLevel);
            player.transform.position = new Vector2(loadedData.playerPosX, loadedData.playerPosY);
        }

        
        RestoreEnemies(loadedData.enemies);

        loadedData = null;

        Debug.Log("Game Loaded");
        FindObjectOfType<PlayerUI>()?.ShowSaveIcon();
    }

    // ------------------- Restore Enemies -------------------
    private void RestoreEnemies(List<EnemySaveData> enemiesData)
    {
        EnemyAI[] enemies = FindObjectsOfType<EnemyAI>();
        foreach (var enemy in enemies)
        {
            foreach (var eData in enemiesData)
            {
                
                if (enemy.gameObject.name + "_" + enemy.GetInstanceID() == eData.enemyID)
                {
                    enemy.currentHealth = eData.currentHealth;
                    enemy.transform.position = new Vector2(eData.posX, eData.posY);
                }
            }
        }
    }
}

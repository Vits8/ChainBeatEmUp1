using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class SaveData
{
    // ------------------- √равець -------------------
    public int playerHealth;      // поточне HP гравц€
    public int comboLevel;        // поточний р≥вень комбо
    public float playerPosX;      // X позиц≥€ гравц€
    public float playerPosY;      // Y позиц≥€ гравц€

    // ------------------- ѕрогрес -------------------
    public int currentScene;      // ≥ндекс поточноњ сцени

    // ------------------- ¬ороги -------------------
    public List<EnemySaveData> enemies = new List<EnemySaveData>();
}

[System.Serializable]
public class EnemySaveData
{
    public string enemyID;       // ун≥кальний ID ворога
    public int currentHealth;    // поточне HP ворога
    public float posX;           // X позиц≥€ ворога
    public float posY;           // Y позиц≥€ ворога
}

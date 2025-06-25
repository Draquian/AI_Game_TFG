using System;
using UnityEngine;

[Serializable]
public class SaveData_Copilot
{
    public string sceneName;

    // Example of player position data
    public float playerPosX;
    public float playerPosY;
    public float playerPosZ;

    public PlayerStats_Copilot pStats;

    // Add a field to store the inventory data.
    public InventoryData_Copilot inventoryData;

    public string magic;

    //Settigns
    public string lenguage = "Català";
    public float UIScale = 1;
    public float sensivility = 1;
    public float masterVolume = 1;
    public float SFXVolume = 1;
    public float musicVolume = 1;
    public bool fullScreen = true;

}

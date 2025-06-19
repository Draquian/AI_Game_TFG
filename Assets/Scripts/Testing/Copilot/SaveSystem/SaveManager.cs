using UnityEngine;

public class SaveManager
{
    // Private constructor prevents outside instantiation.
    private SaveManager()
    {
        // Attempt to load existing save data.
        currentData = SaveSystem_Copilot.Load();

        // If no save exists, initialize a new SaveData_Copilot instance.
        if (currentData == null)
        {
            currentData = new SaveData_Copilot();
            // Initialize the inventory if not already set.
        }
    }

    // The unique instance is stored in a static field.
    private static SaveManager instance;
    public static SaveManager Instance
    {
        get
        {
            if (instance == null)
            {
                instance = new SaveManager();
            }
            return instance;
        }
    }


    // Persistent SaveData instance used to store all game state.
    private SaveData_Copilot currentData;

    // Call this to save the player's position.
    public void SaveGame(Transform playerTransform)
    {
        currentData.playerPosX = playerTransform.position.x;
        currentData.playerPosY = playerTransform.position.y;
        currentData.playerPosZ = playerTransform.position.z;

        SaveSystem_Copilot.Save(currentData);
        Debug.Log("Player position saved successfully.");
    }

    // Call this to save the current scene name.
    public void SaveGame(string sceneName)
    {
        currentData.sceneName = sceneName;

        SaveSystem_Copilot.Save(currentData);
        Debug.Log("Scene saved successfully.");
    }

    public void SaveGame(PlayerStats_Copilot playerStats)
    {
        currentData.pStats = playerStats;
    }

    /// <summary>
    /// Saves the current state of the inventory to disk.
    /// </summary>
    /// <param name="inventory">The live Inventory_Copilot component to save.</param>
    public void SaveGame(Inventory_Copilot inventory)
    {
        // Load the full save data; if none exists, create a new SaveData_Copilot.
        SaveData_Copilot data = SaveSystem_Copilot.Load();
        if (data == null)
        {
            data = new SaveData_Copilot();
        }

        // Create a snapshot of the inventory.
        data.inventoryData = InventorySaveUtility_Copilot.CreateInventoryData(inventory);

        // Save the whole data object to disk.
        SaveSystem_Copilot.Save(data);
        Debug.Log("Inventory saved successfully.");
    }

    public void SaveGame(MagicType magic)
    {
        currentData.magic = magic;
    }

    /// <summary>
    /// Loads the inventory data from disk and applies it to the live inventory.
    /// </summary>
    /// <param name="inventory">The live Inventory_Copilot component to which the data will be applied.</param>
    public void LoadInventory(Inventory_Copilot inventory)
    {
        // Load the current save data.
        SaveData_Copilot data = SaveSystem_Copilot.Load();
        if (data != null && data.inventoryData != null)
        {
            // Apply the saved data into the inventory.
            InventorySaveUtility_Copilot.ApplyInventoryData(inventory, data.inventoryData);
            Debug.Log("Inventory loaded successfully.");
        }
        else
        {
            Debug.LogWarning("No saved inventory data found. Make sure you save first.");
        }
    }

    // Load stats.
    public PlayerStats_Copilot LoadPlayerSats()
    {
        SaveData_Copilot data = SaveSystem_Copilot.Load();
        if (data != null && data.pStats != null)
        {
            Debug.Log("Inventory loaded successfully.");
            return data.pStats;
        }
        else
        {
            Debug.LogWarning("No inventory found; instantiating a new one.");
            return new PlayerStats_Copilot();
        }
    }
}
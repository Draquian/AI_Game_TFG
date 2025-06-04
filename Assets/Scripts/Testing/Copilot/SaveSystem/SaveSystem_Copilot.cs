using System.IO;
using UnityEngine;

public static class SaveSystem_Copilot
{
    // You can change the file name if needed.
    private static string fileName = "savefile.json";

    /// <summary>
    /// Ensures that a save file exists. If not, creates one with default data.
    /// </summary>
    public static void EnsureSaveFileExists()
    {
        string path = GetSavePath();
        if (!File.Exists(path))
        {
            // Create default SaveData_Copilot, adjust default values as needed.
            SaveData_Copilot defaultData = new SaveData_Copilot
            {
                sceneName = "MainMenu", // Set a default scene name or initial scene
                playerPosX = 0f,
                playerPosY = 0f,
                playerPosZ = 0f
            };

            Save(defaultData);
            Debug.Log("Save file created at: " + path);
        }
        else
        {
            Debug.Log("Save file already exists at: " + path);
        }
    }

    /// <summary>
    /// Saves the given SaveData_Copilot to persistent storage.
    /// </summary>
    /// <param name="data">The game state to save.</param>
    public static void Save(SaveData_Copilot data)
    {
        string json = JsonUtility.ToJson(data, true);
        string path = GetSavePath();
        File.WriteAllText(path, json);
        Debug.Log("Game saved to: " + path);
    }

    /// <summary>
    /// Loads the SaveData_Copilot from persistent storage.
    /// </summary>
    /// <returns>A SaveData_Copilot object if a save exists; otherwise, null.</returns>
    public static SaveData_Copilot Load()
    {
        string path = GetSavePath();

        if (File.Exists(path))
        {
            string json = File.ReadAllText(path);
            SaveData_Copilot data = JsonUtility.FromJson<SaveData_Copilot>(json);
            Debug.Log("Game loaded from: " + path);
            return data;
        }
        else
        {
            Debug.Log("Save file not found at: " + path);
            return null;
        }
    }

    /// <summary>
    /// Verifies if a save file exists.
    /// </summary>
    /// <returns>True if a save file exists; otherwise, false.</returns>
    public static bool SaveFileExists()
    {
        return File.Exists(GetSavePath());
    }

    /// <summary>
    /// Deletes the existing save file.
    /// </summary>
    public static void DeleteSaveFile()
    {
        string path = GetSavePath();
        if (File.Exists(path))
        {
            File.Delete(path);
            Debug.Log("Deleted save file at: " + path);
        }
        else
        {
            Debug.Log("No save file exists at: " + path);
        }
    }

    /// <summary>
    /// Gets the full path of the save file.
    /// </summary>
    /// <returns>The full path for the save file.</returns>
    private static string GetSavePath()
    {
        return Path.Combine(Application.persistentDataPath, fileName);
    }
}

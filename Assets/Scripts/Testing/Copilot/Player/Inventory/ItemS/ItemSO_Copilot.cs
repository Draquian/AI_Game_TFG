using UnityEngine;

[CreateAssetMenu(fileName = "New Item", menuName = "Inventory/Item", order = 0)]
public class ItemSO_Copilot : ScriptableObject
{
    public Sprite icon;

    // New: Reference to a 3D model prefab that represents this item in the world.
    public GameObject worldModelPrefab;

    public enum ItemType { Weapon, Equipment, Consumable, Other }
    public ItemType itemType;


    // The stats field will hold a reference to one of the derived stat classes.
    [SerializeReference]
    public BaseItemStats_Copilot stats;

    /*public void Interact(GameObject interactor)
    {
        Debug.Log("Item Interaction");
    }*/
}

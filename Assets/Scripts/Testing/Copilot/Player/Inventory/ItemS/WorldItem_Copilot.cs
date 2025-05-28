using UnityEngine;

public class WorldItem_Copilot : MonoBehaviour, IInteractable_Copilot
{
    //public ItemSO_Copilot itemData; // Assign this via the Inspector or set via code
    public Sprite icon;

    //public ItemType type;
    public string nameItem;
    public string description;

    public ItemSO_Copilot item;


    private void Start()
    {
        // Instantiates the model when the world item is spawned.
        /*if (itemData.worldModelPrefab != null)
        {
            //GameObject model = Instantiate(itemData.worldModelPrefab, transform.position, transform.rotation, transform);
        }*/
    }

    // When the player interacts with this object, it can transfer the item to the inventory.
    public void Interact(GameObject interactor)
    {
        item.icon = icon;
        //item.itemType = type;
        item.stats.itemName = nameItem;
        item.stats.description = description;

        // Example: Add the item to the Inventory
        // Inventory playerInventory = ...; // get a reference to the player's inventory.
        interactor.GetComponent<Inventory_Copilot>().AddItem(item);

        // After picking up the item, you might disable or destroy the world model.
        Destroy(gameObject);
    }
}

using System.Collections.Generic;
using UnityEngine;


public class InventoryBase : MonoBehaviour
{
    [SerializeField] protected List<Slot_UI> slots = new List<Slot_UI>();
    [SerializeField] protected Canvas canvas;
    protected Inventory inventory;
    public string inventoryName;

    protected virtual void Awake()
    {
        canvas = FindObjectOfType<Canvas>();
    }

    protected virtual void Start()
    {
        inventory = GameManager.instance.player.inventoryManager.GetInventoryByName(inventoryName);
        SetUpSlot();
    }

    protected virtual void Update()
    {
        Refresh();
    }

    // 인벤토리 갱신
    public void Refresh()
    {
        if (slots.Count == inventory.slots.Count)
        {
            for (int i = 0; i < slots.Count; i++)
            {
                if (inventory.slots[i].itemName != "")
                {
                    slots[i].SetItem(inventory.slots[i]);
                }
                else
                {
                    slots[i].EmptyItem();
                }
            }
        }
    }

    public void Remove()
    {
        if (inventory == null || UIManager.instance.draggedSlot == null) return;

        int slotID = UIManager.instance.draggedSlot.slotID;
        var slotData = inventory.slots[slotID];

        if (string.IsNullOrEmpty(slotData.itemName)) return;

        Item itemToDrop = GameManager.instance.itemManager.GetItemByName(slotData.itemName);
        if (itemToDrop != null)
        {
            GameManager.instance.player.DropItem(itemToDrop);
            inventory.Remove(slotID);

            // if (UIManager.dragSingle)
            // {
            //     GameManager.instance.player.DropItem(itemToDrop);
            //     inventory.Remove(UIManager.draggedSlot.slotID);
            // }
            // else
            // {
            //     GameManager.instance.player.DropItem(itemToDrop, inventory.slots[UIManager.draggedSlot.slotID].count);
            //     inventory.Remove(UIManager.draggedSlot.slotID, inventory.slots[UIManager.draggedSlot.slotID].count);
            // }
        }

        Refresh();
        UIManager.instance.draggedSlot = null;
    }


    public void SlotBeginDrag(Slot_UI slot)
    {
        UIManager.instance.draggedSlot = slot;
        UIManager.instance.draggedIcon = Instantiate(slot.itemIcon);
        UIManager.instance.draggedIcon.transform.SetParent(canvas.transform);
        UIManager.instance.draggedIcon.raycastTarget = false;
        UIManager.instance.draggedIcon.rectTransform.sizeDelta = new Vector2(100, 100);

        MoveToMousePosition(UIManager.instance.draggedIcon.gameObject);
    }

    public void SlotDrag()
    {
        MoveToMousePosition(UIManager.instance.draggedIcon.gameObject);
    }

    public void SlotEndDrag()
    {
        Destroy(UIManager.instance.draggedIcon.gameObject);
        UIManager.instance.draggedIcon = null;
    }

    public void SlotDrop(Slot_UI slot)
    {
        if (slot == null || UIManager.instance.draggedSlot == null) return;

        var draggedSlot = UIManager.instance.draggedSlot;
        if (UIManager.instance.dragSingle)
        {
            draggedSlot.inventory.MoveSlot(draggedSlot.slotID, slot.slotID, slot.inventory);
        }
        else
        {
            draggedSlot.inventory.MoveSlot(draggedSlot.slotID, slot.slotID, slot.inventory, draggedSlot.inventory.slots[draggedSlot.slotID].count);

        }

        UIManager.instance.RefreshAllInventory();
    }

    public void MoveToMousePosition(GameObject toMove)
    {
        if (canvas != null)
        {
            Vector2 position;

            // 스크린상의 마우스 위치를 캔버스의 로컬 좌표로 변환
            RectTransformUtility.ScreenPointToLocalPointInRectangle(canvas.transform as RectTransform, Input.mousePosition, null, out position);

            // 변환된 로컬 위치를 다시 세계 좌표로 변환하여 게임 오브젝트를 해당 위치로 이동시킴
            toMove.transform.position = canvas.transform.TransformPoint(position);
        }
    }

    protected void SetUpSlot()
    {
        int counter = 0;

        foreach (Slot_UI slot in slots)
        {
            slot.slotID = counter;
            counter++;
            slot.inventory = inventory;
        }
    }
}

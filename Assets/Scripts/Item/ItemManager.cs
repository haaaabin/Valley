using System.Collections.Generic;
using UnityEngine;

// 아이템 관리

public class ItemManager : MonoBehaviour
{
    public Item[] items; // 수집 가능한 아이템 배열

    // 아이템 타입과 아이템 매핑을 위한 딕셔너리
    private Dictionary<string, Item> nameToItemDict = new Dictionary<string, Item>();

    private void Awake()
    {
        foreach (Item item in items)
        {
            AddItem(item);
        }
    }

    public void AddItem(Item item)
    {
        if (!nameToItemDict.ContainsKey(item.itemData.itemName))
        {
            nameToItemDict.Add(item.itemData.itemName, item);
        }
    }

    // 주어진 타입의 아이템 검색
    public Item GetItemByName(string key)
    {
        if (nameToItemDict.ContainsKey(key))
        {
            return nameToItemDict[key]; //해당 타입의 아이템 반환
        }

        return null;
    }
}

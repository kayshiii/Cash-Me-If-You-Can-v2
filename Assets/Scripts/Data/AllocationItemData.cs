using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum AllocationCategory
{
    NeedLunch,
    NeedCommute,
    Want
}

[System.Serializable]
public class AllocationItemData
{
    public string itemId;
    public string itemName;
    public AllocationCategory category;
    public int cost;
    public float happiness;
    public Sprite icon;
}

// SaveData.cs
using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class SaveData
{
    public PlayerSaveData playerData;
    public List<CreatureSaveData> partyData;
    public Vector3SaveData playerPosition;
    public List<InventoryItemSaveData> inventoryData;
    public bool overworldPaused;
    public bool battleInProgress;
}

[Serializable]
public class PlayerSaveData
{
    public string creatureName;
    public int currentHP;
    public List<BodyPartSaveData> bodyParts;
}

[Serializable]
public class BodyPartSaveData
{
    public string slotName;
    public string bodyPartName;
    public int currentHealth;
}

[Serializable]
public class CreatureSaveData
{
    public string creatureName;
    public List<BodyPartSaveData> bodyParts;
}

[Serializable]
public class InventoryItemSaveData
{
    public string itemName;
    public int quantity;
}

[Serializable]
public class Vector3SaveData
{
    public float x;
    public float y;
    public float z;

    public Vector3SaveData(Vector3 vector)
    {
        x = vector.x;
        y = vector.y;
        z = vector.z;
    }

    public Vector3 ToVector3()
    {
        return new Vector3(x, y, z);
    }
}
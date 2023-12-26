using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class UpgradeData
{
    public string Name;
    public int ID;
    public int Lv;
    public int maxLv;
    public string Context;

    [Header("權重"), Range(0, 100)]
    public int Weights;


    public UpgradeData(UpgradeData data)
    {
        this.Name = data.Name;
        this.ID = data.ID;
        this.Lv = data.Lv;
        this.maxLv = data.maxLv;
        this.Context = data.Context;
        this.Weights = data.Weights;
    }
}


public static class UpgradeDataExtension
{
    public static UpgradeData Clone(this UpgradeData data)
    {
        return new UpgradeData(data);
    }
}



[CreateAssetMenu]
public class UpgradeDataAsset : ScriptableObject
{
    public List<UpgradeData> UpgradeList;
}
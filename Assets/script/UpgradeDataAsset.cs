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

    [Header("權重"), Range(0, 10000)]
    public int Weights;
}

[CreateAssetMenu]
public class UpgradeDataAsset : ScriptableObject
{
    public List<UpgradeData> UpgradeList;
}
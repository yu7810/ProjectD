using System;
using UnityEngine;

// Keep the component and csvFile binding used by existing scenes.
public class CSVReader : MonoBehaviour
{
    public TextAsset csvFile;

    private void Awake()
    {
        // Resolve the attached ValueData before any Start, independent of singleton Awake order.
        ValueData data = GetComponent<ValueData>();
        if (data == null) data = ValueData.Instance;
        if (data == null)
        {
            Debug.LogError("Weapon CSV: ValueData is unavailable.", this);
            return;
        }
        try
        {
            if (csvFile == null) throw new FormatException("Missing Weapon.csv reference.");
            WeaponBase[] weapons = WeaponDataTable.Parse(csvFile.text).CreateWeapons();
            // Publish only once the whole table is valid; row order is resolved by ID.
            data.Weapon = weapons;
        }
        catch (FormatException error)
        {
            Debug.LogError("Weapon CSV was not applied; keeping existing data: " + error.Message, this);
        }
    }
}

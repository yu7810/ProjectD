using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CSVReader : MonoBehaviour
{
    public TextAsset csvFile; // 將CSV文件拖到這個欄位中
    private List<Dictionary<string, object>> data = new List<Dictionary<string, object>>();

    void Start()
    {
        ReadCSV();
    }

    void ReadCSV()
    {
        string[] lines = csvFile.text.Split('\n');
        string[] headers = lines[0].Split(',');

        for (int i = 1; i < lines.Length; i++)
        {
            if (string.IsNullOrWhiteSpace(lines[i])) continue;

            string[] values = lines[i].Split(',');
            Dictionary<string, object> row = new Dictionary<string, object>();

            for (int j = 0; j < headers.Length; j++)
            {
                string value = values[j].Trim(); // 去掉多餘的空格

                if(headers[j] == "ID" || headers[j] == "Price") // int
                {
                    if (int.TryParse(value, out int intValue))
                    {
                        row[headers[j]] = intValue;
                    }
                }
                else if(headers[j] == "Name" || headers[j] == "Rarity") // string
                {
                    row[headers[j]] = value;
                }
                else // float
                {
                    if (float.TryParse(value, out float floatValue))
                    {
                        row[headers[j]] = floatValue;
                    }
                }
            }
            data.Add(row);
        }

        int num = 0;
        // 輸出
        foreach (var row in data)
        {
            foreach (var pair in row)
            {
                //Debug.Log($"{pair.Key}: {pair.Value}");
                switch (pair.Key)
                {
                    case "ID":
                        ValueData.Instance.Weapon[num].ID = (int)pair.Value;
                        break;
                    case "Rarity":
                        if ((string)pair.Value == "Normal")
                            ValueData.Instance.Weapon[num].Rarity = RarityType.Normal;
                        else if ((string)pair.Value == "Magic")
                            ValueData.Instance.Weapon[num].Rarity = RarityType.Magic;
                        else if ((string)pair.Value == "Rare")
                            ValueData.Instance.Weapon[num].Rarity = RarityType.Rare;
                        else if ((string)pair.Value == "Unique")
                            ValueData.Instance.Weapon[num].Rarity = RarityType.Unique;
                        break;
                    case "Price":
                        ValueData.Instance.Weapon[num].Price = (int)pair.Value;
                        break;
                    case "Name":
                        ValueData.Instance.Weapon[num].Name = (string)pair.Value;
                        break;
                    case "Damage":
                        ValueData.Instance.Weapon[num].Damage = (float)pair.Value;
                        break;
                    case "Cooldown":
                        ValueData.Instance.Weapon[num].Cooldown = (float)pair.Value;
                        break;
                    case "Size":
                        ValueData.Instance.Weapon[num].Size = (float)pair.Value;
                        break;
                    case "Speed":
                        ValueData.Instance.Weapon[num].Speed = (float)pair.Value;
                        break;
                    case "Cost":
                        ValueData.Instance.Weapon[num].Cost = (float)pair.Value;
                        break;
                    case "Crit":
                        ValueData.Instance.Weapon[num].Crit = (float)pair.Value;
                        break;
                    case "Crit Damage":
                        ValueData.Instance.Weapon[num].CritDmg = (float)pair.Value;
                        break;
                }
            }
            num += 1;
            //Debug.Log("下個武器");
        }
    }
}

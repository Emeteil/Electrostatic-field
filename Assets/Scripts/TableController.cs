using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using TMPro;

[System.Serializable]
public class TableEntry
{
    public float x;
    public float y;
    public float v;
}

public class TableController : MonoBehaviour
{
    [SerializeField] private GameObject rowPrefab;
    [SerializeField] private Transform tableContent;
    
    private List<TableEntry> entries = new List<TableEntry>();
    private List<GameObject> rowObjects = new List<GameObject>();

    public void AddRow(float x, float y, float v)
    {
        TableEntry newEntry = new TableEntry
        {
            x = x,
            y = y,
            v = v
        };
        
        entries.Add(newEntry);
        UpdateTable();
    }

    public void DeleteRow(int index)
    {
        if (index >= 0 && index < entries.Count)
        {
            entries.RemoveAt(index);
            UpdateTable();
        }
    }

    public void ClearTable()
    {
        entries.Clear();
        UpdateTable();
    }

    private void UpdateTable()
    {
        foreach (var row in rowObjects)
            Destroy(row);
        
        rowObjects.Clear();

        for (int i = 0; i < entries.Count; i++)
        {
            var entry = entries[i];
            var row = Instantiate(rowPrefab, tableContent);
            rowObjects.Add(row);

            row.transform.localScale = Vector3.one;

            var texts = row.GetComponentsInChildren<TMP_Text>();
            texts[0].text = entry.x.ToString("F2") + " см";
            texts[1].text = entry.y.ToString("F2") + " см";
            texts[2].text = entry.v.ToString("F2") + " В";

            Button deleteButton = row.GetComponentInChildren<Button>();
            int index = i;
            deleteButton.onClick.RemoveAllListeners();
            deleteButton.onClick.AddListener(() => DeleteRow(index));
        }

        LayoutRebuilder.ForceRebuildLayoutImmediate(tableContent as RectTransform);
    }
}
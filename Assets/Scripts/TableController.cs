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
    public Color color;
    public GameObject dotObject;
}

public class TableController : MonoBehaviour
{
    [SerializeField] private GameObject rowPrefab;
    [SerializeField] private GameObject dotPrefab;
    [SerializeField] private Transform tableContent;
    
    private List<TableEntry> entries = new List<TableEntry>();
    private List<GameObject> rowObjects = new List<GameObject>();

    public void AddRow(float x, float y, float v, Vector3 unityPosition)
    {
        Color color = GetRandomColor();
        GameObject dot = SpawnBall(unityPosition, color);
        
        TableEntry newEntry = new TableEntry
        {
            x = x,
            y = y,
            v = v,
            color = color,
            dotObject = dot
        };
        
        entries.Add(newEntry);
        AddRowToTable(newEntry, entries.Count - 1);
    }

    private GameObject SpawnBall(Vector3 spawnPosition, Color color)
    {
        GameObject ball = Instantiate(dotPrefab, spawnPosition, Quaternion.identity);
        Renderer ballRenderer = ball.GetComponent<Renderer>();
        ballRenderer.material.color = color;
        return ball;
    }

    private Color GetRandomColor()
    {
        return new Color(
            Random.Range(0f, 1f),
            Random.Range(0f, 1f),
            Random.Range(0f, 1f)
        );
    }

    public void DeleteRow(int index)
    {
        if (index >= 0 && index < entries.Count)
        {
            if (entries[index].dotObject != null)
            {
                Destroy(entries[index].dotObject);
            }
            
            if (index < rowObjects.Count && rowObjects[index] != null)
            {
                Destroy(rowObjects[index]);
                rowObjects.RemoveAt(index);
            }
            
            entries.RemoveAt(index);
            
            UpdateRowIndices();
        }
    }

    public void ClearTable()
    {
        foreach (var entry in entries)
            if (entry.dotObject != null)
                Destroy(entry.dotObject);
        
        foreach (var row in rowObjects)
            if (row != null)
                Destroy(row);
        
        entries.Clear();
        rowObjects.Clear();
    }

    private void AddRowToTable(TableEntry entry, int index)
    {
        var row = Instantiate(rowPrefab, tableContent);
        rowObjects.Add(row);
        row.transform.localScale = Vector3.one;

        var texts = row.GetComponentsInChildren<TMP_Text>();
        texts[0].text = entry.x.ToString("F2") + " см";
        texts[1].text = entry.y.ToString("F2") + " см";
        texts[2].text = entry.v.ToString("F2") + " В";

        texts[0].color = entry.color;
        texts[1].color = entry.color;
        texts[2].color = entry.color;

        Button deleteButton = row.GetComponentInChildren<Button>();
        deleteButton.onClick.RemoveAllListeners();
        deleteButton.onClick.AddListener(() => DeleteRow(index));
    }

    private void UpdateRowIndices()
    {
        for (int i = 0; i < rowObjects.Count; i++)
        {
            if (rowObjects[i] != null)
            {
                Button deleteButton = rowObjects[i].GetComponentInChildren<Button>();
                deleteButton.onClick.RemoveAllListeners();
                int newIndex = i;
                deleteButton.onClick.AddListener(() => DeleteRow(newIndex));
            }
        }
    }
}
using UnityEngine;

public class CoordinateNumbering : MonoBehaviour
{
    [SerializeField] private int _count = 5;
    [SerializeField] private float _totalDistance = 10f;
    [SerializeField] private bool _horizontal = true;
    [SerializeField] private Font _font;
    [SerializeField] private float _characterSize = 0.5f;
    [SerializeField] private int _fontSize = 24;
    [SerializeField] private Color _color = Color.black;

    private void Awake()
    {
        GenerateNumbering(
            _count,
            _totalDistance,
            _horizontal,
            _font,
            _characterSize,
            _fontSize,
            _color
        );
    }

    private void GenerateNumberingAuto() {
        GenerateNumbering(
            _count,
            _totalDistance,
            _horizontal,
            _font,
            _characterSize,
            _fontSize,
            _color
        );
    }

    public void GenerateNumbering(
        int _count,
        float _totalDistance,
        bool _horizontal,
        Font _font,
        float _characterSize,
        int _fontSize,
        Color _color
    )
    {
        DestroyOldLabels();

        if (_count < 1) return;

        float step = _count > 1 ? _totalDistance / (_count - 1) : 0f;
        Vector3 basePosition = _horizontal ? Vector3.left * _totalDistance / 2 : Vector3.down * _totalDistance / 2;
        Vector3 direction = _horizontal ? Vector3.right : Vector3.up;

        for (int i = 0; i < _count; i++)
        {
            GameObject labelObject = new GameObject("Label_" + i);
            labelObject.transform.SetParent(transform);
            labelObject.transform.localPosition = basePosition + direction * step * i;

            TextMesh textMesh = labelObject.AddComponent<TextMesh>();
            textMesh.text = i.ToString();
            textMesh.characterSize = _characterSize;
            textMesh.font = _font;
            textMesh.fontSize = _fontSize;
            textMesh.color = _color;
            textMesh.anchor = TextAnchor.MiddleCenter;
            textMesh.alignment = TextAlignment.Center;
        }
    }

    private void DestroyOldLabels()
    {
        while (transform.childCount > 0)
        {
            DestroyImmediate(transform.GetChild(0).gameObject);
        }
    }

#if UNITY_EDITOR
    [UnityEditor.CustomEditor(typeof(CoordinateNumbering))]
    private class CoordinateNumberingEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            
            if (GUILayout.Button("Generate"))
            {
                ((CoordinateNumbering)target).GenerateNumberingAuto();
            }
        }
    }
#endif
}
using TMPro;
using UnityEngine;

public class SurfaceInteraction : MonoBehaviour
{
    [SerializeField] private ElectrodeGui[] electrodeGUI;
    [SerializeField] private TextMesh voltmeter;
    [SerializeField] private TMP_InputField changeGridSizeX;
    [SerializeField] private TMP_InputField changeGridSizeY;

    private Vector2 _lastHoverPosition;
    private TableController _tableController;
    private Surface _surface;
    private Collider _collider;
    private bool _heatMap = false;

    private Vector2[] _saveCoords = new Vector2[2];

    private void Start()
    {
        _surface = GetComponent<Surface>();
        _collider = GetComponent<Collider>();
        _tableController = FindObjectOfType<TableController>();

        changeGridSizeX.onEndEdit.AddListener((value) =>
        {
            if (int.TryParse(value, out int x) && x != _surface.localTopRight.x && x > 0)
            {
                _surface.localTopRight.x = x;
                for (int i = 0; i < electrodeGUI.Length; i++) 
                {
                    if (_surface.electrodes[i].Position.x > x)
                    {
                        electrodeGUI[i].Position_x.text = x.ToString();
                        _surface.electrodes[i].Position.x = x;
                    }
                }
                _surface.FullRecalculetePotential(_heatMap);
            }
        });

        changeGridSizeY.onEndEdit.AddListener((value) =>
        {
            if (int.TryParse(value, out int y) && y != _surface.localTopRight.y && y > 0)
            {
                _surface.localTopRight.y = y;
                for (int i = 0; i < electrodeGUI.Length; i++) 
                {
                    if (_surface.electrodes[i].Position.y > y)
                    {
                        electrodeGUI[i].Position_y.text = y.ToString();
                        _surface.electrodes[i].Position.y = y;
                    }
                }
                _surface.FullRecalculetePotential(_heatMap);
            }
        });

        for (int i = 0; i < electrodeGUI.Length; i++)
        {
            _saveCoords[i] = _surface.electrodes[i].Position;
            int index = i;

            electrodeGUI[i].Position_x.onEndEdit.AddListener((value) =>
            {
                if (int.TryParse(value, out int x) && x <= _surface.localTopRight.x)
                {
                    _surface.electrodes[index].Position.x = x;
                    _surface.FullRecalculetePotential(_heatMap);
                }
            });

            electrodeGUI[i].Position_y.onEndEdit.AddListener((value) =>
            {
                if (int.TryParse(value, out int y) && y <= _surface.localTopRight.y)
                {
                    _surface.electrodes[index].Position.y = y;
                    _surface.FullRecalculetePotential(_heatMap);
                }
            });

            electrodeGUI[i].IsFlatElectrode.onValueChanged.AddListener((isOn) =>
            {
                _surface.electrodes[index].IsFlatElectrode = isOn;
                if (isOn)
                {
                    electrodeGUI[index].Position_y.gameObject.SetActive(_surface.electrodes[index].IsHorizontal);
                    electrodeGUI[index].Position_x.gameObject.SetActive(!_surface.electrodes[index].IsHorizontal);
                    if (_surface.electrodes[index].IsHorizontal)
                    {
                        _saveCoords[index] = _surface.electrodes[index].Position;
                        _surface.electrodes[index].Position.x = 10;
                    }
                    else
                    {
                        _saveCoords[index] = _surface.electrodes[index].Position;
                        _surface.electrodes[index].Position.y = 10;
                    }
                }
                else
                {
                    electrodeGUI[index].Position_y.gameObject.SetActive(true);
                    electrodeGUI[index].Position_x.gameObject.SetActive(true);
                    _surface.electrodes[index].Position.x = (int)_saveCoords[index].x;
                    _surface.electrodes[index].Position.y = (int)_saveCoords[index].y;
                }
                electrodeGUI[index].IsHorizontal.gameObject.SetActive(isOn);
                _surface.FullRecalculetePotential(_heatMap);
            });

            electrodeGUI[i].IsHorizontal.onValueChanged.AddListener((isOn) =>
            {
                _surface.electrodes[index].IsHorizontal = isOn;
                electrodeGUI[index].Position_y.gameObject.SetActive(_surface.electrodes[index].IsHorizontal);
                electrodeGUI[index].Position_x.gameObject.SetActive(!_surface.electrodes[index].IsHorizontal);
                if (isOn)
                {
                    _surface.electrodes[index].Position.y = (int)_saveCoords[index].y;
                    _saveCoords[index] = _surface.electrodes[index].Position;
                    _surface.electrodes[index].Position.x = 10;
                }
                else
                {
                    _surface.electrodes[index].Position.x = (int)_saveCoords[index].x;
                    _saveCoords[index] = _surface.electrodes[index].Position;
                    _surface.electrodes[index].Position.y = 10;
                }
                _surface.FullRecalculetePotential(_heatMap);
            });

            electrodeGUI[i].Position_x.text = _surface.electrodes[i].Position.x.ToString();
            electrodeGUI[i].Position_y.text = _surface.electrodes[i].Position.y.ToString();
            electrodeGUI[i].IsFlatElectrode.isOn = _surface.electrodes[i].IsFlatElectrode;
            electrodeGUI[i].IsHorizontal.gameObject.SetActive(_surface.electrodes[i].IsFlatElectrode);
            electrodeGUI[i].IsHorizontal.isOn = _surface.electrodes[i].IsHorizontal;
            
            changeGridSizeX.text = _surface.localTopRight.x.ToString();
            changeGridSizeY.text = _surface.localTopRight.y.ToString();
        }
    }

    public void HandleInteraction()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (_collider.Raycast(ray, out RaycastHit hit, float.MaxValue))
        {
            var (x, y) = (
                hit.textureCoord.x * _surface.localTopRight.x,
                hit.textureCoord.y * _surface.localTopRight.y
            );
            float potentialValue = _surface.potentialField.GetPotential(x, y);
            _lastHoverPosition = new Vector2(x, y);

            if (voltmeter)
                voltmeter.text = $"{potentialValue:0.00} В";

            if (Input.GetMouseButtonDown(0)) {
                _tableController.AddRow(x, y, potentialValue, hit.point);
            }
        }
    }

    private void Update()
    {
        HandleInteraction();
    }

    public void OnHeatMap(bool selected)
    {
        _heatMap = selected;
        _surface.textureRenderer.UpdateMainTexture(_heatMap);
    }

    private void OnGUI()
    {
        // float potentialValue = _surface.potentialField.GetPotential(_lastHoverPosition.x, _lastHoverPosition.y);
        // GUI.Label(new Rect(10, 10, 300, 20),
        //     $"Координата: {_lastHoverPosition}, Потенциал: {potentialValue:F2} В");
    }
}
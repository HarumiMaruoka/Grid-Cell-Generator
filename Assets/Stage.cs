// 日本語対応
using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

[CreateAssetMenu()]
public class Stage : ScriptableObject
{
    // ToDo: 一応クローンして使うのでClone()作る。
    [SerializeField]
    [Range(1, 20)]
    private int _width = 1;
    [SerializeField]
    [Range(1, 20)]
    private int _height = 1;
    [SerializeField]
    private Cell[] _cells;

    [SerializeField]
    [Range(0, 20)]
    private int _editXPosition = 0;
    [SerializeField]
    [Range(0, 20)]
    private int _editYPosition = 0;

    public int Width => _width;
    public int Height => _height;
    public Cell[] Cells => _cells;

    public Action<Vector2Int, Vector2Int> OnSelectionChanged;

    public void Start() // 初めて起動する際に一度だけ呼んでください。
    {
        foreach (var cell in _cells) cell.Start();
    }

    public void Update() // 毎フレーム実行してください。
    {
        foreach (var cell in _cells) cell.Update();
    }

    private bool IsInIndex(Cell[] cells, int yLength, int xLength, int y, int x)
    {
        return cells != null &&
            y >= 0 && y < yLength &&
            x >= 0 && x < xLength;
    }

    private int GetIndex(int width, int y, int x)
    {
        return y * width + x;
    }

    public bool TryGetCell(Cell[] array, int height, int width, int y, int x, out Cell cell)
    {
        if (IsInIndex(array, height, width, y, x))
        {
            var index = GetIndex(width, y, x);
            if (index >= 0 && index < array.Length)
            {
                cell = array[index];
                return true;
            }
            else
            {
                Debug.LogWarning($"IsInIndexが正しく働いてません。index: {index}, height: {height}, width{width}, y: {y}, x: {x}");
                cell = null;
                return false;
            }
        }
        else
        {
            cell = null;
            return false;
        }
    }

    public void ResizeCells(int oldHeight, int oldWidth)
    {
        var old = _cells;
        _cells = new Cell[_width * _height];
        for (int yIndex = 0; yIndex < _height; yIndex++)
        {
            for (int xIndex = 0; xIndex < _width; xIndex++)
            {
                if (old != null &&
                    IsInIndex(old, oldHeight, oldWidth, yIndex, xIndex) &&
                    IsInIndex(_cells, _height, _width, yIndex, xIndex))
                {
                    _cells[GetIndex(_width, yIndex, xIndex)] = old[GetIndex(oldWidth, yIndex, xIndex)];
                    // オブジェクトがコピーされた場合、古いオブジェクトを破棄
                    old[GetIndex(oldWidth, yIndex, xIndex)] = null;
                }

                if (_cells[GetIndex(_width, yIndex, xIndex)] == null)
                {
                    _cells[GetIndex(_width, yIndex, xIndex)] = CreateInstance<Cell>();
                    AssetDatabase.AddObjectToAsset(_cells[GetIndex(_width, yIndex, xIndex)], this);
                    _cells[GetIndex(_width, yIndex, xIndex)].name = $"Cell: Y: {yIndex}, X: {xIndex}";
                }
            }
        }
        // 不要なオブジェクトを破棄
        for (int i = 0; i < old.Length; i++)
        {
            if (old[i] != null)
            {
                ScriptableObject.DestroyImmediate(old[i], true);
            }
        }
        AssetDatabase.SaveAssets();
    }

    public Stage Clone()
    {
        var clone = CreateInstance<Stage>();
        clone._cells = new Cell[_width * _height];

        for (int i = 0; i < _cells.Length; i++)
        {
            clone._cells[i] = _cells[i].Clone();
        }

        return clone;
    }
}

[CustomEditor(typeof(Stage))]
public class StageInspectorViewer : Editor
{
    private int _cachedWidth;
    private int _cachedHeight;
    private int _cachedXPos;
    private int _cachedYPos;
    private Stage _stage;
    private Editor _selectCellEditor;

    private void OnEnable()
    {
        _stage = target as Stage;
        if (_stage == null)
        {
            Debug.Log("なんかミスってる。");
            return;
        }
        //_cachedWidth = serializedObject.FindProperty("_width").intValue;
        //_cachedHeight = serializedObject.FindProperty("_height").intValue;
        //_cachedXPos = serializedObject.FindProperty("_editXPosition").intValue;
        //_cachedYPos = serializedObject.FindProperty("_editYPosition").intValue;
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        var boldtext = new GUIStyle(GUI.skin.label);
        boldtext.fontStyle = FontStyle.Bold;

        EditorGUILayout.LabelField("Stage Inspector", boldtext);

        var widthProperty = serializedObject.FindProperty("_width");
        var heightProperty = serializedObject.FindProperty("_height");

        var xPosProperty = serializedObject.FindProperty("_editXPosition");
        var yPosProperty = serializedObject.FindProperty("_editYPosition");

        EditorGUILayout.PropertyField(widthProperty);
        EditorGUILayout.PropertyField(heightProperty);

        EditorGUILayout.BeginHorizontal();
        bool isResize = GUILayout.Button("Resize");
        bool isCancel = GUILayout.Button("Cancel");
        EditorGUILayout.EndHorizontal();

        Separator();

        EditorGUILayout.LabelField("Cell Inspector", boldtext);
        EditorGUILayout.PropertyField(xPosProperty);
        EditorGUILayout.PropertyField(yPosProperty);

        // 戒め。バグったときはいろんなものを可視化しよう。
        // EditorGUILayout.PropertyField(serializedObject.FindProperty("_cells"));

        GameObject.DestroyImmediate(_selectCellEditor);
        if (_stage.TryGetCell(_stage.Cells, _cachedHeight, _cachedWidth, _cachedYPos, _cachedXPos, out Cell cell))
        {
            _selectCellEditor = CreateEditor(cell);
            _selectCellEditor.OnInspectorGUI();
        }
        else
        {
            EditorGUILayout.LabelField("HoverItem is null", boldtext);
        }

        if (isResize) Resize(widthProperty, heightProperty);
        if (isCancel) Cancel(widthProperty, heightProperty);

        SelectionChange(xPosProperty, yPosProperty);

        serializedObject.ApplyModifiedProperties();
    }

    private void Resize(SerializedProperty width, SerializedProperty height)
    {
        if (_cachedWidth != width.intValue)
        {
            _stage.ResizeCells(_cachedHeight, _cachedWidth);
            _cachedWidth = width.intValue;
        }
        if (_cachedHeight != height.intValue)
        {
            _stage.ResizeCells(_cachedHeight, _cachedWidth);
            _cachedHeight = height.intValue;
        }
    }

    private void Cancel(SerializedProperty width, SerializedProperty height)
    {
        width.intValue = _cachedWidth;
        height.intValue = _cachedHeight;
    }

    private void SelectionChange(SerializedProperty xPos, SerializedProperty yPos)
    {
        if (_cachedXPos != xPos.intValue)
        {
            var oldPos = new Vector2Int(_cachedXPos, _cachedYPos);
            var newPos = new Vector2Int(xPos.intValue, _cachedYPos);
            _stage.OnSelectionChanged?.Invoke(oldPos, newPos);
            if (_stage.TryGetCell(_stage.Cells, _cachedHeight, _cachedWidth, oldPos.y, oldPos.x, out Cell oldCell))
            {
                oldCell?.Unhover();
            }
            if (_stage.TryGetCell(_stage.Cells, _cachedHeight, _cachedWidth, newPos.y, newPos.x, out Cell newCell))
            {
                newCell?.Hover();
            }
            _cachedXPos = xPos.intValue;
        }
        if (_cachedYPos != yPos.intValue)
        {
            var oldPos = new Vector2Int(_cachedXPos, _cachedYPos);
            var newPos = new Vector2Int(_cachedXPos, yPos.intValue);
            _stage.OnSelectionChanged?.Invoke(oldPos, newPos);
            if (_stage.TryGetCell(_stage.Cells, _cachedHeight, _cachedWidth, oldPos.y, oldPos.x, out Cell oldCell))
            {
                oldCell?.Unhover();
            }
            if (_stage.TryGetCell(_stage.Cells, _cachedHeight, _cachedWidth, newPos.y, newPos.x, out Cell newCell))
            {
                newCell?.Hover();
            }
            _cachedYPos = yPos.intValue;
        }
    }

    public void Separator() // 仕切り線を表示する。
    {
        EditorGUILayout.BeginHorizontal();
        GUILayout.Box("", GUILayout.ExpandWidth(true), GUILayout.Height(3));
        EditorGUILayout.EndHorizontal();
    }
}
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
    private int _width = 0;
    [SerializeField]
    [Range(1, 20)]
    private int _height = 0;
    [SerializeField]
    private Cell[] _cells;

    [SerializeField]
    [Range(0, 20)]
    private int _x = 0;
    [SerializeField]
    [Range(0, 20)]
    private int _y = 0;

    public int Width => _width;
    public int Height => _height;
    public int X => _x;
    public int Y => _y;
    public Cell[] Cells => _cells;

    public Action<Vector2Int, Vector2Int> OnSelectionChanged;

    private bool IsInIndex(ref Cell[] cells, int yLength, int xLength, int y, int x)
    {
        return cells != null &&
            y >= 0 && y < yLength &&
            x >= 0 && x < xLength;
    }

    private int GetIndex(int width, int y, int x)
    {
        // 2次元インデックスを1次元に変換する式
        return y * width + x;
    }

    public Cell GetCell(int y, int x)
    {
        if (IsInIndex(ref _cells, _height, _width, y, x))
        {
            return _cells[GetIndex(_width, y, x)];
        }
        else
        {
            return null;
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
                    IsInIndex(ref old, oldHeight, oldWidth, yIndex, xIndex) &&
                    IsInIndex(ref _cells, _height, _width, yIndex, xIndex))
                {
                    _cells[GetIndex(_width, yIndex, xIndex)] = old[GetIndex(oldWidth, yIndex, xIndex)];
                }

                if (_cells[GetIndex(_width, yIndex, xIndex)] == null)
                {
                    _cells[GetIndex(_width, yIndex, xIndex)] = CreateInstance<Cell>();
                    AssetDatabase.AddObjectToAsset(_cells[GetIndex(_width, yIndex, xIndex)], this);
                }
            }
        }
        AssetDatabase.SaveAssets();
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
        _cachedWidth = serializedObject.FindProperty("_width").intValue;
        _cachedHeight = serializedObject.FindProperty("_height").intValue;
        _cachedXPos = serializedObject.FindProperty("_x").intValue;
        _cachedYPos = serializedObject.FindProperty("_y").intValue;
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        var widthProperty = serializedObject.FindProperty("_width");
        var heightProperty = serializedObject.FindProperty("_height");

        var xPosProperty = serializedObject.FindProperty("_x");
        var yPosProperty = serializedObject.FindProperty("_y");

        EditorGUILayout.PropertyField(widthProperty);
        EditorGUILayout.PropertyField(heightProperty);

        EditorGUILayout.PropertyField(xPosProperty);
        EditorGUILayout.PropertyField(yPosProperty);

        // 戒め。バグったときはいろんなものを可視化しよう。
        // EditorGUILayout.PropertyField(serializedObject.FindProperty("_cells"));

        GameObject.DestroyImmediate(_selectCellEditor);
        if (_stage.GetCell(yPosProperty.intValue, xPosProperty.intValue) == null)
        {
            EditorGUILayout.LabelField("HoverItem is null");
        }
        else
        {
            _selectCellEditor = CreateEditor(_stage.GetCell(yPosProperty.intValue, xPosProperty.intValue));
            _selectCellEditor.OnInspectorGUI();
        }
        serializedObject.ApplyModifiedProperties();

        if (_cachedWidth != widthProperty.intValue)
        {
            _stage.ResizeCells(_cachedHeight, _cachedWidth);
            _cachedWidth = widthProperty.intValue;
        }
        if (_cachedHeight != heightProperty.intValue)
        {
            _stage.ResizeCells(_cachedHeight, _cachedWidth);
            _cachedHeight = heightProperty.intValue;
        }
        if (_cachedXPos != xPosProperty.intValue)
        {
            var oldPos = new Vector2Int(_cachedXPos, _cachedYPos);
            var newPos = new Vector2Int(xPosProperty.intValue, _cachedYPos);
            _stage.OnSelectionChanged?.Invoke(oldPos, newPos);
            _stage.GetCell(oldPos.y, oldPos.x)?.Unhover();
            _stage.GetCell(newPos.y, newPos.x)?.Hover();
            _cachedXPos = xPosProperty.intValue;
        }
        if (_cachedYPos != yPosProperty.intValue)
        {
            var oldPos = new Vector2Int(_cachedXPos, _cachedYPos);
            var newPos = new Vector2Int(_cachedXPos, yPosProperty.intValue);
            _stage.OnSelectionChanged?.Invoke(oldPos, newPos);
            _stage.GetCell(oldPos.y, oldPos.x)?.Unhover();
            _stage.GetCell(newPos.y, newPos.x)?.Hover();
            _cachedYPos = yPosProperty.intValue;
        }
    }
}
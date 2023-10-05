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
    private int _editXPosition = 0;
    [SerializeField]
    [Range(0, 20)]
    private int _editYPosition = 0;

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
        _cachedXPos = serializedObject.FindProperty("_editXPosition").intValue;
        _cachedYPos = serializedObject.FindProperty("_editYPosition").intValue;
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
        if (_stage.GetCell(yPosProperty.intValue, xPosProperty.intValue) == null)
        {
            EditorGUILayout.LabelField("HoverItem is null");
        }
        else
        {
            _selectCellEditor = CreateEditor(_stage.GetCell(yPosProperty.intValue, xPosProperty.intValue));
            _selectCellEditor.OnInspectorGUI();
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
            _stage.GetCell(oldPos.y, oldPos.x)?.Unhover();
            _stage.GetCell(newPos.y, newPos.x)?.Hover();
            _cachedXPos = xPos.intValue;
        }
        if (_cachedYPos != yPos.intValue)
        {
            var oldPos = new Vector2Int(_cachedXPos, _cachedYPos);
            var newPos = new Vector2Int(_cachedXPos, yPos.intValue);
            _stage.OnSelectionChanged?.Invoke(oldPos, newPos);
            _stage.GetCell(oldPos.y, oldPos.x)?.Unhover();
            _stage.GetCell(newPos.y, newPos.x)?.Hover();
            _cachedYPos = yPos.intValue;
        }
    }

    public static void Separator() // 仕切り線表示
    {
        EditorGUILayout.BeginHorizontal();
        GUILayout.Box("", GUILayout.ExpandWidth(true), GUILayout.Height(3));
        EditorGUILayout.EndHorizontal();
    }
}
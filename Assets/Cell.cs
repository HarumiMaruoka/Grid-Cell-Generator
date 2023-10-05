// 日本語対応
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class Cell : ScriptableObject, ISelectable
{
    [SerializeField]
    private CellStatus _cellStatus = CellStatus.None;
    [SerializeField]
    private List<CellComponent> _cellCompoents; // ToDo: セルコンポーネントの取り外しが容易になるようにインスペクタをカスタマイズする。

    private bool _isHovered;
    public bool IsHovered => _isHovered;

    public List<CellComponent> CellComponents { get => _cellCompoents; set => _cellCompoents = value; }

    public event Action OnHovered;
    public event Action OnUnhovered;

    public void Start() // 初めて起動する際に一度だけ呼んでください。
    {
        foreach (var attachment in _cellCompoents) attachment.Start();
    }
    public void Update() // 毎フレーム実行してください。
    {
        foreach (var attachment in _cellCompoents) attachment.Update();
    }

    public void Hover()
    {
        _isHovered = true;
        OnHovered?.Invoke();
    }

    public void Unhover()
    {
        _isHovered = false;
        OnUnhovered?.Invoke();
    }

    public void Dispose()
    {
        OnHovered = null;
        OnUnhovered = null;
        Unhover();
    }

    public Cell Clone()
    {
        return Instantiate(this);
    }
}

[Serializable]
[Flags]
public enum CellStatus : int
{
    None = 0,
    Everything = -1,
    Movable = 1, // 通行可能かどうか
    UnitPlaceable = 2, // ユニットが配置可能かどうか
}

[CustomEditor(typeof(Cell))]
public class CellInspectorView : Editor
{
    private Cell _cell;
    private Editor _attachmentEditor; // このコメントは後で消す。描画の度に破棄しているけどもしかしたら上手く動かないかも。動かなかったらListとかHashSetでもつと上手く動くかも。

    private void OnEnable()
    {
        _cell = target as Cell;
        if (_cell == null)
        {
            Debug.Log("なんかミスってる。");
            return;
        }

        if (_cell.CellComponents == null) _cell.CellComponents = new List<CellComponent>();
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        var select = AttachmentSelector();

        if (select != CellAttachmentType.Select)
        {
            AddCellAttachment(select);
        }

        if (GUILayout.Button("Clear"))
        {
            foreach (var item in _cell.CellComponents)
                DestroyImmediate(item, true);
            _cell.CellComponents.Clear();
        }

        CellAttachmentInspectorView();

        serializedObject.ApplyModifiedProperties();
    }

    private CellAttachmentType AttachmentSelector()
    {
        return (CellAttachmentType)EditorGUILayout.EnumPopup("Select Add Component", CellAttachmentType.Select);
    }

    private void AddCellAttachment(CellAttachmentType type)
    {
        var attachment = type.ToCellAttachment();
        attachment.name = $"{type.ToString()}, Parent: {_cell.name}";
        _cell.CellComponents.Add(attachment);
        AssetDatabase.AddObjectToAsset(attachment, _cell);
        AssetDatabase.SaveAssets();
    }

    private void RemoveCellAttachment(CellAttachmentType type)
    {
        CellComponent removeObj = null;
        foreach (var item in _cell.CellComponents)
        {
            if (item.ToCellAttachmentType() == type)
            {
                removeObj = item;
                break;
            }
        }
        if (removeObj != null)
        {
            _cell.CellComponents.Remove(removeObj);
        }
        else
        {
            Debug.LogWarning("指定された型のオブジェクトが見つかりませんでした。");
        }
        AssetDatabase.SaveAssets();
    }

    private void CellAttachmentInspectorView()
    {
        foreach (var component in _cell.CellComponents)
        {
            DestroyImmediate(_attachmentEditor);
            _attachmentEditor = CreateEditor(component);
            _attachmentEditor.OnInspectorGUI();
        }
    }

    public void Separator() // 仕切り線を表示する。
    {
        EditorGUILayout.BeginHorizontal();
        GUILayout.Box("", GUILayout.ExpandWidth(true), GUILayout.Height(3));
        EditorGUILayout.EndHorizontal();
    }

}

public enum CellAttachmentType
{
    Select, // 選択中を表現する。
    EnemySpawner,
}

public static class Converter
{
    public static CellComponent ToCellAttachment(this CellAttachmentType type)
    {
        switch (type)
        {
            case CellAttachmentType.EnemySpawner: return ScriptableObject.CreateInstance<EnemySpawner>();
            default: throw new ArgumentException(nameof(type));
        }
    }

    public static CellAttachmentType ToCellAttachmentType(this CellComponent obj)
    {
        if (obj is EnemySpawner)
        {
            return CellAttachmentType.EnemySpawner;
        }
        else
        {
            throw new ArgumentException(nameof(obj));
        }
    }
}
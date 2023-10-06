// 日本語対応
using GridCell;
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace GridCell
{
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



    [CustomEditor(typeof(Cell))]
    public class CellInspectorView : Editor
    {
        private Cell _cell;
        private Editor _attachmentEditor;

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

            var cellStatusProperty = serializedObject.FindProperty("_cellStatus");
            EditorGUILayout.PropertyField(cellStatusProperty);

            var select = AttachmentSelector();

            if (select != CellComponentType.Select)
            {
                AddCellAttachment(select);
            }

            if (GUILayout.Button("Clear")) // コンポーネントまとめて削除。（テスト用。）
            {
                foreach (var item in _cell.CellComponents)
                    DestroyImmediate(item, true);
                _cell.CellComponents.Clear();
            }

            CellAttachmentInspectorView();

            serializedObject.ApplyModifiedProperties();
        }

        private CellComponentType AttachmentSelector()
        {
            return (CellComponentType)EditorGUILayout.EnumPopup("Select Add Component", CellComponentType.Select);
        }

        private void AddCellAttachment(CellComponentType type)
        {
            var attachment = type.ToCellComponent();
            attachment.name = $"{type.ToString()}, Parent: {_cell.name}";
            _cell.CellComponents.Add(attachment);
            AssetDatabase.AddObjectToAsset(attachment, _cell);
            AssetDatabase.SaveAssets();
        }

        private void RemoveCellAttachment(CellComponentType type)
        {
            CellComponent removeObj = null;
            foreach (var item in _cell.CellComponents)
            {
                if (item.ToCellComponentType() == type)
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
                Separator();
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

    public enum CellComponentType
    {
        Select, // 選択中を表現する。
        EnemySpawner,
    }
}
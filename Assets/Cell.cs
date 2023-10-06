// 日本語対応
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
        private int _xIndex;
        private int _yIndex;
        private Vector3 _position;

        public List<CellComponent> CellComponents => _cellCompoents;
        public bool IsHovered => _isHovered;
        public int XIndex => _xIndex;
        public int YIndex => _yIndex;
        public Vector3 Position => _position;

        public event Action OnHovered;
        public event Action OnUnhovered;

        public void Initialize(int xIndex, int yIndex, Vector3 position)
        {
            _xIndex = xIndex; _yIndex = yIndex; _position = position;
        }

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
            serializedObject.ApplyModifiedProperties();

            _cell = target as Cell;
            if (_cell == null)
            {
                Debug.Log("なんかミスってる。");
                return;
            }
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            var cellStatusProperty = serializedObject.FindProperty("_cellStatus");
            EditorGUILayout.PropertyField(cellStatusProperty);

            var select = AttachmentSelector();

            if (select != CellComponentType.Select)
            {
                AddCellComponent(select);
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

        private void AddCellComponent(CellComponentType type)
        {
            var attachment = type.ToCellComponent();
            attachment.name = $"{type.ToString()}, Parent: {_cell.name}";
            _cell.CellComponents.Add(attachment);
            AssetDatabase.AddObjectToAsset(attachment, _cell);
            AssetDatabase.SaveAssets();
        }

        private void RemoveCellComponent(CellComponentType type)
        {
            CellComponent removeObj = null;
            foreach (var component in _cell.CellComponents)
            {
                if (component.ToCellComponentType() == type)
                {
                    removeObj = component;
                    break;
                }
            }
            if (removeObj != null)
            {
                _cell.CellComponents.Remove(removeObj);
                DestroyImmediate(removeObj, true);
            }
            else
            {
                Debug.LogWarning("指定された型のオブジェクトが見つかりませんでした。");
            }
            AssetDatabase.SaveAssets();
        }

        private void RemoveCellComponent(CellComponent obj)
        {
            if (obj != null)
            {
                if (_cell.CellComponents.Remove(obj))
                {
                    DestroyImmediate(obj, true);
                }
            }
            else
            {
                Debug.LogWarning("指定された型のオブジェクトが見つかりませんでした。");
            }
            AssetDatabase.SaveAssets();
        }

        private void CellAttachmentInspectorView()
        {
            if (_cell.CellComponents == null) return;
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
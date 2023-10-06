// 日本語対応
using UnityEngine;
using UnityEditor;

namespace GridCell
{
    public abstract class CellComponent : ScriptableObject
    {
        public void Initialize(Cell owner)
        {
            _owner = owner;
        }

        private Cell _owner;

        public Cell Owner => _owner;

        public virtual void Start() { }
        public virtual void Update() { }
    }
}
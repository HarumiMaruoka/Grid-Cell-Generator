// 日本語対応
using UnityEngine;

namespace GridCell
{
    public abstract class CellComponent : ScriptableObject
    {
        public virtual void Start() { }
        public virtual void Update() { }
    }
}
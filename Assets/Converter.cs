// 日本語対応

using System;
using UnityEngine;

namespace GridCell
{
    public static class Converter
    {
        public static CellComponent ToCellComponent(this CellComponentType type)
        {
            switch (type)
            {
                case CellComponentType.EnemySpawner: return ScriptableObject.CreateInstance<EnemySpawner>();
                default: throw new ArgumentException(nameof(type));
            }
        }

        public static CellComponentType ToCellComponentType(this CellComponent obj)
        {
            if (obj is EnemySpawner)
            {
                return CellComponentType.EnemySpawner;
            }
            else
            {
                throw new ArgumentException(nameof(obj));
            }
        }
    }
}
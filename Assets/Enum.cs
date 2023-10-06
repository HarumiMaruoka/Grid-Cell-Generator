// 日本語対応
using System;

namespace GridCell
{
    [Serializable]
    [Flags]
    public enum CellStatus : int
    {
        None = 0,
        Everything = -1,
        Movable = 1, // 通行可能かどうか
        UnitPlaceable = 2, // ユニットが配置可能かどうか
    }
}
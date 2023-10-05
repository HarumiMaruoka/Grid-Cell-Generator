// 日本語対応
using System;
using UnityEngine;

public class Cell : ScriptableObject, ISelectable
{
    // ここにセルのやつを書く。
    [SerializeField]
    private CellStatus _cellStatus = CellStatus.None;

    private bool _isHovered;
    public bool IsHovered => _isHovered;

    public event Action OnHovered;
    public event Action OnUnhovered;

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
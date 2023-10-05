// 日本語対応
using System;

public interface ISelectable
{
    bool IsHovered { get; }

    event Action OnHovered;
    event Action OnUnhovered;

    void Hover();
    void Unhover();
}
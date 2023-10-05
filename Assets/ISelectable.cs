// 日本語対応
using System;

public interface ISelectable
{
    event Action OnHovered;
    event Action OnUnhovered;

    void Hover();
    void Unhover();
}
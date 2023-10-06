// 日本語対応
using System;

namespace GridCell
{
        public interface ISelectable
        {
            bool IsHovered { get; }

            event Action OnHovered;
            event Action OnUnhovered;

            void Hover();
            void Unhover();
        }
}
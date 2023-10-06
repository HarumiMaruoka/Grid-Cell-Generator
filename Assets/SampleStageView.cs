// 日本語対応
using UnityEngine;
using UnityEngine.UI;

namespace GridCell
{
    public class SampleStageView : MonoBehaviour
    {
        [SerializeField]
        private Stage _stageData;
        [SerializeField]
        private GridLayoutGroup _cellViewParent;
        [SerializeField]
        private SampleCellView _cellViewPrefab;

        private void Start()
        {
            _cellViewParent.constraint = GridLayoutGroup.Constraint.FixedRowCount;
            _cellViewParent.constraintCount = _stageData.Height;

            foreach (var item in _stageData.Cells)
            {
                var cellView = Instantiate(_cellViewPrefab, _cellViewParent.transform);
                item.OnHovered += cellView.Hover;
                item.OnUnhovered += cellView.Unhover;
                if (item.IsHovered) { cellView.Hover(); }
            }
        }
        private void OnDestroy()
        {
            foreach (var item in _stageData.Cells)
            {
                item.Dispose();
            }
        }
    }
}
// 日本語対応
using UnityEngine;

public class StageView : MonoBehaviour
{
    [SerializeField]
    private Stage _stageData;
    [SerializeField]
    private Transform _cellViewParent;
    [SerializeField]
    private CellView _cellViewPrefab;

    private void Start()
    {
        foreach (var item in _stageData.Cells)
        {
            var cellView = Instantiate(_cellViewPrefab, _cellViewParent);
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

    // 選択オブジェクトの変更が入ったとき
    private void UpdateSelection(Vector2Int oldPos, Vector2Int newPos)
    {
        Debug.Log("変更されたよ");
        _stageData.GetCell(oldPos.y, oldPos.x);
        _stageData.GetCell(newPos.y, newPos.x);
    }
}
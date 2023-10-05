// 日本語対応
using UnityEngine;
using UnityEngine.UI;

public class CellView : MonoBehaviour
{
    [SerializeField]
    private Image _image;
    [SerializeField]
    private Color _nomalColor = Color.white;
    [SerializeField]
    private Color _hoveredColor = Color.red;

    public void Hover()
    {
        _image.color = _hoveredColor;
    }
    public void Unhover()
    {
        _image.color = _nomalColor;
    }
}
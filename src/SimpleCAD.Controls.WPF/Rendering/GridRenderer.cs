using System.Windows;
using System.Windows.Media;

namespace SimpleCAD.Controls.WPF.Rendering;

/// <summary>
/// グリッド描画レンダラー
/// </summary>
public class GridRenderer
{
    /// <summary>
    /// グリッドを描画
    /// </summary>
    public void DrawGrid(DrawingContext dc, double width, double height, double gridSpacing,
        double viewCenterX, double viewCenterY, double zoomLevel)
    {
        if (gridSpacing <= 0 || zoomLevel <= 0)
            return;

        var scaledSpacing = gridSpacing * zoomLevel;

        // グリッド間隔が小さすぎる場合はスキップ
        if (scaledSpacing < 2)
            return;

        // マイナーグリッド（細い線）
        var minorPen = new Pen(new SolidColorBrush(Color.FromArgb(128, 240, 240, 240)), 0.5);
        minorPen.Freeze();

        // メジャーグリッド（太い線、10単位ごと）
        var majorPen = new Pen(new SolidColorBrush(Color.FromArgb(255, 200, 200, 200)), 1.0);
        majorPen.Freeze();

        // 座標軸
        var xAxisPen = new Pen(new SolidColorBrush(Color.FromArgb(255, 255, 0, 0)), 1.5);
        xAxisPen.Freeze();
        var yAxisPen = new Pen(new SolidColorBrush(Color.FromArgb(255, 0, 0, 255)), 1.5);
        yAxisPen.Freeze();

        // ビュー範囲を計算
        var left = -viewCenterX * zoomLevel + width / 2;
        var top = -viewCenterY * zoomLevel + height / 2;
        var right = left + width;
        var bottom = top + height;

        // グリッド開始位置
        var startX = Math.Floor((-viewCenterX - width / (2 * zoomLevel)) / gridSpacing) * gridSpacing;
        var startY = Math.Floor((-viewCenterY - height / (2 * zoomLevel)) / gridSpacing) * gridSpacing;

        // 縦線を描画
        for (double x = startX; x * zoomLevel + width / 2 < width + viewCenterX * zoomLevel; x += gridSpacing)
        {
            var screenX = (x + viewCenterX) * zoomLevel + width / 2;

            // 座標軸
            if (Math.Abs(x) < gridSpacing / 10)
            {
                dc.DrawLine(yAxisPen, new Point(screenX, 0), new Point(screenX, height));
            }
            // メジャーグリッド
            else if (Math.Abs(x % (gridSpacing * 10)) < gridSpacing / 10)
            {
                dc.DrawLine(majorPen, new Point(screenX, 0), new Point(screenX, height));
            }
            // マイナーグリッド
            else if (scaledSpacing > 5)
            {
                dc.DrawLine(minorPen, new Point(screenX, 0), new Point(screenX, height));
            }
        }

        // 横線を描画
        for (double y = startY; y * zoomLevel + height / 2 < height + viewCenterY * zoomLevel; y += gridSpacing)
        {
            var screenY = (y + viewCenterY) * zoomLevel + height / 2;

            // 座標軸
            if (Math.Abs(y) < gridSpacing / 10)
            {
                dc.DrawLine(xAxisPen, new Point(0, screenY), new Point(width, screenY));
            }
            // メジャーグリッド
            else if (Math.Abs(y % (gridSpacing * 10)) < gridSpacing / 10)
            {
                dc.DrawLine(majorPen, new Point(0, screenY), new Point(width, screenY));
            }
            // マイナーグリッド
            else if (scaledSpacing > 5)
            {
                dc.DrawLine(minorPen, new Point(0, screenY), new Point(width, screenY));
            }
        }
    }
}

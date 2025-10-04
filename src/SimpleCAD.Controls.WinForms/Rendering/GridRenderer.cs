using System.Drawing;
using System.Drawing.Drawing2D;

namespace SimpleCAD.Controls.WinForms.Rendering;

/// <summary>
/// グリッド描画レンダラー（WinForms版）
/// </summary>
public class GridRenderer
{
    /// <summary>
    /// グリッドを描画
    /// </summary>
    public void DrawGrid(Graphics g, float width, float height, double gridSpacing,
        double viewCenterX, double viewCenterY, double zoomLevel)
    {
        if (gridSpacing <= 0 || zoomLevel <= 0)
            return;

        var scaledSpacing = (float)(gridSpacing * zoomLevel);

        // グリッド間隔が小さすぎる場合はスキップ
        if (scaledSpacing < 2)
            return;

        // マイナーグリッド（細い線）
        using var minorPen = new Pen(Color.FromArgb(128, 240, 240, 240), 0.5f);

        // メジャーグリッド（太い線、10単位ごと）
        using var majorPen = new Pen(Color.FromArgb(255, 200, 200, 200), 1.0f);

        // 座標軸
        using var xAxisPen = new Pen(Color.FromArgb(255, 255, 0, 0), 1.5f); // Red
        using var yAxisPen = new Pen(Color.FromArgb(255, 0, 0, 255), 1.5f); // Blue

        // グリッド開始位置
        var startX = Math.Floor((-viewCenterX - width / (2 * zoomLevel)) / gridSpacing) * gridSpacing;
        var startY = Math.Floor((-viewCenterY - height / (2 * zoomLevel)) / gridSpacing) * gridSpacing;

        // 縦線を描画
        for (double x = startX; x * zoomLevel + width / 2 < width + viewCenterX * zoomLevel; x += gridSpacing)
        {
            var screenX = (float)((x + viewCenterX) * zoomLevel + width / 2);

            // 座標軸
            if (Math.Abs(x) < gridSpacing / 10)
            {
                g.DrawLine(yAxisPen, screenX, 0, screenX, height);
            }
            // メジャーグリッド
            else if (Math.Abs(x % (gridSpacing * 10)) < gridSpacing / 10)
            {
                g.DrawLine(majorPen, screenX, 0, screenX, height);
            }
            // マイナーグリッド
            else if (scaledSpacing > 5)
            {
                g.DrawLine(minorPen, screenX, 0, screenX, height);
            }
        }

        // 横線を描画
        for (double y = startY; y * zoomLevel + height / 2 < height + viewCenterY * zoomLevel; y += gridSpacing)
        {
            var screenY = (float)((y + viewCenterY) * zoomLevel + height / 2);

            // 座標軸
            if (Math.Abs(y) < gridSpacing / 10)
            {
                g.DrawLine(xAxisPen, 0, screenY, width, screenY);
            }
            // メジャーグリッド
            else if (Math.Abs(y % (gridSpacing * 10)) < gridSpacing / 10)
            {
                g.DrawLine(majorPen, 0, screenY, width, screenY);
            }
            // マイナーグリッド
            else if (scaledSpacing > 5)
            {
                g.DrawLine(minorPen, 0, screenY, width, screenY);
            }
        }
    }
}

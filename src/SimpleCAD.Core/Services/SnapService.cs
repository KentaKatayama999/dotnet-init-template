using System.Drawing;

namespace SimpleCAD.Core.Services;

/// <summary>
/// グリッドスナップサービス
/// </summary>
public class SnapService
{
    /// <summary>
    /// グリッド間隔
    /// </summary>
    public double GridSpacing { get; set; } = 10.0;

    /// <summary>
    /// スナップが有効かどうか
    /// </summary>
    public bool SnapEnabled { get; set; } = true;

    /// <summary>
    /// 点をグリッドにスナップ
    /// </summary>
    public PointF SnapToGrid(PointF point)
    {
        if (!SnapEnabled || GridSpacing <= 0)
            return point;

        var snappedX = (float)(Math.Round(point.X / GridSpacing) * GridSpacing);
        var snappedY = (float)(Math.Round(point.Y / GridSpacing) * GridSpacing);

        return new PointF(snappedX, snappedY);
    }

    /// <summary>
    /// 点をグリッドにスナップ（double版）
    /// </summary>
    public (double X, double Y) SnapToGrid(double x, double y)
    {
        if (!SnapEnabled || GridSpacing <= 0)
            return (x, y);

        var snappedX = Math.Round(x / GridSpacing) * GridSpacing;
        var snappedY = Math.Round(y / GridSpacing) * GridSpacing;

        return (snappedX, snappedY);
    }

    /// <summary>
    /// 最も近いグリッド点を取得
    /// </summary>
    public PointF GetNearestGridPoint(PointF point)
    {
        if (GridSpacing <= 0)
            return point;

        var snappedX = (float)(Math.Round(point.X / GridSpacing) * GridSpacing);
        var snappedY = (float)(Math.Round(point.Y / GridSpacing) * GridSpacing);

        return new PointF(snappedX, snappedY);
    }

    /// <summary>
    /// 点がグリッド点に近いかどうかを判定
    /// </summary>
    public bool IsNearGridPoint(PointF point, float threshold = 5.0f)
    {
        var nearest = GetNearestGridPoint(point);
        var dx = point.X - nearest.X;
        var dy = point.Y - nearest.Y;
        var distance = (float)Math.Sqrt(dx * dx + dy * dy);

        return distance <= threshold;
    }
}

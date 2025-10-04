using System.Drawing;
using SimpleCAD.Core.Rendering;

namespace SimpleCAD.Core.Entities;

/// <summary>
/// 角度寸法エンティティ
/// </summary>
public class AngularDimensionEntity : DimensionEntity
{
    /// <summary>
    /// 角度の頂点
    /// </summary>
    public PointF CenterPoint { get; set; }

    /// <summary>
    /// 第1点（角度を定義）
    /// </summary>
    public PointF Point1 { get; set; }

    /// <summary>
    /// 第2点（角度を定義）
    /// </summary>
    public PointF Point2 { get; set; }

    /// <summary>
    /// デフォルトの円弧半径
    /// </summary>
    public float DefaultArcRadius { get; set; } = 30f;

    /// <summary>
    /// 角度（度）
    /// </summary>
    public double AngleDegrees
    {
        get
        {
            double angle1 = CalculateAngle(CenterPoint, Point1);
            double angle2 = CalculateAngle(CenterPoint, Point2);
            double diff = angle2 - angle1;

            // -180～180の範囲に正規化
            while (diff > Math.PI) diff -= 2 * Math.PI;
            while (diff < -Math.PI) diff += 2 * Math.PI;

            return Math.Abs(diff) * 180.0 / Math.PI;
        }
    }

    public AngularDimensionEntity()
    {
        Name = "AngularDimension";
        Color = Color.DarkGreen;
    }

    public AngularDimensionEntity(PointF centerPoint, PointF point1, PointF point2, float arcRadius = 30f)
    {
        CenterPoint = centerPoint;
        Point1 = point1;
        Point2 = point2;
        DefaultArcRadius = arcRadius;
        Name = "AngularDimension";
        Color = Color.DarkGreen;
    }

    protected override PointF CalculateDefaultTextPosition()
    {
        // デフォルト: 円弧の中間角度方向に、円弧半径の1.5倍の距離
        double angle1 = CalculateAngle(CenterPoint, Point1);
        double angle2 = CalculateAngle(CenterPoint, Point2);
        double midAngle = (angle1 + angle2) / 2;
        float distance = DefaultArcRadius * 1.5f;

        return new PointF(
            CenterPoint.X + (float)Math.Cos(midAngle) * distance,
            CenterPoint.Y + (float)Math.Sin(midAngle) * distance
        );
    }

    protected override string GetDimensionValue()
    {
        return FormatValue(AngleDegrees, suffix: "°");
    }

    protected override RectangleF GetTextBounds(PointF textPos, float textWidth)
    {
        return new RectangleF(
            textPos.X,
            textPos.Y - TextSize / 2,
            textWidth,
            TextSize
        );
    }

    public override void Draw(IRenderer renderer)
    {
        if (!IsVisible)
            return;

        var color = IsSelected ? Color.Orange : Color;
        var thickness = IsSelected ? 2.0f : 1.0f;

        var textPos = TextPositionOverride ?? CalculateDefaultTextPosition();
        var dimensionValue = GetDimensionValue();
        var textWidth = MeasureTextWidth(renderer, dimensionValue);

        // テキスト位置から円弧半径を決定
        float arcRadius = Distance(CenterPoint, textPos) * 0.7f;
        arcRadius = Math.Max(arcRadius, 15f); // 最小半径

        double angle1 = CalculateAngle(CenterPoint, Point1);
        double angle2 = CalculateAngle(CenterPoint, Point2);

        // 角度を度に変換
        float startAngleDeg = (float)(angle1 * 180 / Math.PI);
        float endAngleDeg = (float)(angle2 * 180 / Math.PI);

        // sweepAngleを計算
        float sweepAngle = endAngleDeg - startAngleDeg;
        while (sweepAngle > 180) sweepAngle -= 360;
        while (sweepAngle < -180) sweepAngle += 360;

        // 1. 延長線（必要に応じて）
        DrawExtensionLine(renderer, CenterPoint, Point1, arcRadius, color, thickness);
        DrawExtensionLine(renderer, CenterPoint, Point2, arcRadius, color, thickness);

        // 2. 寸法円弧
        renderer.DrawArc(CenterPoint, arcRadius, startAngleDeg, sweepAngle, color, thickness);

        // 3. 矢印
        // 開始点の矢印
        PointF arcStart = new PointF(
            CenterPoint.X + arcRadius * (float)Math.Cos(angle1),
            CenterPoint.Y + arcRadius * (float)Math.Sin(angle1)
        );
        // 接線方向（反時計回りの場合は垂直方向）
        PointF tangent1 = new PointF(
            -(float)Math.Sin(angle1) * Math.Sign(sweepAngle),
            (float)Math.Cos(angle1) * Math.Sign(sweepAngle)
        );
        DrawArrow(renderer, arcStart, tangent1);

        // 終了点の矢印
        PointF arcEnd = new PointF(
            CenterPoint.X + arcRadius * (float)Math.Cos(angle2),
            CenterPoint.Y + arcRadius * (float)Math.Sin(angle2)
        );
        PointF tangent2 = new PointF(
            (float)Math.Sin(angle2) * Math.Sign(sweepAngle),
            -(float)Math.Cos(angle2) * Math.Sign(sweepAngle)
        );
        DrawArrow(renderer, arcEnd, tangent2);

        // 4. 引き出し線（円弧の中点からテキストへ）
        double midAngle = (angle1 + angle2) / 2;
        PointF arcMid = new PointF(
            CenterPoint.X + arcRadius * (float)Math.Cos(midAngle),
            CenterPoint.Y + arcRadius * (float)Math.Sin(midAngle)
        );

        if (Distance(arcMid, textPos) > arcRadius * 0.2f)
        {
            DrawLeaderLine(renderer, arcMid, textPos, color, thickness);
        }

        // 5. テキスト（常に水平）
        var textDrawPos = new PointF(textPos.X, textPos.Y - TextSize / 2);
        renderer.DrawText(dimensionValue, textDrawPos, TextSize, color);
    }

    private void DrawExtensionLine(IRenderer renderer, PointF center, PointF point,
        float arcRadius, Color color, float thickness)
    {
        float distance = Distance(center, point);
        if (distance > arcRadius * 1.2f)
        {
            // 延長線が必要
            double angle = CalculateAngle(center, point);
            PointF start = new PointF(
                center.X + arcRadius * 1.1f * (float)Math.Cos(angle),
                center.Y + arcRadius * 1.1f * (float)Math.Sin(angle)
            );
            PointF end = new PointF(
                center.X + distance * 0.9f * (float)Math.Cos(angle),
                center.Y + distance * 0.9f * (float)Math.Sin(angle)
            );
            renderer.DrawLine(start, end, color, thickness * 0.5f);
        }
    }

    private void DrawLeaderLine(IRenderer renderer, PointF fromPoint, PointF toPoint,
        Color color, float thickness)
    {
        renderer.DrawLine(fromPoint, toPoint, color, thickness * 0.7f);
    }

    public override RectangleF GetBounds()
    {
        var textPos = TextPositionOverride ?? CalculateDefaultTextPosition();

        float minX = Math.Min(Math.Min(Math.Min(CenterPoint.X, Point1.X), Point2.X), textPos.X) - 40;
        float minY = Math.Min(Math.Min(Math.Min(CenterPoint.Y, Point1.Y), Point2.Y), textPos.Y) - 40;
        float maxX = Math.Max(Math.Max(Math.Max(CenterPoint.X, Point1.X), Point2.X), textPos.X) + 40;
        float maxY = Math.Max(Math.Max(Math.Max(CenterPoint.Y, Point1.Y), Point2.Y), textPos.Y) + 40;

        return new RectangleF(minX, minY, maxX - minX, maxY - minY);
    }

    public override bool HitTest(PointF point, float tolerance = 5.0f)
    {
        // テキスト領域のヒットテスト
        if (HitTestText(point, tolerance))
            return true;

        // 円弧のヒットテスト（簡易版）
        float distFromCenter = Distance(CenterPoint, point);
        float arcRadius = DefaultArcRadius;

        if (Math.Abs(distFromCenter - arcRadius) <= tolerance)
        {
            // 角度範囲内かチェック
            double pointAngle = CalculateAngle(CenterPoint, point);
            double angle1 = CalculateAngle(CenterPoint, Point1);
            double angle2 = CalculateAngle(CenterPoint, Point2);

            // 簡易的な角度範囲チェック
            return true; // TODO: より正確な実装
        }

        return false;
    }

    public override void Translate(float dx, float dy)
    {
        CenterPoint = new PointF(CenterPoint.X + dx, CenterPoint.Y + dy);
        Point1 = new PointF(Point1.X + dx, Point1.Y + dy);
        Point2 = new PointF(Point2.X + dx, Point2.Y + dy);

        if (TextPositionOverride.HasValue)
        {
            TextPositionOverride = new PointF(
                TextPositionOverride.Value.X + dx,
                TextPositionOverride.Value.Y + dy
            );
        }
    }

    public override void Rotate(PointF center, double angleRadians)
    {
        CenterPoint = RotatePoint(CenterPoint, center, angleRadians);
        Point1 = RotatePoint(Point1, center, angleRadians);
        Point2 = RotatePoint(Point2, center, angleRadians);

        if (TextPositionOverride.HasValue)
        {
            TextPositionOverride = RotatePoint(TextPositionOverride.Value, center, angleRadians);
        }
    }

    public override void Scale(PointF center, double scaleX, double scaleY)
    {
        CenterPoint = ScalePoint(CenterPoint, center, scaleX, scaleY);
        Point1 = ScalePoint(Point1, center, scaleX, scaleY);
        Point2 = ScalePoint(Point2, center, scaleX, scaleY);
        DefaultArcRadius *= (float)Math.Min(scaleX, scaleY);

        if (TextPositionOverride.HasValue)
        {
            TextPositionOverride = ScalePoint(TextPositionOverride.Value, center, scaleX, scaleY);
        }
    }

    private static PointF RotatePoint(PointF point, PointF center, double angleRadians)
    {
        float cos = (float)Math.Cos(angleRadians);
        float sin = (float)Math.Sin(angleRadians);
        float dx = point.X - center.X;
        float dy = point.Y - center.Y;

        return new PointF(
            center.X + dx * cos - dy * sin,
            center.Y + dx * sin + dy * cos
        );
    }

    private static PointF ScalePoint(PointF point, PointF center, double scaleX, double scaleY)
    {
        float dx = point.X - center.X;
        float dy = point.Y - center.Y;

        return new PointF(
            center.X + dx * (float)scaleX,
            center.Y + dy * (float)scaleY
        );
    }
}

using System.Drawing;
using SimpleCAD.Core.Rendering;

namespace SimpleCAD.Core.Entities;

/// <summary>
/// 半径寸法エンティティ
/// </summary>
public class RadialDimensionEntity : DimensionEntity
{
    /// <summary>
    /// 円の中心点
    /// </summary>
    public PointF CenterPoint { get; set; }

    /// <summary>
    /// 円周上の指示点
    /// </summary>
    public PointF RadiusPoint { get; set; }

    /// <summary>
    /// 半径
    /// </summary>
    public float Radius => Distance(CenterPoint, RadiusPoint);

    public RadialDimensionEntity()
    {
        Name = "RadialDimension";
        Color = Color.DarkGreen;
    }

    public RadialDimensionEntity(PointF centerPoint, PointF radiusPoint)
    {
        CenterPoint = centerPoint;
        RadiusPoint = radiusPoint;
        Name = "RadialDimension";
        Color = Color.DarkGreen;
    }

    protected override PointF CalculateDefaultTextPosition()
    {
        // デフォルト: 中心から半径点への方向に、半径の1.5倍の距離
        float angle = (float)CalculateAngle(CenterPoint, RadiusPoint);
        float distance = Radius * 1.5f;

        return new PointF(
            CenterPoint.X + (float)Math.Cos(angle) * distance,
            CenterPoint.Y + (float)Math.Sin(angle) * distance
        );
    }

    protected override string GetDimensionValue()
    {
        return FormatValue(Radius, "R");
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

        // 1. 指示線（中心 → 円周点）+ 矢印
        renderer.DrawLine(CenterPoint, RadiusPoint, color, thickness);

        // 矢印の向き（中心から外向き）
        float dx = RadiusPoint.X - CenterPoint.X;
        float dy = RadiusPoint.Y - CenterPoint.Y;
        DrawArrow(renderer, RadiusPoint, new PointF(dx, dy));

        // 2. 引き出し線（折れ線: 円周点 → 斜め → 水平部分）
        DrawLeaderLine(renderer, RadiusPoint, textPos, textWidth, color, thickness);

        // 3. テキスト（常に水平）
        var textDrawPos = new PointF(textPos.X, textPos.Y - TextSize / 2);
        renderer.DrawText(dimensionValue, textDrawPos, TextSize, color);
    }

    private void DrawLeaderLine(IRenderer renderer, PointF fromPoint, PointF toPoint,
        float textWidth, Color color, float thickness)
    {
        // 引き出し線の折れ点を計算
        // テキストの左端から少し離れた位置で水平部分を開始
        float horizontalStart = toPoint.X - 10f;
        float horizontalY = toPoint.Y;

        // 折れ点
        PointF elbowPoint = new PointF(horizontalStart, horizontalY);

        // 斜め部分（円周点 → 折れ点）
        renderer.DrawLine(fromPoint, elbowPoint, color, thickness * 0.7f);

        // 水平部分（折れ点 → テキスト左端）
        renderer.DrawLine(elbowPoint, new PointF(toPoint.X, horizontalY), color, thickness * 0.7f);
    }

    public override RectangleF GetBounds()
    {
        var textPos = TextPositionOverride ?? CalculateDefaultTextPosition();

        float minX = Math.Min(Math.Min(CenterPoint.X, RadiusPoint.X), textPos.X) - 20;
        float minY = Math.Min(Math.Min(CenterPoint.Y, RadiusPoint.Y), textPos.Y) - 20;
        float maxX = Math.Max(Math.Max(CenterPoint.X, RadiusPoint.X), textPos.X) + 20;
        float maxY = Math.Max(Math.Max(CenterPoint.Y, RadiusPoint.Y), textPos.Y) + 20;

        return new RectangleF(minX, minY, maxX - minX, maxY - minY);
    }

    public override bool HitTest(PointF point, float tolerance = 5.0f)
    {
        // テキスト領域のヒットテスト
        if (HitTestText(point, tolerance))
            return true;

        // 指示線のヒットテスト（点から線分までの距離）
        float distToLine = PointToLineSegmentDistance(point, CenterPoint, RadiusPoint);
        if (distToLine <= tolerance)
            return true;

        return false;
    }

    private static float PointToLineSegmentDistance(PointF point, PointF lineStart, PointF lineEnd)
    {
        float dx = lineEnd.X - lineStart.X;
        float dy = lineEnd.Y - lineStart.Y;
        float lengthSquared = dx * dx + dy * dy;

        if (lengthSquared < 0.0001f)
            return Distance(point, lineStart);

        float t = Math.Max(0, Math.Min(1, ((point.X - lineStart.X) * dx + (point.Y - lineStart.Y) * dy) / lengthSquared));

        float projX = lineStart.X + t * dx;
        float projY = lineStart.Y + t * dy;

        float pdx = point.X - projX;
        float pdy = point.Y - projY;

        return (float)Math.Sqrt(pdx * pdx + pdy * pdy);
    }

    public override void Translate(float dx, float dy)
    {
        CenterPoint = new PointF(CenterPoint.X + dx, CenterPoint.Y + dy);
        RadiusPoint = new PointF(RadiusPoint.X + dx, RadiusPoint.Y + dy);

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
        RadiusPoint = RotatePoint(RadiusPoint, center, angleRadians);

        if (TextPositionOverride.HasValue)
        {
            TextPositionOverride = RotatePoint(TextPositionOverride.Value, center, angleRadians);
        }
    }

    public override void Scale(PointF center, double scaleX, double scaleY)
    {
        CenterPoint = ScalePoint(CenterPoint, center, scaleX, scaleY);
        RadiusPoint = ScalePoint(RadiusPoint, center, scaleX, scaleY);

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

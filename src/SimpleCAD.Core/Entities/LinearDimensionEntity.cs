using System.Drawing;
using System.Numerics;
using SimpleCAD.Core.Rendering;

namespace SimpleCAD.Core.Entities;

/// <summary>
/// 線形寸法エンティティ（水平/垂直寸法）
/// </summary>
public class LinearDimensionEntity : DimensionEntity
{
    /// <summary>
    /// 第1測定点
    /// </summary>
    public PointF Point1 { get; set; }

    /// <summary>
    /// 第2測定点
    /// </summary>
    public PointF Point2 { get; set; }

    /// <summary>
    /// デフォルトの寸法線オフセット距離
    /// </summary>
    public float DefaultOffset { get; set; } = 20f;

    public LinearDimensionEntity()
    {
        Name = "LinearDimension";
        Color = Color.DarkGreen;
    }

    public LinearDimensionEntity(PointF point1, PointF point2, float offset = 20f)
    {
        Point1 = point1;
        Point2 = point2;
        DefaultOffset = offset;
        Name = "LinearDimension";
        Color = Color.DarkGreen;
    }

    /// <summary>
    /// 水平寸法かどうか（falseの場合は垂直）
    /// </summary>
    private bool IsHorizontal()
    {
        return Math.Abs(Point2.X - Point1.X) > Math.Abs(Point2.Y - Point1.Y);
    }

    protected override PointF CalculateDefaultTextPosition()
    {
        if (IsHorizontal())
        {
            // 水平寸法: 中央、上または下にオフセット
            float centerX = (Point1.X + Point2.X) / 2;
            float dimLineY = Math.Min(Point1.Y, Point2.Y) - DefaultOffset;
            return new PointF(centerX, dimLineY);
        }
        else
        {
            // 垂直寸法: 中央、左または右にオフセット
            float centerY = (Point1.Y + Point2.Y) / 2;
            float dimLineX = Math.Min(Point1.X, Point2.X) - DefaultOffset;
            return new PointF(dimLineX, centerY);
        }
    }

    protected override string GetDimensionValue()
    {
        float distance = Distance(Point1, Point2);
        return FormatValue(distance);
    }

    protected override RectangleF GetTextBounds(PointF textPos, float textWidth)
    {
        return new RectangleF(
            textPos.X - textWidth / 2,
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

        if (IsHorizontal())
        {
            DrawHorizontalDimension(renderer, textPos, dimensionValue, textWidth, color, thickness);
        }
        else
        {
            DrawVerticalDimension(renderer, textPos, dimensionValue, textWidth, color, thickness);
        }
    }

    private void DrawHorizontalDimension(IRenderer renderer, PointF textPos, string dimensionValue,
        float textWidth, Color color, float thickness)
    {
        float dimLineY = textPos.Y;

        // 補助線（測定点Y → 寸法線Y）
        DrawExtensionLine(renderer, Point1.X, Point1.Y, Point1.X, dimLineY, color, thickness);
        DrawExtensionLine(renderer, Point2.X, Point2.Y, Point2.X, dimLineY, color, thickness);

        // テキストが中央/左外/右外で分岐
        float leftX = Math.Min(Point1.X, Point2.X);
        float rightX = Math.Max(Point1.X, Point2.X);
        bool textInside = (textPos.X >= leftX && textPos.X <= rightX);

        PointF arrowP1 = new PointF(Point1.X, dimLineY);
        PointF arrowP2 = new PointF(Point2.X, dimLineY);

        if (textInside) // 中央: |<---100--->|
        {
            // 寸法線
            renderer.DrawLine(arrowP1, arrowP2, color, thickness);

            // 矢印（内向き）
            DrawArrow(renderer, arrowP1, new PointF(-1, 0)); // ←
            DrawArrow(renderer, arrowP2, new PointF(1, 0));  // →
        }
        else if (textPos.X + textWidth / 2 < leftX) // 左外
        {
            // 寸法線（テキスト右端 → P2）
            float lineStartX = textPos.X + textWidth / 2;
            renderer.DrawLine(new PointF(lineStartX, dimLineY), arrowP2, color, thickness);

            // 矢印（外向き）
            DrawArrow(renderer, arrowP1, new PointF(1, 0));  // →
            DrawArrow(renderer, arrowP2, new PointF(-1, 0)); // ←
        }
        else // 右外
        {
            // 寸法線（P1 → テキスト左端）
            float lineEndX = textPos.X - textWidth / 2;
            renderer.DrawLine(arrowP1, new PointF(lineEndX, dimLineY), color, thickness);

            // 矢印（外向き）
            DrawArrow(renderer, arrowP1, new PointF(1, 0));  // →
            DrawArrow(renderer, arrowP2, new PointF(-1, 0)); // ←
        }

        // テキスト（水平）
        var textDrawPos = new PointF(textPos.X - textWidth / 2, textPos.Y - TextSize / 2);
        renderer.DrawText(dimensionValue, textDrawPos, TextSize, color);
    }

    private void DrawVerticalDimension(IRenderer renderer, PointF textPos, string dimensionValue,
        float textWidth, Color color, float thickness)
    {
        float dimLineX = textPos.X;

        // 補助線（測定点X → 寸法線X）
        DrawExtensionLine(renderer, Point1.X, Point1.Y, dimLineX, Point1.Y, color, thickness);
        DrawExtensionLine(renderer, Point2.X, Point2.Y, dimLineX, Point2.Y, color, thickness);

        // テキストが中央/上外/下外で分岐
        float topY = Math.Min(Point1.Y, Point2.Y);
        float bottomY = Math.Max(Point1.Y, Point2.Y);
        bool textInside = (textPos.Y >= topY && textPos.Y <= bottomY);

        PointF arrowP1 = new PointF(dimLineX, Point1.Y);
        PointF arrowP2 = new PointF(dimLineX, Point2.Y);

        if (textInside) // 中央
        {
            // 寸法線
            renderer.DrawLine(arrowP1, arrowP2, color, thickness);

            // 矢印（内向き）
            DrawArrow(renderer, arrowP1, new PointF(0, -1)); // ↑
            DrawArrow(renderer, arrowP2, new PointF(0, 1));  // ↓
        }
        else if (textPos.Y + TextSize / 2 < topY) // 上外
        {
            // 寸法線（テキスト下端 → 下の点）
            float lineStartY = textPos.Y + TextSize / 2;
            renderer.DrawLine(new PointF(dimLineX, lineStartY), arrowP2, color, thickness);

            // 矢印（外向き）
            DrawArrow(renderer, arrowP1, new PointF(0, 1));  // ↓
            DrawArrow(renderer, arrowP2, new PointF(0, -1)); // ↑
        }
        else // 下外
        {
            // 寸法線（上の点 → テキスト上端）
            float lineEndY = textPos.Y - TextSize / 2;
            renderer.DrawLine(arrowP1, new PointF(dimLineX, lineEndY), color, thickness);

            // 矢印（外向き）
            DrawArrow(renderer, arrowP1, new PointF(0, 1));  // ↓
            DrawArrow(renderer, arrowP2, new PointF(0, -1)); // ↑
        }

        // テキスト（90°回転）
        var transform = Matrix3x2.CreateRotation((float)(-Math.PI / 2), new System.Numerics.Vector2(textPos.X, textPos.Y));
        renderer.PushTransform(transform);

        var textDrawPos = new PointF(textPos.X - textWidth / 2, textPos.Y - TextSize / 2);
        renderer.DrawText(dimensionValue, textDrawPos, TextSize, color);

        renderer.PopTransform();
    }

    private void DrawExtensionLine(IRenderer renderer, float x1, float y1, float x2, float y2,
        Color color, float thickness)
    {
        // 補助線は少し短めに（測定点から少し離す）
        float gap = 2f;
        float dx = x2 - x1;
        float dy = y2 - y1;
        float length = (float)Math.Sqrt(dx * dx + dy * dy);

        if (length < gap * 2) return;

        float ratio = gap / length;
        float startX = x1 + dx * ratio;
        float startY = y1 + dy * ratio;

        renderer.DrawLine(new PointF(startX, startY), new PointF(x2, y2), color, thickness * 0.5f);
    }

    public override RectangleF GetBounds()
    {
        var textPos = TextPositionOverride ?? CalculateDefaultTextPosition();

        float minX = Math.Min(Math.Min(Point1.X, Point2.X), textPos.X) - 20;
        float minY = Math.Min(Math.Min(Point1.Y, Point2.Y), textPos.Y) - 20;
        float maxX = Math.Max(Math.Max(Point1.X, Point2.X), textPos.X) + 20;
        float maxY = Math.Max(Math.Max(Point1.Y, Point2.Y), textPos.Y) + 20;

        return new RectangleF(minX, minY, maxX - minX, maxY - minY);
    }

    public override bool HitTest(PointF point, float tolerance = 5.0f)
    {
        // テキスト領域のヒットテスト
        if (HitTestText(point, tolerance))
            return true;

        // 寸法線のヒットテスト（簡易版）
        var textPos = TextPositionOverride ?? CalculateDefaultTextPosition();

        if (IsHorizontal())
        {
            float dimLineY = textPos.Y;
            if (Math.Abs(point.Y - dimLineY) <= tolerance)
            {
                float minX = Math.Min(Point1.X, Point2.X);
                float maxX = Math.Max(Point1.X, Point2.X);
                if (point.X >= minX - tolerance && point.X <= maxX + tolerance)
                    return true;
            }
        }
        else
        {
            float dimLineX = textPos.X;
            if (Math.Abs(point.X - dimLineX) <= tolerance)
            {
                float minY = Math.Min(Point1.Y, Point2.Y);
                float maxY = Math.Max(Point1.Y, Point2.Y);
                if (point.Y >= minY - tolerance && point.Y <= maxY + tolerance)
                    return true;
            }
        }

        return false;
    }

    public override void Translate(float dx, float dy)
    {
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
        Point1 = RotatePoint(Point1, center, angleRadians);
        Point2 = RotatePoint(Point2, center, angleRadians);

        if (TextPositionOverride.HasValue)
        {
            TextPositionOverride = RotatePoint(TextPositionOverride.Value, center, angleRadians);
        }
    }

    public override void Scale(PointF center, double scaleX, double scaleY)
    {
        Point1 = ScalePoint(Point1, center, scaleX, scaleY);
        Point2 = ScalePoint(Point2, center, scaleX, scaleY);
        DefaultOffset *= (float)Math.Min(scaleX, scaleY);

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

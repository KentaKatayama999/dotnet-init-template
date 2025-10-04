using System.Drawing;
using SimpleCAD.Core.Rendering;

namespace SimpleCAD.Core.Entities;

/// <summary>
/// すべての寸法エンティティの抽象基底クラス
/// </summary>
public abstract class DimensionEntity : GeometryEntity
{
    /// <summary>
    /// テキスト位置のオーバーライド（nullの場合はデフォルト位置）
    /// </summary>
    public PointF? TextPositionOverride { get; set; }

    /// <summary>
    /// 矢印のサイズ
    /// </summary>
    public float ArrowSize { get; set; } = 5f;

    /// <summary>
    /// テキストのフォントサイズ
    /// </summary>
    public float TextSize { get; set; } = 12f;

    /// <summary>
    /// デフォルトのテキスト位置を計算
    /// </summary>
    protected abstract PointF CalculateDefaultTextPosition();

    /// <summary>
    /// 寸法値を取得（例: "100.00", "R50.00", "45.0°"）
    /// </summary>
    protected abstract string GetDimensionValue();

    /// <summary>
    /// テキストの境界ボックスを取得
    /// </summary>
    protected abstract RectangleF GetTextBounds(PointF textPos, float textWidth);

    /// <summary>
    /// テキスト領域のヒットテスト
    /// </summary>
    public bool HitTestText(PointF point, float tolerance = 5.0f)
    {
        var textPos = TextPositionOverride ?? CalculateDefaultTextPosition();
        var dimensionValue = GetDimensionValue();

        // テキスト幅の概算（正確な測定にはIRendererが必要だが、ヒットテストでは概算で十分）
        var estimatedWidth = TextSize * 0.6f * dimensionValue.Length;
        var textBounds = GetTextBounds(textPos, estimatedWidth);

        // 許容誤差を含めた範囲でテスト
        var expandedBounds = new RectangleF(
            textBounds.X - tolerance,
            textBounds.Y - tolerance,
            textBounds.Width + tolerance * 2,
            textBounds.Height + tolerance * 2
        );

        return expandedBounds.Contains(point);
    }

    /// <summary>
    /// テキストを指定位置に移動
    /// </summary>
    public void MoveTextTo(PointF position)
    {
        TextPositionOverride = position;
    }

    /// <summary>
    /// 矢印を描画（> 形状）
    /// </summary>
    /// <param name="renderer">レンダラー</param>
    /// <param name="tip">矢印の先端位置</param>
    /// <param name="direction">矢印の向き（単位ベクトル）</param>
    protected void DrawArrow(IRenderer renderer, PointF tip, PointF direction)
    {
        // 矢印の長さ
        float length = ArrowSize;
        float width = ArrowSize * 0.5f;

        // 方向ベクトルを正規化
        float dirLength = (float)Math.Sqrt(direction.X * direction.X + direction.Y * direction.Y);
        if (dirLength < 0.001f) return;

        float dx = direction.X / dirLength;
        float dy = direction.Y / dirLength;

        // 垂直ベクトル
        float perpX = -dy;
        float perpY = dx;

        // 矢印の2つの羽根の端点
        var base1 = new PointF(
            tip.X - dx * length + perpX * width,
            tip.Y - dy * length + perpY * width
        );
        var base2 = new PointF(
            tip.X - dx * length - perpX * width,
            tip.Y - dy * length - perpY * width
        );

        var color = IsSelected ? Color.Orange : Color;
        var thickness = IsSelected ? 2.0f : 1.0f;

        // 矢印を2本の線で描画
        renderer.DrawLine(tip, base1, color, thickness);
        renderer.DrawLine(tip, base2, color, thickness);
    }

    /// <summary>
    /// 寸法値をフォーマット
    /// </summary>
    protected string FormatValue(double value, string prefix = "", string suffix = "")
    {
        return $"{prefix}{value:F2}{suffix}";
    }

    /// <summary>
    /// テキスト幅を測定
    /// </summary>
    protected float MeasureTextWidth(IRenderer renderer, string text)
    {
        return renderer.MeasureTextWidth(text, TextSize);
    }

    /// <summary>
    /// 2点間の距離を計算
    /// </summary>
    protected static float Distance(PointF p1, PointF p2)
    {
        float dx = p2.X - p1.X;
        float dy = p2.Y - p1.Y;
        return (float)Math.Sqrt(dx * dx + dy * dy);
    }

    /// <summary>
    /// 角度を計算（ラジアン）
    /// </summary>
    protected static double CalculateAngle(PointF from, PointF to)
    {
        return Math.Atan2(to.Y - from.Y, to.X - from.X);
    }
}

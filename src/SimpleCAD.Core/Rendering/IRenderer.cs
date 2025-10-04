using System.Drawing;
using System.Numerics;

namespace SimpleCAD.Core.Rendering;

/// <summary>
/// プラットフォーム非依存の描画抽象化インターフェース
/// </summary>
public interface IRenderer
{
    // 基本描画

    /// <summary>
    /// 直線を描画
    /// </summary>
    void DrawLine(PointF p1, PointF p2, Color color, float thickness);

    /// <summary>
    /// 円弧を描画
    /// </summary>
    void DrawArc(PointF center, float radius, float startAngle, float sweepAngle, Color color, float thickness);

    /// <summary>
    /// ベジェ曲線を描画
    /// </summary>
    void DrawBezier(PointF p1, PointF p2, PointF p3, PointF p4, Color color, float thickness);

    /// <summary>
    /// パス（連続線）を描画
    /// </summary>
    void DrawPath(PointF[] points, Color color, float thickness);

    /// <summary>
    /// 円を描画
    /// </summary>
    void DrawCircle(PointF center, float radius, Color color, float thickness);

    /// <summary>
    /// 矩形を描画
    /// </summary>
    void DrawRectangle(RectangleF rect, Color color, float thickness);

    // テキスト描画

    /// <summary>
    /// テキストを描画
    /// </summary>
    void DrawText(string text, PointF position, float fontSize, Color color);

    /// <summary>
    /// テキストの幅を測定
    /// </summary>
    float MeasureTextWidth(string text, float fontSize);

    // 塗りつぶし

    /// <summary>
    /// 円を塗りつぶし
    /// </summary>
    void FillCircle(PointF center, float radius, Color color);

    /// <summary>
    /// 矩形を塗りつぶし
    /// </summary>
    void FillRectangle(RectangleF rect, Color color);

    // 変換

    /// <summary>
    /// 変換行列をプッシュ
    /// </summary>
    void PushTransform(Matrix3x2 transform);

    /// <summary>
    /// 変換行列をポップ
    /// </summary>
    void PopTransform();
}

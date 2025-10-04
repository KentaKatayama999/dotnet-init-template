using System.Drawing;
using System.Drawing.Drawing2D;
using System.Numerics;
using SimpleCAD.Core.Rendering;

namespace SimpleCAD.Controls.WinForms.Rendering;

/// <summary>
/// WinForms用レンダラー（Graphics実装）
/// </summary>
public class WinFormsRenderer : IRenderer
{
    private readonly Graphics _graphics;
    private readonly Stack<Matrix> _transformStack = new();

    public WinFormsRenderer(Graphics graphics)
    {
        _graphics = graphics;
        _graphics.SmoothingMode = SmoothingMode.AntiAlias;
    }

    public void DrawLine(PointF p1, PointF p2, Color color, float thickness)
    {
        using var pen = new Pen(color, thickness);
        _graphics.DrawLine(pen, p1, p2);
    }

    public void DrawArc(PointF center, float radius, float startAngle, float sweepAngle,
        Color color, float thickness)
    {
        using var pen = new Pen(color, thickness);

        // GraphicsのDrawArcは左上座標とサイズで矩形を指定
        var rect = new RectangleF(
            center.X - radius,
            center.Y - radius,
            radius * 2,
            radius * 2
        );

        _graphics.DrawArc(pen, rect, startAngle, sweepAngle);
    }

    public void DrawBezier(PointF p1, PointF p2, PointF p3, PointF p4,
        Color color, float thickness)
    {
        using var pen = new Pen(color, thickness);
        _graphics.DrawBezier(pen, p1, p2, p3, p4);
    }

    public void DrawPath(PointF[] points, Color color, float thickness)
    {
        if (points == null || points.Length < 2)
            return;

        using var pen = new Pen(color, thickness);
        _graphics.DrawLines(pen, points);
    }

    public void DrawCircle(PointF center, float radius, Color color, float thickness)
    {
        using var pen = new Pen(color, thickness);
        var rect = new RectangleF(
            center.X - radius,
            center.Y - radius,
            radius * 2,
            radius * 2
        );
        _graphics.DrawEllipse(pen, rect);
    }

    public void DrawRectangle(RectangleF rect, Color color, float thickness)
    {
        using var pen = new Pen(color, thickness);
        _graphics.DrawRectangle(pen, rect.X, rect.Y, rect.Width, rect.Height);
    }

    public void DrawText(string text, PointF position, float fontSize, Color color)
    {
        using var font = new Font("Consolas", fontSize);
        using var brush = new SolidBrush(color);
        _graphics.DrawString(text, font, brush, position);
    }

    public float MeasureTextWidth(string text, float fontSize)
    {
        using var font = new Font("Consolas", fontSize);
        var size = _graphics.MeasureString(text, font);
        return size.Width;
    }

    public void FillCircle(PointF center, float radius, Color color)
    {
        using var brush = new SolidBrush(color);
        var rect = new RectangleF(
            center.X - radius,
            center.Y - radius,
            radius * 2,
            radius * 2
        );
        _graphics.FillEllipse(brush, rect);
    }

    public void FillRectangle(RectangleF rect, Color color)
    {
        using var brush = new SolidBrush(color);
        _graphics.FillRectangle(brush, rect);
    }

    public void PushTransform(Matrix3x2 transform)
    {
        // 現在の変換を保存
        var currentTransform = _graphics.Transform.Clone();
        _transformStack.Push(currentTransform);

        // 新しい変換を適用
        var matrix = new Matrix(
            transform.M11, transform.M12,
            transform.M21, transform.M22,
            transform.M31, transform.M32
        );

        _graphics.MultiplyTransform(matrix);
    }

    public void PopTransform()
    {
        if (_transformStack.Count > 0)
        {
            var previousTransform = _transformStack.Pop();
            _graphics.Transform = previousTransform;
        }
    }
}

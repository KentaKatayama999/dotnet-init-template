using System.Numerics;
using System.Windows;
using System.Windows.Media;
using SimpleCAD.Core.Rendering;
using SysDrawing = System.Drawing;

namespace SimpleCAD.Controls.WPF.Rendering;

/// <summary>
/// WPF用レンダラー（DrawingContext実装）
/// </summary>
public class WPFRenderer : IRenderer
{
    private readonly DrawingContext _dc;
    private readonly Stack<Matrix> _transformStack = new();

    public WPFRenderer(DrawingContext dc)
    {
        _dc = dc;
    }

    public void DrawLine(SysDrawing.PointF p1, SysDrawing.PointF p2, SysDrawing.Color color, float thickness)
    {
        var pen = new Pen(new SolidColorBrush(ToWpfColor(color)), thickness);
        _dc.DrawLine(pen, new Point(p1.X, p1.Y), new Point(p2.X, p2.Y));
    }

    public void DrawArc(SysDrawing.PointF center, float radius, float startAngle, float sweepAngle,
        SysDrawing.Color color, float thickness)
    {
        // WPFでの円弧描画（PathGeometry使用）
        var startRad = startAngle * Math.PI / 180.0;
        var endRad = (startAngle + sweepAngle) * Math.PI / 180.0;

        var startPoint = new Point(
            center.X + radius * Math.Cos(startRad),
            center.Y + radius * Math.Sin(startRad)
        );
        var endPoint = new Point(
            center.X + radius * Math.Cos(endRad),
            center.Y + radius * Math.Sin(endRad)
        );

        var isLargeArc = Math.Abs(sweepAngle) > 180;
        var sweepDirection = sweepAngle > 0 ? SweepDirection.Clockwise : SweepDirection.Counterclockwise;

        var geometry = new PathGeometry(new[]
        {
            new PathFigure(startPoint, new PathSegment[]
            {
                new ArcSegment(endPoint, new Size(radius, radius), 0, isLargeArc, sweepDirection, true)
            }, false)
        });

        var pen = new Pen(new SolidColorBrush(ToWpfColor(color)), thickness);
        _dc.DrawGeometry(null, pen, geometry);
    }

    public void DrawBezier(SysDrawing.PointF p1, SysDrawing.PointF p2, SysDrawing.PointF p3,
        SysDrawing.PointF p4, SysDrawing.Color color, float thickness)
    {
        var geometry = new PathGeometry(new[]
        {
            new PathFigure(new Point(p1.X, p1.Y), new PathSegment[]
            {
                new BezierSegment(
                    new Point(p2.X, p2.Y),
                    new Point(p3.X, p3.Y),
                    new Point(p4.X, p4.Y),
                    true)
            }, false)
        });

        var pen = new Pen(new SolidColorBrush(ToWpfColor(color)), thickness);
        _dc.DrawGeometry(null, pen, geometry);
    }

    public void DrawPath(SysDrawing.PointF[] points, SysDrawing.Color color, float thickness)
    {
        if (points == null || points.Length < 2)
            return;

        var segments = new PathSegment[points.Length - 1];
        for (int i = 1; i < points.Length; i++)
        {
            segments[i - 1] = new LineSegment(new Point(points[i].X, points[i].Y), true);
        }

        var geometry = new PathGeometry(new[]
        {
            new PathFigure(new Point(points[0].X, points[0].Y), segments, false)
        });

        var pen = new Pen(new SolidColorBrush(ToWpfColor(color)), thickness);
        _dc.DrawGeometry(null, pen, geometry);
    }

    public void DrawCircle(SysDrawing.PointF center, float radius, SysDrawing.Color color, float thickness)
    {
        var pen = new Pen(new SolidColorBrush(ToWpfColor(color)), thickness);
        _dc.DrawEllipse(null, pen, new Point(center.X, center.Y), radius, radius);
    }

    public void DrawRectangle(SysDrawing.RectangleF rect, SysDrawing.Color color, float thickness)
    {
        var pen = new Pen(new SolidColorBrush(ToWpfColor(color)), thickness);
        _dc.DrawRectangle(null, pen, new Rect(rect.X, rect.Y, rect.Width, rect.Height));
    }

    public void DrawText(string text, SysDrawing.PointF position, float fontSize, SysDrawing.Color color)
    {
        var formattedText = new FormattedText(
            text,
            System.Globalization.CultureInfo.CurrentCulture,
            FlowDirection.LeftToRight,
            new Typeface("Consolas"),
            fontSize,
            new SolidColorBrush(ToWpfColor(color)),
            1.0);

        _dc.DrawText(formattedText, new Point(position.X, position.Y));
    }

    public float MeasureTextWidth(string text, float fontSize)
    {
        var formattedText = new FormattedText(
            text,
            System.Globalization.CultureInfo.CurrentCulture,
            FlowDirection.LeftToRight,
            new Typeface("Consolas"),
            fontSize,
            Brushes.Black,
            1.0);

        return (float)formattedText.Width;
    }

    public void FillCircle(SysDrawing.PointF center, float radius, SysDrawing.Color color)
    {
        var brush = new SolidColorBrush(ToWpfColor(color));
        _dc.DrawEllipse(brush, null, new Point(center.X, center.Y), radius, radius);
    }

    public void FillRectangle(SysDrawing.RectangleF rect, SysDrawing.Color color)
    {
        var brush = new SolidColorBrush(ToWpfColor(color));
        _dc.DrawRectangle(brush, null, new Rect(rect.X, rect.Y, rect.Width, rect.Height));
    }

    public void PushTransform(Matrix3x2 transform)
    {
        var matrix = new Matrix(
            transform.M11, transform.M12,
            transform.M21, transform.M22,
            transform.M31, transform.M32);

        _transformStack.Push(matrix);
        _dc.PushTransform(new MatrixTransform(matrix));
    }

    public void PopTransform()
    {
        if (_transformStack.Count > 0)
        {
            _transformStack.Pop();
            _dc.Pop();
        }
    }

    private static System.Windows.Media.Color ToWpfColor(SysDrawing.Color color)
    {
        return System.Windows.Media.Color.FromArgb(color.A, color.R, color.G, color.B);
    }
}

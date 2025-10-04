using System.Drawing;
using Geometry;
using SimpleCAD.Core.Rendering;

namespace SimpleCAD.Core.Entities;

/// <summary>
/// 直線エンティティ
/// </summary>
public class LineEntity : GeometryEntity
{
    /// <summary>
    /// 直線オブジェクト
    /// </summary>
    public ObjLine Line { get; set; }

    public LineEntity()
    {
        Line = new ObjLine();
        Name = "Line";
    }

    public LineEntity(ObjLine line)
    {
        Line = line;
        Name = "Line";
    }

    public override void Draw(IRenderer renderer)
    {
        if (!IsVisible || Line == null)
            return;

        var p1 = new PointF((float)Line.StartPoint.X(0), (float)Line.StartPoint.Y(0));
        var p2 = new PointF((float)Line.EndPoint.X(0), (float)Line.EndPoint.Y(0));

        var color = IsSelected ? Color.Orange : Color;
        var thickness = IsSelected ? (float)(Thickness * 2) : (float)Thickness;

        renderer.DrawLine(p1, p2, color, thickness);
    }

    public override RectangleF GetBounds()
    {
        if (Line == null)
            return RectangleF.Empty;

        float x1 = (float)Line.StartPoint.X(0);
        float y1 = (float)Line.StartPoint.Y(0);
        float x2 = (float)Line.EndPoint.X(0);
        float y2 = (float)Line.EndPoint.Y(0);

        float minX = Math.Min(x1, x2);
        float minY = Math.Min(y1, y2);
        float maxX = Math.Max(x1, x2);
        float maxY = Math.Max(y1, y2);

        return new RectangleF(minX, minY, maxX - minX, maxY - minY);
    }

    public override bool HitTest(PointF point, float tolerance = 5.0f)
    {
        if (Line == null)
            return false;

        var p1 = new PointF((float)Line.StartPoint.X(0), (float)Line.StartPoint.Y(0));
        var p2 = new PointF((float)Line.EndPoint.X(0), (float)Line.EndPoint.Y(0));

        // 点から線分までの距離を計算
        float distance = PointToLineSegmentDistance(point, p1, p2);
        return distance <= tolerance;
    }

    public override void Translate(float dx, float dy)
    {
        if (Line == null)
            return;

        var x1 = Line.StartPoint.X(0) + dx;
        var y1 = Line.StartPoint.Y(0) + dy;
        var x2 = Line.EndPoint.X(0) + dx;
        var y2 = Line.EndPoint.Y(0) + dy;

        Line = new ObjLine(new ObjPoints(x1, y1), new ObjPoints(x2, y2));
    }

    public override void Rotate(PointF center, double angleRadians)
    {
        if (Line == null)
            return;

        var p1 = RotatePoint(
            (float)Line.StartPoint.X(0),
            (float)Line.StartPoint.Y(0),
            center.X,
            center.Y,
            angleRadians);

        var p2 = RotatePoint(
            (float)Line.EndPoint.X(0),
            (float)Line.EndPoint.Y(0),
            center.X,
            center.Y,
            angleRadians);

        Line = new ObjLine(new ObjPoints(p1.X, p1.Y), new ObjPoints(p2.X, p2.Y));
    }

    public override void Scale(PointF center, double scaleX, double scaleY)
    {
        if (Line == null)
            return;

        var x1 = center.X + (Line.StartPoint.X(0) - center.X) * scaleX;
        var y1 = center.Y + (Line.StartPoint.Y(0) - center.Y) * scaleY;
        var x2 = center.X + (Line.EndPoint.X(0) - center.X) * scaleX;
        var y2 = center.Y + (Line.EndPoint.Y(0) - center.Y) * scaleY;

        Line = new ObjLine(new ObjPoints(x1, y1), new ObjPoints(x2, y2));
    }

    private static float PointToLineSegmentDistance(PointF point, PointF lineStart, PointF lineEnd)
    {
        float dx = lineEnd.X - lineStart.X;
        float dy = lineEnd.Y - lineStart.Y;
        float lengthSquared = dx * dx + dy * dy;

        if (lengthSquared == 0)
            return Distance(point, lineStart);

        float t = Math.Max(0, Math.Min(1,
            ((point.X - lineStart.X) * dx + (point.Y - lineStart.Y) * dy) / lengthSquared));

        float projX = lineStart.X + t * dx;
        float projY = lineStart.Y + t * dy;

        return Distance(point, new PointF(projX, projY));
    }

    private static float Distance(PointF p1, PointF p2)
    {
        float dx = p2.X - p1.X;
        float dy = p2.Y - p1.Y;
        return (float)Math.Sqrt(dx * dx + dy * dy);
    }

    private static PointF RotatePoint(float x, float y, float cx, float cy, double angleRadians)
    {
        float cos = (float)Math.Cos(angleRadians);
        float sin = (float)Math.Sin(angleRadians);
        float dx = x - cx;
        float dy = y - cy;
        return new PointF(cx + dx * cos - dy * sin, cy + dx * sin + dy * cos);
    }
}

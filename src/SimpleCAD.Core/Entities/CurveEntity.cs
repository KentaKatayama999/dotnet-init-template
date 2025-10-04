using System.Drawing;
using Geometry;
using SimpleCAD.Core.Rendering;

namespace SimpleCAD.Core.Entities;

/// <summary>
/// NURBS曲線エンティティ
/// </summary>
public class CurveEntity : GeometryEntity
{
    /// <summary>
    /// NURBS曲線オブジェクト
    /// </summary>
    public ObjCurve Curve { get; set; }

    public CurveEntity()
    {
        Curve = new ObjCurve();
        Name = "Curve";
    }

    public CurveEntity(ObjCurve curve)
    {
        Curve = curve;
        Name = "Curve";
    }

    public override void Draw(IRenderer renderer)
    {
        if (!IsVisible || Curve == null)
            return;

        // 曲線を分割して描画
        var points = Curve.GetDevidePoints(100);
        if (points == null || points.Np < 2)
            return;

        var pointArray = new PointF[points.Np];
        for (int i = 0; i < points.Np; i++)
        {
            pointArray[i] = new PointF((float)points.X(i), (float)points.Y(i));
        }

        var color = IsSelected ? Color.Orange : Color;
        var thickness = IsSelected ? (float)(Thickness * 2) : (float)Thickness;

        renderer.DrawPath(pointArray, color, thickness);
    }

    public override RectangleF GetBounds()
    {
        if (Curve == null)
            return RectangleF.Empty;

        var points = Curve.GetDevidePoints(100);
        if (points == null || points.Np == 0)
            return RectangleF.Empty;

        float minX = float.MaxValue, minY = float.MaxValue;
        float maxX = float.MinValue, maxY = float.MinValue;

        for (int i = 0; i < points.Np; i++)
        {
            float x = (float)points.X(i);
            float y = (float)points.Y(i);

            minX = Math.Min(minX, x);
            minY = Math.Min(minY, y);
            maxX = Math.Max(maxX, x);
            maxY = Math.Max(maxY, y);
        }

        return new RectangleF(minX, minY, maxX - minX, maxY - minY);
    }

    public override bool HitTest(PointF point, float tolerance = 5.0f)
    {
        if (Curve == null)
            return false;

        var points = Curve.GetDevidePoints(100);
        if (points == null || points.Np < 2)
            return false;

        // 各セグメントに対して点からの距離をチェック
        for (int i = 0; i < points.Np - 1; i++)
        {
            var p1 = new PointF((float)points.X(i), (float)points.Y(i));
            var p2 = new PointF((float)points.X(i + 1), (float)points.Y(i + 1));

            float distance = PointToLineSegmentDistance(point, p1, p2);
            if (distance <= tolerance)
                return true;
        }

        return false;
    }

    public override void Translate(float dx, float dy)
    {
        if (Curve == null)
            return;

        // PassPointsを取得して変換
        var passPoints = Curve.PassPoints;
        if (passPoints == null || passPoints.Np == 0)
            return;

        var newPassPoints = new ObjPoints();
        for (int i = 0; i < passPoints.Np; i++)
        {
            newPassPoints.AddPoint(
                passPoints.X(i) + dx,
                passPoints.Y(i) + dy);
        }

        var newCurve = new ObjCurve();
        newCurve.SetCurve(newPassPoints);
        Curve = newCurve;
    }

    public override void Rotate(PointF center, double angleRadians)
    {
        if (Curve == null)
            return;

        var passPoints = Curve.PassPoints;
        if (passPoints == null || passPoints.Np == 0)
            return;

        var newPassPoints = new ObjPoints();
        for (int i = 0; i < passPoints.Np; i++)
        {
            var rotated = RotatePoint(
                (float)passPoints.X(i),
                (float)passPoints.Y(i),
                center.X,
                center.Y,
                angleRadians);

            newPassPoints.AddPoint(rotated.X, rotated.Y);
        }

        var newCurve = new ObjCurve();
        newCurve.SetCurve(newPassPoints);
        Curve = newCurve;
    }

    public override void Scale(PointF center, double scaleX, double scaleY)
    {
        if (Curve == null)
            return;

        var passPoints = Curve.PassPoints;
        if (passPoints == null || passPoints.Np == 0)
            return;

        var newPassPoints = new ObjPoints();
        for (int i = 0; i < passPoints.Np; i++)
        {
            var newX = center.X + (passPoints.X(i) - center.X) * scaleX;
            var newY = center.Y + (passPoints.Y(i) - center.Y) * scaleY;
            newPassPoints.AddPoint(newX, newY);
        }

        var newCurve = new ObjCurve();
        newCurve.SetCurve(newPassPoints);
        Curve = newCurve;
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

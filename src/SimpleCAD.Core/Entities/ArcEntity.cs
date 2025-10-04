using System.Drawing;
using Geometry;
using SimpleCAD.Core.Rendering;

namespace SimpleCAD.Core.Entities;

/// <summary>
/// 円弧エンティティ
/// </summary>
public class ArcEntity : GeometryEntity
{
    /// <summary>
    /// 円弧オブジェクト
    /// </summary>
    public ObjArc Arc { get; set; }

    public ArcEntity()
    {
        Arc = new ObjArc();
        Name = "Arc";
    }

    public ArcEntity(ObjArc arc)
    {
        Arc = arc;
        Name = "Arc";
    }

    public override void Draw(IRenderer renderer)
    {
        if (!IsVisible || Arc == null)
            return;

        var center = new PointF((float)Arc.Center.X(0), (float)Arc.Center.Y(0));
        var radius = (float)Arc.R;
        var startAngle = (float)Arc.StartDeg;
        var sweepAngle = (float)Arc.EndDeg - startAngle;

        var color = IsSelected ? Color.Orange : Color;
        var thickness = IsSelected ? (float)(Thickness * 2) : (float)Thickness;

        renderer.DrawArc(center, radius, startAngle, sweepAngle, color, thickness);
    }

    public override RectangleF GetBounds()
    {
        if (Arc == null)
            return RectangleF.Empty;

        float centerX = (float)Arc.Center.X(0);
        float centerY = (float)Arc.Center.Y(0);
        float radius = (float)Arc.R;

        // 簡易実装: 円全体のバウンディングボックス
        return new RectangleF(
            centerX - radius,
            centerY - radius,
            radius * 2,
            radius * 2
        );
    }

    public override bool HitTest(PointF point, float tolerance = 5.0f)
    {
        if (Arc == null)
            return false;

        float centerX = (float)Arc.Center.X(0);
        float centerY = (float)Arc.Center.Y(0);
        float radius = (float)Arc.R;

        // 中心からの距離
        float dx = point.X - centerX;
        float dy = point.Y - centerY;
        float distance = (float)Math.Sqrt(dx * dx + dy * dy);

        // 半径付近にあるか
        if (Math.Abs(distance - radius) > tolerance)
            return false;

        // 角度が円弧の範囲内か
        double angle = Math.Atan2(dy, dx) * 180.0 / Math.PI;
        if (angle < 0) angle += 360;

        double startAngle = Arc.StartDeg;
        double endAngle = Arc.EndDeg;

        // 角度範囲のチェック
        if (startAngle <= endAngle)
        {
            return angle >= startAngle && angle <= endAngle;
        }
        else
        {
            return angle >= startAngle || angle <= endAngle;
        }
    }

    public override void Translate(float dx, float dy)
    {
        if (Arc == null)
            return;

        var newCenterX = Arc.Center.X(0) + dx;
        var newCenterY = Arc.Center.Y(0) + dy;

        var newArc = new ObjArc();
        newArc.SetArcAngle(Arc.R, newCenterX, newCenterY, Arc.Center.Z(0), Arc.StartDeg, Arc.EndDeg);
        Arc = newArc;
    }

    public override void Rotate(PointF center, double angleRadians)
    {
        if (Arc == null)
            return;

        // 中心点を回転
        var newCenter = RotatePoint(
            (float)Arc.Center.X(0),
            (float)Arc.Center.Y(0),
            center.X,
            center.Y,
            angleRadians);

        // 角度を回転（度数法で）
        double angleDegrees = angleRadians * 180.0 / Math.PI;
        double newStartAngle = Arc.StartDeg + angleDegrees;
        double newEndAngle = Arc.EndDeg + angleDegrees;

        // 角度を0-360の範囲に正規化
        while (newStartAngle < 0) newStartAngle += 360;
        while (newStartAngle >= 360) newStartAngle -= 360;
        while (newEndAngle < 0) newEndAngle += 360;
        while (newEndAngle >= 360) newEndAngle -= 360;

        var newArc = new ObjArc();
        newArc.SetArcAngle(Arc.R, newCenter.X, newCenter.Y, Arc.Center.Z(0), newStartAngle, newEndAngle);
        Arc = newArc;
    }

    public override void Scale(PointF center, double scaleX, double scaleY)
    {
        if (Arc == null)
            return;

        // 中心点をスケール
        var newCenterX = center.X + (Arc.Center.X(0) - center.X) * scaleX;
        var newCenterY = center.Y + (Arc.Center.Y(0) - center.Y) * scaleY;

        // 半径をスケール（平均値を使用）
        var newRadius = Arc.R * (scaleX + scaleY) / 2.0;

        var newArc = new ObjArc();
        newArc.SetArcAngle(newRadius, newCenterX, newCenterY, Arc.Center.Z(0), Arc.StartDeg, Arc.EndDeg);
        Arc = newArc;
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

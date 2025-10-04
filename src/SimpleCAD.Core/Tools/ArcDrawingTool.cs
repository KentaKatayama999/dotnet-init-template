using System.Drawing;
using Geometry;
using SimpleCAD.Core.Entities;
using SimpleCAD.Core.Rendering;

namespace SimpleCAD.Core.Tools;

/// <summary>
/// 円弧描画ツール（中心・始点・終点の3点指定）
/// </summary>
public class ArcDrawingTool : IDrawingTool
{
    private enum DrawingState
    {
        WaitingForCenter,
        WaitingForStartPoint,
        WaitingForEndPoint
    }

    private DrawingState _state = DrawingState.WaitingForCenter;
    private PointF? _center;
    private PointF? _startPoint;
    private PointF? _currentPoint;

    public string Name => "Arc";
    public bool IsActive => _state != DrawingState.WaitingForCenter || _center.HasValue;

    public event EventHandler<GeometryEntity>? EntityCompleted;

    public void OnMouseDown(PointF worldPoint, bool snapToGrid, double gridSpacing)
    {
        var point = snapToGrid ? SnapPoint(worldPoint, gridSpacing) : worldPoint;

        switch (_state)
        {
            case DrawingState.WaitingForCenter:
                _center = point;
                _state = DrawingState.WaitingForStartPoint;
                break;

            case DrawingState.WaitingForStartPoint:
                _startPoint = point;
                _currentPoint = point;
                _state = DrawingState.WaitingForEndPoint;
                break;

            case DrawingState.WaitingForEndPoint:
                if (_center.HasValue && _startPoint.HasValue)
                {
                    var arc = CreateArc(_center.Value, _startPoint.Value, point);
                    EntityCompleted?.Invoke(this, arc);
                }
                Reset();
                break;
        }
    }

    public void OnMouseMove(PointF worldPoint, bool snapToGrid, double gridSpacing)
    {
        _currentPoint = snapToGrid ? SnapPoint(worldPoint, gridSpacing) : worldPoint;
    }

    public void OnMouseUp(PointF worldPoint, bool snapToGrid, double gridSpacing)
    {
        // 円弧ツールではマウスアップは使用しない
    }

    public void DrawPreview(IRenderer renderer)
    {
        if (!_center.HasValue)
            return;

        // 中心点マーカー
        renderer.FillCircle(_center.Value, 3, Color.Red);

        if (_state == DrawingState.WaitingForStartPoint && _currentPoint.HasValue)
        {
            // 半径プレビュー（円）
            var radius = Distance(_center.Value, _currentPoint.Value);
            renderer.DrawCircle(_center.Value, radius, Color.LightGray, 1.0f);
            renderer.DrawLine(_center.Value, _currentPoint.Value, Color.Gray, 1.0f);
        }
        else if (_state == DrawingState.WaitingForEndPoint && _startPoint.HasValue && _currentPoint.HasValue)
        {
            // 円弧プレビュー
            var radius = Distance(_center.Value, _startPoint.Value);
            var startAngle = GetAngle(_center.Value, _startPoint.Value);
            var endAngle = GetAngle(_center.Value, _currentPoint.Value);

            // 一時的な円弧エンティティでプレビュー
            var arc = new ObjArc();
            arc.SetArcAngle(radius, _center.Value.X, _center.Value.Y, 0, startAngle, endAngle);
            var arcEntity = new ArcEntity(arc) { Color = Color.Gray, Thickness = 1.0 };
            arcEntity.Draw(renderer);

            // 始点・終点マーカー
            renderer.FillCircle(_startPoint.Value, 3, Color.Green);
            renderer.FillCircle(_currentPoint.Value, 3, Color.Blue);
        }
    }

    public void Cancel()
    {
        Reset();
    }

    private void Reset()
    {
        _center = null;
        _startPoint = null;
        _currentPoint = null;
        _state = DrawingState.WaitingForCenter;
    }

    private ArcEntity CreateArc(PointF center, PointF start, PointF end)
    {
        var radius = Distance(center, start);
        var startAngle = GetAngle(center, start);
        var endAngle = GetAngle(center, end);

        var arc = new ObjArc();
        arc.SetArcAngle(radius, center.X, center.Y, 0, startAngle, endAngle);

        return new ArcEntity(arc)
        {
            Color = Color.Black,
            Thickness = 1.5
        };
    }

    private float Distance(PointF p1, PointF p2)
    {
        var dx = p2.X - p1.X;
        var dy = p2.Y - p1.Y;
        return (float)Math.Sqrt(dx * dx + dy * dy);
    }

    private double GetAngle(PointF center, PointF point)
    {
        var dx = point.X - center.X;
        var dy = point.Y - center.Y;
        var angle = Math.Atan2(dy, dx) * 180.0 / Math.PI;
        return angle < 0 ? angle + 360 : angle;
    }

    private PointF SnapPoint(PointF point, double gridSpacing)
    {
        var x = Math.Round(point.X / gridSpacing) * gridSpacing;
        var y = Math.Round(point.Y / gridSpacing) * gridSpacing;
        return new PointF((float)x, (float)y);
    }
}

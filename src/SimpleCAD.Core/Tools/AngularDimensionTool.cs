using System.Drawing;
using SimpleCAD.Core.Entities;
using SimpleCAD.Core.Rendering;

namespace SimpleCAD.Core.Tools;

/// <summary>
/// 角度寸法作成ツール
/// </summary>
public class AngularDimensionTool : IDrawingTool
{
    private enum State
    {
        WaitingForCenterPoint,
        WaitingForFirstPoint,
        WaitingForSecondPoint,
        WaitingForArcPosition
    }

    private State _state = State.WaitingForCenterPoint;
    private PointF? _centerPoint;
    private PointF? _point1;
    private PointF? _point2;
    private PointF? _currentPoint;

    public string Name => "AngularDimension";
    public bool IsActive => _state != State.WaitingForCenterPoint || _centerPoint.HasValue;

    public event EventHandler<GeometryEntity>? EntityCompleted;

    public void OnMouseDown(PointF worldPoint, bool snapToGrid, double gridSpacing)
    {
        var point = snapToGrid ? SnapPoint(worldPoint, gridSpacing) : worldPoint;

        switch (_state)
        {
            case State.WaitingForCenterPoint:
                _centerPoint = point;
                _currentPoint = point;
                _state = State.WaitingForFirstPoint;
                break;

            case State.WaitingForFirstPoint:
                _point1 = point;
                _currentPoint = point;
                _state = State.WaitingForSecondPoint;
                break;

            case State.WaitingForSecondPoint:
                _point2 = point;
                _currentPoint = point;
                _state = State.WaitingForArcPosition;
                break;

            case State.WaitingForArcPosition:
                if (_centerPoint.HasValue && _point1.HasValue && _point2.HasValue)
                {
                    // 円弧半径を計算
                    float arcRadius = Distance(_centerPoint.Value, point);
                    var dimension = new AngularDimensionEntity(
                        _centerPoint.Value,
                        _point1.Value,
                        _point2.Value,
                        arcRadius
                    );
                    EntityCompleted?.Invoke(this, dimension);
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
        // AngularDimensionToolではマウスアップは使用しない
    }

    public void DrawPreview(IRenderer renderer)
    {
        if (_centerPoint.HasValue)
        {
            // 中心点マーカー
            renderer.FillCircle(_centerPoint.Value, 3, Color.Red);
        }

        if (_point1.HasValue)
        {
            // 第1点マーカー
            renderer.FillCircle(_point1.Value, 3, Color.Red);

            // 中心から第1点への線
            renderer.DrawLine(_centerPoint!.Value, _point1.Value, Color.LightGray, 1.0f);
        }

        if (_point2.HasValue)
        {
            // 第2点マーカー
            renderer.FillCircle(_point2.Value, 3, Color.Red);

            // 中心から第2点への線
            renderer.DrawLine(_centerPoint!.Value, _point2.Value, Color.LightGray, 1.0f);
        }

        if (_currentPoint.HasValue && _centerPoint.HasValue)
        {
            // 現在の点マーカー
            renderer.FillCircle(_currentPoint.Value, 3, Color.Blue);

            if (_state == State.WaitingForFirstPoint || _state == State.WaitingForSecondPoint)
            {
                // 中心から現在点への仮の線
                renderer.DrawLine(_centerPoint.Value, _currentPoint.Value, Color.Gray, 1.0f);
            }
            else if (_state == State.WaitingForArcPosition && _point1.HasValue && _point2.HasValue)
            {
                // 角度プレビュー
                DrawAnglePreview(renderer, _centerPoint.Value, _point1.Value, _point2.Value, _currentPoint.Value);
            }
        }
    }

    private void DrawAnglePreview(IRenderer renderer, PointF center, PointF p1, PointF p2, PointF arcPos)
    {
        float arcRadius = Distance(center, arcPos);

        double angle1 = Math.Atan2(p1.Y - center.Y, p1.X - center.X);
        double angle2 = Math.Atan2(p2.Y - center.Y, p2.X - center.X);

        float startAngleDeg = (float)(angle1 * 180 / Math.PI);
        float endAngleDeg = (float)(angle2 * 180 / Math.PI);

        float sweepAngle = endAngleDeg - startAngleDeg;
        while (sweepAngle > 180) sweepAngle -= 360;
        while (sweepAngle < -180) sweepAngle += 360;

        // 円弧プレビュー
        renderer.DrawArc(center, arcRadius, startAngleDeg, sweepAngle, Color.Gray, 1.0f);
    }

    public void Cancel()
    {
        Reset();
    }

    private void Reset()
    {
        _state = State.WaitingForCenterPoint;
        _centerPoint = null;
        _point1 = null;
        _point2 = null;
        _currentPoint = null;
    }

    private static float Distance(PointF p1, PointF p2)
    {
        float dx = p2.X - p1.X;
        float dy = p2.Y - p1.Y;
        return (float)Math.Sqrt(dx * dx + dy * dy);
    }

    private PointF SnapPoint(PointF point, double gridSpacing)
    {
        var x = Math.Round(point.X / gridSpacing) * gridSpacing;
        var y = Math.Round(point.Y / gridSpacing) * gridSpacing;
        return new PointF((float)x, (float)y);
    }
}

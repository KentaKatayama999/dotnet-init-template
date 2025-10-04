using System.Drawing;
using SimpleCAD.Core.Entities;
using SimpleCAD.Core.Rendering;

namespace SimpleCAD.Core.Tools;

/// <summary>
/// 半径寸法作成ツール
/// </summary>
public class RadialDimensionTool : IDrawingTool
{
    private enum State
    {
        WaitingForCenterPoint,
        WaitingForRadiusPoint
    }

    private State _state = State.WaitingForCenterPoint;
    private PointF? _centerPoint;
    private PointF? _currentPoint;

    public string Name => "RadialDimension";
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
                _state = State.WaitingForRadiusPoint;
                break;

            case State.WaitingForRadiusPoint:
                if (_centerPoint.HasValue)
                {
                    var dimension = new RadialDimensionEntity(_centerPoint.Value, point);
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
        // RadialDimensionToolではマウスアップは使用しない
    }

    public void DrawPreview(IRenderer renderer)
    {
        if (_centerPoint.HasValue)
        {
            // 中心点マーカー
            renderer.FillCircle(_centerPoint.Value, 3, Color.Red);

            if (_currentPoint.HasValue && _state == State.WaitingForRadiusPoint)
            {
                // 現在の点マーカー
                renderer.FillCircle(_currentPoint.Value, 3, Color.Blue);

                // プレビュー円
                float radius = Distance(_centerPoint.Value, _currentPoint.Value);
                renderer.DrawCircle(_centerPoint.Value, radius, Color.Gray, 1.0f);

                // プレビュー半径線
                renderer.DrawLine(_centerPoint.Value, _currentPoint.Value, Color.Gray, 1.0f);
            }
        }
    }

    public void Cancel()
    {
        Reset();
    }

    private void Reset()
    {
        _state = State.WaitingForCenterPoint;
        _centerPoint = null;
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

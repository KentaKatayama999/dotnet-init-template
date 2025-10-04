using System.Drawing;
using SimpleCAD.Core.Entities;
using SimpleCAD.Core.Rendering;

namespace SimpleCAD.Core.Tools;

/// <summary>
/// 線形寸法作成ツール
/// </summary>
public class LinearDimensionTool : IDrawingTool
{
    private enum State
    {
        WaitingForFirstPoint,
        WaitingForSecondPoint,
        WaitingForOffsetPoint
    }

    private State _state = State.WaitingForFirstPoint;
    private PointF? _point1;
    private PointF? _point2;
    private PointF? _currentPoint;

    public string Name => "LinearDimension";
    public bool IsActive => _state != State.WaitingForFirstPoint || _point1.HasValue;

    public event EventHandler<GeometryEntity>? EntityCompleted;

    public void OnMouseDown(PointF worldPoint, bool snapToGrid, double gridSpacing)
    {
        var point = snapToGrid ? SnapPoint(worldPoint, gridSpacing) : worldPoint;

        switch (_state)
        {
            case State.WaitingForFirstPoint:
                _point1 = point;
                _currentPoint = point;
                _state = State.WaitingForSecondPoint;
                break;

            case State.WaitingForSecondPoint:
                _point2 = point;
                _currentPoint = point;
                _state = State.WaitingForOffsetPoint;
                break;

            case State.WaitingForOffsetPoint:
                if (_point1.HasValue && _point2.HasValue)
                {
                    // オフセット距離を計算
                    float offset = CalculateOffset(_point1.Value, _point2.Value, point);
                    var dimension = new LinearDimensionEntity(_point1.Value, _point2.Value, offset);
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
        // LinearDimensionToolではマウスアップは使用しない
    }

    public void DrawPreview(IRenderer renderer)
    {
        if (_point1.HasValue)
        {
            // 第1点マーカー
            renderer.FillCircle(_point1.Value, 3, Color.Red);
        }

        if (_point2.HasValue)
        {
            // 第2点マーカー
            renderer.FillCircle(_point2.Value, 3, Color.Red);

            // 測定距離のプレビュー線
            renderer.DrawLine(_point1!.Value, _point2.Value, Color.LightGray, 1.0f);
        }

        if (_currentPoint.HasValue && _point1.HasValue)
        {
            // 現在の点マーカー
            renderer.FillCircle(_currentPoint.Value, 3, Color.Blue);

            if (_state == State.WaitingForSecondPoint)
            {
                // 第1点から現在点への仮の線
                renderer.DrawLine(_point1.Value, _currentPoint.Value, Color.Gray, 1.0f);
            }
            else if (_state == State.WaitingForOffsetPoint && _point2.HasValue)
            {
                // 寸法プレビュー
                DrawDimensionPreview(renderer, _point1.Value, _point2.Value, _currentPoint.Value);
            }
        }
    }

    private void DrawDimensionPreview(IRenderer renderer, PointF p1, PointF p2, PointF offsetPoint)
    {
        bool isHorizontal = Math.Abs(p2.X - p1.X) > Math.Abs(p2.Y - p1.Y);

        if (isHorizontal)
        {
            float dimLineY = offsetPoint.Y;

            // 補助線
            renderer.DrawLine(new PointF(p1.X, p1.Y), new PointF(p1.X, dimLineY), Color.Gray, 0.5f);
            renderer.DrawLine(new PointF(p2.X, p2.Y), new PointF(p2.X, dimLineY), Color.Gray, 0.5f);

            // 寸法線
            renderer.DrawLine(new PointF(p1.X, dimLineY), new PointF(p2.X, dimLineY), Color.Gray, 1.0f);
        }
        else
        {
            float dimLineX = offsetPoint.X;

            // 補助線
            renderer.DrawLine(new PointF(p1.X, p1.Y), new PointF(dimLineX, p1.Y), Color.Gray, 0.5f);
            renderer.DrawLine(new PointF(p2.X, p2.Y), new PointF(dimLineX, p2.Y), Color.Gray, 0.5f);

            // 寸法線
            renderer.DrawLine(new PointF(dimLineX, p1.Y), new PointF(dimLineX, p2.Y), Color.Gray, 1.0f);
        }
    }

    private float CalculateOffset(PointF p1, PointF p2, PointF offsetPoint)
    {
        bool isHorizontal = Math.Abs(p2.X - p1.X) > Math.Abs(p2.Y - p1.Y);

        if (isHorizontal)
        {
            // 水平寸法: Y方向のオフセット
            float avgY = (p1.Y + p2.Y) / 2;
            return Math.Abs(offsetPoint.Y - avgY);
        }
        else
        {
            // 垂直寸法: X方向のオフセット
            float avgX = (p1.X + p2.X) / 2;
            return Math.Abs(offsetPoint.X - avgX);
        }
    }

    public void Cancel()
    {
        Reset();
    }

    private void Reset()
    {
        _state = State.WaitingForFirstPoint;
        _point1 = null;
        _point2 = null;
        _currentPoint = null;
    }

    private PointF SnapPoint(PointF point, double gridSpacing)
    {
        var x = Math.Round(point.X / gridSpacing) * gridSpacing;
        var y = Math.Round(point.Y / gridSpacing) * gridSpacing;
        return new PointF((float)x, (float)y);
    }
}

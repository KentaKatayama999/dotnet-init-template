using System.Collections.ObjectModel;
using System.Drawing;
using SimpleCAD.Core.Entities;
using SimpleCAD.Core.Rendering;

namespace SimpleCAD.Core.Tools;

/// <summary>
/// 回転ツール（選択されたエンティティを回転）
/// </summary>
public class RotateTool : IDrawingTool
{
    private enum RotateState
    {
        WaitingForCenter,
        WaitingForBasePoint,
        WaitingForDestinationPoint
    }

    private readonly ObservableCollection<GeometryEntity> _entities;
    private RotateState _state = RotateState.WaitingForCenter;
    private PointF? _center;
    private PointF? _basePoint;
    private PointF? _currentPoint;
    private List<GeometryEntity> _selectedEntities = new();

    public string Name => "Rotate";
    public bool IsActive => _state != RotateState.WaitingForCenter || _center.HasValue;

    public event EventHandler<GeometryEntity>? EntityCompleted;

    public RotateTool(ObservableCollection<GeometryEntity> entities)
    {
        _entities = entities;
    }

    public void OnMouseDown(PointF worldPoint, bool snapToGrid, double gridSpacing)
    {
        var point = snapToGrid ? SnapPoint(worldPoint, gridSpacing) : worldPoint;

        switch (_state)
        {
            case RotateState.WaitingForCenter:
                // 選択されたエンティティを取得
                _selectedEntities = _entities.Where(e => e.IsSelected).ToList();
                if (_selectedEntities.Count == 0)
                {
                    // 選択されたエンティティがない場合は何もしない
                    return;
                }

                _center = point;
                _state = RotateState.WaitingForBasePoint;
                break;

            case RotateState.WaitingForBasePoint:
                _basePoint = point;
                _currentPoint = point;
                _state = RotateState.WaitingForDestinationPoint;
                break;

            case RotateState.WaitingForDestinationPoint:
                if (_center.HasValue && _basePoint.HasValue)
                {
                    // 回転角度を計算
                    double baseAngle = Math.Atan2(
                        _basePoint.Value.Y - _center.Value.Y,
                        _basePoint.Value.X - _center.Value.X);
                    double targetAngle = Math.Atan2(
                        point.Y - _center.Value.Y,
                        point.X - _center.Value.X);
                    double rotationAngle = targetAngle - baseAngle;

                    // 回転を実行
                    foreach (var entity in _selectedEntities)
                    {
                        entity.Rotate(_center.Value, rotationAngle);
                    }
                }

                Reset();
                break;
        }
    }

    public void OnMouseMove(PointF worldPoint, bool snapToGrid, double gridSpacing)
    {
        if (_state == RotateState.WaitingForBasePoint || _state == RotateState.WaitingForDestinationPoint)
        {
            _currentPoint = snapToGrid ? SnapPoint(worldPoint, gridSpacing) : worldPoint;
        }
    }

    public void OnMouseUp(PointF worldPoint, bool snapToGrid, double gridSpacing)
    {
        // 回転ツールではマウスアップは使用しない
    }

    public void DrawPreview(IRenderer renderer)
    {
        if (!_center.HasValue)
            return;

        // 中心点マーカー
        renderer.FillCircle(_center.Value, 4, Color.Red);

        if (_state == RotateState.WaitingForBasePoint && _currentPoint.HasValue)
        {
            // 中心から現在点までの線
            renderer.DrawLine(_center.Value, _currentPoint.Value, Color.Gray, 1.0f);
            renderer.FillCircle(_currentPoint.Value, 3, Color.Blue);
        }
        else if (_state == RotateState.WaitingForDestinationPoint && _basePoint.HasValue && _currentPoint.HasValue)
        {
            // 基準線
            renderer.DrawLine(_center.Value, _basePoint.Value, Color.Gray, 1.0f);
            renderer.FillCircle(_basePoint.Value, 3, Color.Green);

            // 現在線
            renderer.DrawLine(_center.Value, _currentPoint.Value, Color.Gray, 1.0f);
            renderer.FillCircle(_currentPoint.Value, 3, Color.Blue);

            // 回転角度を表示する円弧（簡略化のため省略）
        }
    }

    public void Cancel()
    {
        Reset();
    }

    private void Reset()
    {
        _center = null;
        _basePoint = null;
        _currentPoint = null;
        _selectedEntities.Clear();
        _state = RotateState.WaitingForCenter;
    }

    private PointF SnapPoint(PointF point, double gridSpacing)
    {
        var x = Math.Round(point.X / gridSpacing) * gridSpacing;
        var y = Math.Round(point.Y / gridSpacing) * gridSpacing;
        return new PointF((float)x, (float)y);
    }
}

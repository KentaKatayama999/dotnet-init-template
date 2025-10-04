using System.Collections.ObjectModel;
using System.Drawing;
using SimpleCAD.Core.Entities;
using SimpleCAD.Core.Rendering;

namespace SimpleCAD.Core.Tools;

/// <summary>
/// 拡大縮小ツール（選択されたエンティティをスケール）
/// </summary>
public class ScaleTool : IDrawingTool
{
    private enum ScaleState
    {
        WaitingForBasePoint,
        WaitingForReferencePoint,
        WaitingForTargetPoint
    }

    private readonly ObservableCollection<GeometryEntity> _entities;
    private ScaleState _state = ScaleState.WaitingForBasePoint;
    private PointF? _basePoint;
    private PointF? _referencePoint;
    private PointF? _currentPoint;
    private List<GeometryEntity> _selectedEntities = new();

    public string Name => "Scale";
    public bool IsActive => _state != ScaleState.WaitingForBasePoint || _basePoint.HasValue;

    public event EventHandler<GeometryEntity>? EntityCompleted;

    public ScaleTool(ObservableCollection<GeometryEntity> entities)
    {
        _entities = entities;
    }

    public void OnMouseDown(PointF worldPoint, bool snapToGrid, double gridSpacing)
    {
        var point = snapToGrid ? SnapPoint(worldPoint, gridSpacing) : worldPoint;

        switch (_state)
        {
            case ScaleState.WaitingForBasePoint:
                // 選択されたエンティティを取得
                _selectedEntities = _entities.Where(e => e.IsSelected).ToList();
                if (_selectedEntities.Count == 0)
                {
                    // 選択されたエンティティがない場合は何もしない
                    return;
                }

                _basePoint = point;
                _state = ScaleState.WaitingForReferencePoint;
                break;

            case ScaleState.WaitingForReferencePoint:
                _referencePoint = point;
                _currentPoint = point;
                _state = ScaleState.WaitingForTargetPoint;
                break;

            case ScaleState.WaitingForTargetPoint:
                if (_basePoint.HasValue && _referencePoint.HasValue)
                {
                    // スケール比率を計算
                    float referenceDistance = Distance(_basePoint.Value, _referencePoint.Value);
                    float targetDistance = Distance(_basePoint.Value, point);

                    if (referenceDistance > 0.001f)
                    {
                        double scale = targetDistance / referenceDistance;

                        // スケールを実行（等倍スケール）
                        foreach (var entity in _selectedEntities)
                        {
                            entity.Scale(_basePoint.Value, scale, scale);
                        }
                    }
                }

                Reset();
                break;
        }
    }

    public void OnMouseMove(PointF worldPoint, bool snapToGrid, double gridSpacing)
    {
        if (_state == ScaleState.WaitingForReferencePoint || _state == ScaleState.WaitingForTargetPoint)
        {
            _currentPoint = snapToGrid ? SnapPoint(worldPoint, gridSpacing) : worldPoint;
        }
    }

    public void OnMouseUp(PointF worldPoint, bool snapToGrid, double gridSpacing)
    {
        // スケールツールではマウスアップは使用しない
    }

    public void DrawPreview(IRenderer renderer)
    {
        if (!_basePoint.HasValue)
            return;

        // 基準点マーカー
        renderer.FillCircle(_basePoint.Value, 4, Color.Red);

        if (_state == ScaleState.WaitingForReferencePoint && _currentPoint.HasValue)
        {
            // 基準点から現在点までの線
            renderer.DrawLine(_basePoint.Value, _currentPoint.Value, Color.Gray, 1.0f);
            renderer.FillCircle(_currentPoint.Value, 3, Color.Blue);
        }
        else if (_state == ScaleState.WaitingForTargetPoint && _referencePoint.HasValue && _currentPoint.HasValue)
        {
            // 基準距離の線
            renderer.DrawLine(_basePoint.Value, _referencePoint.Value, Color.Gray, 1.0f);
            renderer.FillCircle(_referencePoint.Value, 3, Color.Green);

            // 目標距離の線
            renderer.DrawLine(_basePoint.Value, _currentPoint.Value, Color.Blue, 1.0f);
            renderer.FillCircle(_currentPoint.Value, 3, Color.Blue);

            // スケール比率を表示（簡略化のため省略）
        }
    }

    public void Cancel()
    {
        Reset();
    }

    private void Reset()
    {
        _basePoint = null;
        _referencePoint = null;
        _currentPoint = null;
        _selectedEntities.Clear();
        _state = ScaleState.WaitingForBasePoint;
    }

    private float Distance(PointF p1, PointF p2)
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

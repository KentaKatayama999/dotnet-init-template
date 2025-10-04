using System.Collections.ObjectModel;
using System.Drawing;
using SimpleCAD.Core.Entities;
using SimpleCAD.Core.Rendering;

namespace SimpleCAD.Core.Tools;

/// <summary>
/// 移動ツール（選択されたエンティティを移動）
/// </summary>
public class MoveTool : IDrawingTool
{
    private enum MoveState
    {
        WaitingForBasePoint,
        WaitingForDestinationPoint
    }

    private readonly ObservableCollection<GeometryEntity> _entities;
    private MoveState _state = MoveState.WaitingForBasePoint;
    private PointF? _basePoint;
    private PointF? _currentPoint;
    private List<GeometryEntity> _selectedEntities = new();
    private bool _isMovingDimensionText = false;
    private DimensionEntity? _dimensionBeingMoved = null;

    public string Name => "Move";
    public bool IsActive => _state != MoveState.WaitingForBasePoint || _basePoint.HasValue;

    public event EventHandler<GeometryEntity>? EntityCompleted;

    public MoveTool(ObservableCollection<GeometryEntity> entities)
    {
        _entities = entities;
    }

    public void OnMouseDown(PointF worldPoint, bool snapToGrid, double gridSpacing)
    {
        var point = snapToGrid ? SnapPoint(worldPoint, gridSpacing) : worldPoint;

        switch (_state)
        {
            case MoveState.WaitingForBasePoint:
                // 選択されたエンティティを取得
                _selectedEntities = _entities.Where(e => e.IsSelected).ToList();
                if (_selectedEntities.Count == 0)
                {
                    // 選択されたエンティティがない場合は何もしない
                    return;
                }

                // 寸法エンティティのテキスト部分をクリックしたかチェック
                _isMovingDimensionText = false;
                _dimensionBeingMoved = null;

                if (_selectedEntities.Count == 1 && _selectedEntities[0] is DimensionEntity dimEntity)
                {
                    if (dimEntity.HitTestText(point, 10.0f))
                    {
                        _isMovingDimensionText = true;
                        _dimensionBeingMoved = dimEntity;
                    }
                }

                _basePoint = point;
                _currentPoint = point;
                _state = MoveState.WaitingForDestinationPoint;
                break;

            case MoveState.WaitingForDestinationPoint:
                if (_basePoint.HasValue)
                {
                    if (_isMovingDimensionText && _dimensionBeingMoved != null)
                    {
                        // 寸法テキストのみを移動
                        _dimensionBeingMoved.MoveTextTo(point);
                    }
                    else
                    {
                        // エンティティ全体を移動
                        float dx = point.X - _basePoint.Value.X;
                        float dy = point.Y - _basePoint.Value.Y;

                        foreach (var entity in _selectedEntities)
                        {
                            entity.Translate(dx, dy);
                        }
                    }
                }

                Reset();
                break;
        }
    }

    public void OnMouseMove(PointF worldPoint, bool snapToGrid, double gridSpacing)
    {
        if (_state == MoveState.WaitingForDestinationPoint)
        {
            _currentPoint = snapToGrid ? SnapPoint(worldPoint, gridSpacing) : worldPoint;
        }
    }

    public void OnMouseUp(PointF worldPoint, bool snapToGrid, double gridSpacing)
    {
        // 移動ツールではマウスアップは使用しない
    }

    public void DrawPreview(IRenderer renderer)
    {
        if (_state == MoveState.WaitingForDestinationPoint && _basePoint.HasValue && _currentPoint.HasValue)
        {
            float dx = _currentPoint.Value.X - _basePoint.Value.X;
            float dy = _currentPoint.Value.Y - _basePoint.Value.Y;

            // プレビューとして、選択されたエンティティを半透明で表示
            // （簡略化のため、ここでは基準点と現在点を線で結ぶだけ）
            renderer.DrawLine(_basePoint.Value, _currentPoint.Value, Color.Gray, 1.0f);

            // 基準点マーカー
            renderer.FillCircle(_basePoint.Value, 3, Color.Red);
            renderer.FillCircle(_currentPoint.Value, 3, Color.Blue);
        }
    }

    public void Cancel()
    {
        Reset();
    }

    private void Reset()
    {
        _basePoint = null;
        _currentPoint = null;
        _selectedEntities.Clear();
        _isMovingDimensionText = false;
        _dimensionBeingMoved = null;
        _state = MoveState.WaitingForBasePoint;
    }

    private PointF SnapPoint(PointF point, double gridSpacing)
    {
        var x = Math.Round(point.X / gridSpacing) * gridSpacing;
        var y = Math.Round(point.Y / gridSpacing) * gridSpacing;
        return new PointF((float)x, (float)y);
    }
}

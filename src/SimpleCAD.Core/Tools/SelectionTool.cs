using System.Collections.ObjectModel;
using System.Drawing;
using SimpleCAD.Core.Entities;
using SimpleCAD.Core.Rendering;

namespace SimpleCAD.Core.Tools;

/// <summary>
/// 選択ツール（クリックでエンティティを選択）
/// </summary>
public class SelectionTool : IDrawingTool
{
    private readonly ObservableCollection<GeometryEntity> _entities;

    public string Name => "Selection";
    public bool IsActive { get; private set; }

    public event EventHandler<GeometryEntity>? EntityCompleted;

    public SelectionTool(ObservableCollection<GeometryEntity> entities)
    {
        _entities = entities;
    }

    public void OnMouseDown(PointF worldPoint, bool snapToGrid, double gridSpacing)
    {
        IsActive = true;

        // クリック位置に最も近いエンティティを探す
        GeometryEntity? hitEntity = null;
        float minDistance = float.MaxValue;

        foreach (var entity in _entities)
        {
            if (entity.HitTest(worldPoint, 10.0f))
            {
                // エンティティの中心からの距離を計算
                var bounds = entity.GetBounds();
                var centerX = bounds.X + bounds.Width / 2;
                var centerY = bounds.Y + bounds.Height / 2;
                float dx = worldPoint.X - centerX;
                float dy = worldPoint.Y - centerY;
                float distance = (float)Math.Sqrt(dx * dx + dy * dy);

                if (distance < minDistance)
                {
                    minDistance = distance;
                    hitEntity = entity;
                }
            }
        }

        if (hitEntity != null)
        {
            // Ctrlキーが押されていない場合は、他のすべての選択を解除
            // （簡略化のため、ここではCtrlチェックはスキップし、常に単一選択）
            foreach (var entity in _entities)
            {
                if (entity != hitEntity)
                    entity.IsSelected = false;
            }

            // 選択状態をトグル
            hitEntity.IsSelected = !hitEntity.IsSelected;
        }
        else
        {
            // 何もヒットしなかった場合、すべての選択を解除
            foreach (var entity in _entities)
            {
                entity.IsSelected = false;
            }
        }
    }

    public void OnMouseMove(PointF worldPoint, bool snapToGrid, double gridSpacing)
    {
        // 選択ツールではマウス移動は使用しない
    }

    public void OnMouseUp(PointF worldPoint, bool snapToGrid, double gridSpacing)
    {
        // 選択ツールではマウスアップは使用しない
    }

    public void DrawPreview(IRenderer renderer)
    {
        // 選択ツールではプレビューは不要
    }

    public void Cancel()
    {
        IsActive = false;
    }
}

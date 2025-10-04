using System.Drawing;
using SimpleCAD.Core.Entities;

namespace SimpleCAD.Core.Services;

/// <summary>
/// エンティティ選択管理サービス
/// </summary>
public class SelectionService
{
    /// <summary>
    /// ヒット判定の半径（ピクセル）
    /// </summary>
    public float HitTestRadius { get; set; } = 8.0f;

    /// <summary>
    /// 点がエンティティに近いかどうかを判定
    /// </summary>
    public bool HitTest(GeometryEntity entity, PointF point)
    {
        if (entity == null || !entity.IsVisible)
            return false;

        var bounds = entity.GetBounds();

        // バウンディングボックスに余裕を持たせてチェック
        var expandedBounds = new RectangleF(
            bounds.X - HitTestRadius,
            bounds.Y - HitTestRadius,
            bounds.Width + HitTestRadius * 2,
            bounds.Height + HitTestRadius * 2
        );

        return expandedBounds.Contains(point);
    }

    /// <summary>
    /// 指定座標に最も近いエンティティを検索
    /// </summary>
    public GeometryEntity? FindEntityAt(IEnumerable<GeometryEntity> entities, PointF point)
    {
        GeometryEntity? nearest = null;
        float minDistance = float.MaxValue;

        foreach (var entity in entities)
        {
            if (!HitTest(entity, point))
                continue;

            var bounds = entity.GetBounds();
            var centerX = bounds.X + bounds.Width / 2;
            var centerY = bounds.Y + bounds.Height / 2;

            var dx = point.X - centerX;
            var dy = point.Y - centerY;
            var distance = (float)Math.Sqrt(dx * dx + dy * dy);

            if (distance < minDistance)
            {
                minDistance = distance;
                nearest = entity;
            }
        }

        return nearest;
    }

    /// <summary>
    /// 矩形範囲内のエンティティを取得
    /// </summary>
    public IEnumerable<GeometryEntity> GetEntitiesInRectangle(
        IEnumerable<GeometryEntity> entities,
        RectangleF selectionRect)
    {
        var result = new List<GeometryEntity>();

        foreach (var entity in entities)
        {
            if (!entity.IsVisible)
                continue;

            var bounds = entity.GetBounds();

            if (selectionRect.IntersectsWith(bounds))
            {
                result.Add(entity);
            }
        }

        return result;
    }

    /// <summary>
    /// エンティティを選択
    /// </summary>
    public void SelectEntity(GeometryEntity entity, bool multiSelect = false)
    {
        if (!multiSelect)
        {
            // 単一選択の場合、他の選択を解除
            entity.IsSelected = true;
        }
        else
        {
            // 複数選択の場合、トグル
            entity.IsSelected = !entity.IsSelected;
        }
    }

    /// <summary>
    /// すべての選択を解除
    /// </summary>
    public void ClearSelection(IEnumerable<GeometryEntity> entities)
    {
        foreach (var entity in entities)
        {
            entity.IsSelected = false;
        }
    }
}

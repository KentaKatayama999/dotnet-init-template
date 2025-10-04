using System.Drawing;
using SimpleCAD.Core.Entities;
using SimpleCAD.Core.Rendering;

namespace SimpleCAD.Core.Tools;

/// <summary>
/// 描画ツールのインターフェース
/// </summary>
public interface IDrawingTool
{
    /// <summary>
    /// ツール名
    /// </summary>
    string Name { get; }

    /// <summary>
    /// ツールがアクティブかどうか
    /// </summary>
    bool IsActive { get; }

    /// <summary>
    /// マウスダウン時の処理
    /// </summary>
    void OnMouseDown(PointF worldPoint, bool snapToGrid, double gridSpacing);

    /// <summary>
    /// マウス移動時の処理
    /// </summary>
    void OnMouseMove(PointF worldPoint, bool snapToGrid, double gridSpacing);

    /// <summary>
    /// マウスアップ時の処理
    /// </summary>
    void OnMouseUp(PointF worldPoint, bool snapToGrid, double gridSpacing);

    /// <summary>
    /// プレビュー描画
    /// </summary>
    void DrawPreview(IRenderer renderer);

    /// <summary>
    /// ツールをキャンセル
    /// </summary>
    void Cancel();

    /// <summary>
    /// エンティティが完成した時のイベント
    /// </summary>
    event EventHandler<GeometryEntity>? EntityCompleted;
}

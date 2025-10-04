using System.Collections.ObjectModel;
using SimpleCAD.Core.Entities;

namespace SimpleCAD.Core.ViewModels;

/// <summary>
/// CADキャンバスの共通ViewModel
/// </summary>
public class CADCanvasViewModel
{
    /// <summary>
    /// エンティティコレクション
    /// </summary>
    public ObservableCollection<GeometryEntity> Entities { get; }

    /// <summary>
    /// グリッド表示
    /// </summary>
    public bool GridVisible { get; set; } = true;

    /// <summary>
    /// グリッドスナップ
    /// </summary>
    public bool SnapToGrid { get; set; } = true;

    /// <summary>
    /// グリッド間隔
    /// </summary>
    public double GridSpacing { get; set; } = 10.0;

    /// <summary>
    /// ズームレベル
    /// </summary>
    public double ZoomLevel { get; set; } = 1.0;

    /// <summary>
    /// ビュー中心座標（X）
    /// </summary>
    public double ViewCenterX { get; set; } = 0.0;

    /// <summary>
    /// ビュー中心座標（Y）
    /// </summary>
    public double ViewCenterY { get; set; } = 0.0;

    /// <summary>
    /// コントロールポイント表示
    /// </summary>
    public bool ShowControlPoints { get; set; } = true;

    /// <summary>
    /// 通過点表示
    /// </summary>
    public bool ShowPassPoints { get; set; } = true;

    /// <summary>
    /// 初期状態表示
    /// </summary>
    public bool ShowInitialState { get; set; } = false;

    /// <summary>
    /// 読み取り専用モード
    /// </summary>
    public bool IsReadOnly { get; set; } = false;

    public CADCanvasViewModel()
    {
        Entities = new ObservableCollection<GeometryEntity>();
    }

    /// <summary>
    /// エンティティを追加
    /// </summary>
    public void AddEntity(GeometryEntity entity)
    {
        Entities.Add(entity);
    }

    /// <summary>
    /// エンティティを削除
    /// </summary>
    public void RemoveEntity(GeometryEntity entity)
    {
        Entities.Remove(entity);
    }

    /// <summary>
    /// すべてのエンティティをクリア
    /// </summary>
    public void Clear()
    {
        Entities.Clear();
    }

    /// <summary>
    /// 選択されたエンティティを取得
    /// </summary>
    public IEnumerable<GeometryEntity> GetSelectedEntities()
    {
        return Entities.Where(e => e.IsSelected);
    }

    /// <summary>
    /// すべての選択を解除
    /// </summary>
    public void ClearSelection()
    {
        foreach (var entity in Entities)
        {
            entity.IsSelected = false;
        }
    }
}

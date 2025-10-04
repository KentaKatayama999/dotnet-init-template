using System.Drawing;
using SimpleCAD.Core.Rendering;

namespace SimpleCAD.Core.Entities;

/// <summary>
/// すべての幾何エンティティの抽象基底クラス
/// </summary>
public abstract class GeometryEntity
{
    /// <summary>
    /// 一意のID
    /// </summary>
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>
    /// エンティティの名前
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// 描画色
    /// </summary>
    public Color Color { get; set; } = Color.Black;

    /// <summary>
    /// 線の太さ
    /// </summary>
    public double Thickness { get; set; } = 1.0;

    /// <summary>
    /// 表示/非表示
    /// </summary>
    public bool IsVisible { get; set; } = true;

    /// <summary>
    /// 選択状態
    /// </summary>
    public bool IsSelected { get; set; } = false;

    /// <summary>
    /// エンティティを描画
    /// </summary>
    /// <param name="renderer">描画用レンダラー</param>
    public abstract void Draw(IRenderer renderer);

    /// <summary>
    /// エンティティのバウンディングボックスを取得
    /// </summary>
    /// <returns>バウンディングボックス</returns>
    public abstract RectangleF GetBounds();

    /// <summary>
    /// 指定した点がエンティティに含まれるかをテスト
    /// </summary>
    /// <param name="point">テストする点</param>
    /// <param name="tolerance">許容誤差（ピクセル単位）</param>
    /// <returns>含まれる場合はtrue</returns>
    public abstract bool HitTest(PointF point, float tolerance = 5.0f);

    /// <summary>
    /// エンティティを移動
    /// </summary>
    public abstract void Translate(float dx, float dy);

    /// <summary>
    /// エンティティを回転
    /// </summary>
    public abstract void Rotate(PointF center, double angleRadians);

    /// <summary>
    /// エンティティを拡大縮小
    /// </summary>
    public abstract void Scale(PointF center, double scaleX, double scaleY);
}

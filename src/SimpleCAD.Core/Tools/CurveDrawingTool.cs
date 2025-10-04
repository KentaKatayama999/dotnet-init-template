using System.Drawing;
using Geometry;
using SimpleCAD.Core.Entities;
using SimpleCAD.Core.Rendering;

namespace SimpleCAD.Core.Tools;

/// <summary>
/// NURBS曲線描画ツール（複数点を指定）
/// </summary>
public class CurveDrawingTool : IDrawingTool
{
    private readonly List<PointF> _points = new();
    private PointF? _currentPoint;
    private bool _isDrawing;

    public string Name => "Curve";
    public bool IsActive => _isDrawing;

    public event EventHandler<GeometryEntity>? EntityCompleted;

    public void OnMouseDown(PointF worldPoint, bool snapToGrid, double gridSpacing)
    {
        var point = snapToGrid ? SnapPoint(worldPoint, gridSpacing) : worldPoint;

        _points.Add(point);
        _currentPoint = point;
        _isDrawing = true;
    }

    public void OnMouseMove(PointF worldPoint, bool snapToGrid, double gridSpacing)
    {
        if (_isDrawing)
        {
            _currentPoint = snapToGrid ? SnapPoint(worldPoint, gridSpacing) : worldPoint;
        }
    }

    public void OnMouseUp(PointF worldPoint, bool snapToGrid, double gridSpacing)
    {
        // NURBS曲線ツールではマウスアップは使用しない
    }

    public void DrawPreview(IRenderer renderer)
    {
        if (!_isDrawing)
            return;

        // 既存の点をマーカー表示
        for (int i = 0; i < _points.Count; i++)
        {
            var color = i == 0 ? Color.Red : (i == _points.Count - 1 ? Color.Green : Color.Orange);
            renderer.FillCircle(_points[i], 3, color);
        }

        // 点を結ぶポリラインを描画（仮のプレビュー）
        if (_points.Count > 0 && _currentPoint.HasValue)
        {
            // 前の点から現在点までの線
            renderer.DrawLine(_points[_points.Count - 1], _currentPoint.Value, Color.Gray, 1.0f);

            // 現在点マーカー
            renderer.FillCircle(_currentPoint.Value, 3, Color.Blue);
        }

        // 曲線が描画可能な場合はプレビュー表示
        if (_points.Count >= 3)
        {
            try
            {
                var curve = CreateCurve(_points);
                curve.Draw(renderer);
            }
            catch
            {
                // 曲線生成に失敗した場合は無視
            }
        }

        // ポリライン表示（仮）
        if (_points.Count > 1)
        {
            for (int i = 0; i < _points.Count - 1; i++)
            {
                renderer.DrawLine(_points[i], _points[i + 1], Color.LightGray, 0.5f);
            }
        }
    }

    public void Cancel()
    {
        _points.Clear();
        _currentPoint = null;
        _isDrawing = false;
    }

    /// <summary>
    /// 曲線を完成させる（外部から呼び出し）
    /// </summary>
    public void Complete()
    {
        if (_points.Count >= 2)
        {
            var curve = CreateCurve(_points);
            EntityCompleted?.Invoke(this, curve);
        }

        _points.Clear();
        _currentPoint = null;
        _isDrawing = false;
    }

    /// <summary>
    /// 最後の点を削除（外部から呼び出し）
    /// </summary>
    public void Undo()
    {
        if (_points.Count > 0)
        {
            _points.RemoveAt(_points.Count - 1);

            if (_points.Count == 0)
            {
                _isDrawing = false;
            }
        }
    }

    private CurveEntity CreateCurve(List<PointF> points)
    {
        var passPoints = new ObjPoints();
        foreach (var p in points)
        {
            passPoints.AddPoint(p.X, p.Y);
        }

        var curve = new ObjCurve();
        curve.SetCurve(passPoints);

        return new CurveEntity(curve)
        {
            Color = Color.Black,
            Thickness = 1.5
        };
    }

    private PointF SnapPoint(PointF point, double gridSpacing)
    {
        var x = Math.Round(point.X / gridSpacing) * gridSpacing;
        var y = Math.Round(point.Y / gridSpacing) * gridSpacing;
        return new PointF((float)x, (float)y);
    }
}

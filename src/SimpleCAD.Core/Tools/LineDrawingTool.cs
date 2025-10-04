using System.Drawing;
using Geometry;
using SimpleCAD.Core.Entities;
using SimpleCAD.Core.Rendering;

namespace SimpleCAD.Core.Tools;

/// <summary>
/// 直線描画ツール
/// </summary>
public class LineDrawingTool : IDrawingTool
{
    private PointF? _startPoint;
    private PointF? _currentPoint;
    private bool _isDrawing;

    public string Name => "Line";
    public bool IsActive => _isDrawing;

    public event EventHandler<GeometryEntity>? EntityCompleted;

    public void OnMouseDown(PointF worldPoint, bool snapToGrid, double gridSpacing)
    {
        var point = snapToGrid ? SnapPoint(worldPoint, gridSpacing) : worldPoint;

        if (!_isDrawing)
        {
            // 始点を設定
            _startPoint = point;
            _currentPoint = point;
            _isDrawing = true;
        }
        else
        {
            // 終点を設定して直線を完成
            if (_startPoint.HasValue)
            {
                var line = CreateLine(_startPoint.Value, point);
                EntityCompleted?.Invoke(this, line);
            }

            // リセット
            _startPoint = null;
            _currentPoint = null;
            _isDrawing = false;
        }
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
        // 直線ツールではマウスアップは使用しない
    }

    public void DrawPreview(IRenderer renderer)
    {
        if (_isDrawing && _startPoint.HasValue && _currentPoint.HasValue)
        {
            // プレビュー線を描画（点線で表示）
            renderer.DrawLine(_startPoint.Value, _currentPoint.Value, Color.Gray, 1.0f);

            // 始点と現在点にマーカーを表示
            renderer.FillCircle(_startPoint.Value, 3, Color.Red);
            renderer.FillCircle(_currentPoint.Value, 3, Color.Blue);
        }
    }

    public void Cancel()
    {
        _startPoint = null;
        _currentPoint = null;
        _isDrawing = false;
    }

    private LineEntity CreateLine(PointF start, PointF end)
    {
        var startPoints = new ObjPoints(start.X, start.Y);
        var endPoints = new ObjPoints(end.X, end.Y);
        var objLine = new ObjLine(startPoints, endPoints);

        return new LineEntity(objLine)
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

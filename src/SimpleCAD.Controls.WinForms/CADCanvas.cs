using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using SimpleCAD.Controls.WinForms.Rendering;
using SimpleCAD.Core.ViewModels;
using SimpleCAD.Core.Services;
using SimpleCAD.Core.Tools;

namespace SimpleCAD.Controls.WinForms;

/// <summary>
/// CADキャンバス WinForms UserControl
/// </summary>
public class CADCanvas : UserControl
{
    private readonly CADCanvasViewModel _viewModel;
    private readonly GridRenderer _gridRenderer;
    private readonly SelectionService _selectionService;
    private readonly SnapService _snapService;
    private readonly DrawingToolManager _toolManager;

    private Point _lastMousePosition;
    private bool _isPanning;
    private Point _panStartPosition;
    private double _panStartCenterX;
    private double _panStartCenterY;

    public CADCanvas()
    {
        _viewModel = new CADCanvasViewModel();
        _gridRenderer = new GridRenderer();
        _selectionService = new SelectionService();
        _snapService = new SnapService();
        _toolManager = new DrawingToolManager();

        // 編集ツールを初期化
        _toolManager.InitializeEditingTools(_viewModel.Entities);

        // 寸法ツールを初期化
        _toolManager.InitializeDimensionTools();

        // ダブルバッファリングを有効化
        DoubleBuffered = true;
        ResizeRedraw = true;

        // ViewModelの変更を監視
        _viewModel.Entities.CollectionChanged += (s, e) => Invalidate();

        // ツールマネージャーのイベント
        _toolManager.EntityCompleted += (s, e) =>
        {
            _viewModel.AddEntity(e);
            Invalidate();
        };
    }

    #region Properties

    /// <summary>
    /// ViewModel
    /// </summary>
    public CADCanvasViewModel ViewModel => _viewModel;

    /// <summary>
    /// 描画ツールマネージャー
    /// </summary>
    public DrawingToolManager ToolManager => _toolManager;

    /// <summary>
    /// グリッド表示
    /// </summary>
    [Browsable(true)]
    [DefaultValue(true)]
    [Category("Appearance")]
    [Description("グリッドを表示するかどうか")]
    public bool GridVisible
    {
        get => _viewModel.GridVisible;
        set
        {
            _viewModel.GridVisible = value;
            Invalidate();
        }
    }

    /// <summary>
    /// グリッドスナップ
    /// </summary>
    [Browsable(true)]
    [DefaultValue(true)]
    [Category("Behavior")]
    [Description("グリッドにスナップするかどうか")]
    public bool SnapToGrid
    {
        get => _viewModel.SnapToGrid;
        set => _viewModel.SnapToGrid = value;
    }

    /// <summary>
    /// グリッド間隔
    /// </summary>
    [Browsable(true)]
    [DefaultValue(10.0)]
    [Category("Appearance")]
    [Description("グリッドの間隔")]
    public double GridSpacing
    {
        get => _viewModel.GridSpacing;
        set
        {
            _viewModel.GridSpacing = value;
            Invalidate();
        }
    }

    /// <summary>
    /// ズームレベル
    /// </summary>
    [Browsable(true)]
    [DefaultValue(1.0)]
    [Category("Appearance")]
    [Description("ズームレベル")]
    public double ZoomLevel
    {
        get => _viewModel.ZoomLevel;
        set
        {
            _viewModel.ZoomLevel = Math.Max(0.1, Math.Min(100, value));
            Invalidate();
        }
    }

    #endregion

    #region Rendering

    protected override void OnPaint(PaintEventArgs e)
    {
        base.OnPaint(e);

        var g = e.Graphics;
        g.SmoothingMode = SmoothingMode.AntiAlias;

        // 背景を白で塗りつぶし
        g.Clear(Color.White);

        // グリッド描画
        if (_viewModel.GridVisible)
        {
            _gridRenderer.DrawGrid(
                g,
                Width,
                Height,
                _viewModel.GridSpacing,
                _viewModel.ViewCenterX,
                _viewModel.ViewCenterY,
                _viewModel.ZoomLevel);
        }

        // 座標変換を適用
        var state = g.Save();
        g.TranslateTransform(Width / 2, Height / 2);
        g.ScaleTransform((float)_viewModel.ZoomLevel, (float)_viewModel.ZoomLevel);
        g.TranslateTransform((float)_viewModel.ViewCenterX, (float)_viewModel.ViewCenterY);

        // エンティティ描画
        var renderer = new WinFormsRenderer(g);
        foreach (var entity in _viewModel.Entities)
        {
            entity.Draw(renderer);
        }

        // ツールプレビュー描画
        _toolManager.DrawPreview(renderer);

        g.Restore(state);

        // 座標表示（右下）
        DrawCoordinateDisplay(g);
    }

    private void DrawCoordinateDisplay(Graphics g)
    {
        var worldPos = ScreenToWorld(_lastMousePosition);
        var text = $"X: {worldPos.X:F2}, Y: {worldPos.Y:F2}  Zoom: {_viewModel.ZoomLevel:F2}x";

        using var font = new Font("Consolas", 10);
        var textSize = g.MeasureString(text, font);

        var bgRect = new RectangleF(
            Width - textSize.Width - 10,
            Height - textSize.Height - 5,
            textSize.Width + 10,
            textSize.Height + 5
        );

        using (var bgBrush = new SolidBrush(Color.FromArgb(200, 255, 255, 255)))
        {
            g.FillRectangle(bgBrush, bgRect);
        }

        using (var textBrush = new SolidBrush(Color.Black))
        {
            g.DrawString(text, font, textBrush, bgRect.Left + 5, bgRect.Top);
        }
    }

    #endregion

    #region Mouse Events

    protected override void OnMouseMove(MouseEventArgs e)
    {
        base.OnMouseMove(e);

        _lastMousePosition = e.Location;

        if (_isPanning && e.Button == MouseButtons.Right)
        {
            var delta = new Point(
                e.Location.X - _panStartPosition.X,
                e.Location.Y - _panStartPosition.Y
            );

            _viewModel.ViewCenterX = _panStartCenterX + delta.X / _viewModel.ZoomLevel;
            _viewModel.ViewCenterY = _panStartCenterY + delta.Y / _viewModel.ZoomLevel;
            Invalidate();
        }
        else
        {
            // ツールがアクティブな場合、ツールマネージャーに通知
            if (_toolManager.ActiveTool != null)
            {
                var worldPos = ScreenToWorld(e.Location);
                _toolManager.OnMouseMove(worldPos, _viewModel.SnapToGrid, _viewModel.GridSpacing);
            }

            Invalidate();
        }
    }

    protected override void OnMouseWheel(MouseEventArgs e)
    {
        base.OnMouseWheel(e);

        // ズーム
        var mousePos = e.Location;
        var worldPosBefore = ScreenToWorld(mousePos);

        var delta = e.Delta > 0 ? 1.1 : 0.9;
        ZoomLevel *= delta;

        var worldPosAfter = ScreenToWorld(mousePos);
        _viewModel.ViewCenterX += worldPosBefore.X - worldPosAfter.X;
        _viewModel.ViewCenterY += worldPosBefore.Y - worldPosAfter.Y;

        Invalidate();
    }

    protected override void OnMouseDown(MouseEventArgs e)
    {
        base.OnMouseDown(e);

        if (e.Button == MouseButtons.Right)
        {
            // パン開始
            _isPanning = true;
            _panStartPosition = e.Location;
            _panStartCenterX = _viewModel.ViewCenterX;
            _panStartCenterY = _viewModel.ViewCenterY;
            Capture = true;
        }
        else if (e.Button == MouseButtons.Left)
        {
            Focus();

            // ツールがアクティブな場合、ツールマネージャーに通知
            if (_toolManager.ActiveTool != null)
            {
                var worldPos = ScreenToWorld(e.Location);
                _toolManager.OnMouseDown(worldPos, _viewModel.SnapToGrid, _viewModel.GridSpacing);
                Invalidate();
            }
        }
    }

    protected override void OnMouseUp(MouseEventArgs e)
    {
        base.OnMouseUp(e);

        if (e.Button == MouseButtons.Right)
        {
            // パン終了
            if (_isPanning)
            {
                _isPanning = false;
                Capture = false;
            }
        }
        else if (e.Button == MouseButtons.Left)
        {
            // ツールがアクティブな場合、ツールマネージャーに通知
            if (_toolManager.ActiveTool != null)
            {
                var worldPos = ScreenToWorld(e.Location);
                _toolManager.OnMouseUp(worldPos, _viewModel.SnapToGrid, _viewModel.GridSpacing);
                Invalidate();
            }
        }
    }

    #endregion

    #region Coordinate Conversion

    private PointF ScreenToWorld(Point screenPoint)
    {
        var x = (screenPoint.X - Width / 2.0) / _viewModel.ZoomLevel - _viewModel.ViewCenterX;
        var y = (screenPoint.Y - Height / 2.0) / _viewModel.ZoomLevel - _viewModel.ViewCenterY;
        return new PointF((float)x, (float)y);
    }

    private PointF WorldToScreen(PointF worldPoint)
    {
        var x = (worldPoint.X + _viewModel.ViewCenterX) * _viewModel.ZoomLevel + Width / 2.0;
        var y = (worldPoint.Y + _viewModel.ViewCenterY) * _viewModel.ZoomLevel + Height / 2.0;
        return new PointF((float)x, (float)y);
    }

    #endregion

    #region Public Methods

    /// <summary>
    /// エンティティを追加
    /// </summary>
    public void AddEntity(Core.Entities.GeometryEntity entity)
    {
        _viewModel.AddEntity(entity);
    }

    /// <summary>
    /// エンティティを削除
    /// </summary>
    public void RemoveEntity(Core.Entities.GeometryEntity entity)
    {
        _viewModel.RemoveEntity(entity);
    }

    /// <summary>
    /// すべてクリア
    /// </summary>
    public void Clear()
    {
        _viewModel.Clear();
    }

    /// <summary>
    /// 全体表示
    /// </summary>
    public void ZoomToFit()
    {
        // TODO: 実装
        ZoomLevel = 1.0;
        _viewModel.ViewCenterX = 0;
        _viewModel.ViewCenterY = 0;
        Invalidate();
    }

    #endregion
}

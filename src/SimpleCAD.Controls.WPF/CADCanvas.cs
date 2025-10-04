using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using SimpleCAD.Controls.WPF.Rendering;
using SimpleCAD.Core.Entities;
using SimpleCAD.Core.ViewModels;
using SimpleCAD.Core.Services;
using SimpleCAD.Core.Tools;

namespace SimpleCAD.Controls.WPF;

/// <summary>
/// CADキャンバス WPF UserControl
/// </summary>
public class CADCanvas : FrameworkElement
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

        Focusable = true;
        ClipToBounds = true;

        // イベントハンドラ登録
        MouseMove += OnMouseMove;
        MouseWheel += OnMouseWheel;
        MouseLeftButtonDown += OnMouseLeftButtonDown;
        MouseLeftButtonUp += OnMouseLeftButtonUp;
        MouseRightButtonDown += OnMouseRightButtonDown;
        MouseRightButtonUp += OnMouseRightButtonUp;

        // ViewModelの変更を監視
        _viewModel.Entities.CollectionChanged += (s, e) => InvalidateVisual();

        // ツールマネージャーのイベント
        _toolManager.EntityCompleted += (s, e) =>
        {
            _viewModel.AddEntity(e);
            InvalidateVisual();
        };
    }

    #region Properties

    /// <summary>
    /// エンティティコレクション
    /// </summary>
    public CADCanvasViewModel ViewModel => _viewModel;

    /// <summary>
    /// 描画ツールマネージャー
    /// </summary>
    public DrawingToolManager ToolManager => _toolManager;

    /// <summary>
    /// グリッド表示
    /// </summary>
    public bool GridVisible
    {
        get => _viewModel.GridVisible;
        set
        {
            _viewModel.GridVisible = value;
            InvalidateVisual();
        }
    }

    /// <summary>
    /// グリッドスナップ
    /// </summary>
    public bool SnapToGrid
    {
        get => _viewModel.SnapToGrid;
        set => _viewModel.SnapToGrid = value;
    }

    /// <summary>
    /// グリッド間隔
    /// </summary>
    public double GridSpacing
    {
        get => _viewModel.GridSpacing;
        set
        {
            _viewModel.GridSpacing = value;
            InvalidateVisual();
        }
    }

    /// <summary>
    /// ズームレベル
    /// </summary>
    public double ZoomLevel
    {
        get => _viewModel.ZoomLevel;
        set
        {
            _viewModel.ZoomLevel = Math.Max(0.1, Math.Min(100, value));
            InvalidateVisual();
        }
    }

    #endregion

    #region Rendering

    protected override void OnRender(DrawingContext drawingContext)
    {
        base.OnRender(drawingContext);

        // 背景を白で塗りつぶし
        drawingContext.DrawRectangle(Brushes.White, null, new Rect(0, 0, ActualWidth, ActualHeight));

        // グリッド描画
        if (_viewModel.GridVisible)
        {
            _gridRenderer.DrawGrid(
                drawingContext,
                ActualWidth,
                ActualHeight,
                _viewModel.GridSpacing,
                _viewModel.ViewCenterX,
                _viewModel.ViewCenterY,
                _viewModel.ZoomLevel);
        }

        // 座標変換を適用
        drawingContext.PushTransform(new TranslateTransform(ActualWidth / 2, ActualHeight / 2));
        drawingContext.PushTransform(new ScaleTransform(_viewModel.ZoomLevel, _viewModel.ZoomLevel));
        drawingContext.PushTransform(new TranslateTransform(_viewModel.ViewCenterX, _viewModel.ViewCenterY));

        // エンティティ描画
        var renderer = new WPFRenderer(drawingContext);
        foreach (var entity in _viewModel.Entities)
        {
            entity.Draw(renderer);
        }

        // ツールプレビュー描画
        _toolManager.DrawPreview(renderer);

        drawingContext.Pop();
        drawingContext.Pop();
        drawingContext.Pop();

        // 座標表示（右下）
        DrawCoordinateDisplay(drawingContext);
    }

    private void DrawCoordinateDisplay(DrawingContext dc)
    {
        var worldPos = ScreenToWorld(_lastMousePosition);
        var text = $"X: {worldPos.X:F2}, Y: {worldPos.Y:F2}  Zoom: {_viewModel.ZoomLevel:F2}x";

        var formattedText = new FormattedText(
            text,
            System.Globalization.CultureInfo.CurrentCulture,
            FlowDirection.LeftToRight,
            new Typeface("Consolas"),
            12,
            Brushes.Black,
            1.0);

        var bg = new Rect(
            ActualWidth - formattedText.Width - 10,
            ActualHeight - formattedText.Height - 5,
            formattedText.Width + 10,
            formattedText.Height + 5);

        dc.DrawRectangle(new SolidColorBrush(Color.FromArgb(200, 255, 255, 255)), null, bg);
        dc.DrawText(formattedText, new Point(bg.Left + 5, bg.Top));
    }

    #endregion

    #region Mouse Events

    private void OnMouseMove(object sender, MouseEventArgs e)
    {
        _lastMousePosition = e.GetPosition(this);

        if (_isPanning && e.RightButton == MouseButtonState.Pressed)
        {
            var delta = _lastMousePosition - _panStartPosition;
            _viewModel.ViewCenterX = _panStartCenterX + delta.X / _viewModel.ZoomLevel;
            _viewModel.ViewCenterY = _panStartCenterY + delta.Y / _viewModel.ZoomLevel;
            InvalidateVisual();
        }
        else
        {
            // ツールがアクティブな場合、ツールマネージャーに通知
            if (_toolManager.ActiveTool != null)
            {
                var worldPos = ScreenToWorld(_lastMousePosition);
                var worldPointF = new System.Drawing.PointF((float)worldPos.X, (float)worldPos.Y);
                _toolManager.OnMouseMove(worldPointF, _viewModel.SnapToGrid, _viewModel.GridSpacing);
            }

            InvalidateVisual();
        }
    }

    private void OnMouseWheel(object sender, MouseWheelEventArgs e)
    {
        // ズーム
        var mousePos = e.GetPosition(this);
        var worldPosBefore = ScreenToWorld(mousePos);

        var delta = e.Delta > 0 ? 1.1 : 0.9;
        ZoomLevel *= delta;

        var worldPosAfter = ScreenToWorld(mousePos);
        _viewModel.ViewCenterX += worldPosBefore.X - worldPosAfter.X;
        _viewModel.ViewCenterY += worldPosBefore.Y - worldPosAfter.Y;

        InvalidateVisual();
        e.Handled = true;
    }

    private void OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        Focus();

        // ツールがアクティブな場合、ツールマネージャーに通知
        if (_toolManager.ActiveTool != null)
        {
            var screenPos = e.GetPosition(this);
            var worldPos = ScreenToWorld(screenPos);
            var worldPointF = new System.Drawing.PointF((float)worldPos.X, (float)worldPos.Y);
            _toolManager.OnMouseDown(worldPointF, _viewModel.SnapToGrid, _viewModel.GridSpacing);
            InvalidateVisual();
        }

        e.Handled = true;
    }

    private void OnMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
    {
        // ツールがアクティブな場合、ツールマネージャーに通知
        if (_toolManager.ActiveTool != null)
        {
            var screenPos = e.GetPosition(this);
            var worldPos = ScreenToWorld(screenPos);
            var worldPointF = new System.Drawing.PointF((float)worldPos.X, (float)worldPos.Y);
            _toolManager.OnMouseUp(worldPointF, _viewModel.SnapToGrid, _viewModel.GridSpacing);
            InvalidateVisual();
        }

        e.Handled = true;
    }

    private void OnMouseRightButtonDown(object sender, MouseButtonEventArgs e)
    {
        // パン開始
        _isPanning = true;
        _panStartPosition = e.GetPosition(this);
        _panStartCenterX = _viewModel.ViewCenterX;
        _panStartCenterY = _viewModel.ViewCenterY;
        CaptureMouse();
        e.Handled = true;
    }

    private void OnMouseRightButtonUp(object sender, MouseButtonEventArgs e)
    {
        // パン終了
        if (_isPanning)
        {
            _isPanning = false;
            ReleaseMouseCapture();
        }
        e.Handled = true;
    }

    #endregion

    #region Coordinate Conversion

    private Point ScreenToWorld(Point screenPoint)
    {
        var x = (screenPoint.X - ActualWidth / 2) / _viewModel.ZoomLevel - _viewModel.ViewCenterX;
        var y = (screenPoint.Y - ActualHeight / 2) / _viewModel.ZoomLevel - _viewModel.ViewCenterY;
        return new Point(x, y);
    }

    private Point WorldToScreen(Point worldPoint)
    {
        var x = (worldPoint.X + _viewModel.ViewCenterX) * _viewModel.ZoomLevel + ActualWidth / 2;
        var y = (worldPoint.Y + _viewModel.ViewCenterY) * _viewModel.ZoomLevel + ActualHeight / 2;
        return new Point(x, y);
    }

    #endregion

    #region Public Methods

    /// <summary>
    /// エンティティを追加
    /// </summary>
    public void AddEntity(GeometryEntity entity)
    {
        _viewModel.AddEntity(entity);
    }

    /// <summary>
    /// エンティティを削除
    /// </summary>
    public void RemoveEntity(GeometryEntity entity)
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
        InvalidateVisual();
    }

    #endregion
}

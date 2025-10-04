using System.Collections.ObjectModel;
using System.Drawing;
using SimpleCAD.Core.Entities;
using SimpleCAD.Core.Rendering;

namespace SimpleCAD.Core.Tools;

/// <summary>
/// 描画ツールマネージャー
/// </summary>
public class DrawingToolManager
{
    private readonly Dictionary<string, IDrawingTool> _tools = new();
    private IDrawingTool? _activeTool;

    public DrawingToolManager()
    {
        // デフォルトツールを登録
        RegisterTool(new LineDrawingTool());
        RegisterTool(new ArcDrawingTool());
        RegisterTool(new CurveDrawingTool());
    }

    /// <summary>
    /// エンティティコレクションを設定し、編集ツールを登録
    /// </summary>
    public void InitializeEditingTools(ObservableCollection<GeometryEntity> entities)
    {
        RegisterTool(new SelectionTool(entities));
        RegisterTool(new MoveTool(entities));
        RegisterTool(new RotateTool(entities));
        RegisterTool(new ScaleTool(entities));
    }

    /// <summary>
    /// 寸法ツールを登録
    /// </summary>
    public void InitializeDimensionTools()
    {
        RegisterTool(new LinearDimensionTool());
        RegisterTool(new RadialDimensionTool());
        RegisterTool(new AngularDimensionTool());
    }

    /// <summary>
    /// アクティブなツール
    /// </summary>
    public IDrawingTool? ActiveTool => _activeTool;

    /// <summary>
    /// アクティブなツール名
    /// </summary>
    public string? ActiveToolName => _activeTool?.Name;

    /// <summary>
    /// 登録済みツールの名前一覧
    /// </summary>
    public IEnumerable<string> ToolNames => _tools.Keys;

    /// <summary>
    /// エンティティが完成した時のイベント
    /// </summary>
    public event EventHandler<GeometryEntity>? EntityCompleted;

    /// <summary>
    /// ツールを登録
    /// </summary>
    public void RegisterTool(IDrawingTool tool)
    {
        _tools[tool.Name] = tool;
        tool.EntityCompleted += OnToolEntityCompleted;
    }

    /// <summary>
    /// ツールをアクティブ化
    /// </summary>
    public bool ActivateTool(string toolName)
    {
        if (!_tools.ContainsKey(toolName))
            return false;

        // 既存のツールをキャンセル
        _activeTool?.Cancel();

        _activeTool = _tools[toolName];
        return true;
    }

    /// <summary>
    /// ツールを非アクティブ化
    /// </summary>
    public void DeactivateTool()
    {
        _activeTool?.Cancel();
        _activeTool = null;
    }

    /// <summary>
    /// マウスダウン時の処理
    /// </summary>
    public void OnMouseDown(PointF worldPoint, bool snapToGrid, double gridSpacing)
    {
        _activeTool?.OnMouseDown(worldPoint, snapToGrid, gridSpacing);
    }

    /// <summary>
    /// マウス移動時の処理
    /// </summary>
    public void OnMouseMove(PointF worldPoint, bool snapToGrid, double gridSpacing)
    {
        _activeTool?.OnMouseMove(worldPoint, snapToGrid, gridSpacing);
    }

    /// <summary>
    /// マウスアップ時の処理
    /// </summary>
    public void OnMouseUp(PointF worldPoint, bool snapToGrid, double gridSpacing)
    {
        _activeTool?.OnMouseUp(worldPoint, snapToGrid, gridSpacing);
    }

    /// <summary>
    /// プレビュー描画
    /// </summary>
    public void DrawPreview(IRenderer renderer)
    {
        _activeTool?.DrawPreview(renderer);
    }

    /// <summary>
    /// 現在のツールをキャンセル
    /// </summary>
    public void CancelCurrentTool()
    {
        _activeTool?.Cancel();
    }

    /// <summary>
    /// 曲線ツールを完成させる
    /// </summary>
    public void CompleteCurveTool()
    {
        if (_activeTool is CurveDrawingTool curveTool)
        {
            curveTool.Complete();
        }
    }

    /// <summary>
    /// 曲線ツールで最後の点を削除
    /// </summary>
    public void UndoCurvePoint()
    {
        if (_activeTool is CurveDrawingTool curveTool)
        {
            curveTool.Undo();
        }
    }

    /// <summary>
    /// 登録されたツール名を取得
    /// </summary>
    public IEnumerable<string> GetToolNames()
    {
        return _tools.Keys;
    }

    private void OnToolEntityCompleted(object? sender, GeometryEntity entity)
    {
        EntityCompleted?.Invoke(this, entity);
    }
}

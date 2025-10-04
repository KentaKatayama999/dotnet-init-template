using Geometry;
using SimpleCAD.Core.Entities;
using System.Drawing;

namespace SimpleCAD.Demo.WinForms;

public partial class Form1 : Form
{
    private Random _random = new Random();

    public Form1()
    {
        InitializeComponent();
    }

    private void Form1_Load(object sender, EventArgs e)
    {
        // 初期サンプルエンティティを追加
        AddSampleEntities();
        lblStatus.Text = "初期サンプルエンティティを追加しました";
    }

    private void AddSampleEntities()
    {
        // サンプル直線
        var line1 = new ObjLine(new ObjPoints(0, 0), new ObjPoints(100, 100));
        cadCanvas.AddEntity(new LineEntity(line1) { Color = Color.Blue, Thickness = 2 });

        var line2 = new ObjLine(new ObjPoints(0, 100), new ObjPoints(100, 0));
        cadCanvas.AddEntity(new LineEntity(line2) { Color = Color.Red, Thickness = 2 });

        // サンプル円弧
        var arc = new ObjArc();
        arc.SetArcAngle(30, 50, 50, 0, 0, 180);
        cadCanvas.AddEntity(new ArcEntity(arc) { Color = Color.Green, Thickness = 2 });

        // サンプルNURBS曲線
        var passPoints = new ObjPoints();
        passPoints.AddPoint(-80, -50);
        passPoints.AddPoint(-40, 50);
        passPoints.AddPoint(0, -30);
        passPoints.AddPoint(40, 60);
        passPoints.AddPoint(80, -40);
        var curve = new ObjCurve();
        curve.SetCurve(passPoints);
        cadCanvas.AddEntity(new CurveEntity(curve) { Color = Color.Purple, Thickness = 2 });
    }

    private void BtnSelectionTool_Click(object sender, EventArgs e)
    {
        cadCanvas.ToolManager.ActivateTool("Selection");
        lblStatus.Text = "選択ツールがアクティブです - クリックでエンティティを選択";
    }

    private void BtnLineTool_Click(object sender, EventArgs e)
    {
        cadCanvas.ToolManager.ActivateTool("Line");
        lblStatus.Text = "直線ツールがアクティブです - クリックで始点・終点を指定";
    }

    private void BtnArcTool_Click(object sender, EventArgs e)
    {
        cadCanvas.ToolManager.ActivateTool("Arc");
        lblStatus.Text = "円弧ツールがアクティブです - クリックで中心・始点・終点を指定";
    }

    private void BtnCurveTool_Click(object sender, EventArgs e)
    {
        cadCanvas.ToolManager.ActivateTool("Curve");
        lblStatus.Text = "曲線ツールがアクティブです - クリックで点を追加、Enterで完成、Backspaceで取消";
    }

    private void BtnMoveTool_Click(object sender, EventArgs e)
    {
        cadCanvas.ToolManager.ActivateTool("Move");
        lblStatus.Text = "移動ツールがアクティブです - 基準点、移動先を指定";
    }

    private void BtnRotateTool_Click(object sender, EventArgs e)
    {
        cadCanvas.ToolManager.ActivateTool("Rotate");
        lblStatus.Text = "回転ツールがアクティブです - 中心点、基準点、目標点を指定";
    }

    private void BtnScaleTool_Click(object sender, EventArgs e)
    {
        cadCanvas.ToolManager.ActivateTool("Scale");
        lblStatus.Text = "拡大縮小ツールがアクティブです - 基準点、基準距離、目標距離を指定";
    }

    private void BtnLinearDimension_Click(object sender, EventArgs e)
    {
        cadCanvas.ToolManager.ActivateTool("LinearDimension");
        lblStatus.Text = "線形寸法ツールがアクティブです - 第1点、第2点、寸法線位置を指定";
    }

    private void BtnRadialDimension_Click(object sender, EventArgs e)
    {
        cadCanvas.ToolManager.ActivateTool("RadialDimension");
        lblStatus.Text = "半径寸法ツールがアクティブです - 中心点、円周上の点を指定";
    }

    private void BtnAngularDimension_Click(object sender, EventArgs e)
    {
        cadCanvas.ToolManager.ActivateTool("AngularDimension");
        lblStatus.Text = "角度寸法ツールがアクティブです - 中心点、第1点、第2点、円弧位置を指定";
    }

    private void BtnCancelTool_Click(object sender, EventArgs e)
    {
        cadCanvas.ToolManager.CancelCurrentTool();
        cadCanvas.ToolManager.DeactivateTool();
        lblStatus.Text = "ツールをキャンセルしました";
    }

    private void BtnCompleteCurve_Click(object sender, EventArgs e)
    {
        cadCanvas.ToolManager.CompleteCurveTool();
        lblStatus.Text = "曲線を完成しました";
    }

    private void BtnClear_Click(object sender, EventArgs e)
    {
        cadCanvas.Clear();
        lblStatus.Text = "すべてのエンティティをクリアしました";
    }

    private void BtnZoomFit_Click(object sender, EventArgs e)
    {
        cadCanvas.ZoomToFit();
        lblStatus.Text = "全体表示にリセットしました";
    }

    private void ChkGrid_Click(object sender, EventArgs e)
    {
        cadCanvas.GridVisible = chkGrid.Checked;
        lblStatus.Text = $"グリッド表示: {(cadCanvas.GridVisible ? "ON" : "OFF")}";
    }

    private void ChkSnap_Click(object sender, EventArgs e)
    {
        cadCanvas.SnapToGrid = chkSnap.Checked;
        lblStatus.Text = $"グリッドスナップ: {(cadCanvas.SnapToGrid ? "ON" : "OFF")}";
    }

    protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
    {
        switch (keyData)
        {
            case Keys.Escape:
                cadCanvas.ToolManager.CancelCurrentTool();
                cadCanvas.ToolManager.DeactivateTool();
                lblStatus.Text = "ツールをキャンセルしました";
                return true;

            case Keys.Enter:
                cadCanvas.ToolManager.CompleteCurveTool();
                lblStatus.Text = "曲線を完成しました";
                return true;

            case Keys.Back:
                cadCanvas.ToolManager.UndoCurvePoint();
                lblStatus.Text = "最後の点を削除しました";
                return true;
        }

        return base.ProcessCmdKey(ref msg, keyData);
    }
}

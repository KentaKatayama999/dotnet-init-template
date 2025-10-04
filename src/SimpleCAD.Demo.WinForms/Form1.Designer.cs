namespace SimpleCAD.Demo.WinForms;

partial class Form1
{
    /// <summary>
    ///  Required designer variable.
    /// </summary>
    private System.ComponentModel.IContainer components = null;

    /// <summary>
    ///  Clean up any resources being used.
    /// </summary>
    /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
    protected override void Dispose(bool disposing)
    {
        if (disposing && (components != null))
        {
            components.Dispose();
        }
        base.Dispose(disposing);
    }

    #region Windows Form Designer generated code

    /// <summary>
    ///  Required method for Designer support - do not modify
    ///  the contents of this method with the code editor.
    /// </summary>
    private void InitializeComponent()
    {
        toolStrip = new ToolStrip();
        btnSelectionTool = new ToolStripButton();
        toolStripSeparator1 = new ToolStripSeparator();
        btnLineTool = new ToolStripButton();
        btnArcTool = new ToolStripButton();
        btnCurveTool = new ToolStripButton();
        toolStripSeparator2 = new ToolStripSeparator();
        btnMoveTool = new ToolStripButton();
        btnRotateTool = new ToolStripButton();
        btnScaleTool = new ToolStripButton();
        toolStripSeparator3 = new ToolStripSeparator();
        btnLinearDimension = new ToolStripButton();
        btnRadialDimension = new ToolStripButton();
        btnAngularDimension = new ToolStripButton();
        toolStripSeparator6 = new ToolStripSeparator();
        btnCancelTool = new ToolStripButton();
        btnCompleteCurve = new ToolStripButton();
        toolStripSeparator4 = new ToolStripSeparator();
        btnClear = new ToolStripButton();
        btnZoomFit = new ToolStripButton();
        toolStripSeparator5 = new ToolStripSeparator();
        chkGrid = new ToolStripButton();
        chkSnap = new ToolStripButton();
        statusStrip = new StatusStrip();
        lblStatus = new ToolStripStatusLabel();
        cadCanvas = new SimpleCAD.Controls.WinForms.CADCanvas();
        toolStrip.SuspendLayout();
        statusStrip.SuspendLayout();
        SuspendLayout();
        //
        // toolStrip
        //
        toolStrip.Items.AddRange(new ToolStripItem[] { btnSelectionTool, toolStripSeparator1, btnLineTool, btnArcTool, btnCurveTool, toolStripSeparator2, btnMoveTool, btnRotateTool, btnScaleTool, toolStripSeparator3, btnLinearDimension, btnRadialDimension, btnAngularDimension, toolStripSeparator6, btnCancelTool, btnCompleteCurve, toolStripSeparator4, btnClear, btnZoomFit, toolStripSeparator5, chkGrid, chkSnap });
        toolStrip.Location = new Point(0, 0);
        toolStrip.Name = "toolStrip";
        toolStrip.Size = new Size(900, 25);
        toolStrip.TabIndex = 0;
        //
        // btnSelectionTool
        //
        btnSelectionTool.DisplayStyle = ToolStripItemDisplayStyle.Text;
        btnSelectionTool.Name = "btnSelectionTool";
        btnSelectionTool.Size = new Size(40, 22);
        btnSelectionTool.Text = "選択";
        btnSelectionTool.Click += BtnSelectionTool_Click;
        //
        // toolStripSeparator1
        //
        toolStripSeparator1.Name = "toolStripSeparator1";
        toolStripSeparator1.Size = new Size(6, 25);
        //
        // btnLineTool
        //
        btnLineTool.DisplayStyle = ToolStripItemDisplayStyle.Text;
        btnLineTool.Name = "btnLineTool";
        btnLineTool.Size = new Size(40, 22);
        btnLineTool.Text = "直線";
        btnLineTool.Click += BtnLineTool_Click;
        //
        // btnArcTool
        //
        btnArcTool.DisplayStyle = ToolStripItemDisplayStyle.Text;
        btnArcTool.Name = "btnArcTool";
        btnArcTool.Size = new Size(40, 22);
        btnArcTool.Text = "円弧";
        btnArcTool.Click += BtnArcTool_Click;
        //
        // btnCurveTool
        //
        btnCurveTool.DisplayStyle = ToolStripItemDisplayStyle.Text;
        btnCurveTool.Name = "btnCurveTool";
        btnCurveTool.Size = new Size(40, 22);
        btnCurveTool.Text = "曲線";
        btnCurveTool.Click += BtnCurveTool_Click;
        //
        // toolStripSeparator2
        //
        toolStripSeparator2.Name = "toolStripSeparator2";
        toolStripSeparator2.Size = new Size(6, 25);
        //
        // btnMoveTool
        //
        btnMoveTool.DisplayStyle = ToolStripItemDisplayStyle.Text;
        btnMoveTool.Name = "btnMoveTool";
        btnMoveTool.Size = new Size(40, 22);
        btnMoveTool.Text = "移動";
        btnMoveTool.Click += BtnMoveTool_Click;
        //
        // btnRotateTool
        //
        btnRotateTool.DisplayStyle = ToolStripItemDisplayStyle.Text;
        btnRotateTool.Name = "btnRotateTool";
        btnRotateTool.Size = new Size(40, 22);
        btnRotateTool.Text = "回転";
        btnRotateTool.Click += BtnRotateTool_Click;
        //
        // btnScaleTool
        //
        btnScaleTool.DisplayStyle = ToolStripItemDisplayStyle.Text;
        btnScaleTool.Name = "btnScaleTool";
        btnScaleTool.Size = new Size(70, 22);
        btnScaleTool.Text = "拡大縮小";
        btnScaleTool.Click += BtnScaleTool_Click;
        //
        // toolStripSeparator3
        //
        toolStripSeparator3.Name = "toolStripSeparator3";
        toolStripSeparator3.Size = new Size(6, 25);
        //
        // btnLinearDimension
        //
        btnLinearDimension.DisplayStyle = ToolStripItemDisplayStyle.Text;
        btnLinearDimension.Name = "btnLinearDimension";
        btnLinearDimension.Size = new Size(70, 22);
        btnLinearDimension.Text = "線形寸法";
        btnLinearDimension.Click += BtnLinearDimension_Click;
        //
        // btnRadialDimension
        //
        btnRadialDimension.DisplayStyle = ToolStripItemDisplayStyle.Text;
        btnRadialDimension.Name = "btnRadialDimension";
        btnRadialDimension.Size = new Size(70, 22);
        btnRadialDimension.Text = "半径寸法";
        btnRadialDimension.Click += BtnRadialDimension_Click;
        //
        // btnAngularDimension
        //
        btnAngularDimension.DisplayStyle = ToolStripItemDisplayStyle.Text;
        btnAngularDimension.Name = "btnAngularDimension";
        btnAngularDimension.Size = new Size(70, 22);
        btnAngularDimension.Text = "角度寸法";
        btnAngularDimension.Click += BtnAngularDimension_Click;
        //
        // toolStripSeparator6
        //
        toolStripSeparator6.Name = "toolStripSeparator6";
        toolStripSeparator6.Size = new Size(6, 25);
        //
        // btnCancelTool
        //
        btnCancelTool.DisplayStyle = ToolStripItemDisplayStyle.Text;
        btnCancelTool.Name = "btnCancelTool";
        btnCancelTool.Size = new Size(100, 22);
        btnCancelTool.Text = "キャンセル (ESC)";
        btnCancelTool.Click += BtnCancelTool_Click;
        //
        // btnCompleteCurve
        //
        btnCompleteCurve.DisplayStyle = ToolStripItemDisplayStyle.Text;
        btnCompleteCurve.Name = "btnCompleteCurve";
        btnCompleteCurve.Size = new Size(110, 22);
        btnCompleteCurve.Text = "曲線完成 (Enter)";
        btnCompleteCurve.Click += BtnCompleteCurve_Click;
        //
        // toolStripSeparator3
        //
        toolStripSeparator3.Name = "toolStripSeparator3";
        toolStripSeparator3.Size = new Size(6, 25);
        //
        // btnClear
        //
        btnClear.DisplayStyle = ToolStripItemDisplayStyle.Text;
        btnClear.Name = "btnClear";
        btnClear.Size = new Size(40, 22);
        btnClear.Text = "クリア";
        btnClear.Click += BtnClear_Click;
        //
        // btnZoomFit
        //
        btnZoomFit.DisplayStyle = ToolStripItemDisplayStyle.Text;
        btnZoomFit.Name = "btnZoomFit";
        btnZoomFit.Size = new Size(60, 22);
        btnZoomFit.Text = "全体表示";
        btnZoomFit.Click += BtnZoomFit_Click;
        //
        // toolStripSeparator2
        //
        toolStripSeparator2.Name = "toolStripSeparator2";
        toolStripSeparator2.Size = new Size(6, 25);
        //
        // chkGrid
        //
        chkGrid.Checked = true;
        chkGrid.CheckOnClick = true;
        chkGrid.CheckState = CheckState.Checked;
        chkGrid.DisplayStyle = ToolStripItemDisplayStyle.Text;
        chkGrid.Name = "chkGrid";
        chkGrid.Size = new Size(80, 22);
        chkGrid.Text = "グリッド表示";
        chkGrid.Click += ChkGrid_Click;
        //
        // chkSnap
        //
        chkSnap.Checked = true;
        chkSnap.CheckOnClick = true;
        chkSnap.CheckState = CheckState.Checked;
        chkSnap.DisplayStyle = ToolStripItemDisplayStyle.Text;
        chkSnap.Name = "chkSnap";
        chkSnap.Size = new Size(100, 22);
        chkSnap.Text = "グリッドスナップ";
        chkSnap.Click += ChkSnap_Click;
        //
        // statusStrip
        //
        statusStrip.Items.AddRange(new ToolStripItem[] { lblStatus });
        statusStrip.Location = new Point(0, 575);
        statusStrip.Name = "statusStrip";
        statusStrip.Size = new Size(900, 25);
        statusStrip.TabIndex = 1;
        //
        // lblStatus
        //
        lblStatus.Name = "lblStatus";
        lblStatus.Size = new Size(60, 20);
        lblStatus.Text = "準備完了";
        //
        // cadCanvas
        //
        cadCanvas.Dock = DockStyle.Fill;
        cadCanvas.GridSpacing = 10.0;
        cadCanvas.GridVisible = true;
        cadCanvas.Location = new Point(0, 25);
        cadCanvas.Name = "cadCanvas";
        cadCanvas.Size = new Size(900, 550);
        cadCanvas.SnapToGrid = true;
        cadCanvas.TabIndex = 2;
        cadCanvas.ZoomLevel = 1.0;
        //
        // Form1
        //
        AutoScaleDimensions = new SizeF(7F, 15F);
        AutoScaleMode = AutoScaleMode.Font;
        ClientSize = new Size(900, 600);
        Controls.Add(cadCanvas);
        Controls.Add(statusStrip);
        Controls.Add(toolStrip);
        Name = "Form1";
        Text = "SimpleCAD WinForms Demo";
        Load += Form1_Load;
        toolStrip.ResumeLayout(false);
        toolStrip.PerformLayout();
        statusStrip.ResumeLayout(false);
        statusStrip.PerformLayout();
        ResumeLayout(false);
        PerformLayout();
    }

    #endregion

    private ToolStrip toolStrip;
    private ToolStripButton btnSelectionTool;
    private ToolStripSeparator toolStripSeparator1;
    private ToolStripButton btnLineTool;
    private ToolStripButton btnArcTool;
    private ToolStripButton btnCurveTool;
    private ToolStripSeparator toolStripSeparator2;
    private ToolStripButton btnMoveTool;
    private ToolStripButton btnRotateTool;
    private ToolStripButton btnScaleTool;
    private ToolStripSeparator toolStripSeparator3;
    private ToolStripButton btnLinearDimension;
    private ToolStripButton btnRadialDimension;
    private ToolStripButton btnAngularDimension;
    private ToolStripSeparator toolStripSeparator6;
    private ToolStripButton btnCancelTool;
    private ToolStripButton btnCompleteCurve;
    private ToolStripSeparator toolStripSeparator4;
    private ToolStripButton btnClear;
    private ToolStripButton btnZoomFit;
    private ToolStripSeparator toolStripSeparator5;
    private ToolStripButton chkGrid;
    private ToolStripButton chkSnap;
    private StatusStrip statusStrip;
    private ToolStripStatusLabel lblStatus;
    private SimpleCAD.Controls.WinForms.CADCanvas cadCanvas;
}

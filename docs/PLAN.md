# SimpleCAD - 2D CAD ライブラリ 開発計画

## 📋 プロジェクト概要

**SimpleCAD**は、Geometryライブラリを活用した再利用可能な2D CAD UserControlライブラリです。
**WPFとWinFormsの両方**に対応し、NURBS曲線を中心とした高度な幾何計算機能をGUIで操作可能にします。

## 🎯 設計方針

### アーキテクチャ

#### 3層構造
1. **SimpleCAD.Core** - 共通ロジック層（.NET Standard 2.0）
   - プラットフォーム非依存の共通コード
   - エンティティ管理
   - ツールロジック
   - 幾何計算エンジン
   - ファイルI/O (JSON/SVG)

2. **SimpleCAD.Controls.WPF** - WPF版UI層（.NET 9.0-windows）
   - WPF UserControl (CADCanvas)
   - DrawingContext による描画
   - WPF固有のイベント処理

3. **SimpleCAD.Controls.WinForms** - WinForms版UI層（.NET 9.0-windows）
   - WinForms UserControl (CADCanvas)
   - GDI+ (Graphics) による描画
   - WinForms固有のイベント処理

#### 設計原則
- **共通API**: 両プラットフォームで同じプロパティ・メソッド・イベントを提供
- **UserControlライブラリ**: 他のプロジェクトで簡単に参照可能
- **再利用性重視**: 単一のコントロールとして配置可能
- **右クリックコンテキストメニュー**: ツールパレット不要のシンプルなUI
- **フローティングオーバーレイ**: 最小限のUIで最大限の機能

### 主要コンポーネント

#### CADCanvas (メインコントロール)

**WPF版:**
```xaml
<Window xmlns:cad="clr-namespace:SimpleCAD.Controls.WPF;assembly=SimpleCAD.Controls.WPF">
    <cad:CADCanvas />
</Window>
```

**WinForms版:**
```csharp
using SimpleCAD.Controls.WinForms;

public partial class Form1 : Form
{
    private CADCanvas cadCanvas;

    public Form1()
    {
        InitializeComponent();
        cadCanvas = new CADCanvas();
        cadCanvas.Dock = DockStyle.Fill;
        this.Controls.Add(cadCanvas);
    }
}
```

## 🛠 主要機能

### 1. グリッド機能
- **メジャーグリッド**: 10単位ごとの太い線（濃いグレー、0.5px）
- **マイナーグリッド**: 1単位ごとの細い線（薄いグレー、0.2px）
- **座標軸表示**: X軸（赤）、Y軸（青）で原点を明示
- **グリッドスナップ**: ON/OFF切替可能（ショートカット: `G`キー）
  - マウスカーソルを最も近いグリッド点に吸着
  - スナップ時の視覚的フィードバック（小さな円を表示）
- **動的グリッド**: ズームレベルに応じた密度自動調整
  - ズームアウト時: マイナーグリッド非表示
  - ズームイン時: より細かいグリッド表示
- **グリッド間隔設定**: 1, 2, 5, 10, 20, 50, 100 から選択可能

### 2. 座標表示
- **リアルタイム座標**: 画面右下に現在のマウス座標を表示
- **フォーマット**: `X: 123.45, Y: 67.89`
- **グリッドスナップ時**: 整数値表示

### 3. インタラクティブ編集機能

#### 表示モード切替
- **ReadOnlyモード**: 図形の表示のみ（編集不可）
  - すべての編集操作を無効化
  - コントロールポイント・通過点の非表示
  - 選択・ドラッグ操作の無効化
  - ビュー操作（パン・ズーム）は使用可能
- **編集モード**: すべての編集機能が有効（デフォルト）
- **プロパティ**: `IsReadOnly` (bool)

#### エンティティの選択（編集モードのみ）
- **マウスクリック選択**: 図形またはコントロールポイントをクリックで選択
- **ヒット判定**: 8px半径の判定領域
- **選択状態の視覚化**: 選択された図形を強調表示（オレンジ色）

#### コントロールポイントの編集
- **ドラッグ&ドロップ**: コントロールポイントをマウスでドラッグして移動
- **リアルタイムプレビュー**: ドラッグ中に曲線形状をリアルタイム更新
- **元の位置表示**: ドラッグ中、元の位置から現在位置への点線を表示
- **ドラッグ中の視覚的フィードバック**:
  - ドラッグ中のポイントを拡大表示
  - 色を変更（オレンジ色）
  - 半透明の円でプレビュー表示

#### 初期状態の管理
- **初期状態の保存**: 図形作成時の初期状態を自動保存
- **グレーアウト表示**: 初期状態をグレーで背景に表示（比較用）
- **表示切替**: `H`キーで初期状態の表示/非表示を切替
- **リセット機能**: `R`キーで図形を初期状態に戻す

#### 表示要素
- **通過点**: 赤い円で表示
- **コントロールポイント**: 緑の四角で表示
- **コントロールポイント接続線**: 緑の破線で接続
- **ポイント番号**: 10点以下の場合、各ポイントに番号を表示
- **初期状態の要素**: すべてグレーアウトで表示

#### パフォーマンス最適化
- **DoubleBuffering**: ちらつき防止のためのダブルバッファリング
- **isDirtyフラグ**: 変更検出による最小限の再描画
- **補間点のキャッシュ**: 曲線の補間点を事前計算してキャッシュ
- **静的描画リソース**: Pen/Brushを静的に保持して再利用
- **パフォーマンス表示**: レンダリング時間をミリ秒単位で表示

### 4. 描画ツール

#### 点列NURBS曲線ツール
- クリックで点を追加
- 自動NURBS補間
- Escキーでキャンセル
- Enterキーで確定

#### 直線ツール
- 2点指定で直線を作成
- グリッドスナップ対応

#### 円弧ツール
- 中心・半径・角度指定
- プレビュー表示

### 5. 編集ツール

#### オフセットツール
- 曲線選択
- 距離入力（テキストボックス or マウス）
- 方向指定（マウスカーソル位置で自動判定）
- 適応的サンプリング対応

#### フィレットツール
- 2曲線選択
- 半径入力
- 自動延長対応
- 反復収束法による高精度計算

#### 延長ツール
- 曲線選択
- 延長距離入力
- 始点/終点選択
- C2連続性を保った延長

#### トリミングツール
- 曲線選択
- パラメータ範囲指定
- プレビュー表示

### 6. 寸法ツール

#### 水平寸法
- X軸方向の距離を測定
- 自動寸法線配置
- 寸法値テキスト表示

#### 垂直寸法
- Y軸方向の距離を測定
- 自動寸法線配置

#### 半径寸法
- 円弧の半径を表示
- 表記: `R25` 形式
- リーダー線（引出線）で中心から表示

#### 直径寸法
- 円の直径を表示
- 表記: `Ø50` 形式（φ記号付き）
- 円を貫通する寸法線

#### 角度寸法
- 2直線または2曲線の接線間の角度
- 表記: `45°` 形式
- 円弧状の寸法線

#### 引出線（リーダー）
- 注釈・テキストを図形に関連付け
- 矢印付きの線で接続
- テキスト編集可能

#### 寸法共通要素
- **寸法線**: 測定範囲を示す線
- **寸法補助線**: 測定点から寸法線までの線
- **矢印終端**: 矢印、点、斜線などから選択可能
- **寸法値**: 自動計算された数値（手動編集も可）
- **スタイル設定**: 色、線幅、フォントサイズ、矢印サイズ

### 7. 解析ツール

#### 交点自動表示
- 曲線同士の交点を自動検出
- マーカー表示（小さな円）
- 座標値の表示（オプション）

#### 内接円計算
- 3要素（直線・曲線）を選択
- Levenberg-Marquardt法による最適化
- 自動計算・表示
- 詳細情報表示（中心座標、半径、収束回数）

#### 距離測定
- 2点間距離
- 点-曲線間最短距離
- リアルタイム表示

### 8. ビュー機能

#### パン（移動）
- 中ボタンドラッグ
- Spaceキー + ドラッグ
- スムーズな移動

#### ズーム
- マウスホイール（カーソル位置中心）
- Ctrl + マウスホイール（より細かい調整）
- ズーム率表示（ステータスバー）

#### 全体表示
- `F`キー: すべての図形が見えるようにズーム調整
- 自動中心配置

#### ホームビュー
- `Home`キー: 原点中心に戻る
- デフォルトズーム率に復帰

### 9. 右クリックコンテキストメニュー

```
右クリック
├─ 描画ツール ►
│  ├─ NURBS曲線 (C)
│  ├─ 直線 (L)
│  └─ 円弧 (A)
├─ 編集ツール ►
│  ├─ オフセット (O)
│  ├─ フィレット (F)
│  ├─ 延長 (E)
│  └─ トリミング (T)
├─ 寸法ツール ►
│  ├─ 水平寸法
│  ├─ 垂直寸法
│  ├─ 半径寸法
│  ├─ 直径寸法
│  ├─ 角度寸法
│  └─ 引出線
├─ 解析 ►
│  ├─ 交点表示切替
│  ├─ 内接円計算
│  └─ 距離測定
├─ ビュー ►
│  ├─ グリッド表示切替 (G)
│  ├─ スナップ切替 (S)
│  ├─ 全体表示 (F)
│  └─ ホームビュー (H)
├─ 選択時のみ ►
│  ├─ プロパティ編集...
│  ├─ 削除 (Del)
│  └─ コピー (Ctrl+C)
└─ ファイル ►
   ├─ エクスポート (SVG)
   └─ エクスポート (PNG)
```

### 10. UIオーバーレイ

#### 下部ステータスバー（半透明）
- 現在座標: `X: 123.45, Y: 67.89`
- ズーム率: `100%`
- グリッドスナップ状態: `スナップ: ON`

#### 左上ツールヒント（半透明、自動非表示）
- 現在のツール名
- 例: `🖊 NURBS曲線を描画中... (Escでキャンセル)`
- 操作ヒント表示

### 11. ファイル機能

#### JSON形式保存/開く
- 図形データのシリアライゼーション
- 設定情報（グリッド、ビュー状態など）の保存
- バージョン管理対応

#### SVGエクスポート
- ベクター形式での出力
- スケーラブルな図面データ
- Web・印刷対応

#### PNGエクスポート
- ラスター画像として出力
- 解像度指定可能
- 背景色設定（透明/白/カスタム）

## 🔧 公開API

### CADCanvas クラス

```csharp
public class CADCanvas : UserControl
{
    // プロパティ
    public ObservableCollection<GeometryEntity> Entities { get; }
    public bool IsReadOnly { get; set; }                // 表示専用モード（編集不可）
    public bool GridVisible { get; set; }
    public bool SnapToGrid { get; set; }
    public double GridSpacing { get; set; }
    public DrawingTool CurrentTool { get; set; }
    public double ZoomLevel { get; set; }
    public Point ViewCenter { get; set; }
    public bool ShowControlPoints { get; set; }         // コントロールポイント表示（IsReadOnly=trueの場合は無視）
    public bool ShowPassPoints { get; set; }            // 通過点表示（IsReadOnly=trueの場合は無視）
    public bool ShowInitialState { get; set; }          // 初期状態の表示

    // メソッド
    public void AddEntity(GeometryEntity entity);
    public void RemoveEntity(GeometryEntity entity);
    public void Clear();
    public void ExportToSvg(string filePath);
    public void ExportToPng(string filePath, int width, int height);
    public void ZoomToFit();
    public void ResetView();
    public void SaveToJson(string filePath);
    public void LoadFromJson(string filePath);

    // イベント
    public event EventHandler<EntityAddedEventArgs> EntityAdded;
    public event EventHandler<EntityRemovedEventArgs> EntityRemoved;
    public event EventHandler<EntitySelectedEventArgs> EntitySelected;
    public event EventHandler<ToolChangedEventArgs> ToolChanged;
    public event EventHandler<ViewChangedEventArgs> ViewChanged;
    public event EventHandler<IsReadOnlyChangedEventArgs> IsReadOnlyChanged;  // ReadOnlyモード切替時
}
```

### GeometryEntity (基底クラス - SimpleCAD.Core)

```csharp
// プラットフォーム非依存の抽象基底クラス
public abstract class GeometryEntity
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public System.Drawing.Color Color { get; set; }  // System.Drawing.Color は .NET Standard 2.0 で利用可能
    public double Thickness { get; set; }
    public bool IsVisible { get; set; }
    public bool IsSelected { get; set; }

    // 描画は IRenderer インターフェースを通じて抽象化
    public abstract void Draw(IRenderer renderer);
    public abstract RectangleF GetBounds();
}
```

### IRenderer インターフェース (SimpleCAD.Core)

```csharp
// プラットフォーム非依存の描画抽象化
public interface IRenderer
{
    // 基本描画
    void DrawLine(PointF p1, PointF p2, System.Drawing.Color color, float thickness);
    void DrawArc(PointF center, float radius, float startAngle, float sweepAngle, System.Drawing.Color color, float thickness);
    void DrawBezier(PointF p1, PointF p2, PointF p3, PointF p4, System.Drawing.Color color, float thickness);
    void DrawPath(PointF[] points, System.Drawing.Color color, float thickness);

    // テキスト描画
    void DrawText(string text, PointF position, float fontSize, System.Drawing.Color color);

    // 塗りつぶし
    void FillCircle(PointF center, float radius, System.Drawing.Color color);
    void FillRectangle(RectangleF rect, System.Drawing.Color color);

    // 変換
    void PushTransform(Matrix3x2 transform);
    void PopTransform();
}
```

### WPF/WinForms での実装

**WPF版 (SimpleCAD.Controls.WPF):**
```csharp
public class WPFRenderer : IRenderer
{
    private DrawingContext _dc;
    private Stack<Matrix> _transformStack = new Stack<Matrix>();

    public WPFRenderer(DrawingContext dc)
    {
        _dc = dc;
    }

    public void DrawLine(PointF p1, PointF p2, System.Drawing.Color color, float thickness)
    {
        var pen = new Pen(new SolidColorBrush(ToWpfColor(color)), thickness);
        _dc.DrawLine(pen, new Point(p1.X, p1.Y), new Point(p2.X, p2.Y));
    }
    // ... その他のメソッド実装
}
```

**WinForms版 (SimpleCAD.Controls.WinForms):**
```csharp
public class GDIRenderer : IRenderer
{
    private Graphics _graphics;
    private Stack<Matrix> _transformStack = new Stack<Matrix>();

    public GDIRenderer(Graphics graphics)
    {
        _graphics = graphics;
    }

    public void DrawLine(PointF p1, PointF p2, System.Drawing.Color color, float thickness)
    {
        using (var pen = new Pen(color, thickness))
        {
            _graphics.DrawLine(pen, p1, p2);
        }
    }
    // ... その他のメソッド実装
}
```

### 具体的なエンティティ

```csharp
public class CurveEntity : GeometryEntity
{
    public ObjCurve Curve { get; set; }
}

public class LineEntity : GeometryEntity
{
    public ObjLine Line { get; set; }
}

public class ArcEntity : GeometryEntity
{
    public ObjArc Arc { get; set; }
}

public class DimensionEntity : GeometryEntity
{
    public DimensionType Type { get; set; }
    public double Value { get; set; }
    public string Text { get; set; }
}
```

## ⌨️ キーボードショートカット

### 描画ツール
- `L`: 直線ツール
- `C`: 曲線ツール（NURBS）
- `A`: 円弧ツール

### 編集ツール
- `O`: オフセットツール
- `F`: フィレットツール
- `E`: 延長ツール
- `T`: トリミングツール

### 寸法ツール
- `D`: 寸法モード切替

### 表示
- `H`: 初期状態の表示/非表示切替
- `G`: グリッドスナップ切替

### ビュー
- `F`: 全体表示
- `Home`: ホームビュー（原点中心に戻る）
- `Space + ドラッグ`: パン

### 編集
- `R`: リセット（選択中の図形を初期状態に戻す）
- `Ctrl+S`: 保存
- `Ctrl+O`: 開く
- `Ctrl+Z`: 元に戻す
- `Ctrl+Y`: やり直し
- `Ctrl+C`: コピー
- `Delete`: 削除
- `Esc`: ツールキャンセル

## 📦 プロジェクト構成

### 3層構造の詳細

```
SimpleCAD/
├── src/
│   ├── SimpleCAD.Core/                          ← 共通ロジック層 (.NET Standard 2.0)
│   │   ├── Entities/
│   │   │   ├── GeometryEntity.cs               (抽象基底クラス)
│   │   │   ├── CurveEntity.cs
│   │   │   ├── LineEntity.cs
│   │   │   ├── ArcEntity.cs
│   │   │   └── DimensionEntity.cs
│   │   ├── Tools/
│   │   │   ├── IDrawingTool.cs                 (ツールインターフェース)
│   │   │   ├── CurveTool.cs
│   │   │   ├── LineTool.cs
│   │   │   ├── ArcTool.cs
│   │   │   ├── OffsetTool.cs
│   │   │   ├── FilletTool.cs
│   │   │   ├── ExtendTool.cs
│   │   │   ├── TrimTool.cs
│   │   │   └── DimensionTools/
│   │   │       ├── HorizontalDimensionTool.cs
│   │   │       ├── VerticalDimensionTool.cs
│   │   │       ├── RadiusDimensionTool.cs
│   │   │       ├── DiameterDimensionTool.cs
│   │   │       ├── AngleDimensionTool.cs
│   │   │       └── LeaderTool.cs
│   │   ├── ViewModels/
│   │   │   ├── CADCanvasViewModel.cs           (共通ViewModel)
│   │   │   └── ToolViewModel.cs
│   │   ├── Services/
│   │   │   ├── FileService.cs                  (JSON/SVG)
│   │   │   ├── SelectionService.cs
│   │   │   ├── SnapService.cs
│   │   │   └── GeometryCalculator.cs           (幾何計算ヘルパー)
│   │   ├── Rendering/
│   │   │   └── IRenderer.cs                    (描画抽象化インターフェース)
│   │   └── SimpleCAD.Core.csproj
│   │
│   ├── SimpleCAD.Controls.WPF/                  ← WPF UI層 (.NET 9.0-windows)
│   │   ├── CADCanvas.cs                        (WPF UserControl)
│   │   ├── Rendering/
│   │   │   ├── WPFRenderer.cs                  (DrawingContext実装)
│   │   │   ├── GridRenderer.cs
│   │   │   ├── EntityRenderer.cs
│   │   │   └── OverlayRenderer.cs
│   │   ├── Controls/
│   │   │   └── ContextMenuBuilder.cs           (右クリックメニュー)
│   │   └── SimpleCAD.Controls.WPF.csproj
│   │       ├── <ProjectReference> SimpleCAD.Core
│   │       └── <PackageReference> Geometry
│   │
│   ├── SimpleCAD.Controls.WinForms/             ← WinForms UI層 (.NET 9.0-windows)
│   │   ├── CADCanvas.cs                        (WinForms UserControl)
│   │   ├── Rendering/
│   │   │   ├── GDIRenderer.cs                  (GDI+ Graphics実装)
│   │   │   ├── GridRenderer.cs
│   │   │   ├── EntityRenderer.cs
│   │   │   └── OverlayRenderer.cs
│   │   ├── Controls/
│   │   │   └── ContextMenuBuilder.cs           (右クリックメニュー)
│   │   └── SimpleCAD.Controls.WinForms.csproj
│   │       ├── <ProjectReference> SimpleCAD.Core
│   │       └── <PackageReference> Geometry
│   │
│   ├── SimpleCAD.Demo.WPF/                      ← WPFデモアプリ
│   │   ├── MainWindow.xaml
│   │   ├── MainWindow.xaml.cs
│   │   └── SimpleCAD.Demo.WPF.csproj
│   │       └── <ProjectReference> SimpleCAD.Controls.WPF
│   │
│   └── SimpleCAD.Demo.WinForms/                 ← WinFormsデモアプリ
│       ├── Form1.cs
│       ├── Form1.Designer.cs
│       ├── Form1.resx
│       └── SimpleCAD.Demo.WinForms.csproj
│           └── <ProjectReference> SimpleCAD.Controls.WinForms
│
├── tests/
│   ├── SimpleCAD.Core.Tests/                    (Core層のユニットテスト)
│   ├── SimpleCAD.Controls.WPF.Tests/            (WPF層のテスト)
│   └── SimpleCAD.Controls.WinForms.Tests/       (WinForms層のテスト)
│
└── docs/
    ├── PLAN.md                                  (本ファイル)
    ├── API.md                                   (API仕様書)
    ├── USAGE.md                                 (使用方法)
    └── ARCHITECTURE.md                          (アーキテクチャ詳細)
```

### プロジェクト間の依存関係

```
SimpleCAD.Demo.WPF ──────────┐
                             │
                             ├──> SimpleCAD.Controls.WPF ──┐
                             │                              │
SimpleCAD.Demo.WinForms ─────┤                              │
                             │                              ├──> SimpleCAD.Core ──> Geometry
                             └──> SimpleCAD.Controls.WinForms ┘
```

### 各プロジェクトの役割

#### SimpleCAD.Core (.NET Standard 2.0)
- **目的**: プラットフォーム非依存の共通ロジック
- **含む機能**:
  - エンティティモデル（GeometryEntity とその派生クラス）
  - ツールロジック（描画・編集・寸法ツールのビジネスロジック）
  - サービス（ファイルI/O、選択管理、スナップ計算）
  - 描画抽象化（IRenderer インターフェース）
- **参照**: Geometry, MathNet.Numerics

#### SimpleCAD.Controls.WPF (.NET 9.0-windows)
- **目的**: WPF向けのUI実装
- **含む機能**:
  - CADCanvas (WPF UserControl)
  - WPFRenderer (DrawingContext による描画)
  - WPF固有のイベント処理
  - ContextMenu (WPF版)
- **参照**: SimpleCAD.Core, Geometry

#### SimpleCAD.Controls.WinForms (.NET 9.0-windows)
- **目的**: WinForms向けのUI実装
- **含む機能**:
  - CADCanvas (WinForms UserControl)
  - GDIRenderer (GDI+ による描画)
  - WinForms固有のイベント処理
  - ContextMenu (WinForms版)
- **参照**: SimpleCAD.Core, Geometry

#### デモアプリケーション
- **SimpleCAD.Demo.WPF**: WPF版の使用例
- **SimpleCAD.Demo.WinForms**: WinForms版の使用例

## 🔄 実装フェーズ

### Phase 0: プロジェクト構成（準備）
- [x] SimpleCADプロジェクト作成
- [ ] SimpleCAD.Core プロジェクト作成
- [ ] SimpleCAD.Controls.WPF プロジェクト作成
- [ ] SimpleCAD.Controls.WinForms プロジェクト作成
- [ ] SimpleCAD.Demo.WPF プロジェクト作成
- [ ] SimpleCAD.Demo.WinForms プロジェクト作成
- [ ] プロジェクト参照の設定
- [ ] Geometry ライブラリへの参照追加

### Phase 1: Core層 - 基本インフラ
- [ ] IRenderer インターフェース実装
- [ ] GeometryEntity 基底クラス実装
- [ ] CurveEntity, LineEntity, ArcEntity 実装
- [ ] CADCanvasViewModel 実装（共通ViewModel）
- [ ] SelectionService 実装
- [ ] SnapService 実装

### Phase 2: WPF層 - 基本UI
- [ ] CADCanvas (WPF UserControl) 実装
- [ ] WPFRenderer 実装
- [ ] GridRenderer (WPF版) 実装
- [ ] グリッドスナップ機能
- [ ] パン・ズーム機能
- [ ] 座標表示オーバーレイ
- [ ] WPFデモアプリ作成

### Phase 3: WinForms層 - 基本UI
- [ ] CADCanvas (WinForms UserControl) 実装
- [ ] GDIRenderer 実装
- [ ] GridRenderer (WinForms版) 実装
- [ ] グリッドスナップ機能
- [ ] パン・ズーム機能
- [ ] 座標表示オーバーレイ
- [ ] WinFormsデモアプリ作成

### Phase 4: Core層 - 描画ツール
- [ ] IDrawingTool インターフェース
- [ ] CurveTool (NURBS曲線) 実装
- [ ] LineTool (直線) 実装
- [ ] ArcTool (円弧) 実装
- [ ] エンティティ管理（追加・削除・選択）

### Phase 5: UI層 - 描画ツールUI連携
- [ ] WPF版: ツールUI統合
- [ ] WinForms版: ツールUI統合
- [ ] 右クリックコンテキストメニュー（WPF）
- [ ] 右クリックコンテキストメニュー（WinForms）

### Phase 6: Core層 - 編集ツール
- [ ] OffsetTool 実装
- [ ] FilletTool 実装
- [ ] ExtendTool 実装
- [ ] TrimTool 実装

### Phase 7: Core層 - 寸法ツール
- [ ] DimensionEntity 実装
- [ ] HorizontalDimensionTool 実装
- [ ] VerticalDimensionTool 実装
- [ ] RadiusDimensionTool 実装
- [ ] DiameterDimensionTool 実装
- [ ] AngleDimensionTool 実装
- [ ] LeaderTool 実装

### Phase 8: Core層 - 解析・ファイル機能
- [ ] 交点自動表示機能
- [ ] 内接円計算機能
- [ ] 距離測定ツール
- [ ] FileService (JSON保存/開く) 実装
- [ ] SVGエクスポート機能

### Phase 9: UI改善・仕上げ
- [ ] UIオーバーレイ（両プラットフォーム）
- [ ] キーボードショートカット（両プラットフォーム）
- [ ] 元に戻す/やり直し機能
- [ ] パフォーマンス最適化
- [ ] ドキュメント整備（API.md, USAGE.md）

## 🎨 デザインガイドライン

### カラースキーム
- **背景**: `#FFFFFF` (白)
- **メジャーグリッド**: `#CCCCCC` (濃いグレー)
- **マイナーグリッド**: `#F0F0F0` (薄いグレー)
- **X軸**: `#FF0000` (赤)
- **Y軸**: `#0000FF` (青)
- **図形デフォルト**: `#000000` (黒)
- **選択時**: `#FF6600` (オレンジ)
- **スナップポイント**: `#00FF00` (緑)

### フォント
- **座標表示**: Consolas, 12pt
- **寸法値**: Arial, 10pt
- **ツールヒント**: Segoe UI, 11pt

### 線幅
- **メジャーグリッド**: 0.5px
- **マイナーグリッド**: 0.2px
- **座標軸**: 1.5px
- **図形デフォルト**: 1.0px
- **選択時**: 2.0px

## 🔗 依存ライブラリ

### SimpleCAD.Core
- **Geometry** - NURBS計算ライブラリ
- **MathNet.Numerics** - 線形代数計算
- **NumericalOptimization** - 数値最適化
- **.NET Standard 2.0**

### SimpleCAD.Controls.WPF
- **SimpleCAD.Core**
- **Geometry**
- **.NET 9.0-windows**

### SimpleCAD.Controls.WinForms
- **SimpleCAD.Core**
- **Geometry**
- **.NET 9.0-windows**

## 📄 ライセンス

MIT License

## 🚀 使用例

### WPF版の使用例

**XAML (MainWindow.xaml):**
```xaml
<Window x:Class="SimpleCAD.Demo.WPF.MainWindow"
        xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
        xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
        xmlns:cad="clr-namespace:SimpleCAD.Controls.WPF;assembly=SimpleCAD.Controls.WPF"
        Title="SimpleCAD WPF Demo" Height="600" Width="800">
    <cad:CADCanvas x:Name="cadCanvas" />
</Window>
```

**C# (MainWindow.xaml.cs):**
```csharp
using System.Windows;
using SimpleCAD.Controls.WPF;
using SimpleCAD.Core.Entities;
using Geometry;

namespace SimpleCAD.Demo.WPF;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();

        // プログラムから操作
        cadCanvas.GridVisible = true;
        cadCanvas.SnapToGrid = true;
        cadCanvas.GridSpacing = 10;

        // イベントハンドラ登録
        cadCanvas.EntityAdded += (s, e) =>
        {
            Console.WriteLine($"Entity added: {e.Entity.Name}");
        };

        // NURBS曲線を追加
        var points = new ObjPoints();
        points.AddPoint(0, 0, 0);
        points.AddPoint(10, 10, 0);
        points.AddPoint(20, 0, 0);

        var curve = new ObjCurve();
        curve.SetCurve(points);

        cadCanvas.AddEntity(new CurveEntity
        {
            Curve = curve,
            Color = System.Drawing.Color.Black,
            Thickness = 1.0
        });
    }
}
```

### WinForms版の使用例

**C# (Form1.cs):**
```csharp
using System;
using System.Drawing;
using System.Windows.Forms;
using SimpleCAD.Controls.WinForms;
using SimpleCAD.Core.Entities;
using Geometry;

namespace SimpleCAD.Demo.WinForms;

public partial class Form1 : Form
{
    private CADCanvas cadCanvas;

    public Form1()
    {
        InitializeComponent();
        InitializeCADCanvas();
    }

    private void InitializeCADCanvas()
    {
        // CADCanvasコントロールを作成
        cadCanvas = new CADCanvas
        {
            Dock = DockStyle.Fill,
            GridVisible = true,
            SnapToGrid = true,
            GridSpacing = 10
        };

        // フォームに追加
        this.Controls.Add(cadCanvas);

        // イベントハンドラ登録
        cadCanvas.EntityAdded += (s, e) =>
        {
            Console.WriteLine($"Entity added: {e.Entity.Name}");
        };

        // NURBS曲線を追加
        var points = new ObjPoints();
        points.AddPoint(0, 0, 0);
        points.AddPoint(10, 10, 0);
        points.AddPoint(20, 0, 0);

        var curve = new ObjCurve();
        curve.SetCurve(points);

        cadCanvas.AddEntity(new CurveEntity
        {
            Curve = curve,
            Color = Color.Black,
            Thickness = 1.0
        });
    }
}
```

### 共通のプログラマティック操作

**両プラットフォームで同じAPIを使用:**
```csharp
// エンティティ追加
cadCanvas.AddEntity(entity);

// エンティティ削除
cadCanvas.RemoveEntity(entity);

// 全クリア
cadCanvas.Clear();

// ズームフィット
cadCanvas.ZoomToFit();

// ビューリセット
cadCanvas.ResetView();

// ファイル保存
cadCanvas.SaveToJson("drawing.json");

// ファイル読込
cadCanvas.LoadFromJson("drawing.json");

// SVGエクスポート
cadCanvas.ExportToSvg("drawing.svg");

// PNGエクスポート
cadCanvas.ExportToPng("drawing.png", 1920, 1080);
```

---

**作成日**: 2025-10-04
**最終更新**: 2025-10-04
**バージョン**: 1.0.0

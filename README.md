# PropellerDataset


プロペラ形状データを扱う .NET ライブラリ。
- **入出力**: JSON によるシリアライズ / デシリアライズ
- **用途**: プロペラのピッチなど各種メタデータの参照、他ツールとのデータ連携


## 動作環境
- .NET SDK: 8 もしくは 9
- OS: Windows / macOS / Linux


## クイックスタート
```bash
# 取得後（またはテンプレ生成後）
dotnet restore
# コード整形（差分なし確認）
dotnet format --verify-no-changes
# ビルド & テスト
dotnet build -c Release
dotnet test -c Release --no-build
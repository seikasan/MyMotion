# TweenPerformance の再現

LitMotion の README に載っている [AnnulusGames/TweenPerformance](https://github.com/AnnulusGames/TweenPerformance) の計測を、MyMotion を加えて再現したコードです。

## 本家と同じにしたこと

- **各ライブラリの Tween の書き方**: 本家の `Benchmarks/FloatProperty` と `Benchmarks/Position` に合わせた。MyMotion は、LitMotion と同じ呼び出しで `LMotion` を `MMotion` に置き換えている。
- **件数**: float は 64,000 件、`transform.position` は 50,000 件。
- **開始の測り方**: 容量を確保した後、全件を作る `Run()` を1回だけ測る（`Measure.Method` の `MeasurementCount(1)` と同じ）。
- **更新の測り方**: `Run()` の後、3 フレームの warmup に続けて 600 フレームのフレーム時間を取り、その中央値を使う（`Measure.Frames` と同じ）。
- **GC の測り方**: `GC.GetTotalMemory(true)` の差を件数で割る（`RunGCAlloc` と同じ）。値は数 KB 単位でしか変わらないため、1件あたり 1 byte 未満は 0 B と表示している。

## 本家と違うこと

- **ハーネス**: Performance Testing API の PlayMode テストではなく、Release IL2CPP Player を `-batchmode -nographics` で起動して測る。ライブラリと項目ごとに別プロセスで5回実行し、中央値をとる。
- **ECS**: 計測用プロジェクトに Unity Entities を入れていないため、本家の ECS 無効化処理は不要。
- **環境**: Windows 11、Core i7-1185G7、Unity 6000.6.8f1。
- **バージョン**: LitMotion 2.0.2、PrimeTween 1.5.3、LeanTween 2.50。DOTween と FastTweener は、MyMotion の開発リポジトリに同梱していたソース。本家は LitMotion 1.0.0、Apple M2、Unity 2022.3 で測っているため、絶対値は比べられない。

## ファイル

- `TweenPerformanceRunner.cs`: 計測本体。`-tpLibrary <名前> -tpCase <項目> -tpOut <CSV>` で1項目を測って終了する。
- `BenchBuilder.cs`: Release IL2CPP の Windows Player を作る Editor スクリプト（`Assets/Bench/Editor/` に置く）。
- `manifest.json`: 計測用プロジェクトの `Packages/manifest.json`。PrimeTween は Asset Store 版の tgz を、DOTween・LeanTween・FastTweener はソースを `Assets/` に置く。

項目は `startup_float`、`update_float`、`startup_position`、`update_position`、`gc_position` の5つ。

結果: [docs/results/tweenperformance-20261125.csv](../../results/tweenperformance-20261125.csv)

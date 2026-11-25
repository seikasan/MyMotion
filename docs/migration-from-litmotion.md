# LitMotion からの移行

MyMotion は LitMotion 2.0 の API を同じ形で提供します。メソッド名、引数、既定値、実行の順序は LitMotion に合わせています。

## 置き換え

| LitMotion | MyMotion |
|---|---|
| `using LitMotion;` | `using MyMotion;` |
| `using LitMotion.Extensions;` | `using MyMotion.Extensions;` |
| `using LitMotion.Adapters;` | `using MyMotion.Adapters;` |
| `LMotion.Create(...)` | `MMotion.Create(...)` |
| `LMotion.Punch.Create(...)` | `MMotion.Punch.Create(...)` |
| `LMotion.Shake.Create(...)` | `MMotion.Shake.Create(...)` |
| `LMotion.String.Create64Bytes(...)` など | `MMotion.String.Create64Bytes(...)` など |
| `LSequence.Create()` | `MSequence.Create()` |
| `LitMotionAnimation` コンポーネント | `MyMotionAnimation` コンポーネント |
| Window > LitMotion > Motion Debugger | Window > MyMotion > Motion Debugger |

`MotionHandle`、`MotionBuilder`、`Ease`、`LoopType`、`DelayType`、`MotionScheduler`、`ManualMotionDispatcher`、`CompositeMotionHandle`、`IMotionAdapter<TValue, TOptions>` などの型名は同じです。

## 挙動の違い

### builder を何度でも使える

LitMotion の builder は pool から借りた buffer を持つため、一度 Bind すると再利用できません。MyMotion の `MotionBuilder` は不変の値なので、同じ設定から何度でも Bind できます。

```csharp
var builder = MMotion.Create(0f, 1f, 0.5f).WithEase(Ease.OutQuad);
builder.Bind(x => a = x);
builder.Bind(x => b = x); // MyMotion では有効
```

`Dispose()` は `using` 構文との互換のためにあり、何もしません。

### MotionHandle が参照を持たない

MyMotion の `MotionHandle` は、LitMotion と同じく managed の参照を持たない値です。Dispose された `ManualMotionDispatcher` で作った handle は、無効な handle として扱われます（`IsActive()` は false、`TryCancel()` は false を返します）。

### Punch・Shake・独自 Adapter の builder

LitMotion と同じく、`MMotion.Punch`、`MMotion.Shake`、`MMotion.String`、独自 Adapter の型付き builder（`MotionBuilder<TValue, TOptions, TAdapter>`）は、Transform、Rigidbody、uGUI、TextMeshPro などの拡張メソッドでそのまま Bind できます。

```csharp
MMotion.Punch.Create(Vector3.zero, Vector3.one, 0.5f)
    .BindToLocalPosition(transform);
```

### 動作環境

MyMotion は Unity 6000.5 以降、Burst 1.8.26、Collections 2.6.3、Mathematics 1.3.3 が必要です。

## 性能の違い

LitMotion と同じ使い方のまま、更新と作成が速くなります。また、LitMotion は一度伸びた storage の容量全体を毎フレーム走査するため、Tween が 0 本でもコストが残ります。MyMotion は動いている Tween だけを処理します。詳しくは[既存ライブラリとの性能比較](performance.md)を参照してください。

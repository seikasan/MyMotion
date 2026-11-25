# 最初の Tween

MMotion.Create で開始値、終了値、再生時間を指定し、WithEase で補間方法を選び、Bind で値を適用します。再生時間の単位は秒です。次の例は `ScaleTween.cs` という名前の C# ファイルに置きます。

~~~csharp
using MyMotion;
using UnityEngine;

public sealed class ScaleTween : MonoBehaviour
{
    private void Start()
    {
        MMotion.Create(0f, 1f, 0.5f)
            .WithEase(Ease.OutQuad)
            .Bind(value => transform.localScale = Vector3.one * value)
            .AddTo(gameObject);
    }
}
~~~

Bind は MotionHandle を返します。既定では Bind の呼び出し中に開始値を適用します。必要なら `.WithImmediateBind(false)` でこの初期適用を無効にできます。例では AddTo(gameObject) を付け、対象の GameObject が破棄されたときに Tween をキャンセルします。明示的に scheduler を指定しない場合、Tween は Update の PlayerLoop で進みます。

Bind の callback で例外が発生した場合、既定ではエラーを通知して Tween の再生を続けます。エラー時にキャンセルする場合は、Bind より前に `.WithCancelOnError()` を指定します。

MMotion.Create は float、double、int、long、Vector2、Vector3、Vector4、Quaternion、Color、Rect を受け取ります。カスタム型の補間では IMotionAdapter<TValue, TOptions> を実装して汎用 Create を使います。


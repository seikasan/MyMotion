# MyMotion

MyMotion is a high-performance, zero-allocation tween library for Unity with a LitMotion-compatible API.

MyMotion は、LitMotion と同じ形の API を持つ、高速で GC アロケーションのない Unity 向け Tween ライブラリです。

```csharp
using MyMotion;
using MyMotion.Extensions;

MMotion.Create(Vector3.zero, Vector3.one, 0.5f)
    .WithEase(Ease.OutQuad)
    .BindToLocalScale(transform);
```

- Documentation / ドキュメント: https://github.com/seikasan/MyMotion/tree/main#readme
- Requires Unity 6000.6 or later / Unity 6000.6 以降が必要です。

## Installation / インストール

```text
https://github.com/seikasan/MyMotion.git?path=/src/MyMotion/Assets/MyMotion
```

## License / ライセンス

MyMotion is released under the [MIT License](LICENSE.md). Code adapted from LitMotion is listed in [ThirdPartyNotices.md](ThirdPartyNotices.md).

MyMotion は [MIT License](LICENSE.md) で配布します。LitMotion から取り入れたコードは [ThirdPartyNotices.md](ThirdPartyNotices.md) に記載しています。

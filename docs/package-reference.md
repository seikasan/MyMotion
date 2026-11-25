# パッケージ構成と依存関係

## パッケージ情報

| 項目 | 値 |
| --- | --- |
| Package ID | com.seikasan.mymotion |
| Version | 1.0.0 |
| Unity 最低バージョン | 6000.5 |
| Unity 検証環境 | 6000.6.8f1 |
| License | MIT |

## 必須依存関係

package.json は次の依存関係を宣言しています。Unity Package Manager が MyMotion と一緒に解決します。

| Package ID | Version |
| --- | --- |
| com.unity.burst | 1.8.26 |
| com.unity.collections | 2.6.3 |
| com.unity.mathematics | 1.3.3 |
| com.unity.modules.audio | 1.0.0 |
| com.unity.modules.physics | 1.0.0 |
| com.unity.modules.physics2d | 1.0.0 |

組み込みモジュールの audio、physics、physics2d は、Runtime の拡張メソッド（`AudioSource`、`AudioMixer`、`Rigidbody`、`Rigidbody2D` への Bind）が使います。

## 連携用パッケージ

次の連携アセンブリもパッケージに含まれます。表の Package ID は各アセンブリの `versionDefines` が参照する ID です。これらの依存関係は MyMotion の `package.json` には含まれないため、連携を使う場合は対応するパッケージをプロジェクトへ追加してください。

| 連携 | 必要な Package ID |
| --- | --- |
| uGUI | com.unity.ugui |
| TextMeshPro | com.unity.ugui |
| UI Toolkit | Unity の UIElements モジュール（com.unity.modules.uielements） |
| SRP Rendering | com.unity.render-pipelines.core |
| Visual Effect Graph | com.unity.visualeffectgraph |
| R3 | com.cysharp.r3 |
| UniRx | com.neuecc.unirx |
| UniTask | com.cysharp.unitask |

`MyMotion.Animation.asmdef` は uGUI、TextMeshPro、Rendering の連携アセンブリを参照します。連携パッケージの一部を入れない構成で `MyMotion.Animation` がコンパイルできるかは、すべての組み合わせを検証していません。Core の必須依存関係は上の `package.json` 宣言を参照してください。

## 含まれる主なアセンブリ

- MyMotion.Runtime: Tween の作成、設定、再生、値のバインドを提供します。
- MyMotion.Editor: Motion Debugger など Editor 用の機能を提供します。
- MyMotion.Animation: Animation Component と Inspector 用の Editor 機能を提供します。
- MyMotion.Extensions.*: uGUI、TextMeshPro、UI Toolkit、Rendering、Visual Effect Graph、R3、UniRx、UniTask との連携を含みます。

MyMotion.Extensions.* の一部は、対応する Unity または外部パッケージがプロジェクトにある場合にコンパイルされます。R3、UniRx、UniTask などの外部パッケージは、必要な連携を使う場合に別途追加してください。

## ライセンス

MyMotion のライセンスはパッケージ内の [LICENSE.md](../src/MyMotion/Assets/MyMotion/LICENSE.md) を参照してください。LitMotion 由来のコードとテストの著作権・ライセンス表記は [ThirdPartyNotices.md](../src/MyMotion/Assets/MyMotion/ThirdPartyNotices.md) に記載しています。

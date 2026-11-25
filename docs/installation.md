# インストール

MyMotion は Unity Package Manager から Git URL でインストールできます。最低対応バージョンは Unity 6000.6 です。Unity Editor 6000.6.8f1 で開発・確認しています。

## Package Manager から追加する

Unity のメニューから Window > Package Manager を開き、Git URL で追加します。

~~~text
https://github.com/seikasan/MyMotion.git?path=/src/MyMotion/Assets/MyMotion
~~~

Unity Package Manager の Git URL では、サブフォルダーを指す path を #revision より前に指定します。詳しくは [Unity Manual: Git dependencies](https://docs.unity3d.com/6000.0/Documentation/Manual/upm-git.html) を参照してください。

## manifest.json に追加する

プロジェクトの Packages/manifest.json に次の依存関係を追加します。

~~~json
{
  "dependencies": {
    "com.seikasan.mymotion": "https://github.com/seikasan/MyMotion.git?path=/src/MyMotion/Assets/MyMotion"
  }
}
~~~

同じ名前の依存関係がすでにある場合は、その値を Git URL に置き換えます。Unity Package Manager が package.json に記載された依存関係も解決します。

# UnityModule
Particle System のように、要素を選択して機能を追加できるライブラリを公開しました。

<img src="https://cdn-ak.f.st-hatena.com/images/fotolife/h/hacchi_man/20200506/20200506235438.png" width="300" alt="UnityModule">

# 使い方
## 1. ModuleControl の作成
まず、ModuleControlAbstract を継承したクラスを作成します。
このクラスは MonoBehaviour を継承しており、各 Module のセットアップを管理します。

```csharp
using System;

public class SampleModuleControl : ModuleControlAbstract
{
    private void Awake()
    {
        // SetUp を呼び出すことで、各 Module の SetUp 処理を実行します
        SetUp();
    }
 
    // Module として使うベースクラスを指定する
    public override Type PartsType()
    {
        return typeof(SampleModuleAbstract);
    }
}
```

## 2. Module のベースクラスの作成
次に、Module のベースクラスとなる ModuleAbstract を継承したクラスを作成します。

```csharp
public abstract class SampleModuleAbstract : ModuleAbstract
{
    // Owner にはコントローラー（ModuleControl）を取得します
    protected SampleModuleControl Owner => GetController<SampleModuleControl>();
}
```

## 3. 実際の Module の作成
先ほど作成した SampleModuleAbstract を継承し、各機能を実装します。

#### X 座標移動 Module
```csharp
using UnityEngine;

public class SampleModuleMoveX : SampleModuleAbstract
{
    [SerializeField]
    private int _value;
    
    private float _defaultX;
    private bool _isPong;

    protected override void OnSetUp()
    {
        _defaultX = Owner.transform.localPosition.x;
    }

    protected override void OnUpdate()
    {
        var pos = Owner.transform.localPosition;
        if (_isPong)
        {
            pos.x -= Time.deltaTime;
            if (pos.x < _defaultX - _value)
                _isPong = !_isPong;
        }
        else
        {
            pos.x += Time.deltaTime;
            if (pos.x > _defaultX + _value)
                _isPong = !_isPong;
        }
        Owner.transform.localPosition = pos;
    }
}
```

#### Y 座標移動 Module

```csharp
using UnityEngine;

public class SampleModuleMoveY : SampleModuleAbstract
{
    [SerializeField]
    private int _value;
    
    private float _defaultY;
    private bool _isPong;

    protected override void OnSetUp()
    {
        _defaultY = Owner.transform.localPosition.y;
    }

    protected override void OnUpdate()
    {
        var pos = Owner.transform.localPosition;
        if (_isPong)
        {
            pos.y -= Time.deltaTime;
            if (pos.y < _defaultY - _value)
                _isPong = !_isPong;
        }
        else
        {
            pos.y += Time.deltaTime;
            if (pos.y > _defaultY + _value)
                _isPong = !_isPong;
        }
        Owner.transform.localPosition = pos;
    }
}
```

#### 4. Inspector 上での表示と機能の有効化
SampleModuleAbstract を継承した各 Module は Inspector 上に表示され、
チェックボックスをクリックすることでその機能が有効になります。

例：X 移動 Module にチェックが入っている場合、オブジェクトは X 座標のみ移動します。
<img src="https://cdn-ak.f.st-hatena.com/images/fotolife/h/hacchi_man/20200507/20200507001917.png" width="300" alt="Module Inspector 表示">
<img src="https://cdn-ak.f.st-hatena.com/images/fotolife/h/hacchi_man/20200507/20200507001838.png" width="300" alt="有効化状態">

実際にはコンポーネントが付与されていますが、HideFlag により Inspector 上では非表示にすることで、
擬似的に Module として機能させています。

# ライセンス
本プロジェクトは MIT License の下でライセンスされています。<br>
詳細については、LICENSE ファイルをご覧ください。

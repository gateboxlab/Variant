# Gatebox.Variant

Gatebox.Varaint は JSON のデータをシリアライズ・デシリアライズではなく、C#として自然な形で扱うことを目的とした Unity 向けのライブラリです。
特に Unity に依存する箇所があるわけではありませんが、Unity から利用しやすいようにしているつもりです。

# 導入

以下の URL を Package Manager の `Add package from git URL` で指定してください。
```
https://github.com/gateboxlab/Variant.git?path=Gatebox.Variant
```

# 目的

Unity における JSON の扱いは `JsonUtility` による シリアライズ・デシリアライズが最も手軽ですが、`JsonUtility` は特定の型と JSON 文字列間の変換であるため、JSON をどの型にマッピングするかはあらかじめわかっている必要があります。

内容が未知であったり、異なる構造を持つデータが同じ JSON に格納されている場合などには JSON を JSON の構造としてパースする必要がありますが Unity にはその標準的な手法が用意されていません。

`Gatebox.Varaint` はそのような要求に対して、JSON を JSON のデータとして扱うことを目的としたライブラリです。あまり大規模ではない、柔軟なデータを JSON で扱う場合を想定しています。

特定の型との相互変換はそれなりにサポートはしますが、本来の目的ではありません。

# 主なクラス

JSON を扱う上で利用するクラス・構造体は以下となります。

- JVariant
- JObject
- JArray

また以下のクラスについても理解が必要です。

- JVariantTag

## JVariant

JVaraint は「JSON のような値を持てる class」です。

各種のプリミティブ、string 等からの暗黙の変換を持ち、AsInt(), AsString() 等のメソッドでその内容を特定の型で取得します。

インデクサが int, string に対してオーバーロードされており、JObject, JArray が格納されている場合はインデクサを利用してその内容にアクセスできます。

基本的に、内容の型が違う場合は例外を投げるのではなく、それっぽく変換するようになっています。例えば int の 10 が格納されている JVariant に対して AsString() を行った結果は例外を投げるのではなく "10" を返却します。

```csharp
JVariant i = 10;
JVariant s = "string";

JVariant v = new JVariant();
v["a"] = "a";

int i = i.AsInt();
string s = s.AsString()
```

### JSON 文字列との変換

JSON 文字列を JVariant にするには JVaraint.ParseJSON() を利用します。
JVaraint を JSON 文字列に変換するには ToJSON() を利用します。（ToString()ではないので注意してください。）

### 注意点

JVariant は参照型であり、null と、null を指す JVarariant が別のものとして存在するので注意してください。

## JObject

JObject は「JSON の Object を示す struct」です。

IDictionary<string, JVariant> を実装しており、JVariant の Dictionary として自然に扱うことができるようになっています。

JObject は JVariant とは異なり、それ自身は内部データの参照のみを持つ struct です。厳密には空の内部データを参照している状態と何も参照していない状態は別のものですが、通常の利用ではそれを意識する必要はないようになっています。

```csharp
// IDictionary<string, JVariant> として利用できる。
var obj = new JObject();
obj.Add("A", 1);
obj["B"] = "string";

foreach (var p in obj)
{
	Console.WriteLine($"{p.Key} = {p.Value}");
}

// コレクション初期化子にも対応している。
obj = new JObject(){
    {"A", 1},
    {"B", "string"},
    {"C", true},
};
```

## JArray

JArray は「JSON の 配列 を示す struct」です。

IList<JVariant> を実装しており、JVariant の List として自然に扱うことができるようになっています。

JArray は JVariant とは異なり、それ自身は内部データの参照のみを持つ struct です。厳密には空の内部データを参照している状態と何も参照していない状態は別のものですが、通常の利用ではそれを意識する必要はないようになっています。

## JVariantTag

JVariantTag は内部に JVariant のみを持つ struct です。

普段のコードではプリミティブが JVariant に変換されることは、簡潔なコードを書くことに役立つのですが、暗黙の変換はオーバーロードをしづらくします。そのため、「JVariant に変換可能な何らかの型」ではなく 「JVariant」 を要求したい場合は JVariantTag を引数とします。

JVariantTag 自身は「不変な JVariant」として扱うことができるようメンバを持っています。

# 型変換

Gatebox.Varaint はシリアライズ/デシリアライズを意図するものではありませんが、いくつかのルールにしたがって JVarainat と任意の型を変換することができるようになっています。変換には JVariant の static メンバである Create と、インスタンスメンバである As&lt;T&gt;() を利用します。

```csharp
var x = new MyClass();

// JVariant への変換。
var v = JVariant.Create(x);

// JVariant からの変換
x = v.As<MyClass>()
```

## IJVariantConvertible

JVariant.Create() および JVariant.As&lt;T&gt;() に対応させる一つの方法として以下の方法があります。
- IJVariantConvertible を実装する
- JVariantTag 受けるコンストラクタを提供する。

```csharp
public class MyClass : IJVariantConvertible
{
    private readonly JObject m_Body;

    public MyClass(JVariantTag v)
    {
        m_Body = v.AsObject();
    }

    public JVariant AsVariant()
    {
        return m_Body;
    }
}
```

## ConvertTrait

JVariant.Create() および JVariant.As&lt;T&gt;() に対応させるもう一つの方法として以下の方法があります。
- Gatebox.Variant.ConvertTrait もしくは Gatebox.Variant.ConvertTrait&lt;T&gt; を継承したクラスを作る。
- 属性 Gatebox.Variant.ConvertTraitAttribute をつける

```csharp
using Gatebox.Variant;

[ConvertTrait(typeof(MyClass))]
public class MyClassConvertTraits : ConvertTrait<MyClass>
{
	public override MyClass ConvertVariant(JVariant variant)
	{
		var r = new MyClass();
		r.Initialize(variant);
		return r;
	}

	public override JVariant CreateVariant(MyClass v)
	{
		JVariant v = new JVariant();
		v["A"] = v.A;
		v["B"] = v.B;
		return v;
	}
}
```





using System;
using System.Collections;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Text;
using Gatebox.Variant.Internal;
using Gatebox.Variant.Parser.U16;
using Gatebox.Variant.Parser.U8;

using SystemDebug = System.Diagnostics.Debug;


namespace Gatebox.Variant
{

	/// <summary>
	/// JVariant の拡張メソッド
	/// </summary>
	public static class JVariantExtensions
	{
		/// <summary>
		/// 格納されている型
		/// </summary>
		public static VariantType GetVariantType(this JVariant v) => v?.VariantType ?? VariantType.Null;


		/// <summary>
		/// Null であるかどうか
		/// <para>
		/// C# としての null と JVariant の Null は別物ですが、両者を同様に扱います。
		/// </para>
		/// </summary>
		public static bool IsNull(this JVariant v) => v == null || v.VariantType == VariantType.Null;


		/// <summary>
		/// 複合型であるとき true.
		/// </summary>
		public static bool IsComposite(this JVariant v)
		{
			var t = v.GetVariantType();
			return (t == VariantType.Array || t == VariantType.Object);
		}

		/// <summary>
		/// Type が Boolean のとき true.
		/// </summary>
		public static bool IsBoolean(this JVariant v) => (v.GetVariantType() == VariantType.Boolean);

		/// <summary>
		/// Type が Integer か Float のとき true.
		/// </summary>
		public static bool IsNumber(this JVariant v)
		{
			var t = v.GetVariantType();
			return (t == VariantType.Integer || t == VariantType.Float);
		}

		/// <summary>
		/// Type が String のとき true.
		/// </summary>
		public static bool IsString(this JVariant v) => (v.GetVariantType() == VariantType.String);

		/// <summary>
		/// Type が Array のとき true.
		/// </summary>
		public static bool IsArray(this JVariant v) => (v.GetVariantType() == VariantType.Array);

		/// <summary>
		/// Type が Object のとき true.
		/// </summary>
		public static bool IsObject(this JVariant v) => (v.GetVariantType() == VariantType.Object);


		/// <summary>
		/// 内容が ExceptEmpty(空) であるかどうかを返す。
		/// <para>
		/// ExceptEmpty の意味は内容の型によって異なります。<see cref="JVariant.IsEmpty">IsEmpty</see> を参照してください。
		/// </summary>
		public static bool IsEmpty(this JVariant v)
		{
			return v?.IsEmpty ?? true;
		}

		

		/// <summary>
		/// Bool 値を返す。
		/// <para>
		/// bool 以外が格納されていた場合の挙動は <see cref="JVariant.BoolValue">BoolValue</see> を参照してください。</para>
		/// <para>
		/// null の場合は false を返します。
		/// 早めに落としてほしい場合は <see cref="JVariant.BoolValue">BoolValue</see> を利用してください。</para>
		/// </summary>
		public static bool AsBool(this JVariant v)
		{
			return v?.BoolValue ?? false;
		}

		/// <summary>
		/// int 値を返す。
		/// <para>
		/// 整数以外が格納されていた場合の挙動は <see cref="JVariant.IntValue">IntValue</see> を参照してください。</para>
		/// <para>
		/// null の場合は 0 を返します。</para>
		/// </summary>
		public static int AsInt(this JVariant v)
		{
			return v?.IntValue ?? 0;
		}

		/// <summary>
		/// long 値を返す。
		/// <para>
		/// 整数以外が格納されていた場合の挙動は <see cref="JVariant.LongValue">LongValue</see> を参照してください。</para>
		/// <para>
		/// null の場合は 0 を返します。</para>
		/// </summary>
		public static long AsLong(this JVariant v)
		{
			return v?.LongValue ?? 0;
		}

		/// <summary>
		/// float 値を返す。
		/// <para>
		/// Number 以外が格納されていた場合の挙動は <see cref="JVariant.FloatValue">FloatValue</see> を参照してください。</para>
		/// <para>
		/// null の場合は 0.0f を返します。</para>
		/// </summary>
		public static float AsFloat(this JVariant v)
		{
			return v?.FloatValue ?? 0.0f;
		}

		/// <summary>
		/// double 値を返す。
		/// <para>
		/// Number 以外が格納されていた場合の挙動は <see cref="JVariant.DoubleValue">DoubleValue</see> を参照してください。</para>
		/// <para>
		/// null の場合は 0.0 を返します。</para>
		/// </summary>
		public static double AsDouble(this JVariant v)
		{
			return v?.DoubleValue ?? 0.0;
		}

		/// <summary>
		/// 文字列値を返す。
		/// <para>
		/// 文字列以外が格納されていた場合の挙動は <see cref="JVariant.StringValue">StringValue</see> を参照してください。</para>
		/// <para>
		/// null の場合は string.ExceptEmpty を返します。</para>
		/// </summary>
		public static string AsString(this JVariant v)
		{
			return v?.StringValue ?? string.Empty;
		}

		/// <summary>
		/// 配列値を返す。
		/// <para>
		/// 内部が配列以外であった場合の挙動は <see cref="JVariant.ArrayValue">ArrayValue</see> を参照してください。</para>
		/// <para>
		/// null の場合は空の配列を返します。</para>
		/// </summary>
		public static JArray AsArray(this JVariant v)
		{
			return v?.ArrayValue ?? new JArray();
		}

		/// <summary>
		/// Object 値を返す。
		/// <para>
		/// 内部が Object 以外であった場合の挙動は <see cref="JVariant.ObjectValue">ObjectValue</see> を参照してください。</para>
		/// <para>
		/// null の場合は空の Object を返します。</para>
		/// </summary>
		public static JObject AsObject(this JVariant v)
		{
			return v?.ObjectValue ?? new JObject();
		}


		/// <summary>
		/// 引数が null でなければそのまま返す、null ならば Null を示す JVariant を返す。
		/// </summary>
		public static JVariant AsVariant(this JVariant v)
		{
			return v ?? new JVariant();
		}

		/// <summary>
		/// JVariant を指定した型に変換する。
		/// <para>
		/// 変換可能な型は以下の通りです。
		/// <list type="table">
		/// <listheader>
		/// <term>対象型</term>
		/// <description>変換方法</description>
		/// </listheader>
		/// <item>
		///		<term>JVariant 関連型及びプリミティブ、もしくは string.</term>
		///		<description>このクラス（拡張メソッド）の As*** 系のメソッドを参照してください。</description>
		/// </item>
		/// <item>
		///		<term>object</term>
		///		<description>複合型ではない場合はそれを示す形へ、JObject は Expando、 JArray は List&lt;object&gt; へ。</description>
		/// </item>
		/// 
		/// <item>
		///		<term>IJVariantConvertible 実装クラス</term>
		///		<description>JVariantTag を受けるコンストラクタによる</description>
		/// </item>
		/// <item>
		///		<term>配列</term>
		///		<description>中に JArray が入っているものとして各要素を再帰的に変換</description>
		/// </item>
		/// <item>
		///		<term>enum</term>
		///		<term>Number であればそれを enum の値として解釈。string であれば enum の名前として解釈。</term>
		/// </item>
		/// <item>
		///		<term>public なデフォルトコンストラクタがあり、IDictionary&lt;string,&gt; を実装している</term>
		///		<description>中に JObject が入っているものとして各要素を再帰的に変換</description>
		/// </item>
		/// <item>
		///		<term>public なデフォルトコンストラクタがあり、IConnection&lt;&gt; を実装している</term>
		///		<description>中に JArray が入っているものとして各要素を再帰的に変換</description>
		/// </item>
		/// <item>
		///		<term>それ以外</term>
		///		<description>public なデフォルトコンストラクタで構築し、各フィールドをリフレクションで設定することを試みる。</description>
		/// </item>
		/// </list>
		/// </para>
		/// <para>
		/// これらの挙動は <see cref="VariantConverter.Default"/> に対して
		/// <see cref="VariantConverter.RegisterTrait{T}(ConvertTrait{T})">RegisterTrait()</see> を行う、
		/// あるいは <see cref="ConvertTraitAttribute" /> を付与した <see cref="ConvertTrait{T}"/> を実装することで
		/// カスタマイズできます。
		/// <see cref="VariantConverter"/> の各種メソッドを参照してください。</para>
		/// <para>
		/// 失敗時はデフォルトでは T の初期値を返します。
		/// 例外を投げるべき場合は第2引数に true を指定してください。その場合は VariantException を投げます。</para>
		/// </summary>
		public static T As<T>(this JVariant value, bool throws = false)
		{
			if (value is null)
			{
				return default;
			}
			object v = VariantConverter.ConvertVariantFixed(value, typeof(T));
			if (v is not null)
			{
				return (T)v;
			}

			var context = ConvertContext.Acquire();
			try
			{
				return context.Converter.Unmarshal<T>(value);
			}
			catch (VariantException)
			{
				if (throws)
				{
					throw;
				}
				return default;
			}
			finally
			{
				context.Release();
			}
		}

		/// <summary>
		/// T に変換可能であればそれを、変換できない場合は def を返す。
		/// </summary>
		public static T GetOrDefault<T>(this JVariant v, T def = default)
		{
			try
			{
				return v.IsNull() ? def : v.As<T>();
			}
			catch (VariantException)
			{
				return def;
			}
		}

		/// <summary>
		/// JVariant を指定した型に変換する。
		/// <para>
		/// <see cref="As{T}"/> の非ジェネリック版です。
		/// </para>
		/// </summary>
		public static object ConvertTo(this JVariant value, Type type, bool throws = false)
		{
			if (value is null)
			{
				return default;
			}
			object v = VariantConverter.ConvertVariantFixed(value, type);
			if (v is not null)
			{
				return v;
			}

			var context = ConvertContext.Acquire();
			try
			{
				return context.Converter.Unmarshal(value, type);
			}
			catch (VariantException)
			{
				if (throws)
				{
					throw;
				}
				return default;
			}
			finally
			{
				context.Release();
			}
		}


	

		/// <summary>
		/// JVariant をダイナミックオブジェクトに変換する。
		/// </summary>
		public static object ToDynamic(this JVariant v)
		{
			// null は null
			if (v == null)
			{
				return null;
			}

			// 数値。
			// 一回 double でとって整数ピッタリのときは long で返す。
			if (v.IsNumber)
			{
				if( v.VariantType == VariantType.Integer)
				{
					return v.LongValue;
				}

				var d = v.DoubleValue;
				if ((d % 1) == 0)
				{
					return (long)d;
				}
				return d;
			}

			// string
			if (v.IsString)
			{
				return v.StringValue;
			}

			// bool
			if (v.IsBoolean)
			{
				return v.BoolValue;
			}

			// object
			if (v.IsObject)
			{
				var expando = new ExpandoObject();
				var expandoDic = (IDictionary<string, object>)expando;

				foreach (var prop in v.ObjectValue)
				{
					expandoDic.Add(prop.Key, prop.Value.ToDynamic());
				}
				return expando;
			}

			// array
			if (v.IsArray)
			{
				return v.ArrayValue.Select(e => e.ToDynamic()).ToList();
			}

			return null;
		}
	}







	/// <summary>
	/// JSON のような値を持てる型.
	/// <para>
	/// JVariant, JArray, JObject のクラス群は、
	/// それほど大きくないJavaScript的オブジェクトをその構造のままプログラムで扱うことを志向しています。
	/// C# の別のクラスへの変換などが要求される場合は別の手段を用いてください。</para>
	/// <para>
	/// JVariant は JavaScript の変数に相当します。Null Bool値 数値 文字列 配列 オブジェクト を内部に持つことができます。
	/// 配列やオブジェクトに関してもこのクラスで汎用的な操作は提供されますが、
	/// より特化された JArray, JObject が用意されているため、配列、オブジェクトを扱いたい場合はそちらを利用してください。</para>
	/// <para>
	/// JArray は「JVariantのList」、JObject は [string と JVariantのDictionary] として C# から自然に利用できるようになっています。</para>
	/// <para>
	/// 各種のプリミティブ、string, JArray, JObject からの暗黙の変換による生成を提供しています。
	/// 別途それぞれにオーバーロードされたコンストラクタも用意してありますが、
	/// JVariant を要求するシグネチャに、利用者が JVariant を new するコードを書く必要はありません。
	/// ですが、あくまでこの型は参照型なので注意してください。</para>
	/// <para>
	/// このクラスは可変かつ参照型です。
	/// 「Javascript の Null を示す JVariant のインスタンス」と「C# の null」が別のものとして存在すること、
	/// 一般的な参照型のように C# の変数をによって情報が共有されることがあること、
	/// その内容は可変であるため、参照を介して別の箇所が変更されてしまうことなどに注意してください。</para>
	/// </summary>
	public class JVariant : IEnumerable<JVariant>, IEquatable<JVariant>
	{
		//==============================================================================
		// operators
		//==============================================================================

		public static implicit operator JVariant(bool v)
		{
			return new JVariant(v);
		}
		public static implicit operator JVariant(long v)
		{
			return new JVariant(v);
		}
		public static implicit operator JVariant(double v)
		{
			return new JVariant(v);
		}
		public static implicit operator JVariant(string v)
		{
			return new JVariant(v);
		}
		public static implicit operator JVariant(JArray v)
		{
			return new JVariant(v);
		}
		public static implicit operator JVariant(JObject v)
		{
			return new JVariant(v);
		}


		/// <summary>
		/// bool への変換
		/// <para>
		/// この変換は条件式として bool が要求される文脈で利用されるものです。
		/// 内容として bool を持つときの値は　BoolValue を利用してください。</para>
		/// <para>
		/// 変換は IsEmpty が利用されます。（BoolValue とは異なる値を返します）
		/// </para>
		/// </summary>
		public static bool operator true(JVariant v)
		{
			return !v.IsEmpty();
		}

		/// <summary>
		/// bool への変換
		/// <para>
		/// この変換は条件式として bool が要求される文脈で利用されるものです。
		/// 内容として bool を持つときの値は　BoolValue を利用してください。</para>
		/// <para>
		/// 変換は IsEmpty が利用されます。（BoolValue とは異なる値を返します）
		/// </para>
		/// </summary>
		public static bool operator false(JVariant v)
		{
			return v.IsEmpty();
		}

		/// <summary>
		/// 否定
		/// <para>
		/// operator false() と同じです。</para>
		/// </summary>
		public static bool operator !(JVariant v)
		{
			return v.IsEmpty();
		}

		/// <summary>
		/// 同値性比較
		/// <para>
		/// 内部がオブジェクトもしくは配列の場合は、参照している内部オブジェクト同じものであるかどうかを返します。
		/// 内容が同じであることを比較する場合は 
		/// <see cref="EquivalentTo(JVariant, int, int)">EquivalentTo()</see> を利用してください。</para>
		/// </summary>
		public static bool operator ==(JVariant a, JVariant b)
		{
			if (a is null || b is null)
			{
				return (a is null) && (b is null);
			}
			return a.Equals(b);
		}

		/// <summary>
		/// 非同値性比較
		/// <para>
		/// !( a==b )
		/// </para>
		/// </summary>
		public static bool operator !=(JVariant a, JVariant b)
		{
			return !(a == b);
		}

		//==============================================================================
		// static members
		//==============================================================================

		public const int DefaultMaxDepth = 64;

	

		/// <summary>
		/// JSON をパースして JVariant として返す。
		/// <para>
		/// JSON として正しくない文字列が与えられたとき、デフォルトでは Null を示す JVariant を返します。
		/// (null ではなく 「null を示す JVariant」であることに注意してください。)</para>
		/// <para>
		/// 例外を投げるべき場合は第2引数に true を指定してください。</para>
		/// <para>
		/// 多少パースはゆるくなっていて、厳密には JSON ではない文字列もパースします。
		/// ・ オブジェクトのキーが " でくくられていなくとも良い。アルファベットのみの連続はキー名として扱われる。
		/// ・ 数値の解釈が int.TryParse で行われる。無駄な先行 + などは JSON 的には ill-formed だが、パースされる。
		/// ・ Object Array の末尾に , があって良い。
		/// これらが問題になることはないと思われますが、これを期待することは避けてください。</para>
		/// </summary>
		/// <param name="source">パースする文字列</param>
		/// <param name="throws">例外を投げるとき true.</param>
		/// <exception cref="VariantException">throws に true が指定され、パースに失敗したとき。</exception>
		public static JVariant ParseJSON(StringView source, bool throws = false, IStringCache stock = null)
		{
			try
			{
				if (source.IsBlank())
				{
					return new JVariant();
				}

				using var temp = stock == null ? StringCache.CreateTemporary() : null;
				if (temp != null)
				{
					stock = temp;
				}

				var parser = new JsonParserU16(stock);
				return parser.Parse(source);
			}
			catch (VariantException)
			{
				if (throws)
				{
					throw;
				}
				return new JVariant();
			}
		}


		/// <summary>
		/// UTF-8 文字列をパースして JVaraint として返す。
		/// </summary>
		/// <param name="source">パース対象の UTF-8 バイナリ</param>
		/// <param name="throws">失敗時例外を投げるなら true</param>
		/// <param name="stock">同じ文字列を重ねるためのもの</param>
		/// <exception cref="VariantException">throws に true が指定され、パースに失敗したとき。</exception>
		public static JVariant ParseJSON(U8View source, bool throws = false, IStringCache stock = null)
		{
			try
			{
				if (source.IsEmpty())
				{
					return new JVariant();
				}

				using var temp = stock == null ? StringCache.CreateTemporary() : null;
				if (temp != null)
				{
					stock = temp;
				}

				var parser = new JsonParserU8(stock);
				return parser.Parse(source);
			}
			catch (VariantException)
			{
				if (throws)
				{
					throw;
				}
				return new JVariant();
			}
		}




		/// <summary>
		/// 動的な生成
		/// <para>
		/// object として引数を受け、それを示す JVariant を生成して返します。</para>
		/// <para>
		/// 引数はその動的な型で解釈され、以下のように扱われます。
		/// <list type="table">
		/// <listheader>
		/// <term>対象型</term>
		/// <description>変換方法</description>
		/// </listheader>
		/// <item>
		///		<term>JVariant 関連型及びプリミティブ、もしくは string.</term>
		///		<description>そのまま JVaraint に格納されて返却されます。</description>
		/// </item>
		/// <item>
		///		<term>object</term>
		///		<description>空の JObject.</description>
		/// </item>
		/// <item>
		///		<term>IJVariantConvertible 実装クラス</term>
		///		<description><see cref="IJVariantConvertible.AsVariant()"/> を利用。</description>
		/// </item>
		/// <item>
		///		<term>配列</term>
		///		<description>JArrayとして各要素を再帰的に変換。</description>
		/// </item>
		/// <item>
		///		<term>enum</term>
		///		<term>名前を文字列として。</term>
		/// </item>
		/// <item>
		///		<term>public なデフォルトコンストラクタがあり、IDictionary&lt;string,&gt; を実装している</term>
		///		<description>JObject として各要素を再帰的に変換</description>
		/// </item>
		/// <item>
		///		<term>public なデフォルトコンストラクタがあり、IConnection&lt;&gt; を実装している</term>
		///		<description>JArray として各要素を再帰的に変換</description>
		/// </item>
		/// <item>
		///		<term>それ以外</term>
		///		<description>JObjectとして各フィールドをリフレクションで設定することを試みる。</description>
		/// </item>
		/// </list>
		/// </para>
		/// <para>
		/// これらの挙動は <see cref="VariantConverter.Default"/> に対して
		/// <see cref="VariantConverter.RegisterTrait{T}(ConvertTrait{T})">RegisterTrait()</see> を行う、
		/// あるいは <see cref="ConvertTraitAttribute" /> を付与した <see cref="ConvertTrait{T}"/> を実装することで
		/// カスタマイズできます。
		/// <see cref="VariantConverter"/> の各種メソッドを参照してください。</para>
		/// <para>
		/// 変換に失敗した場合は VariantException を投げます。</para>
		/// </summary>
		/// <param name="t">もととなる情報、省略時は null</param>
		public static JVariant Create(object t = null)
		{
			if (t is null)
			{
				return new JVariant();
			}

			var v = VariantConverter.CreateVariantFixed(t);
			if (v is not null)
			{
				return v;
			}

			var context = ConvertContext.Acquire();
			try
			{
				return context.Converter.Marshal(t);
			}
			finally
			{
				context.Release();
			}
		}


		//==============================================================================
		// instance members
		//==============================================================================

		private VariantType m_Type;
		private long m_IntValue;
		private double m_FloatValue;
		private object m_RefValue;


		/// <summary>
		/// デフォルトコンストラクタ。 Null を指すものとして生成されます。
		/// </summary>
		public JVariant()
			: this(VariantType.Null)
		{
		}

		/// <summary>
		/// bool によるコンストラクタ。
		/// </summary>
		public JVariant(bool v)
			: this(VariantType.Boolean)
		{
			m_IntValue = v ? 1 : 0;
		}

		/// <summary>
		/// 整数値によるコンストラクタ。
		/// </summary>
		public JVariant(long v)
			: this(VariantType.Integer)
		{
			m_IntValue = v;
		}

		/// <summary>
		/// 少数値によるコンストラクタ。 
		/// </summary>
		public JVariant(double v)
			: this(VariantType.Float)
		{
			m_FloatValue = v;
		}

		/// <summary>
		/// 文字列によるコンストラクタ
		/// <para>
		/// null が与えられたときは null を示すJVariant になるので注意してください。</para>
		/// </summary>
		public JVariant(string v)
			: this(VariantType.String)
		{
			if (v == null)
			{
				Clear();
				return;
			}
			m_RefValue = v ?? string.Empty;
		}

		/// <summary>
		/// JArrayによるコンストラクタ。
		/// </summary>
		public JVariant(JArray v)
			: this(VariantType.Array)
		{
			m_RefValue = v.GetBody();
		}

		/// <summary>
		/// JObject によるコンストラクタ。
		/// </summary>
		public JVariant(JObject v)
			: this(VariantType.Object)
		{
			m_RefValue = v.GetBody();
		}

		/// <summary>
		/// JVariantTag によるコンストラクタ。
		/// </summary>
		public JVariant(JVariantTag v)
			 : this(v.AsVariant())
		{
		}


		/// <summary>
		/// コピーによるコンストラクタ。
		/// <para>
		/// いわゆるシャロウコピーです。
		/// C# のオブジェクトとしては別物になりますが、内部に配列もしくはオブジェクトが入っていた場合は同じものを指すことになります。
		/// null が与えられた場合は Null を指す JVariant として初期化されます。</para>
		/// <para>
		/// ディープコピーは <see cref="Duplicate"/> を利用してください。</para>
		/// </summary>
		public JVariant(JVariant obj)
		{
			if (obj == null)
			{
				Clear();
				return;
			}
			m_Type = obj.m_Type;
			m_IntValue = obj.m_IntValue;
			m_FloatValue = obj.m_FloatValue;
			m_RefValue = obj.m_RefValue;
		}

		/// <summary>
		/// 保持している値の種類。
		/// <para>
		/// - <seealso cref="JVariantExtensions.GetVariantType(JVariant)">GetVariantType()</seealso><br/>
		/// - <seealso cref="JVariantExtensions.IsNull(JVariant)">IsNull()</seealso><br/>
		/// - <seealso cref="JVariantExtensions.IsBoolean(JVariant)">IsBoolean()</seealso><br/>
		/// - <seealso cref="JVariantExtensions.IsNumber(JVariant)">IsNumber()</seealso><br/>
		/// - <seealso cref="JVariantExtensions.IsString(JVariant)">IsString()</seealso><br/>
		/// - <seealso cref="JVariantExtensions.IsArray(JVariant)">IsArray()</seealso><br/>
		/// - <seealso cref="JVariantExtensions.IsObject(JVariant)">IsObject()</seealso><br/>
		/// </para>
		/// </summary>
		public VariantType VariantType => m_Type;

		/// <summary><see cref="JVariantExtensions.IsNull(JVariant)"/></summary>
		public bool IsNull => (m_Type == VariantType.Null);

		/// <summary><see cref="JVariantExtensions.IsComposite(JVariant)"/></summary>
		public bool IsComposite => (m_Type == VariantType.Array || m_Type == VariantType.Object);

		/// <summary><see cref="JVariantExtensions.IsBoolean(JVariant)"/></summary>
		public bool IsBoolean => (m_Type == VariantType.Boolean);

		/// <summary><see cref="JVariantExtensions.IsNumber(JVariant)"/></summary>
		public bool IsNumber => (m_Type == VariantType.Integer || m_Type == VariantType.Float);

		/// <summary><see cref="JVariantExtensions.IsString(JVariant)"/></summary>
		public bool IsString => (m_Type == VariantType.String);

		/// <summary><see cref="JVariantExtensions.IsArray(JVariant)"/></summary>
		public bool IsArray => (m_Type == VariantType.Array);

		/// <summary><see cref="JVariantExtensions.IsObject(JVariant)"/></summary>
		public bool IsObject => (m_Type == VariantType.Object);

		/// <summary>
		/// 内容が ExceptEmpty(空) であるかどうかを返す。
		/// <para>
		/// ExceptEmpty の意味は内容の型によって異なり、以下のとおりです。
		/// <list>
		/// <item>Null     ⇒ true.</item>
		/// <item>Boolean  ⇒ false であるか</item>
		/// <item>Integer  ⇒ 0 であるか</item>
		/// <item>Float    ⇒ 0.0 であるか</item>
		/// <item>String   ⇒ 長さ 0 であるか</item>
		/// <item>Array    ⇒ 要素数が 0 であるか</item>
		/// <item>Object   ⇒ 要素数が 0 であるか</item>
		/// </list>
		/// </para>
		/// <para>
		/// 多くの場合中身がないことを確認するのに、自分自身が null であるかどうかを意識する必要はないと思われます。
		/// 拡張メソッド <see cref="JVariantExtensions.IsEmpty(JVariant)">IsEmpty()</see> を利用してください。 
		/// </para>
		/// </summary>
		public bool IsEmpty
		{
			get
			{
				switch (m_Type)
				{
					case VariantType.Null: return true;
					case VariantType.Boolean: return m_IntValue == 0;
					case VariantType.Integer: return m_IntValue == 0;
					case VariantType.Float: return m_FloatValue == 0.0;
					case VariantType.String: return String.IsNullOrEmpty(m_RefValue as string);
					case VariantType.Array: return GetArrayBody().Count == 0;
					case VariantType.Object: return GetObjectBody().Count == 0;
				}
				SystemDebug.Assert(false);
				return true;
			}
		}



		/// <summary>
		/// 要素数。
		/// <para>
		/// VariantType が Array, Object のときはその要素数を、
		/// Null のときは 0 を、それ以外のときは 1 を返します。</para>
		/// <para>
		/// 配列・オブジェクト以外のとき挙動は GetEnumerator() の挙動と合わせるためのものです。
		/// JVariant は内容が配列・オブジェクト以外であるとき、自分自身を値として一つだけ持つコンテナとして作用します。</para>
		/// </summary>
		public int Count
		{
			get
			{
				if (m_Type == VariantType.Array)
				{
					return GetArrayBody().Count;
				}
				if (m_Type == VariantType.Object)
				{
					return GetObjectBody().Count;
				}
				if (m_Type == VariantType.Null)
				{
					return 0;
				}
				return 1;
			}
		}



		/// <summary>
		/// bool 値。bool 以外を持っている場合はそれなりに変換しますが、それに依存しないようにしてください。
		/// <para>
		/// bool 以外を持っていた場合は以下の値を返します。
		/// Null   ⇒ false
		/// Integer⇒ 0 以外のとき true
		/// Float  ⇒ 0.0 と等しくないとき true
		/// String ⇒ 数値として解釈可能であればそれが 0 以外のとき true. 数値ではないときは "true" と Case Insensitive に比較した結果
		/// Array  ⇒ 要素数が 0 ではないとき true
		/// Object ⇒ 要素数が 0 ではないとき true</para>
		/// <seealso cref="JVariantExtensions.AsBool(JVariant)"/>
		/// </summary>
		public bool BoolValue
		{
			get
			{
				switch (m_Type)
				{
					case VariantType.Boolean: return m_IntValue != 0;
					case VariantType.Null: return false;
					case VariantType.Integer: return m_IntValue != 0;
					case VariantType.Float: return m_FloatValue != 0.0;
					case VariantType.String:
						string s = (m_RefValue as string).Trim();
						if (int.TryParse(s, out int x))
						{
							return x != 0;
						}
						return string.Compare(s, "true", StringComparison.OrdinalIgnoreCase) == 0;
					case VariantType.Array: return GetArrayBody().Count != 0;
					case VariantType.Object: return GetObjectBody().Count != 0;
				}
				SystemDebug.Assert(false);
				return false;
			}
		}

		/// <summary>
		/// long 値。数値以外を持っている場合はそれなりに変換しますが、それに依存しないようにしてください。
		/// <para>
		/// 整数以外を持っていた場合は以下の値を返します。
		/// Null   ⇒ 0
		/// Bool   ⇒ true は 1, false は 0 
		/// Float  ⇒ int キャストした結果
		/// String ⇒ 数値として解釈可能なときはその数値、でなければ 0.
		/// Array  ⇒ 要素数
		/// Object ⇒ 要素数</para>
		/// <seealso cref="JVariantExtensions.AsLong(JVariant)"/>
		/// </summary>
		public long LongValue
		{
			get
			{
				switch (m_Type)
				{
					case VariantType.Integer: return m_IntValue;
					case VariantType.Null: return 0;
					case VariantType.Boolean: return m_IntValue;
					case VariantType.Float: return (long)m_FloatValue;
					case VariantType.String:
						if (long.TryParse(m_RefValue as string, out long ret))
						{
							return ret;
						}
						return 0;

					case VariantType.Array:
						return GetArrayBody().Count;
					case VariantType.Object:
						return GetObjectBody().Count;
				}

				SystemDebug.Assert(false);
				return 0;
			}
		}


		/// <summary>
		/// int 値。数値以外を持っている場合はそれなりに変換しますが、それに依存しないようにしてください。
		/// <para>
		/// 整数以外を持っていた場合は以下の値を返します。
		/// Null   ⇒ 0
		/// Bool   ⇒ true は 1, false は 0 
		/// Float  ⇒ int キャストした結果
		/// String ⇒ 数値として解釈可能なときはその数値、でなければ 0.
		/// Array  ⇒ 要素数
		/// Object ⇒ 要素数</para>
		/// <seealso cref="JVariantExtensions.AsInt(JVariant)"/>
		/// </summary>
		public int IntValue => (int)this.LongValue;


		/// <summary>
		/// double 値。数値以外を持っている場合はそれなりに変換しますが、それに依存しないようにしてください。
		/// <para>
		/// 少数以外を持っていた場合は以下の値を返します。
		/// Null   ⇒ 0.0
		/// Bool   ⇒ true は 1.0, false は 0.0
		/// Integer⇒ そのまま
		/// String ⇒ 数値として解釈可能なときはその数値、でなければ 0.
		/// Array  ⇒ 要素数
		/// Object ⇒ 要素数</para>
		/// <seealso cref="JVariantExtensions.AsDouble(JVariant)"/>
		/// </summary>
		public double DoubleValue
		{
			get
			{
				switch (m_Type)
				{
					case VariantType.Float: return m_FloatValue;
					case VariantType.Null: return 0.0;
					case VariantType.Boolean: return m_IntValue;
					case VariantType.Integer: return m_IntValue;
					case VariantType.String:
						if (double.TryParse(m_RefValue as string, out double ret))
						{
							return ret;
						}
						if (m_RefValue as string == "NaN")
						{
							return double.NaN;
						}
						if (m_RefValue as string == "infinity")
						{
							return double.PositiveInfinity;
						}
						if (m_RefValue as string == "negative infinity")
						{
							return double.NegativeInfinity;
						}
						return 0;

					case VariantType.Array:
						return GetArrayBody().Count;
					case VariantType.Object:
						return GetObjectBody().Count;
				}

				SystemDebug.Assert(false);
				return 0;
			}
		}

		/// <summary>
		/// float 値。数値以外を持っている場合はそれなりに変換しますが、それに依存しないようにしてください。
		/// <para>
		/// 少数以外を持っていた場合は以下の値を返します。
		/// Null   ⇒ 0.0
		/// Bool   ⇒ true は 1.0, false は 0.0
		/// Integer⇒ そのまま
		/// String ⇒ 数値として解釈可能なときはその数値、でなければ 0.
		/// Array  ⇒ 要素数
		/// Object ⇒ 要素数</para>
		/// <seealso cref="JVariantExtensions.AsFloat(JVariant)"/>
		/// </summary>
		public float FloatValue => (float)this.DoubleValue;

		/// <summary>
		/// 文字列表現を返す。
		/// <para>ToString() と同じです。
		/// string 以外を持っているときはなんとなく内容を表す文字列を返します。
		/// string以外を持っているときにここから返却される文字列に依存しないようにしてください。</para>
		/// </summary>
		public string StringValue => ToString();


		/// <summary>
		/// 配列としての値。
		/// <para>
		/// 内部が配列の場合はそれを返します。
		/// Null の場合は空の配列、
		/// Objectですべてのキーが int として解釈可能な場合はそれぞれの要素を各indexに詰めた配列。
		/// それ以外では現在の要素を一つだけ持つ配列を返しますが、
		/// それに依存しないようにしてください。</para>
		/// <para>
		/// 内部が配列値であった場合はその配列をそのまま返し、それは参照を共有しています。
		/// それ以外の場合は、ここで配列が新たに生成されて返されるので注意してください。</para>
		/// <seealso cref="JVariantExtensions.AsArray(JVariant)"/>
		/// </summary>
		public JArray ArrayValue
		{
			get
			{
				if (m_Type == VariantType.Array)
				{
					return JArray.CreateInternal(this.GetArrayBody());
				}

				JArray ret = new JArray();

				switch (m_Type)
				{
					case VariantType.Null:
						return ret;

					case VariantType.Boolean:
						ret.Add(m_IntValue != 0);
						return ret;

					case VariantType.Integer:
						ret.Add(m_IntValue);
						return ret;

					case VariantType.Float:
						ret.Add(m_FloatValue);
						return ret;

					case VariantType.String:
						ret.Add(this.StringValue);
						return ret;

					case VariantType.Object:
						JObject obj = JObject.CreateInternal(GetObjectBody());
						JArray tmp;
						if (obj.TryConvertToArray(out tmp))
						{
							return tmp;
						}
						ret.Add(obj);
						return ret;
				}

				SystemDebug.Assert(false);
				return ret;
			}
		}

		/// <summary>
		/// Object としての値
		/// <para>
		/// 内部がオブジェクトではない場合、
		/// Null の場合は空のオブジェクト
		/// 配列の場合はインデックスを文字列表現にした ( Object [1,2,3] => { "0":1, "1":2, "2":3 } )
		/// それ以外では現在の要素を value という項目に入れたオブジェクトを返しますが、
		/// それに依存しないようにしてください。</para>
		/// <para>
		/// 内部がオブジェクトである場合の返却値は参照を共有していますが、
		/// それ以外の場合は新たなオブジェクトが生成されて返されるので注意してください。</para>
		/// <seealso cref="JVariantExtensions.AsObject(JVariant)"/>
		/// </summary>
		public JObject ObjectValue
		{
			get
			{
				if (m_Type == VariantType.Object)
				{
					return JObject.CreateInternal(this.GetObjectBody());
				}

				JObject ret = new JObject();

				switch (m_Type)
				{
					case VariantType.Null:
						return ret;


					case VariantType.Boolean:
						ret.Set("value", m_IntValue != 0);
						return ret;

					case VariantType.Integer:
						ret.Set("value", m_IntValue);
						return ret;

					case VariantType.Float:
						ret.Set("value", m_FloatValue);
						return ret;

					case VariantType.String:
						ret.Set("value", this.StringValue);
						return ret;

					case VariantType.Array:
						var array = JArray.CreateInternal(GetArrayBody());
						return array.ConvertToObject();
				}

				SystemDebug.Assert(false);
				return ret;
			}
		}


		/// <summary>
		/// 同一性比較
		/// <para>
		/// 配列やオブジェクトを持っている場合の同一性は、
		/// 同じオブジェクトを参照しているときのみ true になります。
		/// 内容を検証するわけではないので注意してください。
		/// </para>
		/// </summary>
		public override bool Equals(object obj)
		{
			if (obj is JVariant other)
			{
				return this.Equals(other);
			}
			return false;
		}

		/// <summary>
		/// 同一性比較
		/// <para>
		/// 配列やオブジェクトを持っている場合の同一性は、
		/// 同じオブジェクトを参照しているときのみ true になります。
		/// 内容を検証するわけではないので注意してください。
		/// </para>
		/// </summary>
		public bool Equals(JVariant other)
		{
			if (other is null)
			{
				return false;
			}

			if (ReferenceEquals(this, other))
			{
				return true;
			}


			var vt = m_Type;

			// 型が違う
			if (vt != other.m_Type)
			{
				return false;
			}

			if (vt == VariantType.Null)
			{
				return true;
			}
			if (vt == VariantType.Boolean)
			{
				return ((m_IntValue != 0) == (other.m_IntValue != 0));
			}
			if (vt == VariantType.Integer)
			{
				return (m_IntValue == other.m_IntValue);
			}
			if (vt == VariantType.Float)
			{
				return (m_FloatValue == other.m_FloatValue);
			}
			if (vt == VariantType.String)
			{
				return (m_RefValue as string).Equals(other.m_RefValue);
			}
			if (vt == VariantType.Array)
			{
				return object.ReferenceEquals(m_RefValue, other.m_RefValue);
			}
			if (vt == VariantType.Object)
			{
				return object.ReferenceEquals(m_RefValue, other.m_RefValue);
			}
			return false;
		}


		public bool EquivalentTo(JVariant other, int maxDepth = DefaultMaxDepth, int depth = 0)
		{
			if (depth > maxDepth)
			{
				throw new VariantException("Too deep comparison. Circular reference suspected.");
			}

			var vt = m_Type;

			if (vt != other.m_Type)
			{
				return false;
			}
			if (vt == VariantType.Null)
			{
				return true;
			}
			if (vt == VariantType.Boolean)
			{
				return ((m_IntValue != 0) == (other.m_IntValue != 0));
			}
			if (vt == VariantType.Integer)
			{
				return (m_IntValue == other.m_IntValue);
			}
			if (vt == VariantType.Float)
			{
				return (m_FloatValue == other.m_FloatValue);
			}
			if (vt == VariantType.String)
			{
				return (m_RefValue as string).Equals(other.m_RefValue);
			}
			if (vt == VariantType.Array)
			{
				var a1 = GetArrayBody();
				var a2 = other.GetArrayBody();
				if (a1.Count != a2.Count)
				{
					return false;
				}
				for (int i = 0; i < a1.Count; i++)
				{
					if (!a1[i].EquivalentTo(a2[i], maxDepth, depth + 1))
					{
						return false;
					}
				}
				return true;
			}
			if (vt == VariantType.Object)
			{
				var o1 = JObject.CreateInternal(GetObjectBody());
				var o2 = JObject.CreateInternal(other.GetObjectBody());

				return o1.EquivalentTo(o2, maxDepth, depth);
			}

			return false;
		}


		/// <summary>
		/// ハッシュコード
		/// <para>
		/// それなりに仕様に則ったハッシュコードを返しますが、
		/// JVariant をハッシュのキーにするようなことは避けてください。
		/// </para>
		/// </summary>
		public override int GetHashCode()
		{
			var vt = m_Type;
			int ret = (int)vt * 419;

			if (vt == VariantType.Null)
			{
				return ret;
			}
			if (vt == VariantType.Integer || vt == VariantType.Boolean)
			{
				return ret + m_IntValue.GetHashCode();
			}
			if (vt == VariantType.Integer || vt == VariantType.Boolean)
			{
				return ret + m_FloatValue.GetHashCode();
			}
			if (vt == VariantType.String)
			{
				return ret + this.StringValue.GetHashCode();
			}

			return ret + m_RefValue.GetHashCode();
		}

		/// <summary>
		/// 文字列化
		/// <para>
		/// string の場合は設定されている string そのもの。
		/// その他はなんとなく内容を表す文字列を返します。JSON表現ではないので注意してください。</para>
		/// <para>
		/// null のとき "null" ではなく空の文字列を返します。</para>
		/// <seealso cref="ToJSON(JsonFormatPolicy)"/>
		/// </summary>
		public override string ToString()
		{
			switch (m_Type)
			{
				case VariantType.Null: return "";
				case VariantType.Boolean: return IntValue != 0 ? "true" : "false";
				case VariantType.Integer: return m_IntValue.ToString();
				case VariantType.Float: return m_FloatValue.ToString();
				case VariantType.String: return m_RefValue as string;
			}
			if (m_Type == VariantType.Array)
			{
				var body = GetArrayBody();
				if (body == null || body.Count == 0)
				{
					return "[]";
				}
				StringBuilder sb = new StringBuilder();
				sb.Append("[ ");

				for (int i = 0; i < body.Count; i++)
				{
					if (i != 0)
					{
						sb.Append(", ");
					}
					sb.Append(body[i]?.GetSimpleString());
				}
				sb.Append(" ]");
				return sb.ToString();
			}

			if (m_Type == VariantType.Object)
			{
				var body = GetObjectBody();
				if (body == null || body.Count == 0)
				{
					return "{}";
				}
				var keys = body.Keys.ToArray();

				var sb = new StringBuilder();
				sb.Append("{ ");
				for (int i = 0; i < keys.Length; i++)
				{
					if (i != 0)
					{
						sb.Append(", ");
					}
					var key = keys[i];
					sb.Append(key);
					sb.Append(":");
					sb.Append(body[key]?.GetSimpleString());
				}
				sb.Append(" }");
				return sb.ToString();
			}

			SystemDebug.Assert(false);
			return "";
		}


		/// <summary>
		/// IEnumerable の実装。IEnumerator を返す。
		/// <para>
		/// Null の場合は何も反復しない反復子を返します。
		/// Array の場合は内容をそのまま反復します。
		/// Object の場合はその Values を反復します。（キーは失われます）
		/// それ以外は自分自身を一度だけ反復します。
		/// </para>
		public IEnumerator<JVariant> GetEnumerator()
		{
			if (m_Type == VariantType.Null)
			{
				yield break;
			}
			if (m_Type == VariantType.Array)
			{
				foreach (var v in GetArrayBody())
				{
					yield return v;
				}
				yield break;
			}

			if (m_Type == VariantType.Object)
			{
				var values = this.GetObjectBody().Values;
				foreach (var v in values)
				{
					yield return v;
				}
				yield break;
			}

			yield return this;
			yield break;
		}

		/// <summary>
		/// クリア
		/// <para>
		/// Null を示すようになります。</para>
		/// </summary>
		public void Clear()
		{
			m_Type = VariantType.Null;
			m_IntValue = 0;
			m_FloatValue = 0;
			m_RefValue = 0;
		}

		/// <summary>代入。</summary>
		public void Assign(bool v)
		{
			Clear();
			m_Type = VariantType.Boolean;
			m_IntValue = v ? 1 : 0;
		}

		/// <summary>代入。</summary>
		public void Assign(long v)
		{
			Clear();
			m_Type = VariantType.Integer;
			m_IntValue = v;
		}

		/// <summary>代入。</summary>
		public void Assign(double v)
		{
			Clear();
			m_Type = VariantType.Float;
			m_FloatValue = v;
		}

		/// <summary>代入。</summary>
		public void Assign(string v)
		{
			Clear();
			m_Type = VariantType.String;
			m_RefValue = v;
		}

		/// <summary>代入。</summary>
		public void Assign(JArray v)
		{
			Clear();
			m_Type = VariantType.Array;
			m_RefValue = v.GetBody();
		}

		/// <summary>代入。</summary>
		public void Assign(JObject v)
		{
			Clear();
			m_Type = VariantType.Object;
			m_RefValue = v.GetBody();
		}

		/// <summary>代入。</summary>
		public void Assign(JVariant v)
		{
			if (v == null)
			{
				Clear();
				return;
			}

			m_Type = v.m_Type;
			m_IntValue = v.m_IntValue;
			m_FloatValue = v.m_FloatValue;
			m_RefValue = v.m_RefValue;
		}

		/// <summary>
		/// 追加。
		/// <para>
		/// 配列としての追加を行います。格納されている値が配列ではない場合は配列に変換したものを自身に格納し、そこに追加します。
		/// 配列への変換は ArrayValue を参照してください。</para>
		/// <para>
		/// 「配列ではない状態で配列として値を追加しようとすると、まず自分自身を配列に変換してしまう」ということには十分注意してください。
		/// </para>
		/// </summary>
		public void Add(bool v) { this.SwitchToArray().Add(v); }
		public void Add(long v) { this.SwitchToArray().Add(v); }
		public void Add(double v) { this.SwitchToArray().Add(v); }
		public void Add(string v) { this.SwitchToArray().Add(v); }
		public void Add(JArray v) { this.SwitchToArray().Add(v); }
		public void Add(JObject v) { this.SwitchToArray().Add(v); }
		public void Add(JVariant v) { this.SwitchToArray().Add(v); }


		/// <summary>
		/// 値の設定
		/// <para>
		/// 配列として値を設定します。格納されている値がオブジェクトの場合は index は文字列として解釈されます。
		/// 配列でもオブジェクトでもない場合は、配列に変換したものを自身に格納し、その index で示される要素を変更します。</para>
		/// </summary>
		public void Set(int index, bool v) { EnsureItem(index).Assign(v); }
		public void Set(int index, long v) { EnsureItem(index).Assign(v); }
		public void Set(int index, double v) { EnsureItem(index).Assign(v); }
		public void Set(int index, string v) { EnsureItem(index).Assign(v); }
		public void Set(int index, JArray v) { EnsureItem(index).Assign(v); }
		public void Set(int index, JObject v) { EnsureItem(index).Assign(v); }
		public void Set(int index, JVariant v) { EnsureItem(index).Assign(v); }


		/// <summary>
		/// 配列としての要素の取得。
		/// <para>
		/// 指定されたindexの要素を返します。格納されている値がオブジェクトである場合はindexは文字列解釈されます。
		/// 配列でもオブジェクトでもない場合は Null を示す JVariant を返します。</para>
		/// <para>
		/// （インデクサとは異なり）この操作によって内容が変更されることはありません。
		/// </para>
		/// </summary>
		public JVariant Get(int index)
		{
			if (m_Type == VariantType.Array)
			{
				return JArray.CreateInternal(GetArrayBody()).Get(index);
			}
			if (m_Type == VariantType.Object)
			{
				return this.Get(index.ToString());
			}
			return new JArray();
		}

		/// <summary>
		/// 配列としてのインデクサ
		/// <para>
		/// index で示される要素を取得設定します。
		/// 自分自身がオブジェクトの場合は index は文字列として解釈されます。
		/// 配列でもオブジェクトでもない場合は、自分自身を配列に変換し、その index で示される要素をあつかいます。</para>
		/// <para>
		/// 取得操作であっても内容が変更されることに注意してください。
		/// （自分自身が配列でなければ配列に変換される、指定された要素がなければ確保する）
		/// その挙動が望ましくない場合は Get() を利用してください。</para>
		/// </summary>
		public JVariant this[int index]
		{
			get => EnsureItem(index);
			set => EnsureItem(index).Assign(value);
		}

		/// <summary>
		/// 値の設定
		/// <para>
		/// オブジェクトとして値を設定します。格納されている値が配列の場合は key は数値として解釈されます。
		/// 配列でもオブジェクトでもないか、配列で key が数値として解釈できなかった場合
		/// オブジェクトに変換したものを自身に格納し、その key で示される要素を変更します。</para>
		/// <para>
		/// 「オブジェクトではない状態でオブジェクトとして値を追加しようとすると、
		/// まず自分自身をオブジェクトに変換してしまう」ということには十分注意してください。 </para>
		/// </summary>
		public void Set(string key, bool v) { EnsureItem(key).Assign(v); }
		public void Set(string key, long v) { EnsureItem(key).Assign(v); }
		public void Set(string key, double v) { EnsureItem(key).Assign(v); }
		public void Set(string key, string v) { EnsureItem(key).Assign(v); }
		public void Set(string key, JArray v) { EnsureItem(key).Assign(v); }
		public void Set(string key, JObject v) { EnsureItem(key).Assign(v); }
		public void Set(string key, JVariant v) { EnsureItem(key).Assign(v); }

		/// <summary>
		/// オブジェクトしての要素の取得。
		/// <para>
		/// 指定された key の要素を返します。格納されている値が配列である場合は key は数値として解釈されます。
		/// 配列でもオブジェクトでもない場合は Null を示す JVariant を返します。</para>
		/// <para>
		/// （インデクサとは異なり）この操作によって内容が変更されることはありません。</para>
		/// </summary>
		public JVariant Get(string key)
		{
			if (m_Type == VariantType.Object)
			{
				return JObject.CreateInternal(GetObjectBody()).Get(key);
			}
			if (m_Type == VariantType.Array)
			{
				if (int.TryParse(key.Trim(), out int index))
				{
					return Get(index);
				}
			}
			return new JArray();
		}

		/// <summary>
		/// オブジェクトとしてのインデクサ
		/// <para>
		/// key で示される要素を取得設定します。
		/// 自分自身が配列の場合は key は数値として解釈されます。
		/// 配列でもオブジェクトでもないか、配列で key が数値として解釈できなかった場合
		/// オブジェクトに変換したものを自身に格納し、その key で示される要素を変更します。</para>
		/// <para>
		/// 取得操作であっても内容が変更されることに注意してください。
		/// （自分自身がオブジェクトでなければオブジェクトに変換される、指定された要素がなければ確保する）
		/// その挙動が望ましくない場合は Get() を利用してください。</para>
		/// </summary>
		public JVariant this[string key]
		{
			get => EnsureItem(key);
			set => EnsureItem(key).Assign(value);
		}

		/// <summary>
		/// 指定されたキーの要素が存在するか返す。
		/// <para>
		/// 内容がオブジェクトの場合は JObject.ContainsKey() と同等です。
		/// 配列の場合は引数が数値として解釈可能であればそれと要素数を比較します。
		/// それ以外の場合は false を返します。
		/// </para>
		/// </summary>
		public bool ContainsKey(string x)
		{
			if (this.IsObject)
			{
				return JObject.CreateInternal(GetObjectBody()).ContainsKey(x);
			}
			if (this.IsArray)
			{
				if (int.TryParse(x, out int index))
				{
					return index < this.Count;
				}
			}
			return false;
		}



		/// <summary>
		/// UTF-8 バイナリによる JSON 化。
		/// </summary>
		public U8View ToU8JSON(JsonFormatPolicy policy = null)
		{
			// そのまま文字列になるもの
			switch (m_Type)
			{
				case VariantType.Null: return Literal.Null.U8;
				case VariantType.Boolean: return m_IntValue != 0 ? Literal.True.U8 : Literal.False.U8;
			}

			// ポリシーに合わせて加工が必要
			policy ??= JsonFormatPolicy.OneLiner;
			var context = StringifyContext.ForU8(policy);
			try
			{
				ConvertToJSON(ref context);
				return context.U8Result();
			}
			finally
			{
				context.Dispose();
			}
		}

		/// <summary>
		/// JSON 化。
		/// <para>
		/// 内容の JSON 表現を返します。</para>
		/// <para>
		/// 配列やオブジェクトは内部に配列やオブジェクトを持ち、更に参照経由で内容を共有することができるため、
		/// JVariant の親子関係は循環していることがあります。
		/// この実装ではそれらを検出することはできないため階層の深さを見ています。
		/// 64(JsonFormatPolicy.MaxDepth) 以上ネストしたデータは変換できません。
		/// <para>
		/// 以下の状況で <see cref="VaraintException"/> を投げます。
		/// <list type="bullet">
		///   <item>ネストが <see cref="JsonFormatPolicy.MaxDepth"/> を越えている。</item>
		///   <item><see cref="JsonFormatPolicy.SpecialFloatPolicy"/> に Throw が指定され、Number 値に NaN, Infinity 等が含まれる。</item>
		/// </list>
		/// </para>
		/// </summary>
		/// <exception cref="VaraintException"></exception>
		/// <param name="policy">フォーマット指定、省略した場合は改行なし、NaN は文字列出力。</param>
		public string ToJSON(JsonFormatPolicy policy = null)
		{
			// そのまま文字列になるもの
			switch (m_Type)
			{
				case VariantType.Null: return "null";
				case VariantType.Boolean: return m_IntValue != 0 ? "true" : "false";
				case VariantType.Integer: return m_IntValue.ToString();
			}

			// ポリシーに合わせて加工が必要
			policy ??= JsonFormatPolicy.OneLiner;
			var context = StringifyContext.ForString(policy);

			try
			{
				ConvertToJSON(ref context);
				return context.StringResult();
			}
			finally
			{
				context.Dispose();
			}
		}

	

		/// <summary>
		/// ドット表記による子要素の参照
		/// <para>
		/// 文字列を受け、それを . で分割し、それぞれをキーとみなして子要素を辿って返します。
		///	<code>
		///	value = JObject.Create();
		/// value["1"] = JObject.Create();
		///	value["1"]["1"] = JObject.Create();
		///	value["1"]["1"]["1"] = "Item 1-1-1";
		/// 
		///	value.Pick( "1.1.1" );  // -> "Item 1-1-1"
		/// </code>
		/// たどることができるのは JObject もしくは JArray のみです。
		/// 末尾以外で JObject JArray 以外の要素になった場合は Null を示す JVariant を返すします。
		/// どこまで辿れたかを結果から知ることはできません。</para>
		/// <para>
		/// . で分割したそれぞれの要素は trim されます。
		///	（v.Pick( "1.1.1" ) と v.Pick( "1.1   .1" ) は同じ、空白で始まるもしくは空白で終わる要素を Pick() でたどることはできない)</para>
		///	<para>
		///	JArray に対しては、キーが整数のみでなる場合のみ要素をたどります。
		///	（例えば array.pick( "1X" ) は v[1] を辿らない、Null をかえす）</para>
		/// </summary>
		public JVariant Pick(string path)
		{
			// . で分割
			var keys = path.Split('.');

			JVariant v = this;

			for (int i = 0; i < keys.Length; i++)
			{
				string key = keys[i].Trim();

				if (v.IsObject)
				{
					v = v.ObjectValue.Get(key);
					continue;
				}
				if (v.IsArray && int.TryParse(key, out int index))
				{
					v = v.ArrayValue.Get(index);
					continue;
				}

				return new JVariant();
			}
			return v;
		}


		/// <summary>
		/// ディープコピー
		/// <para>
		/// この JVariant がもつ内容と同じ内容を持つ JVariant を新たに作成して返す。</para>
		/// <para>
		/// 内容が Object Array である場合は、各項目はそれぞれ再帰的に内容のコピーを作成します。</para>
		/// <para>
		/// Object Array は項目に自分自身を持ちえますが、そのような場合の配慮はされていません。
		/// 永久に再帰し、スタックオーバーフローを起こします。
		/// </para>
		/// </summary>
		public JVariant Duplicate()
		{
			if (IsObject)
			{
				return new JVariant(JObject.CreateInternal(GetObjectBody()).Duplicate());
			}

			if (IsArray)
			{
				return new JVariant(JArray.CreateInternal(GetArrayBody()).Duplicate());
			}

			return new JVariant(this);
		}


		/// <summary>
		/// 非ジェネリックIEnumerable の実装。
		/// </summary>
		IEnumerator IEnumerable.GetEnumerator()
		{
			IEnumerable<JVariant> gen = this as IEnumerable<JVariant>;
			return gen.GetEnumerator();
		}

		// private コンストラクタ
		private JVariant(VariantType t)
		{
			m_Type = t;
			m_IntValue = 0;
			m_FloatValue = 0;
			m_RefValue = 0;
		}

		// 配列としての Body を返す。配列ではないときは null を返すので注意。
		private List<JVariant> GetArrayBody()
		{
			if (m_Type == VariantType.Array)
			{
				return m_RefValue as List<JVariant>;
			}
			return null;
		}

		// 配列としての Body を返す。配列ではないときは null を返すので注意。
		private Dictionary<string, JVariant> GetObjectBody()
		{
			if (m_Type == VariantType.Object)
			{
				return m_RefValue as Dictionary<string, JVariant>;
			}
			return null;
		}

		// 自分自身を Array に変換
		private JArray SwitchToArray()
		{
			if (m_Type != VariantType.Array)
			{
				var tmp = this.ArrayValue;
				this.Assign(tmp);
			}
			return JArray.CreateInternal(GetArrayBody());
		}
		// 自分自身を Object に変換
		private JObject SwitchToObject()
		{
			if (m_Type != VariantType.Object)
			{
				var tmp = this.ObjectValue;
				this.Assign(tmp);
			}
			return JObject.CreateInternal(GetObjectBody());
		}

		private string GetSimpleString()
		{
			switch (m_Type)
			{
				case VariantType.Null: return "null";
				case VariantType.Boolean: return IntValue != 0 ? "true" : "false";
				case VariantType.Integer: return m_IntValue.ToString();
				case VariantType.Float: return m_FloatValue.ToString();
				case VariantType.String: return "\"" + (m_RefValue as string) + "\"";
				case VariantType.Array: return "<array>";
				case VariantType.Object: return "<object>";
			}
			return "";
		}

		internal void ConvertToJSON(ref StringifyContext context)
		{
			var buffer = context.GetBuffer();
			switch (m_Type)
			{
				case VariantType.Null:
					buffer.Append(Literal.Null);
					break;
				case VariantType.Boolean:
					buffer.Append(m_IntValue != 0 ? Literal.True : Literal.False);
					break;
				case VariantType.Integer:
					buffer.Append(m_IntValue);
					break;
				case VariantType.Float:
					AppendFloat(ref context, m_FloatValue);
					break;
				case VariantType.String:
					AppendString(ref context, m_RefValue as string);
					break;
				case VariantType.Array:
					JArray.CreateInternal(GetArrayBody()).ConvertToJSON(ref context);
					break;
				case VariantType.Object:
					JObject.CreateInternal(GetObjectBody()).ConvertToJSON(ref context);
					break;
			}
		}


		private static void AppendString(ref StringifyContext context, string v)
		{
			string escaped = TextUtil.EscapeJsonString(v, context.Policy.EscapeUnicode);
			var buffer = context.GetBuffer();

			buffer.Append('"');
			buffer.Append(escaped);
			buffer.Append('"');
		}

		private static void AppendFloat(ref StringifyContext context, double v)
		{
			var floatPolicy = context.Policy.SpecialFloatPolicy;
			var buffer = context.GetBuffer();

			void AppendSpecialFloat(Literal literal)
			{
				if (floatPolicy == SpecialFloatPolicy.Throw)
				{
					throw new JsonFormatException($"{literal.U16} is not allowed.");
				}
				if (floatPolicy == SpecialFloatPolicy.AsJsLiteral)
				{
					buffer.Append(literal);
					return;
				}
				if (floatPolicy == SpecialFloatPolicy.AsString)
				{
					buffer.Append('"');
					buffer.Append(literal);
					buffer.Append('"');
					return;
				}
			}

			if (double.IsNaN(v))
			{
				AppendSpecialFloat(Literal.NaN);
				return;
			}
			if (double.IsPositiveInfinity(v))
			{
				AppendSpecialFloat(Literal.Infinity);
				return;
			}
			if (double.IsNegativeInfinity(v))
			{
				AppendSpecialFloat(Literal.NegativeInfinity);
				return;
			}

			buffer.Append(v);
		}

		// 指定されたindexの要素を作ってそれを返す。
		// 自分が配列であれば普通に要素を返す
		// 自分がオブジェクトであれば index は文字列解釈する。
		// 自分がそれ以外のときは自分自身を配列に変換して要素を確保して返す。
		private JVariant EnsureItem(int index)
		{
			if (m_Type == VariantType.Object)
			{
				return JObject.CreateInternal(GetObjectBody())[index.ToString()];
			}
			SwitchToArray();
			return JArray.CreateInternal(this.GetArrayBody())[index];
		}

		// 指定された key の要素を作ってそれを返す。
		// 自分がオブジェクトであれば普通に要素を返す
		// 自分が配列であれば key が数値解釈可能であれば数値解釈する。
		// 自分がそれ以外のときは自分自身をオブジェクトに変換して要素を確保して返す。
		private JVariant EnsureItem(string key)
		{
			if (m_Type == VariantType.Array)
			{
				if (int.TryParse(key, out int index))
				{
					return JArray.CreateInternal(this.GetArrayBody())[index];
				}
			}
			SwitchToObject();
			return JObject.CreateInternal(GetObjectBody())[key];
		}


	}
}

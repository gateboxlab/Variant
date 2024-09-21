using System;


namespace Gatebox.Variant
{

	/// <summary>
	/// JVariant のみを内部に持つ構造体
	/// <para>
	/// JVariant はいろんなものから暗黙変換があるので、オーバーロードの解決が難しいことがあります。
	/// 引数等では JVariantTag を受けることで、明確に JSON の値を受ける、ということを明示することが出来ます。</para>
	/// <para>
	/// JVariantTag 自体は immutable です。
	/// JVariant は mutable ですが、JVariantTag はその性質を隠蔽し不変の値として扱います。
	/// また 「JVariant 自体が null」 であることと 「Null を持つ JVariant」 は別のものであり、JVariant はそれを意識する必要がありますが、
	/// JVariantTag ではそれを同一視します。</para>
	/// </summary>
	public readonly struct JVariantTag
	{

		//==============================================================================
		// operators
		//==============================================================================

		/// <summary>
		/// JVariant からの暗黙変換
		/// <para>
		/// JVariant はいろんなものから暗黙変換できます。
		/// これは利便性を考えると必要なことではあるのですが、メソッドシグネチャとしてみた場合、危険であることがあります。
		/// そのため、引数としては JVariantTag でうけ、JVariant として利用する、という利用方法を可能にします。
		/// </para>
		/// </summary>
		public static implicit operator JVariantTag(JVariant v)
		{
			return new JVariantTag(v);
		}

		public static implicit operator JVariant(JVariantTag v)
		{
			return v.Value ?? new JVariant();
		}

		//==============================================================================
		// instance members
		//==============================================================================


		// 唯一のメンバ、 null であることがありますが、それを利用者に意識させないようにします。
		private readonly JVariant m_Value;


		/// <summary>
		/// JVariant によるコンストラクタ
		/// <para>
		/// JVariant はいろんなものから暗黙変換があるので、このコンストラクタはいろんなものを受け取ることができます。
		/// しかし、暗黙変換の連鎖はおこらないので、
		/// JVariantTag を受けるメソッドは明示的に JVariant もしくは JVariantTag のみを受け付けることになります。
		/// </para>
		/// </summary>
		public JVariantTag(JVariant v)
		{
			// Null をもっている、 Null であるを同一視したい。
			// デフォルトコンストラクタを用意しなければいけない以上、
			// Value == null の状態に統一せざるを得ないのでその方向で。

			if( v == null || v.IsNull)
			{
				m_Value = null;
			}
			else
			{
				m_Value = v;
			}
		}

		
		/// <summary>
		/// JVariant 
		/// <para>
		/// 内部的には null を持っていることがありますが、このプロパティは null を返しません。
		/// null を持っているときは Null を示す JVariant を返します。</para>
		/// <para>
		/// JVariantTag は JVariant を参照で持っているため、一つの JVariant を JVariantTag で共有するということがありえること。
		/// しかしながら Null を示す JVariant に関しては JVariantTag 内で null 側に合わせるために、共有されないことに注意してください。</para>
		/// <para>
		/// ほとんどの場合これが問題になることはないはずです。
		/// JVariantTag 内の JVariant が共有されていることに期待するコードを書くことは推奨しません。</para>
		/// </summary>
		public readonly JVariant Value
		{
			get => m_Value ?? new JVariant();
		}


		/// <summary>
		/// 保持している値の種類を返す。
		/// </summary>
		public readonly VariantType GetVariantType() => m_Value?.VariantType ?? VariantType.Null;

		/// <summary>
		/// 内容が ExceptEmpty であるかどうかを返す。
		/// <para>
		/// ExceptEmpty の意味は内容の型によって異なります。
		/// <list>
		/// <item>Null     ⇒ true.</item>
		/// <item>Boolean  ⇒ false であるか</item>
		/// <item>Integer  ⇒ 0 であるか</item>
		/// <item>Float    ⇒ 0.0 であるか</item>
		/// <item>String   ⇒ 長さ 0 の文字列であるか</item>
		/// <item>Array    ⇒ 要素数が 0 であるか</item>
		/// <item>Object   ⇒ 要素数が 0 であるか</item>
		/// </list>
		/// </para>
		/// </summary>
		public readonly bool IsEmpty() => ( m_Value == null || m_Value.IsEmpty());

		/// <summary>Null であるかどうか</summary>
		public readonly bool IsNull() => m_Value == null || m_Value.IsNull;


		public readonly bool IsBoolean() => m_Value != null && m_Value.IsBoolean;
		public readonly bool IsNumber() => m_Value != null && m_Value.IsNumber;
		public readonly bool IsString() => m_Value != null && m_Value.IsString;
		public readonly bool IsObject() => m_Value != null && m_Value.IsObject;
		public readonly bool IsArray() => m_Value != null && m_Value.IsArray;

		
		public readonly bool AsBool() => m_Value.AsBool();
		public readonly int AsInt() => m_Value.AsInt();
		public readonly long AsLong() => m_Value.AsLong();
		public readonly float AsFloat() => m_Value.AsFloat();
		public readonly double AsDouble() => m_Value.AsDouble();
		public readonly string AsString() => m_Value.AsString();
		public readonly JObject AsObject() => m_Value.AsObject();
		public readonly JArray AsArray() => m_Value.AsArray();

		public readonly JVariant AsVariant() => m_Value.AsVariant();

		/// <summary>
		/// 任意型に変換
		/// <summary>
		public readonly T As<T>() => m_Value.As<T>();


		/// <summary>
		/// オブジェクトとしてのインデクサ
		/// <para>
		/// 読み取り専用です。内部が Object ではない、あるいは指定されたキーがない場合は null を示す JVariant を返します。</para>
		/// </summary>
		public readonly JVariantTag this[string key]
		{
			get {
				if( m_Value != null && m_Value.IsObject)
				{
					return m_Value[key];
				}

				return new JVariantTag();
			}
		}




		public readonly JVariant Pick(string path)
		{
			if(m_Value == null)
			{
				return new JVariant();
			}
			return m_Value.Pick(path);
		}


		public readonly JVariant Duplicate()
		{
			if (m_Value == null)
			{
				return new JVariant();
			}

			return m_Value.Duplicate();
		}

	}
}

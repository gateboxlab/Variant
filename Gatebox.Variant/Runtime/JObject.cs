using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Gatebox.Variant.Internal;

namespace Gatebox.Variant
{

	/// <summary>
	/// javascript 的なオブエジェクトを表す値型。
	/// <para>
	/// IDictionary&lt;string, JVariant&gt; を実装します。string と JSON の値のマップとして扱うことができます。</para>
	/// <para>
	/// 内部に IDictionary&lt;string, JVariant&gt; を持っています。
	/// このクラスは値型ですが「参照を値で持っている」という状態であるため注意してください。
	/// JObject をコピーする、という行為はその参照をコピーすることになり、
	/// 結果として内部情報である IDictionary&lt;string, JVariant&gt; は共有されることになります。</para>
	/// </summary>
	public struct JObject : IDictionary<string, JVariant>, IJVariantConvertible
	{

		//==============================================================================
		// static members
		//==============================================================================

		/// <summary>
		/// 内部情報を受け取っての生成。
		/// </summary>
		/// <param name="b">そのまま受け取る string-JVariant の Dictionary</param>
		internal static JObject CreateInternal(Dictionary<string, JVariant> b)
		{
			return new JObject(b);
		}

		/// <summary>
		/// 内部に Dictionary を保持して生成。
		/// <para>
		/// JObject は内部に Dictionary を持っており、
		/// それが null の場合と内容がない場合を同一視するようにしていますが、
		/// boxing が発生したときなどには微妙に挙動が異なることになります。</para>
		/// <para>
		/// デフォルトコンストラクタでは null の状態で生成されますが、
		/// このメソッドは内部の Dictionary を空の状態としてもつ JObject を返します。</para>
		/// <para>
		/// 内部情報が共有されてほしいときはこれを利用してください。</para>
		/// </summary>
		public static JObject Create()
		{
			return new JObject(new Dictionary<string, JVariant>());
		}

		/// <summary>
		/// 動的な型からの生成。
		/// <para>
		/// 受けたオブエジェクトの型に応じて JObject を生成して返します。</para>
		/// </summary>
		/// <exception cref="ArgumentException" />
		public static JObject Create(object obj)
		{
			if (obj == null)
			{
				return new JObject();
			}

			JVariant v = JVariant.Create(obj);
			if (v.IsObject)
			{
				return v.AsObject();
			}

			throw new VariantException($"cannot create a JObject from {obj}");
		}


		//==============================================================================
		// instance members
		//==============================================================================

		private Dictionary<string, JVariant> m_Body;


		/// <summary>
		/// IJVariantConvertible の規定によるコンストラクタ。
		/// </summary>
		public JObject(JVariantTag v)
		{
			m_Body = v.AsObject().GetBody();
		}


		/// <summary>
		/// キーの配列
		/// </summary>
		public readonly ICollection<string> Keys
		{
			get
			{
				if (m_Body == null)
				{
					return new List<string>();
				}
				return m_Body.Keys;
			}
		}

		/// <summary>
		/// 値の配列
		/// </summary>
		public readonly ICollection<JVariant> Values
		{
			get
			{
				if (m_Body == null)
				{
					return new List<JVariant>();
				}
				return m_Body.Values;
			}
		}

		/// <summary>
		/// 要素数
		/// </summary>
		public readonly int Count
		{
			get
			{
				if (m_Body == null)
				{
					return 0;
				}
				return m_Body.Count;
			}
		}

		/// <summary>
		/// 読み取り専用か。
		/// <para>
		/// IDictionary の実装のためのものです。常に false を返します。</para>
		/// </summary>
		public readonly bool IsReadOnly => false;


		/// <summary>
		/// インデクサ
		/// <para>
		/// 存在しないキーに対するアクセスは、そのキーの要素を追加した後にそれを返します。
		/// 取得操作で内容が変更されることがあるため注意してください。その挙動が望ましくない場合は Get() を利用してください。</para>
		/// </summary>
		public JVariant this[string key]
		{
			get => EnsureItem(key);
			set => Set(key, value);
		}


		/// <summary>
		/// 要素を持っていないとき true
		/// </summary>
		public readonly bool IsEmpty() => (Count == 0);


		/// <summary>
		/// 追加。
		/// <para>
		/// すでに同じキーがある場合は AugumentException を投げます。
		/// この挙動が望ましくない場合は Set() を利用してください。</para>
		/// </summary>
		public void Add(string key, bool v) { AddInternal(key, new JVariant(v)); }
		public void Add(string key, long v) { AddInternal(key, new JVariant(v)); }
		public void Add(string key, double v) { AddInternal(key, new JVariant(v)); }
		public void Add(string key, string v) { AddInternal(key, new JVariant(v)); }
		public void Add(string key, JArray v) { AddInternal(key, new JVariant(v)); }
		public void Add(string key, JObject v) { AddInternal(key, new JVariant(v)); }
		public void Add(string key, JVariant v) { AddInternal(key, new JVariant(v)); }

		/// <summary>
		/// KeyValuePair による追加。
		/// <para>
		/// IDictionary の実装のためのものです。</para>
		/// </summary>
		public void Add(KeyValuePair<string, JVariant> item)
		{
			if (item.Value == null)
			{
				item = new KeyValuePair<string, JVariant>(item.Key, new JVariant());
			}

			EnsureBody();
			((IDictionary<string, JVariant>)m_Body).Add(item);
		}

		/// <summary>
		/// 要素の取得。
		/// <para>
		/// 指定された要素が存在しないときは Null を示す JVariant を返します。</para>
		/// <para>
		/// このメソッドで Null が入っているのと存在しないのを区別することはできません。
		/// <see cref="ContainsKey"/> などを利用してください。</para>
		/// </summary>
		public readonly JVariant Get(string key)
		{
			if (m_Body == null || !m_Body.ContainsKey(key))
			{
				return new JVariant();
			}

			return m_Body[key];
		}

		/// <summary>
		/// 値の設定
		/// <para>
		/// JVariant は可変の参照型ですが、設定時は基本的に参照を替えるのではなく、内容を書き換えようとします。
		/// <code>
		/// JVariant v = new JVariant();
		/// JObject obj = new JObject();
		/// 
		/// // v を int にしておく
		/// v.Assign( 1 );
		/// 
		/// // この行為は v の参照を "A" にもつのではなく、
		/// // A に新しい JVariant をつくり、そこに v の内容を書き込む。
		/// obj.Set( "A", v );
		/// 
		/// // v を書き換える
		/// v = "STRING";
		/// 
		/// // obj["A"] と v は共有されていない。
		/// DebugAssert( obj["A"].IntValue == 1 );
		/// </code>
		/// 多くの場合この仕様は安全な方向に倒れますが、
		/// JObject の内部を外の変数と共有することを期待したコードは動作しないことになります。</para>
		/// <para>
		/// インデクサの get は JObject内部の参照を返すため、それを利用すれば可能ではありますが、
		/// できるだけ、JObject内部の JVariant 参照を外部と共有することを期待する処理はしないようにしてください。</para>
		/// </summary>
		public void Set(string key, bool value) { EnsureItem(key).Assign(value); }
		public void Set(string key, long value) { EnsureItem(key).Assign(value); }
		public void Set(string key, double value) { EnsureItem(key).Assign(value); }
		public void Set(string key, string value) { EnsureItem(key).Assign(value); }
		public void Set(string key, JArray value) { EnsureItem(key).Assign(value); }
		public void Set(string key, JObject value) { EnsureItem(key).Assign(value); }
		public void Set(string key, JVariant value) { EnsureItem(key).Assign(value); }





		/// <summary>
		/// クリア
		/// </summary>
		public void Clear()
		{
			m_Body?.Clear();
		}

		/// <summary>
		/// 指定された要素を含むか。
		/// <para>IDictionary の実装のためのものです。</para>
		/// </summary>
		public readonly bool Contains(KeyValuePair<string, JVariant> item)
		{
			if (m_Body == null)
			{
				return false;
			}
			return ((IDictionary<string, JVariant>)m_Body).Contains(item);
		}

		/// <summary>
		/// 指定されたキーの要素を含むか
		/// </summary>
		public readonly bool ContainsKey(string key)
		{
			if (m_Body == null)
			{
				return false;
			}
			return m_Body.ContainsKey(key);
		}

		/// <summary>
		/// 配列へのコピー。
		/// <para>IDictionary の実装のためのものです。</para>
		/// </summary>
		public readonly void CopyTo(KeyValuePair<string, JVariant>[] array, int arrayIndex)
		{
			if (m_Body == null)
			{
				return;
			}
			((IDictionary<string, JVariant>)m_Body).CopyTo(array, arrayIndex);
		}

		/// <summary>
		/// IEnumeratorを返す。
		/// <para>IDictionary の実装のためのものです。</para>
		/// </summary>
		public readonly IEnumerator<KeyValuePair<string, JVariant>> GetEnumerator()
		{
			if (m_Body == null)
			{
				yield break;
			}

			foreach (var p in m_Body)
			{
				yield return p;
			}
		}

		/// <summary>
		/// 要素削除
		/// </summary>
		public bool Remove(string key)
		{
			return m_Body?.Remove(key) ?? false;
		}

		/// <summary>
		/// 要素削除
		/// <para>IDictionary の実装のためのものです。</para>
		/// </summary>
		public bool Remove(KeyValuePair<string, JVariant> item)
		{
			if (m_Body == null)
			{
				return false;
			}
			return ((IDictionary<string, JVariant>)m_Body).Remove(item);
		}

		/// <summary>
		/// 要素取得
		/// </summary>
		public readonly bool TryGetValue(string key, out JVariant value)
		{
			if (m_Body == null)
			{
				value = new JVariant();
				return false;
			}
			return m_Body.TryGetValue(key, out value);
		}

		/// <summary>
		/// 内部データを返す。
		/// <para>
		/// この JObject の内部データを返します。参照をそのまま返すのでこれを編集する場合は注意してください。
		/// 内部的には null であることがあり、それを JObject 内で同一視するようにしていますが、
		/// このメソッドが null を返すことはありません。</para>
		/// </summary>
		public Dictionary<string, JVariant> GetBody()
		{
			return EnsureBody();
		}


		/// <summary>
		/// なんとなく配列に変換する。
		/// <para>
		/// キーのすべてが int として解釈可能であれば、
		/// そのインデックスに各要素を詰めた配列を p に返します。
		/// 失敗した場合は false を返します。
		/// </para>
		/// <para>
		/// 各項目が int として解釈可能かどうかしか判定していません。
		/// int として解釈した結果同じ値に解決することがありますが( "1" と "+1" とか)
		/// それは配慮されていません、どちらかが失われたうえで、配列に変換可能とされます。
		/// </para>
		/// </summary>
		public readonly bool TryConvertToArray(out JArray p)
		{
			try
			{
				var ret = new JArray();
				foreach (string k in this.Keys)
				{
					int index = int.Parse(k);
					ret.Set(index, Get(k));
				}
				p = ret;
				return true;
			}
			catch (FormatException) { }
			catch (ArgumentException) { }

			p = new JArray();
			return false;
		}

		/// <summary>
		/// 文字列化
		/// <para>
		/// なんとなく内部状態を示す文字列を返します。
		/// JSON 表現は <see cref="ToJSON(JsonFormatPolicy)">ToJSON()</see> を利用してください。</para>
		/// </summary>
		public override readonly string ToString()
		{
			return new JVariant(this).ToString();
		}

		/// <summary>
		/// JSON 表現を返す。
		/// </summary>
		/// <param name="p">フォーマット指定、省略時は改行なし</param>
		public readonly string ToJSON(JsonFormatPolicy policy = null)
		{
			return AsVariant().ToJSON(policy);
		}

		public readonly U8View ToU8JSON(JsonFormatPolicy policy = null)
		{
			return AsVariant().ToU8JSON(policy);
		}


		/// <summary>
		/// ドット表記による子要素の参照
		/// <para>
		/// JVariant.Pick() を参照してください。</para>
		/// </summary>
		public readonly JVariant Pick(string path)
		{
			return new JVariant(this).Pick(path);
		}


		/// <summary>
		/// 内容がシンプルなとき true.
		/// <para>
		/// JSON変換の際の改行の判定に利用されます。
		/// プリミティブな要素を一つだけ持つ場合、シンプルとみなされます。</para>
		/// </summary>
		public readonly bool IsSimple()
		{
			if (this.Count == 0)
			{
				return true;
			}
			if (this.Count == 1)
			{
				return !m_Body.Values.First().IsComposite;
			}
			return false;
		}

		internal readonly void ConvertToJSON(ref StringifyContext context)
		{
			try
			{
				context.Push(m_Body);
				var appender = context.GetAppender(IsEmpty(), IsSimple());

				appender.Append('{');

				bool first = true;
				foreach (var p in this)
				{
					if (!first)
					{
						appender.AppendItemSeparator();
					}
					first = false;

					appender.AppendNewLine();

					appender.Append('"');
					appender.Append(TextUtil.EscapeJsonString(p.Key, context.Policy.EscapeUnicode));
					appender.Append("\": ");
					p.Value.ConvertToJSON(ref context);
				}

				appender.AppendNewLine(-1);
				appender.Append('}');
			}
			finally
			{
				context.Pop(m_Body);
			}
		}


		/// <summary>
		/// ディープコピー
		/// <para>
		/// この JObject がもつ内容と同じ内容を持つ JObject を新たに作成して返す。</para>
		/// <para>
		/// 各項目はそれぞれ再帰的に内容のコピーを作成します。</para>
		/// <para>
		/// JObject は項目に自分自身を持ちえますが、そのような場合の配慮はされていません。（永久ループになります）
		/// ループするようなオブジェクトの構造はまずないとは思いますが、念の為配慮してください。
		/// </para>
		/// </summary>
		public readonly JObject Duplicate()
		{
			var ret = new JObject();

			if (IsEmpty())
			{
				return ret;
			}
			foreach (var p in m_Body)
			{
				ret.AddInternal(p.Key, p.Value.Duplicate());
			}
			return ret;
		}

		/// <summary>
		/// IJVariantConvertible の実装。
		/// </summary>
		public readonly JVariant AsVariant()
		{
			return new JVariant(this);
		}

		public readonly bool EquivalentTo(JObject other, int maxDepth = JVariant.DefaultMaxDepth, int depth = 0)
		{
			if (Count != other.Count)
			{
				return false;
			}

			if (ReferenceEquals(this.m_Body, other.m_Body))
			{
				return true;
			}


			var o1 = m_Body;
			var o2 = other.m_Body;

			foreach (var key in o1.Keys)
			{
				if (!o2.TryGetValue(key, out var v2))
				{
					return false;
				}
				var v1 = o1[key];
				if (!v1.EquivalentTo(v2, maxDepth, depth + 1))
				{
					return false;
				}
			}
			return true;
		}



		/// <summary>
		/// IEnumeratorを返す。(IEnumerable の実装のため。)
		/// </summary>
		IEnumerator IEnumerable.GetEnumerator()
		{
			EnsureBody();
			return m_Body.GetEnumerator();
		}

		// add の実装のため。
		private void AddInternal(string key, JVariant value)
		{
			EnsureBody();
			m_Body.Add(key, value);
		}

		// Set の実装のため。確実に key の要素が存在するようにしてそれを返す。Setはそこに Assign すると無駄がない。
		private JVariant EnsureItem(string key)
		{
			EnsureBody();
			JVariant ret = null;
			if (m_Body.TryGetValue(key, out ret))
			{
				if (ret != null)
				{
					return ret;
				}
			}
			ret = new JVariant();
			m_Body[key] = ret;
			return ret;
		}

		// CreateInternal 実装用
		private JObject(Dictionary<string, JVariant> b)
		{
			m_Body = b;
		}

		// body が null なら新しく作る
		private Dictionary<string, JVariant> EnsureBody()
		{
			m_Body ??= new Dictionary<string, JVariant>();
			return m_Body;
		}


	};

}

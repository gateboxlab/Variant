using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using Gatebox.Variant.Internal;


namespace Gatebox.Variant
{
	/// <summary>
	/// Javascript 的な配列を表す値型。
	/// <para>
	/// IList&lt;JVariant&gt; を実装します。JSON の値の配列として扱うことができます。</para>
	/// <para>
	/// 内部に List&lt;JVariant&gt; を持っています。
	/// このクラスは値型ですが「参照を値で持っている」という状態であるため注意してください。
	/// JArray をコピーする、という行為はその参照をコピーすることになり、結果として内部情報である List&lt;JVariant&gt; は共有されることになります。</para>
	/// <para>
	/// 多数のメソッドがありますが、
	/// 末尾への追加は Add()、設定は Set(), 取得は Get() です。</para>
	/// <para>
	/// [] による要素へのアクセスもできますが、 [] による要素のアクセスはそれが存在しない場合にそこまで要素を作って返すことに注意してください。
	/// (いきなり array[1000] とかやると 1001 個の要素を作ってその中の一つを返してきます。)</para>
	/// </summary>
	public struct JArray : IList<JVariant>, IJVariantConvertible
	{
		//==============================================================================
		// static members
		//==============================================================================

		/// <summary>
		/// 内部情報を受け取っての生成。
		/// </summary>
		/// <param name="b">そのまま受け取る JVariant の配列</param>
		internal static JArray CreateInternal(List<JVariant> b)
		{
			return new JArray(b);
		}

		/// <summary>
		/// 内部に List を保持して生成。
		/// <para>
		/// JArray は内部に List を持っており、
		/// それが null の場合と内容がない場合を同一視するようにしていますが、
		/// boxing が発生したときなどには微妙に挙動が異なることになります。</para>
		/// <para>
		/// デフォルトコンストラクタでは null の状態で生成されますが、
		/// このメソッドは内部の List を空の状態としてもつ JArray を返します。</para>
		/// <para>
		/// 内部情報が共有されてほしいときはこれを利用してください。</para>
		/// </summary>
		public static JArray Create()
		{
			return new JArray(new List<JVariant>());
		}

		public static JArray Create<T>( IList<T> list)
		{
			List<JVariant> v = new();
			foreach (var o in list)
			{
				v.Add(JVariant.Create(o));
			}
			return new JArray(v);
		}


		//==============================================================================
		// instance members
		//==============================================================================

		// 本体。生成時は null. 一度 非 null になったら変更されることはない。
		// JVariant は参照型だが、 null が入っていることはないように。(Null を示す JVariant を new してそれぞれ設定)
		private List<JVariant> m_Body;


		/// <summary>
		/// 配列からの生成。
		/// <para>
		/// 配列やListから、その内容を持つ JArray を生成します。
		/// 内容に関しては実行時に型チェックを行うため、
		/// 多少動作は重く、また処理できない型の配列に関しては失敗(ArgumentException を throw) します。</para>
		/// <para>
		/// 内容の制約に関しては JVariant.Create() を参照してください。
		/// </para>
		/// </summary>
		/// <param name="t">配列</param>
		public JArray(System.Collections.IList t)
		{
			if (t == null)
			{
				m_Body = null;
				return;
			}
			m_Body = new List<JVariant>();
			foreach (object o in t)
			{
				this.Add(JVariant.Create(o));
			}
		}

		/// <summary>
		/// コピーによるコンストラクタ
		/// <para>
		/// 引数で与えられた JArray と内容を共有します。</para>
		/// <para>
		/// 値型なのでわざわざこれを呼ぶ必要はありません、単純に代入するのと結果は同じです。
		/// </para>
		/// </summary>
		public JArray(JArray other)
		{
			m_Body = other.GetBody();
		}


		/// <summary>
		/// 要素数。
		/// </summary>
		public readonly int Count
		{
			get
			{
				return m_Body == null ? 0 : m_Body.Count;
			}
		}



		/// <summary>
		/// 読み取り専用か。
		/// <para>
		/// IList の実装です。常に false を返します。</para>
		/// </summary>
		public readonly bool IsReadOnly => false;


		/// <summary>
		/// インデクサ。
		/// <para>
		/// 範囲外の index の取得は それが入るところまで Null で埋められたのち、その Null を示す JVariant を返します。</para>
		/// <para>
		/// 範囲外の index へ設定は、それが入るところまで Null で埋められます。</para>
		/// <para>
		/// 取得操作で内容が変更されることがあることに注意してください。
		/// その挙動が望ましくない場合は Get() を利用してください。</para>
		/// </summary>
		/// <param name="index">インデックス</param>
		public JVariant this[int index]
		{
			get
			{
				EnsureIndex(index);
				return m_Body[index];
			}
			set
			{
				Set(index, value);
			}
		}

		/// <summary>
		/// インデクサ
		/// </summary>
		public JVariant this[Index index]
		{
			get
			{
				var offset = index.GetOffset(Count);
				EnsureIndex(offset);
				return m_Body[offset];
			}
			set
			{
				var offset = index.GetOffset(Count);
				Set(offset, value);
			}
		}


		/// <summary>
		/// 要素を持っていないとき true.
		/// </summary>
		public readonly bool IsEmpty() => (Count == 0);


		/// <summary>追加。</summary>
		public void Add(bool v) { AddInternal(new JVariant(v)); }
		public void Add(long v) { AddInternal(new JVariant(v)); }
		public void Add(double v) { AddInternal(new JVariant(v)); }
		public void Add(string v) { AddInternal(new JVariant(v)); }
		public void Add(JArray v) { AddInternal(new JVariant(v)); }
		public void Add(JObject v) { AddInternal(new JVariant(v)); }
		public void Add(JVariant v) { AddInternal(new JVariant(v)); }

		/// <summary>
		/// 要素の取得。
		/// 指定された要素が存在しないときは Null を示す JVariant を返します。
		/// (このメソッドで Null が入っているのと存在しないのを区別することはできません。Count などを利用してください。)</summary>
		public readonly JVariant Get(int index)
		{
			Debug.Assert(index >= 0);
			if (m_Body == null || index < 0 || index >= m_Body.Count)
			{
				return new JVariant();
			}
			return m_Body[index];
		}

		/// <summary>
		/// 要素設定。
		/// <para>範囲外のインデックスを指定した場合、それが入るところまで Null で埋められます。</para>
		/// </summary>
		public void Set(int index, bool v) { EnsureIndex(index).Assign(v); }
		public void Set(int index, long v) { EnsureIndex(index).Assign(v); }
		public void Set(int index, double v) { EnsureIndex(index).Assign(v); }
		public void Set(int index, string v) { EnsureIndex(index).Assign(v); }
		public void Set(int index, JArray v) { EnsureIndex(index).Assign(v); }
		public void Set(int index, JObject v) { EnsureIndex(index).Assign(v); }
		public void Set(int index, JVariant item) { EnsureIndex(index).Assign(item); }

		/// <summary>
		/// サイズ変更。
		/// <para>
		/// this.Count が指定されたサイズより大きければ要素を後ろから削除し、
		/// this.Count が指定されたサイズより小さければ Null を表す JVariant を後ろに追加します。</para>
		/// </summary>
		public void Resize(int size)
		{
			Debug.Assert(size >= 0);

			EnsureBody();
			int current = m_Body.Count;

			if (size < current)
			{
				m_Body.RemoveRange(size, current - size);
			}
			else if (size > current)
			{
				for (int i = 0; i < (size - current); i++)
				{
					m_Body.Add(new JVariant());
				}
			}
		}


		/// <summary>
		/// クリア。
		/// <para>
		/// 内容を空にします。
		/// この（値型としての）変数のクリアではなく、持っている配列のクリアであることに注意してください。
		/// 配列が共有されている場合はそれにも影響を与えます。</para>
		/// </summary>
		public readonly void Clear()
		{
			// この JArray 単体を見れば、body を捨てればいいだけ、
			// これによって共有されているオブジェクトが分断されるが、
			// 利用者の意図がどちらなのかはここではわからない。
			// body = null;

			// オブジェクトが共有されているということに従うならば、そのオブジェクトをクリアすべき。
			// この実装はそれに従う。
			m_Body?.Clear();
		}

		/// <summary>
		/// 指定された要素が含まれるか？
		/// </summary>
		public readonly bool Contains(JVariant item)
		{
			return m_Body == null ? false : m_Body.Contains(item);
		}

		/// <summary>
		/// 配列へのコピー
		/// </summary>
		public readonly void CopyTo(JVariant[] array, int arrayIndex)
		{
			m_Body?.CopyTo(array, arrayIndex);
		}

		/// <summary>
		/// 反復子を返す。
		/// </summary>
		public IEnumerator<JVariant> GetEnumerator()
		{
			return EnsureBody().GetEnumerator();
		}

		/// <summary>
		/// 指定された要素があればその位置を返す。なければ -1 を返す。
		/// </summary>
		public readonly int IndexOf(JVariant item)
		{
			return m_Body == null ? -1 : m_Body.IndexOf(item);
		}

		/// <summary>
		/// 要素挿入
		/// </summary>
		public void Insert(int index, JVariant item)
		{
			EnsureBody().Insert(index, item);
		}

		/// <summary>
		/// 指定要素削除
		/// </summary>
		public bool Remove(JVariant item)
		{
			return EnsureBody().Remove(item);
		}

		/// <summary>
		/// 指定位置要素削除
		/// </summary>
		public void RemoveAt(int index)
		{
			EnsureBody().RemoveAt(index);
		}


		/// <summary>	
		/// 内部データを返す。
		/// <para>
		/// この JArray の内部データを返します。参照をそのまま返すのでこれを編集する場合は注意してください。
		/// JArray は内部情報が null の場合と 0 件の場合があり、表面的にはそれを同等のものとして扱っています。</para>
		/// <para>
		/// このメソッドは内部状態が null の場合、 0 件の情報を生成してそれを返します。(null を返すことはありません) </para>
		/// </summary>
		public List<JVariant> GetBody()
		{
			return EnsureBody();
		}

		/// <summary>
		/// オブジェクトに変換する。
		/// <para>
		/// インデックスを文字列に変えたオブジェクトを返します。
		/// [0,1,2] であれば { "0":0, "1":1, "2":2 } が返却されます。
		/// </para>
		/// </summary>
		public readonly JObject ConvertToObject()
		{
			var ret = new JObject();
			if (m_Body == null)
			{
				return ret;
			}

			for (int i = 0; i < m_Body.Count; i++)
			{
				ret.Set(i.ToString(), m_Body[i]);
			}
			return ret;
		}

		/// <summary>
		/// 文字列化
		/// <para>
		/// なんとなく内部状態を示す文字列を返します。JSON 表現は ToJSON() を利用してください。</para>
		/// </summary>
		public override readonly string ToString()
		{
			return new JVariant(this).ToString();
		}

		/// <summary>
		/// 内容がシンプルなとき true.
		/// <para>
		/// JSON変換の際の改行の判定に利用されます。
		/// プリミティブのみからなるとき true になります。</para>
		/// </summary>
		public readonly bool IsSimple()
		{
			for( int i=0; i<this.Count; i++)
			{
				if(Get(i).IsComposite)
				{
					return false;
				}
			}
			return true;
		}

		/// <summary>
		/// JSON 表現を返す。
		/// <para>
		/// フォーマット指定を省略すると改行無しでベタの JSON が返却されます。
		/// 改行してくれたほうがいいときは JsonFormatPolicy の Pretty 等を利用してください。</para>
		/// </summary>
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

		internal readonly void ConvertToJSON(ref StringifyContext context)
		{
			try
			{
				context.Push(m_Body);
				var appender = context.GetAppender(IsEmpty(), IsSimple());

				appender.Append('[');

				// 各要素、続きならカンマ、改行、要素
				for (int i = 0; i < this.Count; i++)
				{
					if (i != 0)
					{
						appender.AppendItemSeparator();
					}
					appender.AppendNewLine();
					this.Get(i).ConvertToJSON(ref context);
				}

				appender.AppendNewLine(-1);
				appender.Append(']');
			}
			finally
			{
				context.Pop(m_Body);
			}
		}


		/// <summary>
		/// ディープコピー
		/// <para>
		/// この JArray がもつ内容と同じ内容を持つ JArray を新たに作成して返す。</para>
		/// <para>
		/// 各項目はそれぞれ再帰的に内容のコピーを作成します。</para>
		/// <para>
		/// JArray は項目に自分自身を持ちえますが、そのような場合の配慮はされていません。（永久ループになります）
		/// ループするようなオブジェクトの構造はまずないとは思いますが、念の為配慮してください。
		/// </para>
		/// </summary>
		public readonly JArray Duplicate()
		{
			JArray ret = new JArray();

			if (IsEmpty())
			{
				return ret;
			}

			for (int i = 0; i < m_Body.Count; i++)
			{
				ret.AddInternal(m_Body[i].Duplicate());
			}

			return ret;
		}

		/// <summary>
		/// JVariant として自分を返す。
		/// </summary>
		public readonly JVariant AsVariant()
		{
			return new JVariant(this);
		}


		public bool EquivalentTo(JArray other, int maxDepth = JVariant.DefaultMaxDepth, int depth = 0)
		{
			if( Count != other.Count)
			{
				return false;
			}

			if(ReferenceEquals(this.m_Body, other.m_Body))
			{
				return true;
			}

			for(int i=0; i<Count; i++)
			{
				if(!Get(i).EquivalentTo(other.Get(i), maxDepth, depth+1))
				{
					return false;
				}
			}
			
			return true;
		}


		/// <summary>
		/// 反復子を返す。(非ジェネリック)
		/// </summary>
		IEnumerator IEnumerable.GetEnumerator()
		{
			return EnsureBody().GetEnumerator();
		}

		// 内部情報を受け取っての生成。CreateInternal() の実装用です。
		internal JArray(List<JVariant> b)
		{
			m_Body = b;
		}

		// body が null ならば新しいのを作る
		private List<JVariant> EnsureBody()
		{
			m_Body ??= new List<JVariant>();
			return m_Body;
		}

		// 指定されたインデックスの要素がある状態にする。（サイズじゃなくてインデックスなので注意）
		private JVariant EnsureIndex(int index)
		{
			if (index < 0)
			{
				throw new ArgumentException();
			}
			if (index >= this.Count)
			{
				this.Resize(index + 1);
			}
			return m_Body[index];
		}

		// add の実装のため。
		private void AddInternal(JVariant value)
		{
			EnsureBody();
			m_Body.Add(value);
		}

	}
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gatebox.Variant.Internal
{

	/// <summary>
	/// JSON 文字列化のコンテキスト
	/// <para>
	/// 状態を持つ struct であるため注意
	/// </para>
	/// </summary>
	internal struct StringifyContext : IDisposable
	{
		//==============================================================================
		// inner types
		//==============================================================================

		public readonly struct Appender
		{
			private readonly JsonFormatPolicy m_Policy;
			private readonly IBuffer m_Buffer;
			private readonly int m_Depth;
			private readonly bool m_NeedsLine;


			public Appender( JsonFormatPolicy policy, IBuffer buffer, int depth, bool newline)
			{
				m_Policy = policy;
				m_Buffer = buffer;
				m_Depth = depth;
				m_NeedsLine = newline;
			}

			public void Append(char c)
			{
				m_Buffer.Append(c);
			}

			public void Append(string s)
			{
				m_Buffer.Append(s);
			}


			public void AppendItemSeparator()
			{
				m_Buffer.Append(',');
				if (! m_NeedsLine)
				{ 
					m_Buffer.Append(' ');
				}
			}

			public void AppendNewLine(int indent_difference = 0)
			{
				if (m_NeedsLine)
				{
					m_Buffer.Append('\n');

					for (int i = 0; i < m_Depth + indent_difference; i++)
					{
						if( m_Buffer.BufferType == BufferType.U16)
						{
							m_Buffer.Append(m_Policy.Indent);
						}
						else
						{
							m_Buffer.Append(m_Policy.IndentU8);
						}
					}
				}
			}
		}


		//==============================================================================
		// static members
		//==============================================================================

		public static StringifyContext ForU8(JsonFormatPolicy policy)
		{
			return new StringifyContext(new U8Buffer(), policy);
		}
		public static StringifyContext ForString(JsonFormatPolicy policy)
		{
			return new StringifyContext(new U16Buffer(), policy);
		}

		//==============================================================================
		// instance members
		//==============================================================================

		private readonly IBuffer m_Buffer;
		private readonly JsonFormatPolicy m_Policy;
		private int m_Depth;


		/// <summary>
		/// コンストラクタ
		/// </summary>
		public StringifyContext(IBuffer buffer, JsonFormatPolicy policy)
		{
			m_Depth = 0;
			m_Buffer = buffer;
			m_Policy = policy;
		}

		/// <summary>
		/// ポリシー
		/// </summary>
		public readonly JsonFormatPolicy Policy => m_Policy;


		/// <summary>
		/// バッファ
		/// </summary>
		public readonly IBuffer GetBuffer() => m_Buffer;


		/// <summary>
		/// 破棄
		/// </summary>
		public readonly void Dispose()
		{
			m_Buffer?.Dispose();
		}

		/// <summary>
		///  push
		/// <para>
		/// 循環参照の検出とインデントの処理のため。</para>
		/// <para>
		/// 引数は JArray や JObject の内部オブジェクトですが、使っていません。
		/// この引数を使えばちゃんと循環参照を検出することができるのですが、
		/// ほぼありえないことに対応するために仰々しい実装が必要になってしまうので、単純な深さのみで例外を投げます。
		/// </para>
		/// </summary>
		public void Push(object o)
		{
			m_Depth += 1;
			if(m_Depth > m_Policy.MaxDepth)
			{
				throw new JsonFormatException("Exceeded maximum depth. Circular reference suspected.");
			}
		}

		/// <summary>
		/// スタックから pop
		/// <para>
		/// 引数を受ける意味はありません。形式的なものです。
		/// </para>
		/// </summary>
		public void Pop(object o)
		{
			System.Diagnostics.Debug.Assert(m_Depth >= 1);
			m_Depth -=1;
		}

		/// <summary>
		/// 追記のための Appender を返す。
		/// <para>
		/// 一定のレベル内の追記は Appender を使って行います。Appender は readonly struct です。
		/// </para>
		/// </summary>
		public readonly Appender GetAppender()
		{
			return new Appender(m_Policy, m_Buffer, m_Depth, m_Policy.ReturnPolicy == ReturnPolicy.Every);
		}
		public readonly Appender GetAppender( bool isEmpty, bool isSimple)
		{
			var returnPolicy = m_Policy.ReturnPolicy;

			if (returnPolicy == ReturnPolicy.Never)
			{
				return new Appender(m_Policy, m_Buffer, m_Depth, false);
			}
			if (returnPolicy == ReturnPolicy.Every)
			{
				return new Appender(m_Policy, m_Buffer, m_Depth, true);
			}
			if (isEmpty)
			{
				return new Appender(m_Policy, m_Buffer, m_Depth, false);
			}
			if (returnPolicy == ReturnPolicy.Simple)
			{
				return new Appender(m_Policy, m_Buffer, m_Depth, !isSimple);
			}

			return new Appender(m_Policy, m_Buffer, m_Depth, true);
		}

		public readonly Appender GetAppenderFor( JObject obj)
		{
			var returnPolicy = m_Policy.ReturnPolicy;

			if (returnPolicy == ReturnPolicy.Never)
			{
				return new Appender(m_Policy, m_Buffer, m_Depth, false);
			}
			if (returnPolicy == ReturnPolicy.Every)
			{
				return new Appender(m_Policy, m_Buffer, m_Depth, true);
			}
			if (obj.IsEmpty())
			{
				return new Appender(m_Policy, m_Buffer, m_Depth, false);
			}
			if (returnPolicy == ReturnPolicy.Simple)
			{
				return new Appender(m_Policy, m_Buffer, m_Depth, !obj.IsSimple());
			}

			return new Appender(m_Policy, m_Buffer, m_Depth, true);
		}

		


		public readonly string StringResult()
		{
			return m_Buffer.GetStringView().ToString();
		}

		public readonly U8View U8Result()
		{
			return m_Buffer.GetU8View();
		}
	}
}

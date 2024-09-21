﻿using System;
using System.Text;

namespace Gatebox.Variant.Internal
{

	internal enum BufferType
	{
		U16,
		U8,
	} 


	/// <summary>
	/// バッファ。
	/// <para>
	/// char を追記していく機構と byte を追記していく機構がどうしてもまとめられなかったので
	/// インターフェース経由で共通化しています。
	/// </para>
	/// </summary>
	internal interface IBuffer : IDisposable
	{
		BufferType BufferType {get;}

		void Append(char c);
		void Append(string s);
		void Append(long l);
		void Append(double d);
		void Append(U8View v);
		void Append(Literal lit);

		/// <summary>
		/// 内容を string で得る。
		/// <para>
		/// StringView を返却してはいますが、その内容は string 全体を指すものです。</para>
		/// </summary>
		StringView GetStringView();


		/// <summary>
		/// 内容を UTF-8 の並びとして得る。
		/// <para>
		/// 処理の最後に結果を得るためのものとして用意しています。
		/// <para>>
		/// このメソッドは char 版では例外を投げます。
		/// また、返却されれる U8View の内容は Dispose しても大丈夫なものである反面、
		/// IBuffer としては副作用的に内容がクリアされるので、
		/// 処理の最後に結果を得る以外の用途で利用しないでください。</para>
		/// </summary>
		U8View GetU8View();
	}

	internal class U16Buffer : IBuffer
	{
		private readonly StringBuilder m_Builder;

		public BufferType BufferType => BufferType.U16;

		public U16Buffer()
		{
			m_Builder = StringBuilderPool.Rent();
		}

		
		public void Append(char c)
		{
			m_Builder.Append(c);
		}

		public void Append(string s)
		{
			m_Builder.Append(s);
		}

		public void Append(long l)
		{
			m_Builder.Append(l);
		}

		public void Append(double d)
		{
			m_Builder.Append(d);
		}

		public void Append(U8View v)
		{
			m_Builder.Append(v.ToString());
		}
		public void Append(Literal lit)
		{
			m_Builder.Append(lit.U16);
		}

		public void Dispose()
		{
			StringBuilderPool.Return(m_Builder);
		}

		public StringView GetStringView()
		{
			return m_Builder.ToString();
		}
		public U8View GetU8View()
		{
			throw new InvalidOperationException();
		}
	}

	internal class U8Buffer : IBuffer
	{
		private U8Builder m_Builder;

		public BufferType BufferType => BufferType.U8;

		public U8Buffer()
		{
			m_Builder = new U8Builder();
		}

		public void Append(char c)
		{
			m_Builder.Append(c);
		}

		public void Append(string s)
		{
			m_Builder.Append(s);
		}

		public void Append(long l)
		{
			m_Builder.Append(l);
		}

		public void Append(double d)
		{
			// これがすごく効率が悪い。dtoa の C# 実装が必要
			string s = d.ToString();
			m_Builder.Append(s);
		}

		public void Append(U8View v)
		{
			m_Builder.Append(v);
		}

		public void Append(Literal lit)
		{
			m_Builder.Append(lit.U8);
		}

		public void Dispose()
		{
			m_Builder.Dispose();
		}

		public StringView GetStringView()
		{
			return m_Builder.ToString();
		}
		public U8View GetU8View()
		{
			return m_Builder.Detach();
		}
	}





}

using System;
using System.Collections.Generic;
using System.Threading;

namespace Gatebox.Variant.Internal
{
	/// <summary>
	/// 変換中の情報
	/// <para>
	/// JVariant ⇔ 任意型 間の変換は <see cref="VariantConverter"/> によって行うようにしつつも、
	/// <see cref="IJVariantConvertible"/> については変換対象の型に変換を任せています。</para>
	/// <para>
	/// そのうえで、
	/// <see cref="IJVariantConvertible"/> が <see cref="VariantConverter"/> を意識せずに変換を実装できるようにしつつ、
	/// 大局の <see cref="VariantConverter"/> は引き継がれるようにするために、このクラスを AsyncLocal で保持しています。</para>
	/// <para>
	/// JVariant ⇔ 任意型 間の変換を行う際にはそのスコープで <see cref="Acquire"/> を呼び出し、
	/// 変換が終わったら <see cref="Release"/> を呼び出してください。</para>
	/// <para>
	/// 特に指定がない場合は <see cref="Converter"/> プロパティを使って <see cref="VariantConverter"/> を取得して変換を行い、
	/// <see cref="VariantConverter"/> を変更する場合は
	/// <see cref="PushConverter(VariantConverter)"/>, <see cref="PopConverter"/> を確実に対応させて呼び出してください。</para>
	/// <para>
	/// このようにすることにより、スタック状に <see cref="VariantConverter"/> が保持され、
	/// 特に意識せずに変換を行った場合はより上位で設定された <see cref="VariantConverter"/> が使われます。</para>
	/// </summary>
	internal class ConvertContext
	{
		//==============================================================================
		// static members
		//==============================================================================


		private readonly static AsyncLocal<ConvertContext> s_Current = new AsyncLocal<ConvertContext>();


		/// <summary>
		/// ConvertContext のインスタンスを取得する。
		/// <para>
		/// コンテキスト内の上位で生成されている場合はそれを、なければここで生成して ConvertContext を返します。</para>
		/// <para>
		/// 変換が終わったら確実に<see cref="Release()" />を呼び出してください。</para>
		/// </summary>
		/// <remarks>
		/// ConvertContextScope みたいなものを返して IDisposable で Release を呼び出すようにするのもありかもしれませんが、
		/// どうしてもそのためだけのオブジェクトを生成する必要が生じ、
		/// Gatebox.Variant 内のみの利用と考えたときあまりメリットがありません。
		/// 間違えないようにすれば良い、と考えます。
		/// </remarks>
		public static ConvertContext Acquire()
		{
			var current = s_Current.Value;
			if (current != null)
			{
				current.Increment();
				return current;
			}

			var context = new ConvertContext();
			s_Current.Value = context;
			return context;
		}

		//==============================================================================
		// instance members
		//==============================================================================

		private int m_Depth;
		private List<VariantConverter> m_Converters;

		// コンストラクタ
		private ConvertContext()
		{
			m_Depth = 0;
		}


		/// <summary>
		/// 現在の変換の深さ
		/// </summary>
		public int Depth => m_Depth;

		/// <summary>
		/// 現在の VariantConverter
		/// </summary>
		public VariantConverter Converter
		{
			get
			{
				if (m_Converters is null || m_Converters.Count == 0)
				{
					return VariantConverter.Default;
				}
				return m_Converters[^1];
			}
		}

		/// <summary>
		/// VariantConverter を Push する。
		/// <para>
		/// VariantConverter を明示的に指定する場合にこれを呼び出してください。
		/// その変換の最中に <see cref="IJVariantConvertible"/> が新たな変換を行った場合、
		/// ここで Push した VariantConverter が引き継がれて利用されます。</para>
		/// <para>
		/// 変換のスコープに合わせて <see cref="PopConverter"/> を呼び出してください。
		/// </para>
		/// </summary>
		public void PushConverter(VariantConverter converter)
		{
			m_Converters ??= new List<VariantConverter>();
			m_Converters.Add(converter);
		}

		/// <summary>
		/// VariantConverter を Pop する。
		/// <para>
		/// <see cref="PushConverter(VariantConverter)"/> と対応させて確実に呼び出してください。</para>
		/// </summary>
		public void PopConverter()
		{
			if (m_Converters is null || m_Converters.Count == 0)
			{
				throw new InvalidOperationException("No converter to pop.");
			}
			m_Converters.RemoveAt(m_Converters.Count - 1);
		}

		/// <summary>
		/// スコープ終了
		/// <para>
		/// <see cref="Acquire"/> で取得した ConvertContext のスコープの末尾で呼び出してください。</para>
		/// </summary>
		public void Release()
		{
			if (Decrement() <= 0)
			{
				s_Current.Value = null;
			}
		}


		private int Decrement()
		{
			m_Depth -= 1;
			return m_Depth;
		}

		private void Increment()
		{
			m_Depth += 1;
			if (Depth > JVariant.DefaultMaxDepth)
			{
				throw new InvalidOperationException("Too deep conversion. Circular reference suspected.");
			}
		}

	}
}

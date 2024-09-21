using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gatebox.Variant
{
	/// <summary>
	/// 短い文字列が何度もできないようにするためのもの。
	/// <para>
	/// JSON 等のパース中に、同じ文字列が何度もでてくることが予想されるときがあります。
	/// 長い文字列から切り出してきた場合、それらは同じ文字列を表すにも関わらず違う string のインスタンスになるため、
	/// メモリを無駄に消費します。</para>
	/// <para>
	/// このインターフェースはそれを避けるためのものです。</para>
	/// <see cref="StringCache"/>
	/// </summary>
	public interface IStringCache : IDisposable
	{
		/// <summary>
		/// StringView から string を取得する。
		/// <para>
		/// 基本的に StringView をそのまま string 化したものを返しますが、
		/// SetString されている場合はそちらを返します。</para>
		/// </summary>
		public string GetString(StringView view);

		/// <summary>
		/// StringView に対する string を設定する。
		/// </summary>
		public void SetString(StringView view, string value);

		/// <summary>
		/// StringView から string を取得する。ない場合は null.
		/// </summary>
		public string TryGetString(StringView view);

		/// <summary>
		/// UTF-8 の並びから string を返す。
		/// </summary>
		public string GetString(U8View view);


		/// <summary>
		/// UTF-8 の並び に対する string を設定する。
		/// </summary>
		public void SetString(U8View view, string value);


		/// <summary>
		/// UTF-8 の並びから string を返す。ない場合は null.
		/// </summary>
		public string TryGetString(U8View view);
	}


}

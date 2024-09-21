using System;

namespace Gatebox.Variant
{

	/// <summary>
	/// JSON への変換時の改行指定
	/// </summary>
	public enum ReturnPolicy
	{
		/// <summary>常に改行する</summary>
		Every,

		/// <summary>空配列空オブジェクト以外は改行する。</summary>
		ExceptEmpty,

		/// <summary>それなりにシンプルな配列、オブジェクトは改行しない</summary>
		Simple,

		/// <summary>すべて一行で出力する</summary>
		Never,
	};


	/// <summary>
	/// NaN, Infinity の扱い
	/// </summary>
	public enum SpecialFloatPolicy
	{
		/// <summary>NaN, Infinity があったら文字列で出力する</summary>
		AsString,

		/// <summary>NaN, Infinity があったら JavaScript のリテラルとして出力する。</summary>
		AsJsLiteral,

		/// <summary>NaN, Infinity があったら例外を投げる</summary>
		Throw,
	};


	/// <summary>
	/// JSON への変換のフォーマット指定。
	/// <para>
	/// いつ改行するか、インデントは何で行うかを持ちます。</para>
	/// </summary>
	public class JsonFormatPolicy
	{
		/// <summary>一行出力</summary>
		public static readonly JsonFormatPolicy OneLiner = new JsonFormatPolicy(ReturnPolicy.Never);

		/// <summary>シンプルな内容は一行で、インデントは空白２つ</summary>
		public static readonly JsonFormatPolicy Mixed = new JsonFormatPolicy(ReturnPolicy.Simple, "  ");

		/// <summary>空配列空オブジェクトは改行しない、インデントは空白２つ</summary>
		public static readonly JsonFormatPolicy Pretty = new JsonFormatPolicy(ReturnPolicy.ExceptEmpty, "  ");



		private string m_Indent;
		private byte[] m_IndentU8;


		/// <summary>
		/// コンストラクタ
		/// </summary>
		public JsonFormatPolicy()
		{
			Indent = string.Empty;
			ReturnPolicy = ReturnPolicy.Never;
			SpecialFloatPolicy = SpecialFloatPolicy.AsString;
			EscapeUnicode = false;
		}


		/// <summary>
		/// コンストラクタ
		/// <para>
		/// できれば不変としたいので、各値はこのコンストラクタで設定することになります。
		/// Unity からはプロパティの init が使えないので、コンストラクタで設定するしかありません。
		/// </para>
		/// </summary>
		public JsonFormatPolicy(
			ReturnPolicy p,
			string indent = null,
			SpecialFloatPolicy floatPolicy = SpecialFloatPolicy.AsString,
			bool escapeUnicode = false,
			int maxDepth = JVariant.DefaultMaxDepth)
		{
			Indent = indent ?? string.Empty;
			ReturnPolicy = p;
			SpecialFloatPolicy = floatPolicy;
			EscapeUnicode = escapeUnicode;
			MaxDepth = maxDepth;
		}

		/// <summary>
		/// コピーコンストラクタ
		/// </summary>
		public JsonFormatPolicy(JsonFormatPolicy src)
		{
			Indent = src.Indent;
			ReturnPolicy = src.ReturnPolicy;
			SpecialFloatPolicy = src.SpecialFloatPolicy;
			EscapeUnicode = src.EscapeUnicode;
			MaxDepth = src.MaxDepth;
		}

		/// <summary>
		/// インデント文字列
		/// </summary>
		public string Indent
		{
			get => m_Indent;
			private set
			{
				m_IndentU8 = null;
				m_Indent = value;
			}
		}

		/// <summary>
		/// インデント文字列(UTF-8)
		/// </summary>
		public U8View IndentU8
		{
			get
			{
				m_IndentU8 ??= System.Text.Encoding.UTF8.GetBytes(m_Indent);
				return new U8View(m_IndentU8);
			}
		}

		/// <summary>
		/// 改行指定
		/// </summary>
		public ReturnPolicy ReturnPolicy { get; private set; }

		/// <summary>
		/// Nan, Infinity の扱い
		/// </summary>
		public SpecialFloatPolicy SpecialFloatPolicy { get; private set; } = SpecialFloatPolicy.AsString;

		/// <summary>
		/// マルチバイト文字列をユニコードエスケープするかどうか
		/// </summary>
		public bool EscapeUnicode { get; private set; }

		/// <summary>
		/// 最大深度
		/// <para>
		/// 簡易的な親子間の循環参照の検出のための値です。深さがこの値を超えると例外を投げます。
		/// 実際にここまで深い JSON が必要な場合はこの値を変更してください。</para>
		/// </summary>
		public int MaxDepth { get; private set; } = JVariant.DefaultMaxDepth;

	}
}

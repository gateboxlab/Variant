namespace Gatebox.Variant
{
	/// <summary>
	/// JVaraint と相互変換できるというマーカーインターフェース。
	/// <para>
	/// JVariant を返す AsVariant() と
	/// JVaraintTag を引数に持つコンストラクタを公開してください。
	/// </para>
	/// </summary>
	public interface IJVariantConvertible
	{

		/// <summary>
		/// JVariant に変換する。
		/// </summary>
		public JVariant AsVariant();


		public string ToJSON(JsonFormatPolicy policy = null)
		{
			return AsVariant().ToJSON(policy);
		}

		public U8View ToU8JSON(JsonFormatPolicy policy = null)
		{
			return AsVariant().ToU8JSON(policy);
		}
	}
}

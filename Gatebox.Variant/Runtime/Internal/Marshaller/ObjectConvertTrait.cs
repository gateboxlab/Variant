using System.Collections.Generic;
using System.Dynamic;

namespace Gatebox.Variant.Internal
{
	/// <summary>
	/// enum を変換するためのトレイト
	/// <para>
	/// Variant への変換は実行時の型が利用されるため、ほぼこのクラスが利用されることはない。
	/// できることはないが、落とすべきではないと考え、空の JObject に変換する。</para>
	/// <para>
	/// Variant からの変換は、object であれば何でもいいのであれば JVariant のママであるべきという意見もあるが、
	/// それは変換しないということであり、その意味のない変換をあえて行うことは避けるべきと考える。
	/// ここは内部情報を表す最もシンプルな方に変換するべきとし、
	/// expando もしくは List{object} に変換する。
	/// </para>
	/// </summary>
	internal class ObjectConvertTrait : ConvertTrait
	{
		public override object FromVariant(JVariant variant)
		{
			var vt = variant.VariantType;
			if (vt == VariantType.Null)
			{
				return null;
			}
			if (vt == VariantType.Boolean)
			{
				return variant.BoolValue;
			}
			if (vt == VariantType.Integer)
			{
				long value = variant.LongValue;
				if (value <= int.MaxValue && value >= int.MinValue)
				{
					return (int)value;
				}
				return value;
			}
			if (vt == VariantType.Float)
			{
				return variant.DoubleValue;
			}
			if (vt == VariantType.String)
			{
				return variant.StringValue;
			}
			if (vt == VariantType.Array)
			{
				List<object> list = new List<object>();
				foreach (var item in variant.AsArray())
				{
					list.Add(item.As<object>());
				}
				return list;
			}
			if (vt == VariantType.Object)
			{
				var obj = new ExpandoObject();
				var dict = (IDictionary<string, object>)obj;
				foreach (var pair in variant.AsObject())
				{
					dict.Add(pair.Key, pair.Value.As<object>());
				}
				return obj;
			}
			throw new VariantException($"Unable to convert {vt} to {typeof(object).Name}");
		}

		public override JVariant ToVariant(object v)
		{
			return JObject.Create();
		}
	}
}

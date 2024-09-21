using System;

namespace Gatebox.Variant.Internal
{
	/// <summary>
	/// enum を変換するためのトレイト
	/// </summary>
	internal class EnumTypeConvertTrait<ENUM> : ConvertTrait<ENUM> where ENUM : struct, Enum
	{
		public override ENUM ConvertVariant(JVariant variant)
		{
			try
			{
				if (variant.IsString)
				{
					return (ENUM)Enum.Parse(typeof(ENUM), variant.AsString());
				}
				if (variant.IsNumber)
				{
					return (ENUM)Enum.ToObject(typeof(ENUM), variant.AsInt());
				}
			}
			catch (ArgumentException e)
			{
				throw new VariantException($"Unable to convert \"{variant.ToString()}\"  to {typeof(ENUM).Name}", e);
			}

			throw new VariantException($"Unable to convert {variant.VariantType}  to {typeof(ENUM).Name}");
		}

		public override JVariant CreateVariant(ENUM v)
		{
			return JVariant.Create(v.ToString());
		}
	}

}

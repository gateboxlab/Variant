using System;


namespace Gatebox.Variant.Internal
{

	internal class NullableTypeConvertTrait<T> : ConvertTrait<Nullable<T>> where T : struct
	{
		public override Nullable<T> ConvertVariant(JVariant variant)
		{
			if (variant.IsNull())
			{
				return null;
			}
			else
			{
				return variant.As<T>();
			}
		}

		public override JVariant CreateVariant(Nullable<T> v)
		{
			if (v.HasValue)
			{
				return JVariant.Create(v.Value);
			}
			else
			{
				return new JVariant();
			}
		}
	}

}

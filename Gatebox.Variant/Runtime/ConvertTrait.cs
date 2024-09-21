using System;


namespace Gatebox.Variant
{
	/// <summary>
	/// JVariant と他の型との変換を行うためのトレイト
	/// </summary>
	public abstract class ConvertTrait
	{
		/// <summary>
		/// JVaraint への変換。
		/// <para>
		/// サブクラスでオーバーライドし、v を JVariant に変換して返してください。 
		/// 変換できなかったときは <see cref="VariantException"/> を投げてください。</para>
		/// </summary>
		public abstract JVariant ToVariant(object v );


		/// <summary>
		/// JVariant からの変換。
		/// <para>
		/// サブクラスでオーバーライドし、variant を指定クラスに変換してください。
		/// 変換できなかったときは <see cref="VariantException"/> を投げてください。</para>
		/// </summary>
		public abstract object FromVariant(JVariant variant);
	}

	/// <summary>
	/// ConvertTrait のジェネリック版
	/// </summary>
	public abstract class ConvertTrait<T> : ConvertTrait
	{
		public sealed override JVariant ToVariant(object v)
		{
			if (v is T t)
			{
				return CreateVariant(t);
			}
			throw new VariantException($"Unable to convert {v.GetType().Name} to {typeof(T).Name}.");
		}

		public sealed override object FromVariant(JVariant variant)
		{
			return ConvertVariant(variant);
		}


		/// <summary>
		/// JVaraint への変換。
		/// <para>
		/// サブクラスでオーバーライドし、v を JVariant に変換して返してください。 
		/// 変換できなかったときは <see cref="VariantException"/> を投げてください。</para>
		/// </summary>
		public abstract JVariant CreateVariant(T v);


		/// <summary>
		/// JVaraint への変換。
		/// <para>
		/// サブクラスでオーバーライドし、JVariant を T に変換してください。
		/// 変換できなかったときは <see cref="VariantException"/> を投げてください。</para>
		/// </summary>
		public abstract T ConvertVariant(JVariant variant);
	}




	/// <summary>
	/// ConvertTrait を静的に指定するための属性。
	/// <para>
	/// ConvertTrait の実装にこの属性を付けておくと、アセンブリから自動的に登録されます。</para>
	/// <para>
	/// TargetType は ConvertTrait で変換する型を指定します。
	/// 基本的には具体型で、<see cref="ConvertTrait{T}"/> の T を指定しますが、
	/// ConvertTrait と同じジェネリック型引数で生成できるジェネリック定義型も指定できます。つまり、
	/// <code><pre>class MyValue&lt;T&gt; {}</pre></code>があるとして、
	/// <code><pre>[ConvertTrait(typeof(MyValue&lt;&gt;))]
	/// class MyValueTrait&lt;T&gt; : ConvertTrait&lt;MyValue&lt;T&gt;&gt; {}</pre></code>ととすることで、
	/// たとえば <c>MyValue&lt;intT&gt;</c> に対して、<c>MyValueTrait&lt;int&gt;</c> が生成されて利用されます。
	/// </para>
	/// </summary>
	[AttributeUsage(AttributeTargets.Class)]
	public class ConvertTraitAttribute : Attribute
	{
		public ConvertTraitAttribute(Type t)
		{
			TargetType = t;
		}

		public Type TargetType { get; }
	}


}

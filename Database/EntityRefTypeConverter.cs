using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Reflection;

namespace LoraStatsNet.Database;

class EntityRefTypeConverter : TypeConverter
{
	private readonly Func<long, object> constructor;
	private readonly string entityTypeName;

	public EntityRefTypeConverter(Type entityRefType)
	{
		var entityType = entityRefType.GetGenericArguments()[0];
		constructor = (Func<long, object>)GetType().GetMethod(nameof(CreateEntityRef), BindingFlags.NonPublic | BindingFlags.Static)!.MakeGenericMethod(entityType).CreateDelegate(typeof(Func<long, object>));
		entityTypeName = entityType.Name;
	}

	public override bool CanConvertFrom(ITypeDescriptorContext? context, Type sourceType)
		=> sourceType == typeof(string)
		|| sourceType == typeof(long)
		|| sourceType == typeof(int)
		|| sourceType == typeof(short)
		|| sourceType == typeof(byte)
		|| base.CanConvertFrom(context, sourceType);

	public override object? ConvertFrom(ITypeDescriptorContext? context, CultureInfo? culture, object value)
	{
		if (value is string str)
		{
			var at = str.IndexOf('#');
			if (at >= 0 && str.Substring(0, at) == entityTypeName) str = str.Substring(at + 1);
			if (Int64.TryParse(str, out long id)) return constructor(id);
		}
		else if (value is long longId) return constructor(longId);
		else if (value is int intId) return constructor(intId);
		else if (value is short shortId) return constructor(shortId);
		else if (value is byte byteId) return constructor(byteId);
		return base.ConvertFrom(context, culture, value);
	}

	public override bool CanConvertTo(ITypeDescriptorContext? context, [NotNullWhen(true)] Type? destinationType)
		=> destinationType == typeof(string)
		|| destinationType == typeof(long)
		|| destinationType == typeof(int)
		|| destinationType == typeof(short)
		|| destinationType == typeof(byte)
		|| base.CanConvertTo(context, destinationType);

	public override object? ConvertTo(ITypeDescriptorContext? context, CultureInfo? culture, object? value, Type destinationType)
	{
		if (value is IEntityRef entityRef)
		{
			if (destinationType == typeof(string)) return entityRef.ToString();
			else if (destinationType == typeof(long)) return entityRef.Id;
			else if (destinationType == typeof(int)) return (int)entityRef.Id;
			else if (destinationType == typeof(short)) return (short)entityRef.Id;
			else if (destinationType == typeof(byte)) return (byte)entityRef.Id;
		}
		return base.ConvertTo(context, culture, value, destinationType);
	}

	private static object CreateEntityRef<TEntity>(long id) where TEntity : Entity<TEntity>
	{
		return new EntityRef<TEntity>(id);
	}
}

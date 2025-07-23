namespace LoraStatsNet.Database;

public class Entity
{
}

public class Entity<T> : Entity, IConvertible
	where T : Entity<T>
{
	public EntityRef<T> Ref { get; set; }

	public override string ToString() => Ref.ToString();

	public override bool Equals(object? otherObj) => otherObj is Entity<T> other && other == this;
	public override int GetHashCode() => Ref.GetHashCode();
	public static bool operator ==(Entity<T>? entity1, Entity<T>? entity2) => entity1 is null ? entity2 is null : entity2 is not null && entity1.Ref == entity2.Ref;
	public static bool operator !=(Entity<T>? entity1, Entity<T>? entity2) => !(entity1 == entity2);

	TypeCode IConvertible.GetTypeCode() => TypeCode.Object;
	bool IConvertible.ToBoolean(IFormatProvider? provider) => ((IConvertible)Ref).ToBoolean(provider);
	byte IConvertible.ToByte(IFormatProvider? provider) => ((IConvertible)Ref).ToByte(provider);
	char IConvertible.ToChar(IFormatProvider? provider) => ((IConvertible)Ref).ToChar(provider);
	DateTime IConvertible.ToDateTime(IFormatProvider? provider) => ((IConvertible)Ref).ToDateTime(provider);
	decimal IConvertible.ToDecimal(IFormatProvider? provider) => ((IConvertible)Ref).ToDecimal(provider);
	double IConvertible.ToDouble(IFormatProvider? provider) => ((IConvertible)Ref).ToDouble(provider);
	short IConvertible.ToInt16(IFormatProvider? provider) => ((IConvertible)Ref).ToInt16(provider);
	int IConvertible.ToInt32(IFormatProvider? provider) => ((IConvertible)Ref).ToInt32(provider);
	long IConvertible.ToInt64(IFormatProvider? provider) => ((IConvertible)Ref).ToInt64(provider);
	sbyte IConvertible.ToSByte(IFormatProvider? provider) => ((IConvertible)Ref).ToSByte(provider);
	float IConvertible.ToSingle(IFormatProvider? provider) => ((IConvertible)Ref).ToSingle(provider);
	string IConvertible.ToString(IFormatProvider? provider) => ToString();
	object IConvertible.ToType(Type conversionType, IFormatProvider? provider) => ((IConvertible)Ref).ToType(conversionType, provider);
	ushort IConvertible.ToUInt16(IFormatProvider? provider) => ((IConvertible)Ref).ToUInt16(provider);
	uint IConvertible.ToUInt32(IFormatProvider? provider) => ((IConvertible)Ref).ToUInt32(provider);
	ulong IConvertible.ToUInt64(IFormatProvider? provider) => ((IConvertible)Ref).ToUInt64(provider);
}

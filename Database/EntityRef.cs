using System.ComponentModel;

namespace LoraStatsNet.Database;

interface IEntityRef
{
	long Id { get; }
}

[TypeConverter(typeof(EntityRefTypeConverter))]
public readonly struct EntityRef<T> : IEntityRef, IConvertible where T : Entity<T>
{
	public readonly long Id { get; }
	public readonly long? IdOrNull => Id == 0 ? null : Id;

	public Type Type => typeof(T);

	public EntityRef(long id) => Id = id;
	public EntityRef(long? id) => Id = id.GetValueOrDefault();
	public override string ToString() => Type.Name + "#" + Id;
	public override bool Equals(object? otherObj) => otherObj is EntityRef<T> other && other.Id == Id && other.Type == Type;
	public override int GetHashCode() => Id.GetHashCode();
	public static bool operator ==(EntityRef<T> ref1, EntityRef<T> ref2) => ref1.Id == ref2.Id;
	public static bool operator !=(EntityRef<T> ref1, EntityRef<T> ref2) => ref1.Id != ref2.Id;
	public static explicit operator long(EntityRef<T> r) => r.Id;
	public static explicit operator long?(EntityRef<T> r) => r.Id == 0 ? null : r.Id;
	public static explicit operator EntityRef<T>(long id) => new EntityRef<T>(id);
	public static explicit operator EntityRef<T>(long? id) => new EntityRef<T>(id);
	public static implicit operator EntityRef<T>(Entity<T> entity) => entity == null ? default : entity.Ref;
	public bool IsNull => Id == 0;
	public bool IsNotNull => !IsNull;
	TypeCode IConvertible.GetTypeCode() => TypeCode.Object;
	bool IConvertible.ToBoolean(IFormatProvider? provider) => throw new InvalidCastException($"Reference {this} cannot be converted to bool.");
	byte IConvertible.ToByte(IFormatProvider? provider) => throw new InvalidCastException($"Reference {this} cannot be converted to byte.");
	char IConvertible.ToChar(IFormatProvider? provider) => throw new InvalidCastException($"Reference {this} cannot be converted to char.");
	DateTime IConvertible.ToDateTime(IFormatProvider? provider) => throw new InvalidCastException($"Reference {this} cannot be converted to DateTime.");
	decimal IConvertible.ToDecimal(IFormatProvider? provider) => Id;
	double IConvertible.ToDouble(IFormatProvider? provider) => Id;
	short IConvertible.ToInt16(IFormatProvider? provider) => checked((short)Id);
	int IConvertible.ToInt32(IFormatProvider? provider) => checked((int)Id);
	long IConvertible.ToInt64(IFormatProvider? provider) => Id;
	sbyte IConvertible.ToSByte(IFormatProvider? provider) => throw new InvalidCastException($"Reference {this} cannot be converted to sbyte.");
	float IConvertible.ToSingle(IFormatProvider? provider) => Id;
	string IConvertible.ToString(IFormatProvider? provider) => ToString();
	object IConvertible.ToType(Type conversionType, IFormatProvider? provider) => conversionType == GetType() ? this : throw new InvalidCastException($"Reference {this} cannot be converted to {conversionType}.");
	ushort IConvertible.ToUInt16(IFormatProvider? provider) => checked((ushort)Id);
	uint IConvertible.ToUInt32(IFormatProvider? provider) => checked((uint)Id);
	ulong IConvertible.ToUInt64(IFormatProvider? provider) => checked((ulong)Id);
}
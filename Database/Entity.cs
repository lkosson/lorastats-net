namespace LoraStatsNet.Database;

class Entity
{
	public long Id { get; set; }
}

class Entity<T> : Entity, IConvertible
	where T : Entity<T>
{
	public EntityRef<T> Ref => new EntityRef<T>(Id);

	public override string ToString() => Ref.ToString();

	public override bool Equals(object? otherObj) => otherObj is Entity<T> other && other == this;
	public override int GetHashCode() => Id.GetHashCode();
	public static bool operator ==(Entity<T> entity1, Entity<T> entity2) => entity1 is null ? entity2 is null : entity2 is not null && entity1.Id == entity2.Id;
	public static bool operator !=(Entity<T> entity1, Entity<T> entity2) => !(entity1 == entity2);

	public TypeCode GetTypeCode() => TypeCode.Object;
	public bool ToBoolean(IFormatProvider? provider) => throw new NotSupportedException();
	public byte ToByte(IFormatProvider? provider) => throw new NotSupportedException();
	public char ToChar(IFormatProvider? provider) => throw new NotSupportedException();
	public DateTime ToDateTime(IFormatProvider? provider) => throw new NotSupportedException();
	public decimal ToDecimal(IFormatProvider? provider) => throw new NotSupportedException();
	public double ToDouble(IFormatProvider? provider) => throw new NotSupportedException();
	public short ToInt16(IFormatProvider? provider) => throw new NotSupportedException();
	public int ToInt32(IFormatProvider? provider) => checked((int)Id);
	public long ToInt64(IFormatProvider? provider) => Id;
	public sbyte ToSByte(IFormatProvider? provider) => throw new NotSupportedException();
	public float ToSingle(IFormatProvider? provider) => throw new NotSupportedException();
	public string ToString(IFormatProvider? provider) => ToString();
	public object ToType(Type conversionType, IFormatProvider? provider) => conversionType == typeof(EntityRef<T>) ? Ref : throw new NotSupportedException();
	public ushort ToUInt16(IFormatProvider? provider) => throw new NotSupportedException();
	public uint ToUInt32(IFormatProvider? provider) => throw new NotSupportedException();
	public ulong ToUInt64(IFormatProvider? provider) => throw new NotSupportedException();
}

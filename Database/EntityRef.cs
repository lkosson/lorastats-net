namespace LoraStatsNet.Database;

readonly struct EntityRef<T> where T : Entity<T>
{
	public readonly long Id { get; }

	public Type Typ => typeof(T);

	public EntityRef(long id) => Id = id;
	public override string ToString() => Typ.Name + "#" + Id;
	public override bool Equals(object? otherObj) => otherObj is EntityRef<T> other && other.Id == Id && other.Typ == Typ;
	public override int GetHashCode() => Id.GetHashCode();
	public static bool operator ==(EntityRef<T> ref1, EntityRef<T> ref2) => ref1.Id == ref2.Id;
	public static bool operator !=(EntityRef<T> ref1, EntityRef<T> ref2) => ref1.Id != ref2.Id;
	public static implicit operator long(EntityRef<T> r) => r.Id;
	public static implicit operator long?(EntityRef<T> r) => r.Id == 0 ? null : r.Id;
	public static implicit operator EntityRef<T>(long id) => new EntityRef<T>(id);
	public static implicit operator EntityRef<T>(long? id) => id.GetValueOrDefault();
	public static implicit operator EntityRef<T>(Entity<T> entity) => entity?.Id;
	public bool IsNull => Id == 0;
	public bool IsNotNull => !IsNull;
}

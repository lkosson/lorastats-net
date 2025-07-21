using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace LoraStatsNet.Database;

class EntityRefValueConverter<TEntity> : ValueConverter<EntityRef<TEntity>, long> where TEntity : Entity<TEntity>
{
	public EntityRefValueConverter()
		: base(v => v.Id, v => v)
	{
	}
}

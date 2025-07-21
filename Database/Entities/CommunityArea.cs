using LoraStatsNet.Services;

namespace LoraStatsNet.Database.Entities;

class CommunityArea : Entity<CommunityArea>
{
	public long CommunityId { get; set; }
	public double LatitudeMin { get; set; }
	public double LatitudeMax { get; set; }
	public double LongitudeMin { get; set; }
	public double LongitudeMax { get; set; }

	public EntityRef<Community> CommunityRef { get => CommunityId; set => CommunityId = value; }

	public Community Community { get; set; } = default!;

	public Area Area => new(new(LatitudeMin, LongitudeMin), new(LatitudeMax, LongitudeMax));
}

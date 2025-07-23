using LoraStatsNet.Services;

namespace LoraStatsNet.Database.Entities;

public class CommunityArea : Entity<CommunityArea>
{
	public EntityRef<Community> CommunityId { get; set; }
	public double LatitudeMin { get; set; }
	public double LatitudeMax { get; set; }
	public double LongitudeMin { get; set; }
	public double LongitudeMax { get; set; }

	public Community Community { get; set; } = default!;

	public Area Area => new(new(LatitudeMin, LongitudeMin), new(LatitudeMax, LongitudeMax));
}

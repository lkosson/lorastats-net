namespace LoraStatsNet.Database.Entities;

class Community : Entity<Community>
{
	public required string Name { get; set; }
	public required string UrlName { get; set; }

	public List<CommunityArea> Areas { get; set; } = default!;
	public List<Node> Nodes { get; set; } = default!;

	public override string ToString() => Name;
}

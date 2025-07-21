#pragma warning disable EF1001 // Internal EF Core API usage.
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Sqlite.Metadata.Internal;

namespace LoraStatsNet.Database;

// https://github.com/dotnet/efcore/issues/29519#issuecomment-1975899051
// Workaround for perpetual Sqlite:Autoincrement annotation migrations
public class FixedSqliteAnnotationProvider : SqliteAnnotationProvider
{
	public FixedSqliteAnnotationProvider(RelationalAnnotationProviderDependencies dependencies) : base(dependencies)
	{
	}

	public override IEnumerable<IAnnotation> For(IColumn column, bool designTime)
	{
		if (!designTime) yield break;
		if (column is JsonColumn) yield break;

		var property = column.PropertyMappings.First().Property;
		var primaryKey = property.DeclaringType.ContainingEntityType.FindPrimaryKey();
		if (primaryKey is { Properties.Count: 1 }
			&& primaryKey.Properties[0] == property
			&& property.ValueGenerated == ValueGenerated.OnAdd)
		{
			yield return new Annotation(SqliteAnnotationNames.Autoincrement, true);
		}

		var srid = property.GetSrid();
		if (srid != null) yield return new Annotation(SqliteAnnotationNames.Srid, srid);
	}
}
#pragma warning restore EF1001 // Internal EF Core API usage.

﻿using System.Data.Common;
using LoraStatsNet.Database.Entities;
using LoraStatsNet.Database.Model;
using LoraStatsNet.Services;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Metadata;

namespace LoraStatsNet.Database;

class LoraStatsNetDb : DbContext
{
	private readonly ILogger<LoraStatsNetDb> logger;
	private readonly Configuration configuration;
	private Transaction? currentTransaction;

	public DbSet<CommunityArea> CommunityAreas => Set<CommunityArea>();
	public DbSet<Community> Communities => Set<Community>();
	public DbSet<Node> Nodes => Set<Node>();
	public DbSet<Packet> Packets => Set<Packet>();
	public DbSet<PacketData> PacketDatas => Set<PacketData>();
	public DbSet<PacketReport> PacketReports => Set<PacketReport>();

	public LoraStatsNetDb(ILogger<LoraStatsNetDb> logger, Configuration configuration)
	{
		this.logger = logger;
		this.configuration = configuration;
		ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTrackingWithIdentityResolution;
	}

	protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
	{
		base.OnConfiguring(optionsBuilder);
		optionsBuilder.UseSqlite($"Data Source={configuration.DbPath}");
		optionsBuilder.ConfigureWarnings(warnings => warnings.Ignore(CoreEventId.SensitiveDataLoggingEnabledWarning));
		optionsBuilder.LogTo((evt, level) => evt == RelationalEventId.CommandExecuting || evt == RelationalEventId.CommandExecuted || level >= LogLevel.Warning, LogCommand);
		optionsBuilder.EnableSensitiveDataLogging();
		optionsBuilder.ReplaceService<IRelationalAnnotationProvider, FixedSqliteAnnotationProvider>();
	}

	private void LogCommand(EventData eventData)
	{
		if (eventData.EventId == RelationalEventId.CommandExecuting && eventData is CommandEventData ced)
		{
			logger.LogInformation(ced.CommandId.GetHashCode(), "Executing command\n{command}", ced.Command.CommandText);
			if (ced.Command.Parameters.Count > 0) logger.LogTrace(ced.CommandId.GetHashCode(), "Command parameters:\n{parameters}", String.Join("\n", ced.Command.Parameters.Cast<DbParameter>().Select(p => p.ParameterName + "='" + p.Value + "'")));
		}
		else if (eventData.EventId == RelationalEventId.CommandExecuted && eventData is CommandExecutedEventData ceed) logger.LogDebug(ceed.CommandId.GetHashCode(), "Command executed in {time} ms", ceed.Duration.TotalMilliseconds);
		else if (eventData.EventId == RelationalEventId.CommandError && eventData is CommandErrorEventData cerd && cerd.Exception != null) logger.LogError(cerd.CommandId.GetHashCode(), "Command execution error: {exc}", cerd.Exception);
		else if (eventData.LogLevel == LogLevel.Warning) logger.LogWarning("{warning}", eventData.ToString());
		else if (eventData.LogLevel == LogLevel.Error) logger.LogError("{error}", eventData.ToString());
	}

	protected override void OnModelCreating(ModelBuilder modelBuilder)
	{
		LoraStatsNetModelBuilder.Configure(modelBuilder);
	}

	public async Task InitializeAsync()
	{
		await Database.MigrateAsync();
	}

	public async Task<Transaction> BeginTransactionAsync()
	{
		if (currentTransaction == null || currentTransaction.IsDisposed) currentTransaction = new Transaction(await Database.BeginTransactionAsync());
		currentTransaction.Inc();
		return currentTransaction;
	}

	public async Task StoreAsync<TEntity>(TEntity entity)
		where TEntity : Entity<TEntity>
		=> await StoreAsync([entity]);

	public async Task StoreAsync<TEntity>(IEnumerable<TEntity> entities)
		where TEntity : Entity<TEntity>
	{
		var set = Set<TEntity>();
		foreach (var entity in entities)
		{
			if (entity.Ref.Id <= 0)
			{
				entity.Ref = default;
				set.Add(entity);
			}
			else
			{
				Entry(entity).State = EntityState.Modified;
			}
		}
		await SaveChangesAndUntrackAsync();
	}

	public async Task DeleteAsync<TEntity>(TEntity entity)
		where TEntity : Entity<TEntity>
		=> await DeleteAsync([entity]);

	public async Task DeleteAsync<TEntity>(IEnumerable<TEntity> entities)
		where TEntity : Entity<TEntity>
	{
		Set<TEntity>().RemoveRange(entities);
		await SaveChangesAndUntrackAsync();
	}

	private async Task SaveChangesAndUntrackAsync()
	{
		try
		{
			await SaveChangesAsync();
		}
		finally
		{
			ChangeTracker.Entries().Where(e => e.Entity != null).ToList().ForEach(e => e.State = EntityState.Detached);
		}
	}

	public async Task<TEntity?> GetAsync<TEntity>(EntityRef<TEntity> entityRef)
		where TEntity : Entity<TEntity>
		=> entityRef.IsNull ? null : await Set<TEntity>().FirstOrDefaultAsync(r => r.Ref == entityRef);

	public async Task<IEnumerable<Dictionary<string, object>>> QueryAsync(FormattableString query)
	{
		using var connection = Database.GetDbConnection();
		connection.Open();
		using var command = connection.CreateCommand();
		var parameterNames = new List<string>();
		for (int i = 0; i < query.ArgumentCount; i++)
		{
			var parameterName = "@P" + i;
			parameterNames.Add(parameterName);
			var parameterValue = query.GetArgument(i);
			if (parameterValue == null) parameterValue = DBNull.Value;
			var parameter = command.CreateParameter();
			parameter.ParameterName = parameterName;
			parameter.Value = parameterValue;
			command.Parameters.Add(parameter);
		}
		var sql = String.Format(query.Format, parameterNames);
		command.CommandText = sql;
		using var reader = await command.ExecuteReaderAsync();
		var result = new List<Dictionary<string, object>>();
		while (reader.Read())
		{
			var row = new Dictionary<string, object>();
			for (int i = 0; i < reader.FieldCount; i++)
			{
				row[reader.GetName(i)] = reader.GetValue(i);
			}
			result.Add(row);
		}
		return result;
	}

	public static void FlushPools()
	{
		SqliteConnection.ClearAllPools();
	}
}

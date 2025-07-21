using Microsoft.EntityFrameworkCore.Storage;

namespace LoraStatsNet.Database;

class Transaction(IDbContextTransaction transaction)
{
	public bool IsDisposed => nestingCounter == 0;

	private int nestingCounter;
	private int committedLevel;
	private int rolledBackLevel;

	public async Task CommitAsync()
	{
		if (rolledBackLevel > 0) throw new InvalidOperationException("Nested transaction rolled back.");
		if (nestingCounter == 1) await transaction.CommitAsync();
		committedLevel = nestingCounter;
	}

	public async Task RollbackAsync()
	{
		if (committedLevel > 0) throw new InvalidOperationException("Nested transaction committed.");
		if (nestingCounter == 1) await transaction.RollbackAsync();
		rolledBackLevel = nestingCounter;
	}

	internal void Inc()
	{
		nestingCounter++;
	}

	public void Dispose()
	{
		if (committedLevel > 0 && committedLevel > nestingCounter) throw new InvalidOperationException("Nested transaction committed.");
		if (nestingCounter == 1) transaction.Dispose();
		nestingCounter--;
	}
}

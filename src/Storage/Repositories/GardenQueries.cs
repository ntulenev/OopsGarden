using Abstractions.Repositories;

namespace Storage.Repositories;

/// <summary>
/// Provides EF Core garden read queries.
/// </summary>
public sealed partial class GardenQueries : IGardenQueries
{
    /// <summary>
    /// Initializes a new instance of the <see cref="GardenQueries"/> class.
    /// </summary>
    public GardenQueries(GardenDbContext dbContext)
    {
        ArgumentNullException.ThrowIfNull(dbContext);
        _dbContext = dbContext;
    }

    private readonly GardenDbContext _dbContext;
}

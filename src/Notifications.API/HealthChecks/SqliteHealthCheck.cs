using Dapper;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Notifications.API.HealthChecks;

public class SqliteHealthCheck : IHealthCheck
{
    private readonly IConfiguration _configuration;

    public SqliteHealthCheck(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        try
        {
            using var connection = new SqliteConnection(_configuration.GetConnectionString("DefaultConnection"));
            await connection.OpenAsync(cancellationToken);
            await connection.ExecuteScalarAsync<int>("SELECT 1");
            return HealthCheckResult.Healthy("SQLite responde correctamente.");
        }
        catch (Exception exception)
        {
            return HealthCheckResult.Unhealthy("No se pudo conectar a SQLite.", exception);
        }
    }
}

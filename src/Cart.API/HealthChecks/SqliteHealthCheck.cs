using Dapper;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Cart.API.HealthChecks;

/// <summary>
/// Verifica que la base SQLite responda (usado en /health y /health/ready).
/// </summary>
public class SqliteHealthCheck : IHealthCheck
{
    private readonly IConfiguration _configuration;

    public SqliteHealthCheck(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        try
        {
            using var connection = new SqliteConnection(_configuration.GetConnectionString("DefaultConnection"));
            await connection.OpenAsync(cancellationToken);
            await connection.ExecuteScalarAsync<int>("SELECT 1");
            return HealthCheckResult.Healthy("SQLite responde correctamente.");
        }
        catch (Exception ex)
        {
            return HealthCheckResult.Unhealthy("No se pudo conectar a SQLite.", ex);
        }
    }
}

using Dapper;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace MiniApi.HealthChecks;

public class SqliteHealthCheck : IHealthCheck
{
    private readonly IConfiguration _config;

    public SqliteHealthCheck(IConfiguration config)
    {
        _config = config;
    }

    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var connectionString =
                _config.GetConnectionString(
                    "DefaultConnection");

            if (string.IsNullOrWhiteSpace(connectionString))
            {
                connectionString =
                    "Data Source=products.db";
            }

            using var connection =
                new SqliteConnection(connectionString);

            await connection.OpenAsync(cancellationToken);

            var result =
                await connection.ExecuteScalarAsync<int>(
                    "SELECT 1");

            if (result == 1)
            {
                return HealthCheckResult.Healthy(
                    "SELECT 1 ejecutado correctamente");
            }

            return HealthCheckResult.Unhealthy(
                "SQLite no respondió correctamente");
        }
        catch (Exception exception)
        {
            return HealthCheckResult.Unhealthy(
                description:
                    "No se pudo conectar a SQLite",
                exception: exception);
        }
    }
}
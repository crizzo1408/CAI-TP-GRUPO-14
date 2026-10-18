using MiniApi.Extensions.Endpoints;
using MiniApi.Models;

namespace MiniApi.Extensions;

public static class EndpointsExtensions
{
    public static void MapAppEndpoints(this WebApplication app)
    {
        app.MapOrderEndpoints();
    }
}


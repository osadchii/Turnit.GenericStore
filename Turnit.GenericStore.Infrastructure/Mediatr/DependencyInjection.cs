using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Turnit.GenericStore.Infrastructure.Database.Mappings;

namespace Turnit.GenericStore.Infrastructure.Mediatr;

public static class DependencyInjection
{
    public static void AddMediatr(this IServiceCollection services)
    {
        services.AddMediatR(typeof(ProductMap).Assembly);
    }
}
using Microsoft.Extensions.DependencyInjection;
using WordsNote.Application.Mappings;

namespace WordsNote.Application.Extensions;

public static class ApplicationServiceExtensions
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(ApplicationServiceExtensions).Assembly));
        services.AddAutoMapper(typeof(DeckProfile).Assembly);
        return services;
    }
}

using Microsoft.Extensions.DependencyInjection;

namespace Delobytes.NetCore.EntityReportGeneration;

public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Добавляет генератор отчётов сущностей. Генератор создаёт файл с содержимым всех свойств сущностей,
    /// кроме тех, которые имеют атрибут <see cref="EntityReportIgnoreAttribute"/>.
    /// </summary>
    /// <param name="services">Коллеция сервисов.</param>
    /// <param name="configure">Настройки.</param>
    /// <returns>Коллеция сервисов.</returns>
    public static IServiceCollection AddEntityReportGenerator(this IServiceCollection services, Action<EntityReportGeneratorOptions> configure = null)
    {
        ArgumentNullException.ThrowIfNull(services, nameof(services));

        if (configure != null)
        {
            services.Configure(configure);
        }
        else
        {
            services.AddTransient<EntityReportGeneratorOptions, EntityReportGeneratorOptions>();
        }

        services.AddTransient<IEntityReportGenerator, EntityReportGenerator>();

        return services;
    }
}

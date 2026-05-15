using System.Reflection;
using Microsoft.Extensions.DependencyInjection;

namespace AlgoSphere.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        services.AddMediatR(cfg => {
            cfg.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly());
        });

        // Đăng ký thêm AutoMapper, FluentValidation ở đây nếu cần

        return services;
    }
}

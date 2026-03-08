using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace SharedKernel.Validations.Mediator;

public static class MediatorValidatorsExtensions
{
    extension(IServiceCollection services)
    {
        public IServiceCollection AddMediatorValidators(Assembly assembly)
        {
            var validatorType = typeof(IValidator<>);

            var validators = assembly.GetTypes()
                .Where(t => t is { IsClass: true, IsAbstract: false, IsInterface: false })
                .SelectMany(t
                    => t.GetInterfaces().Where(i
                        => i.IsGenericType && i.GetGenericTypeDefinition() == validatorType
                    ).Select( x =>  new {Service = x, Impl = t})
                );

            foreach (var v in validators)
            {
                services.TryAdd(ServiceDescriptor.Singleton(v.Service, v.Impl));
            }
            
            return services;    
        }
    }
}
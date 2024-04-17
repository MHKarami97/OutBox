using Publisher;

namespace OutBoxPublisher;

public static class ServiceRegistration
{
    public static void Registration(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddSingleton<IProducer, KafkaProducer>();
    }
}
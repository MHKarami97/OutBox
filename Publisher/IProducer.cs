using Publisher.Config;

namespace Publisher;

public interface IProducer
{
    Task ProduceAsync(ProduceConfig config, string message);
}
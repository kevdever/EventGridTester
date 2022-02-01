using Azure;
using Azure.Core;
using Azure.Identity;
using Azure.Messaging.EventGrid;
using EventGridTester.ConfigurationModels;
using Microsoft.Extensions.Options;

namespace EventGridTester.Services
{
    public class EventPublisherService : IEventPublisherService
    {
        private readonly EventGridConfig _config;
        private Lazy<EventGridPublisherClient> _client;
        private ILogger _logger;

        public EventPublisherService(IOptions<EventGridConfig> config, ILogger<EventPublisherService> logger)
        {
            _client = new Lazy<EventGridPublisherClient>(InitializeClient);
            _logger = logger;
            _config = config.Value;
        }

        public async Task<bool> PublishEvent(EventGridEvent eventGridEvent)
        {
            try
            {
                var result = await _client.Value.SendEventAsync(eventGridEvent);
                return result.Status == 200;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Failed to send event. {ex}");
                throw;
            }
        }

        private EventGridPublisherClient InitializeClient()
        {
            EventGridPublisherClient client;
            if (_config.UseKey.HasValue && _config.UseKey.Value)
            {
                client = new EventGridPublisherClient(
                    _config.ToURI(),
                    new AzureKeyCredential(_config.Key));
            }
            else
            {
                client = new EventGridPublisherClient(
                    _config.ToURI(),
                    new DefaultAzureCredential());
            }

            return client;
        }
    }
}

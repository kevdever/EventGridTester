using Azure.Messaging.EventGrid;

namespace EventGridTester.Services
{
    public interface IEventPublisherService
    {
        Task<bool> PublishEvent(EventGridEvent eventGridEvent);
    }
}
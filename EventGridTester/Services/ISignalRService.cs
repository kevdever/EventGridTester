using Azure.Messaging.EventGrid;

namespace EventGridTester.Services
{
    public interface ISignalRService
    {
        public Task<bool> SendMessage(EventGridEvent evt);
    }
}

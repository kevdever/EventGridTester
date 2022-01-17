using Azure.Messaging.EventGrid;

namespace EventGridTester.Services
{
    public interface IEventHistory
    {
        void AddEvent(EventGridEvent evt);
        IEnumerable<EventGridEvent> GetEvents();

        void Flush();
    }
}
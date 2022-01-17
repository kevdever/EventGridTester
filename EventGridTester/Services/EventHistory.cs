using Azure.Messaging.EventGrid;

namespace EventGridTester.Services
{
    public class EventHistory : IEventHistory
    {
        private readonly ICollection<EventGridEvent> _events = new List<EventGridEvent>();

        public void AddEvent(EventGridEvent evt)
        {
            _events.Add(evt);
        }

        public IEnumerable<EventGridEvent> GetEvents() => _events;

        public void Flush() => _events.Clear();

    }
}

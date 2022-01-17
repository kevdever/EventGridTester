using Azure.Messaging.EventGrid;

namespace EventGridTester.Models
{
    public class HomeViewModel
    {
        public IEnumerable<EventGridEvent> ReceivedEvents { get; }

        public HomeViewModel(IEnumerable<EventGridEvent> events)
        {
            ReceivedEvents = events;
        }
    }
}

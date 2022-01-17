using EventGridTester.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Azure.Messaging.EventGrid;
using Azure.Messaging.EventGrid.SystemEvents;
using Microsoft.AspNetCore.Authentication.JwtBearer;

namespace EventGridTester.APIControllers
{
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    [Route("api/[controller]")]
    [ApiController]
    public class EventSubscriptionController : ControllerBase
    {
        private readonly ILogger _log;
        private readonly IEventHistory _eventHistory;
        private readonly IEventPublisherService _eventPublisherService;
        private readonly ISignalRService _signalRService;
        private const string VALIDATIONEVENTNAME = "Microsoft.EventGrid.SubscriptionValidationEvent";

        public EventSubscriptionController(ILogger<EventSubscriptionController> log, IEventHistory eventHistory, IEventPublisherService eventPublisherService, ISignalRService signalRService)
        {
            _log = log;
            _eventHistory = eventHistory;
            _eventPublisherService = eventPublisherService;
            _signalRService = signalRService;
        }

        [HttpGet]
        public IActionResult Get()
        {
            return Ok(_eventHistory.GetEvents());
        }

        //The signature of this resource is not ideal. As-is, it implicitly assumes a json request body matching an EventGridEvent model. 
        //You can't specify an EventGridEvent in the parameters because that won't always deserialize correctly, such as when a validation event is received.
        //A better approach may be to just make a custom event grid model 
        [AllowAnonymous]
        [HttpPost]
        public async Task<IActionResult> OnSubscriptionEvent()
        {
            EventGridEvent eventGridevent;
            try
            {
                BinaryData events = await BinaryData.FromStreamAsync(Request.Body);
                eventGridevent = EventGridEvent.Parse(events);
                _log.LogInformation($"[SubscriptionEventHandler] Subscription Event received.  {events}");
            }
            catch (Exception ex)
            {
                _log.LogError(ex, $"Failed to deserialize request. {ex}");
                throw;
            }

            _log.LogInformation($"[SubscriptionEventHandler] Subscription Event body successfully deserialized. EventType: {eventGridevent.EventType}");

            _eventHistory.AddEvent(eventGridevent);


            if (eventGridevent.EventType == VALIDATIONEVENTNAME
                && eventGridevent.TryGetSystemEventData(out object eventData)
                && eventData is SubscriptionValidationEventData subscriptionValidationEventData) //validate the subscription creation request
            {
                _log.LogInformation($"Got SubscriptionValidation event data. topic: {eventGridevent.Topic}");

                var responseData = new SubscriptionValidationResponse()
                {
                    ValidationResponse = subscriptionValidationEventData.ValidationCode
                };
                return new OkObjectResult(responseData);
            }

            await _signalRService.SendMessage(eventGridevent);

            return new OkResult();
        }

        [HttpPost("Publish")]
        public async Task<IActionResult> PublishEvent([FromBody] EventGridEvent evt)
        {
            evt.Topic = null;
            evt.EventTime = DateTime.UtcNow;
            if (await _eventPublisherService.PublishEvent(evt))
            {
                return Ok();
            }
            else return new StatusCodeResult(500);
        }

        [HttpPost("SignalR")]
        public async Task<IActionResult> SendMessage([FromBody] EventGridEvent evt)
        {
            if (await _signalRService.SendMessage(evt))
                return NoContent();
            else
                return new StatusCodeResult(500);
        }

        [HttpDelete]
        public IActionResult PurgeEvents()
        {
            _eventHistory.Flush();
            return NoContent();
        }
    }
}

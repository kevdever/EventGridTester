using Azure.Messaging.EventGrid;
using EventGridTester.Models;
using EventGridTester.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Diagnostics;

namespace EventGridTester.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IEventHistory _eventHistory;
        private readonly IEventPublisherService _eventPublisherService;

        public HomeController(ILogger<HomeController> logger, IEventHistory eventHistory, IEventPublisherService eventPublisher)
        {
            _logger = logger;
            _eventHistory = eventHistory;
            _eventPublisherService = eventPublisher;
        }

        [HttpGet]
        public IActionResult Index()
        {
            var viewModel = new HomeViewModel(_eventHistory.GetEvents());
            ViewBag.EventHistory = viewModel;
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> SendEvent(PublishEventViewModel viewmodel)
        {
            EventGridEvent eventGridEvent;
            try
            {
                var json = JsonConvert.DeserializeObject<JObject>(viewmodel.Json);
                var subject = json["subject"].Value<string>();
                var eventType = json["eventType"].Value<string>();
                var data = json["data"].ToObject<CustomEventModel>();

                eventGridEvent = new EventGridEvent(subject, eventType, "1.0", data);
            }
            catch (Exception ex)
            {
                var message = $"Failed to convert viewmodel into event object.  {ex}";
                _logger.LogError(ex, message);
                return new ContentResult()
                {
                    Content = message,
                    StatusCode = 500
                };
            }
 
            if (!await _eventPublisherService.PublishEvent(eventGridEvent))
            {
                _logger.LogError("Failed to publish event");
            }
            return RedirectToAction("Index");
        }

        [AllowAnonymous]
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
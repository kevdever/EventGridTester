using Azure.Messaging.EventGrid;
using EventGridTester.Models;
using Microsoft.AspNetCore.SignalR;
using System.Text.Json;

namespace EventGridTester.Services
{
    /// <summary>
    /// A strongly-typed SignalR hub that supports broadcasting but also specific-delivery based on a customer identifier registered in the connection (via query parameter) and provided in the eventgrid data's payload
    /// </summary>
    public class SignalRService : Hub, ISignalRService
    {
        private readonly ILogger _logger;
        private readonly IHubContext<SignalRService> _context;
        private readonly static Dictionary<string, HashSet<string>> _allowedSubscribers = new Dictionary<string, HashSet<string>>();

        public SignalRService(ILogger<SignalRService> logger, IHubContext<SignalRService> context)
        {
            _logger = logger;
            _context = context;
            
        }

        public override async Task OnConnectedAsync()
        {
            var callerContext = base.Context;
            var httpContext = callerContext.GetHttpContext();
            var customIdentifier = httpContext.Request.Query["customIdentifier"];
            if (!string.IsNullOrEmpty(customIdentifier))
            {
                HashSet<string> subs;
                if (!_allowedSubscribers.TryGetValue(customIdentifier, out subs))
                {
                    subs = new HashSet<string>();
                    _allowedSubscribers.Add(customIdentifier, subs);
                }
                subs.Add(Context.ConnectionId);
                
            }
            await base.OnConnectedAsync();
        }

        public async Task<bool> SendMessage(EventGridEvent evt)
        {
            try
            {
                var data = evt.Data.ToObjectFromJson<JsonDocument>();
                if ((data?.RootElement.TryGetProperty("customIdentifier", out var customIdentifer) ?? false) && _allowedSubscribers.TryGetValue(customIdentifer.GetString(), out var connections))
                {
                    var tasks = new List<Task>();
                    foreach (var connectionId in connections.ToList())
                    {
                        var client = _context.Clients.Client(connectionId);
                        if (client is null)
                        {
                            connections.Remove(connectionId);
                            continue;
                        }
                        tasks.Add(_context.Clients.Client(connectionId).SendAsync("ReceiveMessage", evt));
                        _logger.LogInformation($"[SignalR] successfully narrowcast event {evt.Id} for customIdentifier {customIdentifer.GetString()}");
                    }
                    await Task.WhenAll(tasks);
                }
                else
                {
                    //broadcast to everyone if a target audience is not provided
                    await _context.Clients.All.SendAsync("ReceiveMessage", evt);
                    _logger.LogInformation($"[SignalR] successfully broadcast event {evt.Id}");
                }
                
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"[SignalR] failed to deliver event {evt.Id}. {ex}");
                return false;
            }

        }
    }
}

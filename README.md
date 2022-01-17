# Event Grid Tester
## This .Net 6 project is designed to assist in building and troubleshooting applications that rely on Azure Event Grid. It provides a UI to publish and receive events, as well as an API to expose this same functionality.

### The Stack:
* Asp.Net Core 6 (MVC + Web API)
* Azure AD logins for UI and JWT for API (dual registration)
* Azure Event Grid
* SignalR (self-hosted)
* Azure App Service with the Configuration section matching the values in appsettings.json (except replace nesting with colons.  E.g., "AzureAd" { "Instance": "loremipsum"} becomes "AzureAD:Instance":"loremipsum" in Azure App Service's configuration)
* Event Grid Subscription configured to call your Azure App Service at the "/api/EventSubscription" resource

### Setup & Configuration:
* Ensure there's an App Registration setup in Azure AD and that there's a secret available
* Update appsettings.json's "<values>" entries with the corresponding Domain, TenantId, ClientId, and Audience for the Azure AD App Registration.
* Update appsettings.json's "EventGridTopicURI" with the URI for your Event Grid Topic's URI.

## Important Note About Local Debugging:
If you deploy this application to Azure and configure as specified above, events should flow through as expected.  

However, when debugging locally, Azure will not be able to send events to your local machine. To enable this, please refer to [this sample, especially with respect to ngrok](https://docs.microsoft.com/en-us/azure/azure-functions/functions-debug-event-grid-trigger-local)

## Features
* A web UI (with AAD login) that displays all events and enables you to send events.
* When an event is received, the UI is automatically updated via SignalR integration
* A REST API secured by Azure AD (JWT) enabling you to publish events to event grid, purge events, and retrieve events.  Please refer to the included Postman collection for sample calls (be sure to incuded Bearer tokens when making a call).
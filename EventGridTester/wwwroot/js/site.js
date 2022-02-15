
"use strict";

var customerIdentifier = "3.14159265358979"

var connection = new signalR.HubConnectionBuilder().withUrl(`/chatHub?customIdentifier=${customerIdentifier}`).build();

connection.on("ReceiveMessage", function (eventGridEvent) {
    console.log(eventGridEvent);
    var div = document.createElement("div");
    var json = document.createElement("pre");
    json.innerHTML = JSON.stringify(eventGridEvent, null, 2);
    div.appendChild(json);
    div.appendChild(document.createElement("hr"));

    var parent = document.getElementById("eventHistory");
    parent.insertBefore(div, parent.firstChild);
});

connection.start().then(function () {
    console.log("started signalR listener");
}).catch(function (err) {
    return console.error(err.toString());
});

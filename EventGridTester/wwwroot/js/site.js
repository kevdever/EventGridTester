// Please see documentation at https://docs.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.

"use strict";

var customerIdentifier = "3.14159265358979"

var connection = new signalR.HubConnectionBuilder().withUrl(`/chatHub?customIdentifier=${customerIdentifier}`).build();


connection.on("ReceiveMessage", function (eventGridEvent) {
    console.log(eventGridEvent);
    var row = document.createElement("tr");
    const ts = document.createElement("td");
    ts.innerText = eventGridEvent.eventTime;

    const type = document.createElement("td");
    type.innerText = eventGridEvent.eventType;
    const subject = document.createElement("td");
    subject.innerText = eventGridEvent.subject;

    let payload = `MyInteger:${eventGridEvent.data.myInteger}; SomeFlag: ${eventGridEvent.data.someFlag}; Messages: ${eventGridEvent.data.messages.join('; ')}`;
    const payloadCol = document.createElement("td");
    payloadCol.innerText = payload;

    row.appendChild(ts);
    row.appendChild(type);
    row.appendChild(subject);
    row.appendChild(payloadCol);
    document.getElementById("eventHistory").appendChild(row);

});

connection.start().then(function () {
    console.log("started signalR listener")
}).catch(function (err) {
    return console.error(err.toString());
});

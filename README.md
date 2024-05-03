# dotnet-backend
Simple websocket backend built with standard dotnet libraries.
Provides a simple command-line client for testing that reads messages as plaintext json, line by line until an empty line is read.

For example, to log in:
`{"Type": "Login", "Payload": {"DeviceId": "123"}}

Or to add coins:
`{"Type": "UpdateResources", "Payload": {"ResourceType": "Coins", "ResourceValue": 500}}

Routing logic is separate from the actual implementation of handlers for different functions.
Adding new functionality is simple: just add any new message definitions in Messages.cs, implement logic inside a new handler and register the handler in Server.cs
To test out the new functionality, also add a new case in Client.cs

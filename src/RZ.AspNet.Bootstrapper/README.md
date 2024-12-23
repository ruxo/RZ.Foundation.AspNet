# RZ Asp.Net Framework

## Initialization & Shutdown

```csharp
builder.services
       .AspHostEvents()
       .OnStart(() => Console.WriteLine("Starting"))
       .OnShutdown(() => Console.WriteLine("Shutting down"));
```

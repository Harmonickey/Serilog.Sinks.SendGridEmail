# Serilog SendGrid Email Sink

## Create an Instance of the SendGridClient from the SendGrid NuGet Package

```csharp

var client = new SendGridClient("Your Send Grid API Key");

```

## Create an EmailConnectionInfo from the SendGridEmail NuGet Package
```csharp
var emailConnectionInfo = new EmailConnectionInfo
{
	EmailSubject = "Application Error",
	FromEmail = "Your From Email",
	ToEmail = "Your To Email",
	SendGridClient = client,
	FromName = "Your Friendly From Name"
};
```

## Create your own customized Serilog logger, and then include the .WriteTo.Email extension, just like with the Serilog.Sinks.Email NuGet package.
```csharp
Log.Logger = new LoggerConfiguration()
	.MinimumLevel.Is(LogEventLevel.Debug)
	.Enrich.WithProcessId()
	.Enrich.WithThreadId()
	.WriteTo.EventLog("Ayni.Web", "AyniEnterprise", Environment.MachineName, true)
	.WriteTo.Email(emailConnectionInfo, restrictedToMinimumLevel: LogEventLevel.Error)
	.CreateLogger();

```
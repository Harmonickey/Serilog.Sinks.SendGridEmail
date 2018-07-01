# Serilog SendGrid Email Sink

## Download the SendGrid NuGet Package and the SendGridEmail NuGet Package
```csharp
Install-Package Serilog.Sinks.SendGridEmail -Version 1.0.0 
```

## Create an Instance of the SendGridClient (SendGrid namespace)
```csharp
var client = new SendGridClient("Your Send Grid API Key");
```

## Create an EmailConnectionInfo (Serilog.Sinks.Email namespace)
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
	.WriteTo.Email(emailConnectionInfo, restrictedToMinimumLevel: LogEventLevel.Error)
	.CreateLogger();
```
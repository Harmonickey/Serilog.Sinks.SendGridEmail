using System;
using Serilog.Configuration;
using Serilog.Events;
using Serilog.Formatting.Display;
using Serilog.Sinks.Email;
using Serilog.Sinks.PeriodicBatching;

namespace Serilog
{
    public static class LoggerConfigurationEmailExtensions
    {
	    const string DefaultOutputTemplate = "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level}] {Message}{NewLine}{Exception}";
        const int DefaultBatchPostingLimit = 100;
        static readonly TimeSpan DefaultPeriod = TimeSpan.FromSeconds(30);

		/// <summary>
		/// Adds a sink that sends log events via email.
		/// </summary>
		/// <param name="loggerConfiguration">The logger configuration.</param>
		/// <param name="connectionInfo">The connection info used for </param>
		/// <param name="outputTemplate">A message template describing the format used to write to the sink.
		/// the default is "{Timestamp} [{Level}] {Message}{NewLine}{Exception}".</param>
		/// <param name="restrictedToMinimumLevel">The minimum log event level required in order to write an event to the sink.</param>
		/// <param name="batchPostingLimit">The maximum number of events to post in a single batch.</param>
		/// <param name="period">The time to wait between checking for event batches.</param>
		/// <param name="formatProvider">Supplies culture-specific formatting information, or null.</param>
		/// <param name="mailSubject">The subject, can be a plain string or a template such as {Timestamp} [{Level}] occurred.</param>
		/// <returns>Logger configuration, allowing configuration to continue.</returns>
		/// <exception cref="ArgumentNullException">A required parameter is null.</exception>
		public static LoggerConfiguration Email(
		    this LoggerSinkConfiguration loggerConfiguration,
		    EmailConnectionInfo connectionInfo,
		    string outputTemplate = DefaultOutputTemplate,
		    LogEventLevel restrictedToMinimumLevel = LevelAlias.Minimum,
			int batchPostingLimit = DefaultBatchPostingLimit,
			TimeSpan? period = null,
		    IFormatProvider formatProvider = null,
		    string mailSubject = EmailConnectionInfo.DefaultSubject)
	    {
		    if (connectionInfo == null) throw new ArgumentNullException("connectionInfo");

		    if (!string.IsNullOrEmpty(connectionInfo.EmailSubject))
		    {
			    mailSubject = connectionInfo.EmailSubject;
		    }

            var batchingPeriod = period ?? DefaultPeriod;
            var formatter = new MessageTemplateTextFormatter(outputTemplate, formatProvider);
		    var subjectLineFormatter = new MessageTemplateTextFormatter(mailSubject, formatProvider);

            var batchingOptions = new PeriodicBatchingSinkOptions
            {
                BatchSizeLimit = batchPostingLimit,
                Period = batchingPeriod,
                EagerlyEmitFirstEvent = false,  // set default to false, not usable for emailing
                QueueLimit = 10000
            };
            var batchingSink = new PeriodicBatchingSink(new SendGridEmailSink(connectionInfo, formatter, subjectLineFormatter), batchingOptions);

			return loggerConfiguration.Sink(batchingSink, restrictedToMinimumLevel);
	    }
	}
}

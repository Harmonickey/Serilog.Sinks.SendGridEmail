using Serilog.Sinks.PeriodicBatching;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using SendGrid;
using SendGrid.Helpers.Mail;
using Serilog.Events;
using Serilog.Formatting;

namespace Serilog.Sinks.Email
{
	public class SendGridEmailSink : PeriodicBatchingSink
	{
		readonly EmailConnectionInfo _connectionInfo;

		readonly SendGridClient _client;

		readonly ITextFormatter _textFormatter;

		readonly ITextFormatter _subjectLineFormatter;

		/// <summary>
		/// A reasonable default for the number of events posted in
		/// each batch.
		/// </summary>
		public const int DefaultBatchPostingLimit = 100;

		/// <summary>
		/// A reasonable default time to wait between checking for event batches.
		/// </summary>
		public static readonly TimeSpan DefaultPeriod = TimeSpan.FromSeconds(30);

		/// <summary>
		/// 
		/// </summary>
		/// <param name="batchSizeLimit"></param>
		/// <param name="period"></param>
		public SendGridEmailSink(EmailConnectionInfo connectionInfo, int batchSizeLimit, TimeSpan period, ITextFormatter textFormatter, ITextFormatter subjectLineFormatter) 
			: base(batchSizeLimit, period)
		{
			_connectionInfo = connectionInfo ?? throw new ArgumentNullException(nameof(connectionInfo));

			_textFormatter = textFormatter;
			_subjectLineFormatter = subjectLineFormatter;
			_client = connectionInfo.SendGridClient;
		}

		/// <summary>
		/// Emit a batch of log events, running asynchronously.
		/// </summary>
		/// <param name="events">The events to emit.</param>
		/// <remarks>Override either <see cref="PeriodicBatchingSink.EmitBatch"/> or <see cref="PeriodicBatchingSink.EmitBatchAsync"/>,
		/// not both.</remarks>
		protected override async Task EmitBatchAsync(IEnumerable<LogEvent> events)
		{
			if (events == null)
				throw new ArgumentNullException(nameof(events));

			var payload = new StringWriter();

			var eventsSet = events.ToList();
			foreach (var logEvent in eventsSet)
			{
				_textFormatter.Format(logEvent, payload);
			}

			var subject = new StringWriter();
			_subjectLineFormatter.Format(eventsSet.OrderByDescending(e => e.Level).First(), subject);			

			var from = new EmailAddress(_connectionInfo.FromEmail, _connectionInfo.FromName);
			var to = new EmailAddress(_connectionInfo.ToEmail);
			var msg = MailHelper.CreateSingleEmail(from, to, _connectionInfo.EmailSubject, payload.ToString(), payload.ToString());

			await _client.SendEmailAsync(msg);
		}

	}
}

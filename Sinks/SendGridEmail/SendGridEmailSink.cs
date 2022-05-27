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
	class SendGridEmailSink : IBatchedLogEventSink, IDisposable
	{
        readonly EmailConnectionInfo _connectionInfo;

		readonly SendGridClient _client;

		readonly ITextFormatter _textFormatter;

		readonly ITextFormatter _subjectLineFormatter;

        /// <summary>
        /// Construct a sink emailing with the specified details.
		/// </summary>
		/// <param name="connectionInfo">Connection information used to construct the SMTP client and mail messages.</param>
		/// <param name="textFormatter">Supplies culture-specific formatting information, or null.</param>
		/// <param name="subjectLineFormatter">Supplies culture-specific formatting information, or null.</param>
		public SendGridEmailSink(EmailConnectionInfo connectionInfo, ITextFormatter textFormatter, ITextFormatter subjectLineFormatter)
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
		public async Task EmitBatchAsync(IEnumerable<LogEvent> events)
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

        public Task OnEmptyBatchAsync()
        {
			return Task.FromResult(false);
        }

        public void Dispose()
        {
            // nothing to do
        }
	}
}

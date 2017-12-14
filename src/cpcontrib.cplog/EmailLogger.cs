//! dependency=LMCP.CPLog^0.1.0
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CrownPeak.CMSAPI;
using CrownPeak.CMSAPI.Services;
/* Some Namespaces are not allowed. */
namespace CrownPeak.CMSAPI.CustomLibrary
{
	/// <summary>
	/// Logger that sends output to specified recipients
	/// </summary>
	/// <remarks>
	/// Not thread safe.</remarks>
	public class EmailLogger : IDisposable, CPLog.ILogger
	{
		private EmailLogger()
		{
			this.IsDebugEnabled = false;
			this.IsInfoEnabled = true;
			this.IsErrorEnabled = true;
			this.IsWarnEnabled = true;
		}
		public EmailLogger(string subject, string recipients)
			: this()
		{
			this.Subject = subject;
			this.Recipients = recipients;
		}

		StringBuilder sb = new StringBuilder();

		public bool IsDebugEnabled { get; set; }
		public bool IsInfoEnabled { get; set; }
		public bool IsWarnEnabled { get; set; }
		public bool IsErrorEnabled { get; set; }
		public bool DebugWriteLine { get; set; }

		public void Info(string message)
		{
			if(IsInfoEnabled)
			{
				string m = "INFO " + message;
				if(DebugWriteLine) Out.DebugWriteLine(m);
				sb.Append(m).AppendLine();
			}
		}
		public void Info(string format, params object[] args)
		{
			if(IsInfoEnabled)
			{
				string m = "INFO " + string.Format(format, args);
				if(DebugWriteLine) Out.DebugWriteLine(m);
				sb.Append(m).AppendLine();
			}
		}

		public void Warn(Exception exception)
		{
			if(IsWarnEnabled) Warn(exception, "Operation warning");
		}
		public void Warn(Exception exception, string message)
		{
			if(IsWarnEnabled)
			{
				HasErrors = true;
				string m = "WARN " + exception.ToString();
				if(DebugWriteLine) Out.DebugWriteLine(m);
				sb.Append(m).AppendLine();
			}
		}
		public void Warn(Exception exception, string format, params object[] args)
		{
			if(IsWarnEnabled)
			{
				HasErrors = true;
				string m = "WARN " + string.Format(format, args) + " " + exception.ToString();
				if(DebugWriteLine) Out.DebugWriteLine(m);
				sb.Append(m).AppendLine();
			}
		}
		public void Warn(string message)
		{
			if(IsInfoEnabled)
			{
				string m = "WARN " + message;
				if(DebugWriteLine) Out.DebugWriteLine(m);
				sb.Append(m).AppendLine();
			}
		}
		public void Warn(string format, params object[] args)
		{
			if(IsInfoEnabled)
			{
				string m = "WARN " + string.Format(format, args);
				if(DebugWriteLine) Out.DebugWriteLine(m);
				sb.Append(m).AppendLine();
			}
		}
		public void Debug(string message)
		{
			if(IsDebugEnabled)
			{
				string m = "DEBUG " + message;
				if(DebugWriteLine) Out.DebugWriteLine(m);
				sb.Append(m).AppendLine();
			}
		}
		public void Debug(string format, params object[] args)
		{
			if(IsDebugEnabled)
			{
				string m = "DEBUG " + string.Format(format, args);
				if(DebugWriteLine) Out.DebugWriteLine(m);
				sb.Append(m).AppendLine();
			}
		}
		public void Error(Exception exception, string format, params object[] args)
		{
			if(IsErrorEnabled)
			{
				HasErrors = true;
				string m = "ERROR " + string.Format(format, args) + " " + exception.ToString();
				if(DebugWriteLine) Out.DebugWriteLine(m);
				sb.Append(m).AppendLine();
			}
		}
		public void Error(Exception exception, string message)
		{
			if(IsErrorEnabled)
			{
				HasErrors = true;
				string m = "ERROR " + exception.ToString();
				if(DebugWriteLine) Out.DebugWriteLine(m);
				sb.Append(m).AppendLine();
			}
		}
		public void Error(Exception exception)
		{
			if(IsErrorEnabled) Error(exception, "Operation failed");
		}

		public bool HasErrors { get; private set; }

		public string Subject { get; set; }
		public string Recipients { get; set; }

		public string GetLog() { return sb.ToString(); }
		public void Flush()
		{
			if(sb.Length > 0)
			{
				var recipients = this.Recipients.Split(';');

				if(recipients.Count() > 0)
				{
					string subject = Subject;
					if(HasErrors) subject += " HasErrors:true";
					Util.Email(Subject, sb.ToString(), recipients.ToList(), contentType: CrownPeak.CMSAPI.ContentType.TextPlain);
				}
			}

			sb.Clear(); //yes i know, multithreaded access will hurt this.
		}

		#region IDisposable
		private bool _disposed;
		public void Dispose()
		{
			if(_disposed == false)
			{
				Dispose(true);
			}
			_disposed = true;
		}
		#endregion
		private void Dispose(bool disposing)
		{
			this.Flush();
		}


	}
}

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
	public class UtilLogLogger : IDisposable, CPLog.ILogger
	{
		private UtilLogLogger()
		{
			this.Buffered = true;
			this.IsDebugEnabled = false;
			this.IsInfoEnabled = true;
			this.IsErrorEnabled = true;
			this.IsWarnEnabled = true;
		}

		public UtilLogLogger(string componentName)
			: this()
		{
			SetComponentName(componentName);
		}
		public UtilLogLogger(string componentName, Asset asset)
			: this()
		{
			_LogAsset = asset;
			SetComponentName(componentName);
		}

		private Asset _LogAsset;
		private string _MessagePrefix;
		private string _ComponentName;
		public void SetMessagePrefix(string value)
		{
			if(value != null)
			{
				if(value.EndsWith(" ") == false)
				{
					value += " ";
				}
			}
			_MessagePrefix = value;
		}
		public void SetComponentName(string value)
		{
			if(value != null)
			{
				if(value.EndsWith(" ") == false)
				{
					value += " ";
				}
			}
			_ComponentName = value;
		}

		private bool _Buffered = true;
		public bool Buffered
		{
			get { return this._Buffered; }
			set { this._Buffered = value; }
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
				string m = _ComponentName + "INFO " + (_MessagePrefix == null ? "" : _MessagePrefix) + message;
				if(DebugWriteLine) Out.DebugWriteLine(m);
				WriteToLog(m);
			}
		}
		public void Info(string format, params object[] args)
		{
			if(IsInfoEnabled)
			{
				string m = _ComponentName + "INFO " + (_MessagePrefix == null ? "" : _MessagePrefix) + string.Format(format, args);
				if(DebugWriteLine) Out.DebugWriteLine(m);
				WriteToLog(m);
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
				string m = _ComponentName + "WARN " + (_MessagePrefix == null ? "" : _MessagePrefix) + message.TrimEnd(new char[] { '.' }) + ": " + exception.ToString();
				if(DebugWriteLine) Out.DebugWriteLine(m);
				WriteToLog(m);
			}
		}
		public void Warn(Exception exception, string format, params object[] args)
		{
			if(IsWarnEnabled) Warn(exception, string.Format(format, args));
		}
		public void Warn(string message)
		{
			if(IsInfoEnabled)
			{
				string m = _ComponentName + "WARN " + (_MessagePrefix == null ? "" : _MessagePrefix) + message;
				if(DebugWriteLine) Out.DebugWriteLine(m);
				WriteToLog(m);
			}
		}
		public void Warn(string format, params object[] args)
		{
			if(IsInfoEnabled)
			{
				string m = _ComponentName + "WARN " + string.Format(format, args);
				if(DebugWriteLine) Out.DebugWriteLine(m);
				WriteToLog(m);
			}
		}
		public void Debug(string message)
		{
			if(IsDebugEnabled)
			{
				string m = _ComponentName + "DEBUG " + (_MessagePrefix == null ? "" : _MessagePrefix) + message;
				if(DebugWriteLine) Out.DebugWriteLine(m);
				WriteToLog(m);
			}
		}
		public void Debug(string format, params object[] args)
		{
			if(IsDebugEnabled)
			{
				string m = _ComponentName + "DEBUG " + (_MessagePrefix == null ? "" : _MessagePrefix) + string.Format(format, args);
				if(DebugWriteLine) Out.DebugWriteLine(m);
				WriteToLog(m);
			}
		}

		public void Debug(Func<string> messageGeneratorFunc)
		{
			if(IsDebugEnabled)
			{
				this.Debug(messageGeneratorFunc());
			}
		}

		public void Error(Exception exception, string format, params object[] args)
		{
			if(IsErrorEnabled)
			{
				HasErrors = true;
				string m = _ComponentName + "ERROR " + (_MessagePrefix == null ? "" : _MessagePrefix) + string.Format(format, args) + " " + exception.ToString();
				if(DebugWriteLine) Out.DebugWriteLine(m);
				WriteToLog(m);
			}
		}

		public void Error(Exception exception)
		{
			if(IsErrorEnabled) Error(exception, "Operation failed");
		}

		public void Error(Exception exception, string message)
		{
			if(IsErrorEnabled)
			{
				HasErrors = true;
				string m = _ComponentName + "ERROR " + (_MessagePrefix == null ? "" : _MessagePrefix) + exception.ToString();
				if(DebugWriteLine) Out.DebugWriteLine(m);
				WriteToLog(m);
			}
		}

		public bool HasErrors { get; private set; }

		public string Subject { get; set; }
		//public string Recipients { get; set; }

		//public string GetLog() { return sb.ToString(); }

		private void WriteToLog(string message)
		{
			if(this.Buffered == true)
				sb.AppendLine(message);
			else
				_TryWriteToLog(message);
		}
		private void _TryWriteToLog(string message)
		{
			try
			{
				if(_LogAsset != null)
				{
					try { Util.Log(_LogAsset, message); }
					catch(Exception) { message = message.Replace("{", "{{").Replace("}", "}}"); Util.Log(_LogAsset, message); }
				}
				else
				{
					Util.Log(message);
				}
			}
			catch(Exception ex)
			{
				//if log exceptions==true
				StringBuilder sb = new StringBuilder();
				sb.AppendLine("Message to log: -------------------");
				sb.AppendLine(message);
				sb.AppendLine();
				sb.AppendLine("Exception: -------------------");
				sb.AppendLine(ex.ToString());

				string errorEmailDest = "eric.newton@lightmaker.com";
				Util.Email("UtilLogLogger failure: " + ex.Message, sb.ToString(), errorEmailDest);
			}
		}


		public void Flush()
		{
			if(sb.Length > 0)
			{
				_TryWriteToLog(sb.ToString());
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

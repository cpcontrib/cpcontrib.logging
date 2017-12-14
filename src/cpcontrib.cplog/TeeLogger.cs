using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CrownPeak.CMSAPI;
using CrownPeak.CMSAPI.Services;
/* Some Namespaces are not allowed. */
namespace CrownPeak.CMSAPI.CustomLibrary
{
	public class TeeLogger : IDisposable, CPLog.ILogger
	{

		private CPLog.ILogger _Logger1;
		private CPLog.ILogger _Logger2;

		public TeeLogger(CPLog.ILogger logger1, CPLog.ILogger logger2)
		{
			_Logger1 = logger1;
			_Logger2 = logger2;
		}

		public CPLog.ILogger Logger1 { get { return _Logger1; } }
		public CPLog.ILogger Logger2 { get { return _Logger2; } }

		public void Dispose()
		{
			IDisposable _disposable1 = _Logger1 as IDisposable;
			if(_disposable1 != null) _disposable1.Dispose();

			IDisposable _disposable2 = _Logger2 as IDisposable;
			if(_disposable2 != null) _disposable2.Dispose();
		}

		/// <summary>
		/// Gets IsDebugEnabled flag from Logger1
		/// </summary>
		public bool IsDebugEnabled { get { return _Logger1.IsDebugEnabled; } }

		/// <summary>
		/// Gets IsErrorEnabled flag from Logger1
		/// </summary>
		public bool IsErrorEnabled { get { return _Logger1.IsErrorEnabled; }  }

		/// <summary>
		/// Gets IsInfoEnabled flag from Logger1
		/// </summary>
		public bool IsInfoEnabled { get { return _Logger1.IsInfoEnabled; } }

		/// <summary>
		/// Gets IsWarnEnabled flag from Logger1
		/// </summary>
		public bool IsWarnEnabled { get { return _Logger1.IsWarnEnabled; } }

		public void Debug(string message)
		{
			_Logger1.Debug(message);
			_Logger2.Debug(message);
		}

		public void Debug(string format, params object[] args)
		{
			_Logger1.Debug(format, args);
			_Logger2.Debug(format, args);
		}

		public void Error(Exception ex)
		{
			_Logger1.Error(ex);
			_Logger2.Error(ex);
		}

		public void Error(Exception ex, string message)
		{
			_Logger1.Error(ex, message);
			_Logger2.Error(ex, message);
		}

		public void Error(Exception ex, string format, params object[] args)
		{
			_Logger1.Error(ex, format, args);
			_Logger2.Error(ex, format, args);
		}

		public void Info(string message)
		{
			_Logger1.Info(message);
			_Logger2.Info(message);
		}

		public void Info(string format, params object[] args)
		{
			_Logger1.Info(format, args);
			_Logger2.Info(format, args);
		}

		public void Warn(Exception ex)
		{
			_Logger1.Warn(ex);
			_Logger2.Warn(ex);
		}

		public void Warn(string message)
		{
			_Logger1.Warn(message);
			_Logger2.Warn(message);
		}

		public void Warn(string format, params object[] args)
		{
			_Logger1.Warn(format, args);
			_Logger2.Warn(format, args);
		}

		public void Warn(Exception ex, string message)
		{
			_Logger1.Warn(ex, message);
			_Logger2.Warn(ex, message);
		}

		public void Warn(Exception ex, string format, params object[] args)
		{
			_Logger1.Warn(ex, format, args);
			_Logger2.Warn(ex, format, args);
		}
	}
}

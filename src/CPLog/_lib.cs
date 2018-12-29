//!packer:targetFile=CPLog.cs
// PORTIONS Copyright (c) 2017 NewtonWorks LLC
//  this code adapts NLog into CrownPeak CMS.
//
// 
// NLOG PORTIONS Copyright (c) 2004-2017 Jaroslaw Kowalski <jaak@jkowalski.net>
// 
// All rights reserved.
// 
// Redistribution and use in source and binary forms, with or without 
// modification, are permitted provided that the following conditions 
// are met:
// 
// * Redistributions of source code must retain the above copyright notice, 
//   this list of conditions and the following disclaimer. 
// 
// * Redistributions in binary form must reproduce the above copyright notice,
//   this list of conditions and the following disclaimer in the documentation
//   and/or other materials provided with the distribution. 
// 
// * Neither the name of Jaroslaw Kowalski nor the names of its 
//   contributors may be used to endorse or promote products derived from this
//   software without specific prior written permission. 
// 
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS"
// AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE 
// IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE 
// ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT OWNER OR CONTRIBUTORS BE 
// LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR 
// CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF
// SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS 
// INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN 
// CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) 
// ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF 
// THE POSSIBILITY OF SUCH DAMAGE.
// 

using System;
using System.Collections.Generic;
using System.Text;
using CrownPeak.CMSAPI;

namespace CPLog
{
	using System.Runtime.CompilerServices;
	using CPLog.Config;
	using CPLog.Common;

	public class LogManager
	{
		private static LogFactory factory = new LogFactory();

		/// <summary>
		/// Gets the fully qualified name of the class invoking the LogManager, including the 
		/// namespace but not the assembly.    
		/// </summary>
		private static string GetClassFullName()
		{
			string className;
			Type declaringType;
			int framesToSkip = 2;

			do
			{
#if SILVERLIGHT
                StackFrame frame = new StackTrace().GetFrame(framesToSkip);
#else
				System.Diagnostics.StackFrame frame = new System.Diagnostics.StackFrame(framesToSkip, false);
#endif
				System./**/Reflection.MethodBase method = frame.GetMethod();
				declaringType = method.DeclaringType;
				if(declaringType == null)
				{
					className = method.Name;
					break;
				}

				framesToSkip++;
				className = declaringType.FullName;
			} while(declaringType.Module.Name.Equals("mscorlib.dll", StringComparison.OrdinalIgnoreCase));

			return className;
		}

		/// <summary>
		/// Gets the logger named after the currently-being-initialized class.
		/// </summary>
		/// <returns>The logger.</returns>
		/// <remarks>This is a slow-running method. 
		/// Make sure you're not doing this in a loop.</remarks>
		[MethodImpl(MethodImplOptions.NoInlining)]
		public static ILogger GetCurrentClassLogger()
		{
			return factory.GetLogger(GetClassFullName());
		}

		public static ILogger GetLogger(string name)
		{
			return factory.GetLogger(name);
		}
		public static ILogger GetLogger(Type type)
		{
			return factory.GetLogger(type.FullName);
		}
	}
	public class LogFactory
	{
		private readonly object syncRoot = new object();
		private bool configLoaded;

		private LoggingConfiguration config;

		internal LogFactory() { }

		/// <summary>
		/// Gets or sets the current logging configuration.
		/// </summary>
		public LoggingConfiguration Configuration
		{
			get
			{
				lock(this.syncRoot)
				{
					if(this.configLoaded)
					{
						return this.config;
					}

					this.configLoaded = true;

					// Retest the condition as we might have loaded a config.
					if(this.config == null)
					{
						//                        foreach (string configFile in GetCandidateConfigFileNames())
						//                        {
						//#if SILVERLIGHT
						//Uri configFileUri = new Uri(configFile, UriKind.Relative);
						//if (Application.GetResourceStream(configFileUri) != null)
						//{
						//    LoadLoggingConfiguration(configFile);
						//    break;
						//}
						//#else
						//                            if (File.Exists(configFile))
						//                            {
						//                                LoadLoggingConfiguration(configFile);
						//                                break;
						//                            }
						//#endif
						//                        }
						this.config = GetConfigurationFromAsset(Asset.Load("/System/Tools/nlog.config"));
					}

					if(this.config != null)
					{
#if !SILVERLIGHT
						config.Dump();
						try
						{
							//this.watcher.Watch(this.config.FileNamesToWatch);
						}
						catch(Exception exception)
						{
							InternalLogger.Warn("Cannot start file watching: {0}. File watching is disabled", exception);
						}
#endif
						this.config.InitializeAll();
					}

					return this.config;
				}
			}

			//            set
			//            {
			//#if !SILVERLIGHT
			//                try
			//                {
			//                    this.watcher.StopWatching();
			//                }
			//                catch (Exception exception)
			//                {
			//                    if (exception.MustBeRethrown())
			//                    {
			//                        throw;
			//                    }

			//                    InternalLogger.Error("Cannot stop file watching: {0}", exception);
			//                }
			//#endif

			//                lock (this.syncRoot)
			//                {
			//                    LoggingConfiguration oldConfig = this.config;
			//                    if (oldConfig != null)
			//                    {
			//                        InternalLogger.Info("Closing old configuration.");
			//#if !SILVERLIGHT
			//                        this.Flush();
			//#endif
			//                        oldConfig.Close();
			//                    }

			//                    this.config = value;
			//                    this.configLoaded = true;

			//                    if (this.config != null)
			//                    {
			//                        config.Dump();

			//                        this.config.InitializeAll();
			//                        this.ReconfigExistingLoggers();
			////#if !SILVERLIGHT
			////                        try
			////                        {
			////                            this.watcher.Watch(this.config.FileNamesToWatch);
			////                        }
			////                        catch (Exception exception)
			////                        {
			////                            if (exception.MustBeRethrown())
			////                            {
			////                                throw;
			////                            }

			////                            InternalLogger.Warn("Cannot start file watching: {0}", exception);
			////                        }
			////#endif
			//                    }

			//                    //this.OnConfigurationChanged(new LoggingConfigurationChangedEventArgs(value, oldConfig));
			//                }
			//            }
		}

		public LoggingConfiguration GetConfigurationFromAsset(Asset logconfigAsset)
		{
			try
			{
				LoggingConfiguration c = new LoggingConfiguration();

				//System.IO.StringReader sr = new System.IO.StringReader(logconfigAsset.Raw["body"]);

				string configStr = logconfigAsset.Raw["body"];

				foreach(var line in configStr.Split('\n'))
				{
					string[] lineSplit = line.Split('=');
					if(lineSplit[0] == "root")
					{

					}
				}

				return c;
			}
			catch(Exception ex)
			{
				InternalLogger.Error("Problem loading from asset '{1}' ({0}): {2}", logconfigAsset.Id, logconfigAsset.AssetPath, ex);
			}
			return new LoggingConfiguration();
		}
		private Asset _logconfigAsset;

		private Dictionary<string, ILogger> _loggers = new Dictionary<string, ILogger>();
		public ILogger GetLogger(string name)
		{
			ILogger returnVal = null;
			if(_loggers.ContainsKey(name) == false)
			{
				//lock(syncRoot)
				{
					if(_loggers.ContainsKey(name) == false)
					{
						returnVal = new Logger(name);
						_loggers.Add(name, returnVal);
					}
				}
			}
			else
			{
				returnVal = _loggers[name];
			}
			return returnVal;
		}
		public ILogger GetLogger(Type type)
		{
			return GetLogger(type.FullName);
		}
	}

	#region LogLevel
	/// <summary>
	/// Defines available log levels.
	/// </summary>
	public sealed class LogLevel : IComparable, IEquatable<LogLevel>
	{

		/// <summary>
		/// Trace log level.
		/// </summary>
		//[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Security", "CA2104:DoNotDeclareReadOnlyMutableReferenceTypes", Justification = "Type is immutable")]
		public static readonly LogLevel Trace = new LogLevel("Trace", 0);

		/// <summary>
		/// Debug log level.
		/// </summary>
		//[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Security", "CA2104:DoNotDeclareReadOnlyMutableReferenceTypes", Justification = "Type is immutable")]
		public static readonly LogLevel Debug = new LogLevel("Debug", 1);

		/// <summary>
		/// Info log level.
		/// </summary>
		//[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Security", "CA2104:DoNotDeclareReadOnlyMutableReferenceTypes", Justification = "Type is immutable")]
		public static readonly LogLevel Info = new LogLevel("Info", 2);

		/// <summary>
		/// Warn log level.
		/// </summary>
		//[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Security", "CA2104:DoNotDeclareReadOnlyMutableReferenceTypes", Justification = "Type is immutable")]
		public static readonly LogLevel Warn = new LogLevel("Warn", 3);

		/// <summary>
		/// Error log level.
		/// </summary>
		//[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Security", "CA2104:DoNotDeclareReadOnlyMutableReferenceTypes", Justification = "Type is immutable")]
		public static readonly LogLevel Error = new LogLevel("Error", 4);

		/// <summary>
		/// Fatal log level.
		/// </summary>
		//[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Security", "CA2104:DoNotDeclareReadOnlyMutableReferenceTypes", Justification = "Type is immutable")]
		public static readonly LogLevel Fatal = new LogLevel("Fatal", 5);

		/// <summary>
		/// Off log level.
		/// </summary>
		//[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Security", "CA2104:DoNotDeclareReadOnlyMutableReferenceTypes", Justification = "Type is immutable")]
		public static readonly LogLevel Off = new LogLevel("Off", 6);

		private readonly int ordinal;
		private readonly string name;

		/// <summary>
		/// Initializes a new instance of <see cref="LogLevel"/>.
		/// </summary>
		/// <param name="name">The log level name.</param>
		/// <param name="ordinal">The log level ordinal number.</param>
		private LogLevel(string name, int ordinal)
		{
			this.name = name;
			this.ordinal = ordinal;
		}

		/// <summary>
		/// Gets the name of the log level.
		/// </summary>
		public string Name
		{
			get { return this.name; }
		}

		internal static LogLevel MaxLevel
		{
			get { return Fatal; }
		}

		internal static LogLevel MinLevel
		{
			get { return Trace; }
		}

		/// <summary>
		/// Gets the ordinal of the log level.
		/// </summary>
		public int Ordinal
		{
			get { return this.ordinal; }
		}

		/// <summary>
		/// Compares two <see cref="LogLevel"/> objects 
		/// and returns a value indicating whether 
		/// the first one is equal to the second one.
		/// </summary>
		/// <param name="level1">The first level.</param>
		/// <param name="level2">The second level.</param>
		/// <returns>The value of <c>level1.Ordinal == level2.Ordinal</c>.</returns>
		public static bool operator ==(LogLevel level1, LogLevel level2)
		{
			if(ReferenceEquals(level1, null))
			{
				return ReferenceEquals(level2, null);
			}

			if(ReferenceEquals(level2, null))
			{
				return false;
			}

			return level1.Ordinal == level2.Ordinal;
		}

		/// <summary>
		/// Compares two <see cref="LogLevel"/> objects 
		/// and returns a value indicating whether 
		/// the first one is not equal to the second one.
		/// </summary>
		/// <param name="level1">The first level.</param>
		/// <param name="level2">The second level.</param>
		/// <returns>The value of <c>level1.Ordinal != level2.Ordinal</c>.</returns>
		public static bool operator !=(LogLevel level1, LogLevel level2)
		{
			if(ReferenceEquals(level1, null))
			{
				return !ReferenceEquals(level2, null);
			}

			if(ReferenceEquals(level2, null))
			{
				return true;
			}

			return level1.Ordinal != level2.Ordinal;
		}

		private static class ParameterUtils
		{
			public static void AssertNotNull(object source, string parametername)
			{
				//if (source == null) throw new ArgumentNullException(parametername);
			}
		}

		/// <summary>
		/// Compares two <see cref="LogLevel"/> objects 
		/// and returns a value indicating whether 
		/// the first one is greater than the second one.
		/// </summary>
		/// <param name="level1">The first level.</param>
		/// <param name="level2">The second level.</param>
		/// <returns>The value of <c>level1.Ordinal &gt; level2.Ordinal</c>.</returns>
		public static bool operator >(LogLevel level1, LogLevel level2)
		{
			ParameterUtils.AssertNotNull(level1, "level1");
			ParameterUtils.AssertNotNull(level2, "level2");

			return level1.Ordinal > level2.Ordinal;
		}

		/// <summary>
		/// Compares two <see cref="LogLevel"/> objects 
		/// and returns a value indicating whether 
		/// the first one is greater than or equal to the second one.
		/// </summary>
		/// <param name="level1">The first level.</param>
		/// <param name="level2">The second level.</param>
		/// <returns>The value of <c>level1.Ordinal &gt;= level2.Ordinal</c>.</returns>
		public static bool operator >=(LogLevel level1, LogLevel level2)
		{
			ParameterUtils.AssertNotNull(level1, "level1");
			ParameterUtils.AssertNotNull(level2, "level2");

			return level1.Ordinal >= level2.Ordinal;
		}

		/// <summary>
		/// Compares two <see cref="LogLevel"/> objects 
		/// and returns a value indicating whether 
		/// the first one is less than the second one.
		/// </summary>
		/// <param name="level1">The first level.</param>
		/// <param name="level2">The second level.</param>
		/// <returns>The value of <c>level1.Ordinal &lt; level2.Ordinal</c>.</returns>
		public static bool operator <(LogLevel level1, LogLevel level2)
		{
			ParameterUtils.AssertNotNull(level1, "level1");
			ParameterUtils.AssertNotNull(level2, "level2");

			return level1.Ordinal < level2.Ordinal;
		}

		/// <summary>
		/// Compares two <see cref="LogLevel"/> objects 
		/// and returns a value indicating whether 
		/// the first one is less than or equal to the second one.
		/// </summary>
		/// <param name="level1">The first level.</param>
		/// <param name="level2">The second level.</param>
		/// <returns>The value of <c>level1.Ordinal &lt;= level2.Ordinal</c>.</returns>
		public static bool operator <=(LogLevel level1, LogLevel level2)
		{
			ParameterUtils.AssertNotNull(level1, "level1");
			ParameterUtils.AssertNotNull(level2, "level2");

			return level1.Ordinal <= level2.Ordinal;
		}

		/// <summary>
		/// Gets the <see cref="LogLevel"/> that corresponds to the specified ordinal.
		/// </summary>
		/// <param name="ordinal">The ordinal.</param>
		/// <returns>The <see cref="LogLevel"/> instance. For 0 it returns <see cref="LogLevel.Trace"/>, 1 gives <see cref="LogLevel.Debug"/> and so on.</returns>
		public static LogLevel FromOrdinal(int ordinal)
		{
			switch(ordinal)
			{
				case 0:
					return Trace;
				case 1:
					return Debug;
				case 2:
					return Info;
				case 3:
					return Warn;
				case 4:
					return Error;
				case 5:
					return Fatal;
				case 6:
					return Off;

				default:
					throw new ArgumentException("Invalid ordinal.");
			}
		}

		/// <summary>
		/// Returns the <see cref="T:NLog.LogLevel"/> that corresponds to the supplied <see langword="string" />.
		/// </summary>
		/// <param name="levelName">The textual representation of the log level.</param>
		/// <returns>The enumeration value.</returns>
		public static LogLevel FromString(string levelName)
		{
			if(levelName == null)
			{
				throw new ArgumentNullException("levelName");
			}

			if(levelName.Equals("Trace", StringComparison.OrdinalIgnoreCase))
			{
				return Trace;
			}

			if(levelName.Equals("Debug", StringComparison.OrdinalIgnoreCase))
			{
				return Debug;
			}

			if(levelName.Equals("Info", StringComparison.OrdinalIgnoreCase))
			{
				return Info;
			}

			if(levelName.Equals("Warn", StringComparison.OrdinalIgnoreCase))
			{
				return Warn;
			}

			if(levelName.Equals("Error", StringComparison.OrdinalIgnoreCase))
			{
				return Error;
			}

			if(levelName.Equals("Fatal", StringComparison.OrdinalIgnoreCase))
			{
				return Fatal;
			}

			if(levelName.Equals("Off", StringComparison.OrdinalIgnoreCase))
			{
				return Off;
			}

			throw new ArgumentException("Unknown log level: " + levelName);
		}

		/// <summary>
		/// Returns a string representation of the log level.
		/// </summary>
		/// <returns>Log level name.</returns>
		public override string ToString()
		{
			return this.Name;
		}

		/// <summary>
		/// Returns a hash code for this instance.
		/// </summary>
		/// <returns>
		/// A hash code for this instance, suitable for use in hashing algorithms and data structures like a hash table. 
		/// </returns>
		public override int GetHashCode()
		{
			return this.Ordinal;
		}

		/// <summary>
		/// Determines whether the specified <see cref="System.Object"/> is equal to this instance.
		/// </summary>
		/// <param name="obj">The <see cref="System.Object"/> to compare with this instance.</param>
		/// <returns>Value of <c>true</c> if the specified <see cref="System.Object"/> is equal to 
		/// this instance; otherwise, <c>false</c>.</returns>
		public override bool Equals(object obj)
		{
			LogLevel other = obj as LogLevel;
			if((object)other == null)
			{
				return false;
			}

			return this.Ordinal == other.Ordinal;
		}

		/// <summary>
		/// Determines whether the specified <see cref="NLog.LogLevel"/> instance is equal to this instance.
		/// </summary>
		/// <param name="other">The <see cref="NLog.LogLevel"/> to compare with this instance.</param>
		/// <returns>Value of <c>true</c> if the specified <see cref="NLog.LogLevel"/> is equal to 
		/// this instance; otherwise, <c>false</c>.</returns>
		public bool Equals(LogLevel other)
		{
			return other == null ? false : this.Ordinal == other.Ordinal;
		}

		/// <summary>
		/// Compares the level to the other <see cref="LogLevel"/> object.
		/// </summary>
		/// <param name="obj">
		/// The object object.
		/// </param>
		/// <returns>
		/// A value less than zero when this logger's <see cref="Ordinal"/> is 
		/// less than the other logger's ordinal, 0 when they are equal and 
		/// greater than zero when this ordinal is greater than the
		/// other ordinal.
		/// </returns>
		public int CompareTo(object obj)
		{
			if(obj == null)
			{
				throw new ArgumentNullException("obj");
			}

			// The code below does NOT account if the casting to LogLevel returns null. This is 
			// because as this class is sealed and does not provide any public constructors it 
			// is impossible to create a invalid instance.

			LogLevel level = (LogLevel)obj;
			return this.Ordinal - level.Ordinal;
		}
	}
	#endregion

	#region Logger
	public class Logger : ILogger
	{
		private LoggerConfiguration configuration;
		private bool isTraceEnabled = false;
		private bool isDebugEnabled = false;
		private bool isInfoEnabled = true;
		private bool isWarnEnabled = true;
		private bool isErrorEnabled = true;
		private bool isFatalEnabled = true;
		public bool IsDebugEnabled { get { return this.isDebugEnabled; } set { this.isDebugEnabled = value; } }
		public bool IsWarnEnabled { get { return this.isWarnEnabled; } set { this.isWarnEnabled = value; } }
		public bool IsInfoEnabled { get { return this.isInfoEnabled; } set { this.isInfoEnabled = value; } }
		public bool IsErrorEnabled { get { return this.isErrorEnabled; } set { this.isErrorEnabled = value; } }
		public void SetInfo(bool p) { this.isInfoEnabled = p; }
		public void SetWarn(bool p) { this.isWarnEnabled = p; }
		public void SetDebug(bool p) { this.isDebugEnabled = p; }
		public void SetError(bool p) { this.isErrorEnabled = p; }

		internal Logger(string name)
		{
			if(Common.InternalLogger.IsDebugEnabled)
			{
				System.Diagnostics.StackTrace stacktrace = new System.Diagnostics.StackTrace(0);
				Common.InternalLogger.Debug("Creating logger '{0}':\n{1}", name, stacktrace.ToString());
			}

			//remove noise
			name = name.Replace("CrownPeak.CMSAPI.CustomLibrary.", "[CustomLibrary].");
			name = name.Replace("CrownPeak.CMSAPI.", "[CMSAPI].");

			this.Name = name;
		}
		public string Name { get; set; }
		public virtual void Debug(string message)
		{
			if(IsDebugEnabled) Out.DebugWriteLine(Name + "|DEBUG|" + message);
		}
		public virtual void Debug(string format, params object[] args)
		{
			if(IsDebugEnabled) Debug(string.Format(format, args));
		}

		public virtual void Debug(Func<string> messageGeneratorFunc)
		{
			if(IsDebugEnabled)
			{
				try
				{
					this.Debug(messageGeneratorFunc());
				}
				catch { }
			}
		}
		public virtual void Info(string message)
		{
			if(IsInfoEnabled) Out.DebugWriteLine(Name + "|INFO|" + message);
		}
		public virtual void Info(string format, params object[] args)
		{
			if(IsInfoEnabled) Info(string.Format(format, args));
		}
		public virtual void Warn(string message)
		{
			if(IsWarnEnabled) Out.DebugWriteLine(Name + "|WARN|" + message);
		}
		public virtual void Warn(Exception exception)
		{
			this.Warn(exception, "Operation warning");
		}
		public virtual void Warn(Exception exception, string message)
		{
			if(IsWarnEnabled) Warn(Name + "|WARN|" + message + "|" + exception.Message);
		}
		public virtual void Warn(Exception exception, string format, params object[] args)
		{
			if(IsWarnEnabled) Warn(exception, string.Format(format, args));
		}
		public virtual void Warn(string format, params object[] args)
		{
			if(IsWarnEnabled) Warn(string.Format(format, args));
		}

		public virtual void Error(Exception ex)
		{
			Out.DebugWriteLine(Name + "|ERROR|{0}", ex.ToString());
		}
		public virtual void Error(Exception ex, string message)
		{
			Out.DebugWriteLine(Name + "|ERROR|{0}|{1}", message, ex.ToString());
		}
		public virtual void Error(Exception ex, string format, params object[] args)
		{
			Out.DebugWriteLine(Name + "|ERROR|{0}|{1}", string.Format(format, args), ex.ToString());
		}

		internal virtual void SetConfiguration(LoggerConfiguration newConfiguration)
		{
			this.configuration = newConfiguration;

			// pre-calculate 'enabled' flags
			this.isTraceEnabled = newConfiguration.IsEnabled(LogLevel.Trace);
			this.isDebugEnabled = newConfiguration.IsEnabled(LogLevel.Debug);
			this.isInfoEnabled = newConfiguration.IsEnabled(LogLevel.Info);
			this.isWarnEnabled = newConfiguration.IsEnabled(LogLevel.Warn);
			this.isErrorEnabled = newConfiguration.IsEnabled(LogLevel.Error);
			this.isFatalEnabled = newConfiguration.IsEnabled(LogLevel.Fatal);

			//var loggerReconfiguredDelegate = this.LoggerReconfigured;

			//if (loggerReconfiguredDelegate != null)
			//{
			//    loggerReconfiguredDelegate(this, new EventArgs());
			//}
		}
	}
	#endregion

	public interface ILogger
	{
		bool IsDebugEnabled { get; }
		bool IsInfoEnabled { get; }
		bool IsWarnEnabled { get; }
		bool IsErrorEnabled { get; }

		void Debug(string message);
		void Debug(string format, params object[] args);
		void Debug(Func<string> messageGeneratorFunc);
		void Info(string message);
		void Info(string format, params object[] args);
		void Warn(string message);
		void Warn(Exception ex);
		void Warn(Exception ex, string message);
		void Warn(Exception ex, string format, params object[] args);
		void Warn(string format, params object[] args);
		void Error(Exception ex);
		void Error(Exception ex, string message);
		void Error(Exception ex, string format, params object[] args);
	}



}

namespace CPLog.Common
{
	#region InternalLogger

	/// <summary>
	/// NLog internal logger.
	/// </summary>
	public static class InternalLogger
	{
		private static object lockObject = new object();

		/// <summary>
		/// Initializes static members of the InternalLogger class.
		/// </summary>
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1810:InitializeReferenceTypeStaticFieldsInline", Justification = "Significant logic in .cctor()")]
		static InternalLogger()
		{
#if !SILVERLIGHT
			try
			{
				LogToConsole = GetSetting("nlog.internalLogToConsole", "NLOG_INTERNAL_LOG_TO_CONSOLE", false);
				LogToConsoleError = GetSetting("nlog.internalLogToConsoleError", "NLOG_INTERNAL_LOG_TO_CONSOLE_ERROR", false);
				LogLevel = GetSetting("nlog.internalLogLevel", "NLOG_INTERNAL_LOG_LEVEL", LogLevel.Info);
				LogFile = GetSetting("nlog.internalLogFile", "NLOG_INTERNAL_LOG_FILE", string.Empty);
				Info("NLog internal logger initialized.");
#else
            LogLevel = LogLevel.Info;
#endif
				IncludeTimestamp = true;
			}
			catch(Exception ex)
			{
				try { Out.DebugWriteLine("Failed to initialize InternalLogger: {0}", ex.ToString()); }
				catch { }
			}
		}

		/// <summary>
		/// Gets or sets the internal log level.
		/// </summary>
		public static LogLevel LogLevel { get; set; }

		/// <summary>
		/// Gets or sets a value indicating whether internal messages should be written to the console output stream.
		/// </summary>
		public static bool LogToConsole { get; set; }

		/// <summary>
		/// Gets or sets a value indicating whether internal messages should be written to the console error stream.
		/// </summary>
		public static bool LogToConsoleError { get; set; }

		/// <summary>
		/// Gets or sets the name of the internal log file.
		/// </summary>
		/// <remarks>A value of <see langword="null" /> value disables internal logging to a file.</remarks>
		public static string LogFile { get; set; }

		/// <summary>
		/// Gets or sets the text writer that will receive internal logs.
		/// </summary>
		//public static TextWriter LogWriter { get; set; }

		/// <summary>
		/// Gets or sets a value indicating whether timestamp should be included in internal log output.
		/// </summary>
		public static bool IncludeTimestamp { get; set; }

		/// <summary>
		/// Gets a value indicating whether internal log includes Trace messages.
		/// </summary>
		public static bool IsTraceEnabled
		{
			get { return LogLevel.Trace >= LogLevel; }
		}

		/// <summary>
		/// Gets a value indicating whether internal log includes Debug messages.
		/// </summary>
		public static bool IsDebugEnabled
		{
			get { return LogLevel.Debug >= LogLevel; }
		}

		/// <summary>
		/// Gets a value indicating whether internal log includes Info messages.
		/// </summary>
		public static bool IsInfoEnabled
		{
			get { return LogLevel.Info >= LogLevel; }
		}

		/// <summary>
		/// Gets a value indicating whether internal log includes Warn messages.
		/// </summary>
		public static bool IsWarnEnabled
		{
			get { return LogLevel.Warn >= LogLevel; }
		}

		/// <summary>
		/// Gets a value indicating whether internal log includes Error messages.
		/// </summary>
		public static bool IsErrorEnabled
		{
			get { return LogLevel.Error >= LogLevel; }
		}

		/// <summary>
		/// Gets a value indicating whether internal log includes Fatal messages.
		/// </summary>
		public static bool IsFatalEnabled
		{
			get { return LogLevel.Fatal >= LogLevel; }
		}

		/// <summary>
		/// Logs the specified message at the specified level.
		/// </summary>
		/// <param name="level">Log level.</param>
		/// <param name="message">Message which may include positional parameters.</param>
		/// <param name="args">Arguments to the message.</param>
		public static void Log(LogLevel level, string message, params object[] args)
		{
			Write(level, message, args);
		}

		/// <summary>
		/// Logs the specified message at the specified level.
		/// </summary>
		/// <param name="level">Log level.</param>
		/// <param name="message">Log message.</param>
		public static void Log(LogLevel level, [System.ComponentModel.Localizable(false)] string message)
		{
			Write(level, message, null);
		}

		/// <summary>
		/// Logs the specified message at the Trace level.
		/// </summary>
		/// <param name="message">Message which may include positional parameters.</param>
		/// <param name="args">Arguments to the message.</param>
		public static void Trace([System.ComponentModel.Localizable(false)] string message, params object[] args)
		{
			Write(LogLevel.Trace, message, args);
		}

		/// <summary>
		/// Logs the specified message at the Trace level.
		/// </summary>
		/// <param name="message">Log message.</param>
		public static void Trace([System.ComponentModel.Localizable(false)] string message)
		{
			Write(LogLevel.Trace, message, null);
		}

		/// <summary>
		/// Logs the specified message at the Debug level.
		/// </summary>
		/// <param name="message">Message which may include positional parameters.</param>
		/// <param name="args">Arguments to the message.</param>
		public static void Debug([System.ComponentModel.Localizable(false)] string message, params object[] args)
		{
			Write(LogLevel.Debug, message, args);
		}

		/// <summary>
		/// Logs the specified message at the Debug level.
		/// </summary>
		/// <param name="message">Log message.</param>
		public static void Debug([System.ComponentModel.Localizable(false)] string message)
		{
			Write(LogLevel.Debug, message, null);
		}

		/// <summary>
		/// Logs the specified message at the Info level.
		/// </summary>
		/// <param name="message">Message which may include positional parameters.</param>
		/// <param name="args">Arguments to the message.</param>
		public static void Info([System.ComponentModel.Localizable(false)] string message, params object[] args)
		{
			Write(LogLevel.Info, message, args);
		}

		/// <summary>
		/// Logs the specified message at the Info level.
		/// </summary>
		/// <param name="message">Log message.</param>
		public static void Info([System.ComponentModel.Localizable(false)] string message)
		{
			Write(LogLevel.Info, message, null);
		}

		/// <summary>
		/// Logs the specified message at the Warn level.
		/// </summary>
		/// <param name="message">Message which may include positional parameters.</param>
		/// <param name="args">Arguments to the message.</param>
		public static void Warn([System.ComponentModel.Localizable(false)] string message, params object[] args)
		{
			Write(LogLevel.Warn, message, args);
		}

		/// <summary>
		/// Logs the specified message at the Warn level.
		/// </summary>
		/// <param name="message">Log message.</param>
		public static void Warn([System.ComponentModel.Localizable(false)] string message)
		{
			Write(LogLevel.Warn, message, null);
		}

		/// <summary>
		/// Logs the specified message at the Error level.
		/// </summary>
		/// <param name="message">Message which may include positional parameters.</param>
		/// <param name="args">Arguments to the message.</param>
		public static void Error([System.ComponentModel.Localizable(false)] string message, params object[] args)
		{
			Write(LogLevel.Error, message, args);
		}

		/// <summary>
		/// Logs the specified message at the Error level.
		/// </summary>
		/// <param name="message">Log message.</param>
		public static void Error([System.ComponentModel.Localizable(false)] string message)
		{
			Write(LogLevel.Error, message, null);
		}

		/// <summary>
		/// Logs the specified message at the Fatal level.
		/// </summary>
		/// <param name="message">Message which may include positional parameters.</param>
		/// <param name="args">Arguments to the message.</param>
		public static void Fatal([System.ComponentModel.Localizable(false)] string message, params object[] args)
		{
			Write(LogLevel.Fatal, message, args);
		}

		/// <summary>
		/// Logs the specified message at the Fatal level.
		/// </summary>
		/// <param name="message">Log message.</param>
		public static void Fatal([System.ComponentModel.Localizable(false)] string message)
		{
			Write(LogLevel.Fatal, message, null);
		}

		private static void Write(LogLevel level, string message, object[] args)
		{
			if(level < LogLevel)
			{
				return;
			}

			if(args != null)
			{
				Out.DebugWriteLine(level.Name + " " + string.Format(message, args));
			}
			else
			{
				Out.DebugWriteLine(level.Name + " " + message);
			}

			/*
			if (string.IsNullOrEmpty(LogFile) && !LogToConsole && !LogToConsoleError && LogWriter == null)
			{
				return;
			}

			try
			{
				string formattedMessage = message;
				if (args != null)
				{
					formattedMessage = string.Format(CultureInfo.InvariantCulture, message, args);
				}

				var builder = new StringBuilder(message.Length + 32);
				if (IncludeTimestamp)
				{
					builder.Append(TimeSource.Current.Time.ToString("yyyy-MM-dd HH:mm:ss.ffff", CultureInfo.InvariantCulture));
					builder.Append(" ");
				}

				builder.Append(level.ToString());
				builder.Append(" ");
				builder.Append(formattedMessage);
				string msg = builder.ToString();

				// log to file
				var logFile = LogFile;
				if (!string.IsNullOrEmpty(logFile))
				{
					using (var textWriter = File.AppendText(logFile))
					{
						textWriter.WriteLine(msg);
					}
				}

				// log to LogWriter
				var writer = LogWriter;
				if (writer != null)
				{
					lock (lockObject)
					{
						writer.WriteLine(msg);
					}
				}

				// log to console
				if (LogToConsole)
				{
					Console.WriteLine(msg);
				}

				// log to console error
				if (LogToConsoleError)
				{
					Console.Error.WriteLine(msg);
				}
			}
			catch (Exception exception)
			{
				if (exception.MustBeRethrown())
				{
					throw;
				}

				// we have no place to log the message to so we ignore it
			}
			*/
		}


#if !SILVERLIGHT
		private static string GetSettingString(string configName, string envName)
		{
			Asset cplogConfiguration = Asset.Load("/System/Tools/nlog.config");
			string settingValue = cplogConfiguration.Raw[configName];
			//if (settingValue == null)
			//{
			//    try
			//    {
			//        settingValue = Environment.GetEnvironmentVariable(envName);
			//    }
			//    catch (Exception exception)
			//    {
			//        if (exception.MustBeRethrown())
			//        {
			//            throw;
			//        }
			//    }
			//}

			return settingValue;
		}

		private static LogLevel GetSetting(string configName, string envName, LogLevel defaultValue)
		{
			try
			{
				string value = GetSettingString(configName, envName);
				if(value == null)
				{
					return defaultValue;
				}

				return LogLevel.FromString(value);
			}
			catch //(Exception exception)
			{
				//if (exception.MustBeRethrown())
				//{
				//    throw;
				//}

				return defaultValue;
			}
		}

		private static T GetSetting<T>(string configName, string envName, T defaultValue)
		{
			string value = GetSettingString(configName, envName);
			if(value == null)
			{
				return defaultValue;
			}

			try
			{
				return (T)Convert.ChangeType(value, typeof(T), System.Globalization.CultureInfo.InvariantCulture);
			}
			catch //(Exception exception)
			{
				//if (exception.MustBeRethrown())
				//{
				//    throw;
				//}

				return defaultValue;
			}
		}
#endif

	}
	#endregion
}
namespace CPLog.Config
{
	using CPLog.Common;
	using CPLog.Internal;
	using CPLog.Filters;
	using CPLog.Targets;
	using System.Collections.ObjectModel;
	using System.Globalization;

	#region LoggerConfiguration
	/// <summary>
	/// Logger configuration.
	/// </summary>
	internal class LoggerConfiguration
	{
		private readonly TargetWithFilterChain[] targetsByLevel;

		/// <summary>
		/// Initializes a new instance of the <see cref="LoggerConfiguration" /> class.
		/// </summary>
		/// <param name="targetsByLevel">The targets by level.</param>
		public LoggerConfiguration(TargetWithFilterChain[] targetsByLevel)
		{
			this.targetsByLevel = targetsByLevel;
		}

		/// <summary>
		/// Gets targets for the specified level.
		/// </summary>
		/// <param name="level">The level.</param>
		/// <returns>Chain of targets with attached filters.</returns>
		public TargetWithFilterChain GetTargetsForLevel(LogLevel level)
		{
			return this.targetsByLevel[level.Ordinal];
		}

		/// <summary>
		/// Determines whether the specified level is enabled.
		/// </summary>
		/// <param name="level">The level.</param>
		/// <returns>
		/// A value of <c>true</c> if the specified level is enabled; otherwise, <c>false</c>.
		/// </returns>
		public bool IsEnabled(LogLevel level)
		{
			return this.targetsByLevel[level.Ordinal] != null;
		}
	}
	#endregion
	#region LoggingConfiguration
	public class LoggingConfiguration
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="LoggingConfiguration" /> class.
		/// </summary>
		public LoggingConfiguration()
		{
			this.LoggingRules = new List<LoggingRule>();
		}

		/// <summary>
		/// Gets the collection of logging rules.
		/// </summary>
		public IList<LoggingRule> LoggingRules { get; private set; }

		public void Dump() { }
		public void InitializeAll() { }
	}
	#endregion
	#region LoggingRule
	/// <summary>
	/// Represents a logging rule. An equivalent of &lt;logger /&gt; configuration element.
	/// </summary>
	[NLogConfigurationItem]
	public class LoggingRule
	{
		private readonly bool[] logLevels = new bool[LogLevel.MaxLevel.Ordinal + 1];

		private string loggerNamePattern;
		private MatchMode loggerNameMatchMode;
		private string loggerNameMatchArgument;

		/// <summary>
		/// Initializes a new instance of the <see cref="LoggingRule" /> class.
		/// </summary>
		public LoggingRule()
		{
			this.Filters = new List<Filter>();
			this.ChildRules = new List<LoggingRule>();
			this.Targets = new List<Target>();
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="LoggingRule" /> class.
		/// </summary>
		/// <param name="loggerNamePattern">Logger name pattern. It may include the '*' wildcard at the beginning, at the end or at both ends.</param>
		/// <param name="minLevel">Minimum log level needed to trigger this rule.</param>
		/// <param name="target">Target to be written to when the rule matches.</param>
		public LoggingRule(string loggerNamePattern, LogLevel minLevel, Target target)
		{
			this.Filters = new List<Filter>();
			this.ChildRules = new List<LoggingRule>();
			this.Targets = new List<Target>();
			this.LoggerNamePattern = loggerNamePattern;
			this.Targets.Add(target);
			for(int i = minLevel.Ordinal; i <= LogLevel.MaxLevel.Ordinal; ++i)
			{
				this.EnableLoggingForLevel(LogLevel.FromOrdinal(i));
			}
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="LoggingRule" /> class.
		/// </summary>
		/// <param name="loggerNamePattern">Logger name pattern. It may include the '*' wildcard at the beginning, at the end or at both ends.</param>
		/// <param name="target">Target to be written to when the rule matches.</param>
		/// <remarks>By default no logging levels are defined. You should call <see cref="EnableLoggingForLevel"/> and <see cref="DisableLoggingForLevel"/> to set them.</remarks>
		public LoggingRule(string loggerNamePattern, Target target)
		{
			this.Filters = new List<Filter>();
			this.ChildRules = new List<LoggingRule>();
			this.Targets = new List<Target>();
			this.LoggerNamePattern = loggerNamePattern;
			this.Targets.Add(target);
		}

		internal enum MatchMode
		{
			All,
			None,
			Equals,
			StartsWith,
			EndsWith,
			Contains,
		}

		/// <summary>
		/// Gets a collection of targets that should be written to when this rule matches.
		/// </summary>
		public IList<Target> Targets { get; private set; }

		/// <summary>
		/// Gets a collection of child rules to be evaluated when this rule matches.
		/// </summary>
		public IList<LoggingRule> ChildRules { get; private set; }

		/// <summary>
		/// Gets a collection of filters to be checked before writing to targets.
		/// </summary>
		public IList<Filter> Filters { get; private set; }

		/// <summary>
		/// Gets or sets a value indicating whether to quit processing any further rule when this one matches.
		/// </summary>
		public bool Final { get; set; }

		/// <summary>
		/// Gets or sets logger name pattern.
		/// </summary>
		/// <remarks>
		/// Logger name pattern. It may include the '*' wildcard at the beginning, at the end or at both ends but not anywhere else.
		/// </remarks>
		public string LoggerNamePattern
		{
			get
			{
				return this.loggerNamePattern;
			}

			set
			{
				this.loggerNamePattern = value;
				int firstPos = this.loggerNamePattern.IndexOf('*');
				int lastPos = this.loggerNamePattern.LastIndexOf('*');

				if(firstPos < 0)
				{
					this.loggerNameMatchMode = MatchMode.Equals;
					this.loggerNameMatchArgument = value;
					return;
				}

				if(firstPos == lastPos)
				{
					string before = this.LoggerNamePattern.Substring(0, firstPos);
					string after = this.LoggerNamePattern.Substring(firstPos + 1);

					if(before.Length > 0)
					{
						this.loggerNameMatchMode = MatchMode.StartsWith;
						this.loggerNameMatchArgument = before;
						return;
					}

					if(after.Length > 0)
					{
						this.loggerNameMatchMode = MatchMode.EndsWith;
						this.loggerNameMatchArgument = after;
						return;
					}

					return;
				}

				// *text*
				if(firstPos == 0 && lastPos == this.LoggerNamePattern.Length - 1)
				{
					string text = this.LoggerNamePattern.Substring(1, this.LoggerNamePattern.Length - 2);
					this.loggerNameMatchMode = MatchMode.Contains;
					this.loggerNameMatchArgument = text;
					return;
				}

				this.loggerNameMatchMode = MatchMode.None;
				this.loggerNameMatchArgument = string.Empty;
			}
		}

		/// <summary>
		/// Gets the collection of log levels enabled by this rule.
		/// </summary>
		public ReadOnlyCollection<LogLevel> Levels
		{
			get
			{
				var levels = new List<LogLevel>();

				for(int i = LogLevel.MinLevel.Ordinal; i <= LogLevel.MaxLevel.Ordinal; ++i)
				{
					if(this.logLevels[i])
					{
						levels.Add(LogLevel.FromOrdinal(i));
					}
				}

				return levels.AsReadOnly();
			}
		}

		/// <summary>
		/// Enables logging for a particular level.
		/// </summary>
		/// <param name="level">Level to be enabled.</param>
		public void EnableLoggingForLevel(LogLevel level)
		{
			this.logLevels[level.Ordinal] = true;
		}

		/// <summary>
		/// Disables logging for a particular level.
		/// </summary>
		/// <param name="level">Level to be disabled.</param>
		public void DisableLoggingForLevel(LogLevel level)
		{
			this.logLevels[level.Ordinal] = false;
		}

		/// <summary>
		/// Returns a string representation of <see cref="LoggingRule"/>. Used for debugging.
		/// </summary>
		/// <returns>
		/// A <see cref="T:System.String"/> that represents the current <see cref="T:System.Object"/>.
		/// </returns>
		public override string ToString()
		{
			var sb = new StringBuilder();

			sb.AppendFormat(CultureInfo.InvariantCulture, "logNamePattern: ({0}:{1})", this.loggerNameMatchArgument, this.loggerNameMatchMode);
			sb.Append(" levels: [ ");
			for(int i = 0; i < this.logLevels.Length; ++i)
			{
				if(this.logLevels[0])
				{
					sb.AppendFormat(CultureInfo.InvariantCulture, "{0} ", LogLevel.FromOrdinal(i).ToString());
				}
			}

			sb.Append("] appendTo: [ ");
			foreach(Target app in this.Targets)
			{
				sb.AppendFormat(CultureInfo.InvariantCulture, "{0} ", app.Name);
			}

			sb.Append("]");
			return sb.ToString();
		}

		/// <summary>
		/// Checks whether te particular log level is enabled for this rule.
		/// </summary>
		/// <param name="level">Level to be checked.</param>
		/// <returns>A value of <see langword="true"/> when the log level is enabled, <see langword="false" /> otherwise.</returns>
		public bool IsLoggingEnabledForLevel(LogLevel level)
		{
			return this.logLevels[level.Ordinal];
		}

		/// <summary>
		/// Checks whether given name matches the logger name pattern.
		/// </summary>
		/// <param name="loggerName">String to be matched.</param>
		/// <returns>A value of <see langword="true"/> when the name matches, <see langword="false" /> otherwise.</returns>
		public bool NameMatches(string loggerName)
		{
			switch(this.loggerNameMatchMode)
			{
				case MatchMode.All:
					return true;

				default:
				case MatchMode.None:
					return false;

				case MatchMode.Equals:
					return loggerName.Equals(this.loggerNameMatchArgument, StringComparison.Ordinal);

				case MatchMode.StartsWith:
					return loggerName.StartsWith(this.loggerNameMatchArgument, StringComparison.Ordinal);

				case MatchMode.EndsWith:
					return loggerName.EndsWith(this.loggerNameMatchArgument, StringComparison.Ordinal);

				case MatchMode.Contains:
					return loggerName.IndexOf(this.loggerNameMatchArgument, StringComparison.Ordinal) >= 0;
			}
		}
	}
	#endregion
	#region NLogConfigurationItemAttribute
	public class NLogConfigurationItemAttribute : Attribute
	{
	}
	#endregion
	#region StackTraceUsage
	/// <summary>
	/// Value indicating how stack trace should be captured when processing the log event.
	/// </summary>
	public enum StackTraceUsage
	{
		/// <summary>
		/// Stack trace should not be captured.
		/// </summary>
		None = 0,

		/// <summary>
		/// Stack trace should be captured without source-level information.
		/// </summary>
		WithoutSource = 1,

#if !SILVERLIGHT
		/// <summary>
		/// Stack trace should be captured including source-level information such as line numbers.
		/// </summary>
		WithSource = 2,

		/// <summary>
		/// Capture maximum amount of the stack trace information supported on the platform.
		/// </summary>
		Max = 2,
#else
        /// <summary>
        /// Capture maximum amount of the stack trace information supported on the platform.
        /// </summary>
        Max = 1,
#endif
	}
	#endregion
}
namespace CPLog.Filters
{
	public class Filter
	{
	}
}
namespace CPLog.Internal
{
	using CPLog.Common;
	using CPLog.Config;
	using CPLog.Targets;
	using CPLog.Filters;

	#region TargetWithFilterChain
	/// <summary>
	/// Represents target with a chain of filters which determine
	/// whether logging should happen.
	/// </summary>
	[NLogConfigurationItem]
	internal class TargetWithFilterChain
	{
		private StackTraceUsage stackTraceUsage = StackTraceUsage.None;

		/// <summary>
		/// Initializes a new instance of the <see cref="TargetWithFilterChain" /> class.
		/// </summary>
		/// <param name="target">The target.</param>
		/// <param name="filterChain">The filter chain.</param>
		public TargetWithFilterChain(Target target, IList<Filter> filterChain)
		{
			this.Target = target;
			this.FilterChain = filterChain;
			this.stackTraceUsage = StackTraceUsage.None;
		}

		/// <summary>
		/// Gets the target.
		/// </summary>
		/// <value>The target.</value>
		public Target Target { get; private set; }

		/// <summary>
		/// Gets the filter chain.
		/// </summary>
		/// <value>The filter chain.</value>
		public IList<Filter> FilterChain { get; private set; }

		/// <summary>
		/// Gets or sets the next <see cref="TargetWithFilterChain"/> item in the chain.
		/// </summary>
		/// <value>The next item in the chain.</value>
		public TargetWithFilterChain NextInChain { get; set; }

		/// <summary>
		/// Gets the stack trace usage.
		/// </summary>
		/// <returns>A <see cref="StackTraceUsage" /> value that determines stack trace handling.</returns>
		public StackTraceUsage GetStackTraceUsage()
		{
			return this.stackTraceUsage;
		}

		internal void PrecalculateStackTraceUsage()
		{
			this.stackTraceUsage = StackTraceUsage.None;

			//// find all objects which may need stack trace
			//// and determine maximum
			//foreach (var item in ObjectGraphScanner.FindReachableObjects<IUsesStackTrace>(this))
			//{
			//    var stu = item.StackTraceUsage;

			//    if (stu > this.stackTraceUsage)
			//    {
			//        this.stackTraceUsage = stu;

			//        if (this.stackTraceUsage >= StackTraceUsage.Max)
			//        {
			//            break;
			//        }
			//    }
			//}
		}
	}
	#endregion
}
namespace CPLog.Targets
{
	using CPLog.Config;
	using CPLog.Common;

	#region Target
	public abstract class Target
	{
		public string Name { get { return ""; } }
	}
	#endregion
}

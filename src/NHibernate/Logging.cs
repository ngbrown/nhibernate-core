using System;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using NHibernate.Util;

namespace NHibernate
{
	// Since v5.1
	[Obsolete("Implement and use NHibernate.INHibernateLogger")]
	public interface IInternalLogger
	{
		// Since v5.1
		[Obsolete("Please use IsErrorEnabled() INHibernateLogger extension method instead.")]
		bool IsErrorEnabled { get; }
		// Since v5.1
		[Obsolete("Please use IsFatalEnabled() INHibernateLogger extension method instead.")]
		bool IsFatalEnabled { get; }
		// Since v5.1
		[Obsolete("Please use IsDebugEnabled() INHibernateLogger extension method instead.")]
		bool IsDebugEnabled { get; }
		// Since v5.1
		[Obsolete("Please use IsInfoEnabled() INHibernateLogger extension method instead.")]
		bool IsInfoEnabled { get; }
		// Since v5.1
		[Obsolete("Please use IsWarnEnabled() INHibernateLogger extension method instead.")]
		bool IsWarnEnabled { get; }

		// Since v5.1
		[Obsolete("Please use Error(string, params object[]) INHibernateLogger extension method instead.")]
		void Error(object message);
		// Since v5.1
		[Obsolete("Please use Error(Exception, string, params object[]) INHibernateLogger extension method instead.")]
		void Error(object message, Exception exception);
		// Since v5.1
		[Obsolete("Please use Error(string, params object[]) INHibernateLogger extension method instead.")]
		void ErrorFormat(string format, params object[] args);

		// Since v5.1
		[Obsolete("Please use Fatal(string, params object[]) INHibernateLogger extension method instead.")]
		void Fatal(object message);
		// Since v5.1
		[Obsolete("Please use Fatal(Exception, string, params object[]) INHibernateLogger extension method instead.")]
		void Fatal(object message, Exception exception);

		// Since v5.1
		[Obsolete("Please use Debug(string, params object[]) INHibernateLogger extension method instead.")]
		void Debug(object message);
		// Since v5.1
		[Obsolete("Please use Debug(Exception, string, params object[]) INHibernateLogger extension method instead.")]
		void Debug(object message, Exception exception);
		// Since v5.1
		[Obsolete("Please use Debug(string, params object[]) INHibernateLogger extension method instead.")]
		void DebugFormat(string format, params object[] args);

		// Since v5.1
		[Obsolete("Please use Info(string, params object[]) INHibernateLogger extension method instead.")]
		void Info(object message);
		// Since v5.1
		[Obsolete("Please use Info(Exception, string, params object[]) INHibernateLogger extension method instead.")]
		void Info(object message, Exception exception);
		// Since v5.1
		[Obsolete("Please use Info(string, params object[]) INHibernateLogger extension method instead.")]
		void InfoFormat(string format, params object[] args);

		// Since v5.1
		[Obsolete("Please use Warn(string, params object[]) INHibernateLogger extension method instead.")]
		void Warn(object message);
		// Since v5.1
		[Obsolete("Please use Warn(Exception, string, params object[]) INHibernateLogger extension method instead.")]
		void Warn(object message, Exception exception);
		// Since v5.1
		[Obsolete("Please use Warn(string, params object[]) INHibernateLogger extension method instead.")]
		void WarnFormat(string format, params object[] args);
	}

	// Since v5.1
	// Required till look alike methods taking object are dropped.
	[Obsolete("Implement and use NHibernate.INHibernateLogger")]
	public interface ITransitionalInternaLogger : IInternalLogger
	{
		void Fatal(string message);
		void Error(string message);
		void Warn(string message);
		void Info(string message);
		void Debug(string message);
	}

#pragma warning disable 618 // ITransitionalInternaLogger is obsolete, to be removed in an upcoming major version
	/// <summary>
	/// NHibernate internal logger interface.
	/// </summary>
	/// <remarks>
	/// For implementors: only two methods are non obsolete. Use <see cref="NHibernateLoggerBase"/> as a base class
	/// in order to avoiding having to implement all the obsolete methods.
	/// </remarks>
	public interface INHibernateLogger: ITransitionalInternaLogger
#pragma warning restore 618
	{
		/// <summary>Writes a log entry.</summary>
		/// <param name="logLevel">Entry will be written on this level.</param>
		/// <param name="state">The entry to be written.</param>
		/// <param name="exception">The exception related to this entry.</param>
		void Log(InternalLogLevel logLevel, InternalLogValues state, Exception exception);

		/// <summary>
		/// Checks if the given <paramref name="logLevel" /> is enabled.
		/// </summary>
		/// <param name="logLevel">level to be checked.</param>
		/// <returns><c>true</c> if enabled.</returns>
		bool IsEnabled(InternalLogLevel logLevel);
	}

	// Since 5.1
	[Obsolete("Implement INHibernateLoggerFactory instead")]
	public interface ILoggerFactory
	{
		IInternalLogger LoggerFor(string keyName);
		IInternalLogger LoggerFor(System.Type type);
	}

	/// <summary>
	/// Factory interface for providing a <see cref="INHibernateLogger"/>.
	/// </summary>
	public interface INHibernateLoggerFactory
	{
		/// <summary>
		/// Get a logger for the given log key.
		/// </summary>
		/// <param name="keyName">The log key.</param>
		/// <returns>A NHibernate logger.</returns>
		INHibernateLogger LoggerFor(string keyName);
		/// <summary>
		/// Get a logger using the given type as log key.
		/// </summary>
		/// <param name="type">The type to use as log key.</param>
		/// <returns>A NHibernate logger.</returns>
		INHibernateLogger LoggerFor(System.Type type);
	}

	/// <summary>
	/// Provide methods for getting NHibernate loggers according to supplied <see cref="INHibernateLoggerFactory"/>.
	/// </summary>
	/// <remarks>
	/// By default, it will use a <see cref="Log4NetLoggerFactory"/> if log4net is available, otherwise it will
	/// use a <see cref="NoLoggingLoggerFactory"/>.
	/// </remarks>
	public static class LoggerProvider
	{
		private const string nhibernateLoggerConfKey = "nhibernate-logger";
		private static INHibernateLoggerFactory _loggerFactory;

		static LoggerProvider()
		{
			var nhibernateLoggerClass = GetNhibernateLoggerClass();
			var loggerFactory = string.IsNullOrEmpty(nhibernateLoggerClass) ? new NoLoggingLoggerFactory() : GetLoggerFactory(nhibernateLoggerClass);
			SetLoggersFactory(loggerFactory);
		}

		private static INHibernateLoggerFactory GetLoggerFactory(string nhibernateLoggerClass)
		{
			INHibernateLoggerFactory loggerFactory;
			var loggerFactoryType = System.Type.GetType(nhibernateLoggerClass);
			try
			{
				var loadedLoggerFactory = Activator.CreateInstance(loggerFactoryType);
				if (loadedLoggerFactory is INHibernateLoggerFactory newStyleFactory)
				{
					loggerFactory = newStyleFactory;
				}
				else
				{
#pragma warning disable 618
					loggerFactory = new LegacyLoggerFactoryAdaptor((ILoggerFactory) loadedLoggerFactory);
#pragma warning restore 618
				}
			}
			catch (MissingMethodException ex)
			{
				throw new InstantiationException("Public constructor was not found for " + loggerFactoryType, ex, loggerFactoryType);
			}
			catch (InvalidCastException ex)
			{
#pragma warning disable 618
				throw new InstantiationException(loggerFactoryType + "Type does not implement " + typeof(INHibernateLoggerFactory) + " or " + typeof (ILoggerFactory), ex, loggerFactoryType);
#pragma warning restore 618
			}
			catch (Exception ex)
			{
				throw new InstantiationException("Unable to instantiate: " + loggerFactoryType, ex, loggerFactoryType);
			}
			return loggerFactory;
		}

		private static string GetNhibernateLoggerClass()
		{
			var nhibernateLogger = ConfigurationManager.AppSettings.Keys.Cast<string>().FirstOrDefault(k => nhibernateLoggerConfKey.Equals(k.ToLowerInvariant()));
			string nhibernateLoggerClass = null;
			if (string.IsNullOrEmpty(nhibernateLogger))
			{
				// look for log4net.dll
				string baseDir = AppDomain.CurrentDomain.BaseDirectory;
				string relativeSearchPath = AppDomain.CurrentDomain.RelativeSearchPath;
				string binPath = relativeSearchPath == null ? baseDir : Path.Combine(baseDir, relativeSearchPath);
				string log4NetDllPath = binPath == null ? "log4net.dll" : Path.Combine(binPath, "log4net.dll");

				if (File.Exists(log4NetDllPath) || AppDomain.CurrentDomain.GetAssemblies().Any(a => a.GetName().Name == "log4net"))
				{
					nhibernateLoggerClass = typeof (Log4NetLoggerFactory).AssemblyQualifiedName;
				}
			}
			else
			{
				nhibernateLoggerClass = ConfigurationManager.AppSettings[nhibernateLogger];
			}
			return nhibernateLoggerClass;
		}

		// Since 5.1
		[Obsolete("Implement INHibernateLoggerFactory instead")]
		public static void SetLoggersFactory(ILoggerFactory loggerFactory)
		{
			_loggerFactory = new LegacyLoggerFactoryAdaptor(loggerFactory);
		}

		/// <summary>
		/// Specify the logger factory to use for building loggers.
		/// </summary>
		/// <param name="loggerFactory">A logger factory.</param>
		public static void SetLoggersFactory(INHibernateLoggerFactory loggerFactory)
		{
			_loggerFactory = loggerFactory;
		}

		/// <summary>
		/// Get a logger for the given log key.
		/// </summary>
		/// <param name="keyName">The log key.</param>
		/// <returns>A NHibernate logger.</returns>
		public static INHibernateLogger LoggerFor(string keyName)
		{
			return _loggerFactory.LoggerFor(keyName);
		}

		/// <summary>
		/// Get a logger using the given type as log key.
		/// </summary>
		/// <param name="type">The type to use as log key.</param>
		/// <returns>A NHibernate logger.</returns>
		public static INHibernateLogger LoggerFor(System.Type type)
		{
			return _loggerFactory.LoggerFor(type);
		}

		// Since 5.1
		[Obsolete("Used only in Obsolete functions to thunk to INHibernateLoggerFactory")]
		private class LegacyLoggerFactoryAdaptor : INHibernateLoggerFactory
		{
			private readonly ILoggerFactory _factory;

			public LegacyLoggerFactoryAdaptor(ILoggerFactory factory)
			{
				_factory = factory;
			}

			INHibernateLogger INHibernateLoggerFactory.LoggerFor(string keyName)
			{
				return new NHibernateLoggerThunk(_factory.LoggerFor(keyName));
			}

			INHibernateLogger INHibernateLoggerFactory.LoggerFor(System.Type type)
			{
				return new NHibernateLoggerThunk(_factory.LoggerFor(type));
			}
		}
	}

	// Since 5.1
	[Obsolete("Used only in Obsolete functions to thunk to INHibernateLoggerFactory")]
	internal class NHibernateLoggerThunk : NHibernateLoggerBase, INHibernateLogger
	{
		private readonly IInternalLogger _internalLogger;

		public NHibernateLoggerThunk(IInternalLogger internalLogger)
		{
			_internalLogger = internalLogger ?? throw new ArgumentNullException(nameof(internalLogger));
		}

		public void Log(InternalLogLevel logLevel, InternalLogValues state, Exception exception)
		{
			if (!IsEnabled(logLevel))
				return;

			switch (logLevel)
			{
				case InternalLogLevel.Debug:
				case InternalLogLevel.Trace:
					if (exception != null)
						_internalLogger.Debug(state, exception);
					else if (state.Args?.Length > 0)
						_internalLogger.DebugFormat(state.Format, state.Args);
					else
						_internalLogger.Debug(state);
					break;
				case InternalLogLevel.Info:
					if (exception != null)
						_internalLogger.Info(state, exception);
					else if (state.Args?.Length > 0)
						_internalLogger.InfoFormat(state.Format, state.Args);
					else
						_internalLogger.Info(state);
					break;
				case InternalLogLevel.Warn:
					if (exception != null)
						_internalLogger.Warn(state, exception);
					else if (state.Args?.Length > 0)
						_internalLogger.WarnFormat(state.Format, state.Args);
					else
						_internalLogger.Warn(state);
					break;
				case InternalLogLevel.Error:
					if (exception != null)
						_internalLogger.Error(state, exception);
					else if (state.Args?.Length > 0)
						_internalLogger.ErrorFormat(state.Format, state.Args);
					else
						_internalLogger.Error(state);
					break;
				case InternalLogLevel.Fatal:
					if (exception != null)
						_internalLogger.Fatal(state, exception);
					else
						_internalLogger.Fatal(state);
					break;
				case InternalLogLevel.None:
					break;
				default:
					throw new ArgumentOutOfRangeException(nameof(logLevel), logLevel, null);
			}
		}

		public bool IsEnabled(InternalLogLevel logLevel)
		{
			switch (logLevel)
			{
				case InternalLogLevel.Trace:
				case InternalLogLevel.Debug:
					return _internalLogger.IsDebugEnabled;
				case InternalLogLevel.Info:
					return _internalLogger.IsInfoEnabled;
				case InternalLogLevel.Warn:
					return _internalLogger.IsWarnEnabled;
				case InternalLogLevel.Error:
					return _internalLogger.IsErrorEnabled;
				case InternalLogLevel.Fatal:
					return _internalLogger.IsFatalEnabled;
				case InternalLogLevel.None:
					return !_internalLogger.IsFatalEnabled;
				default:
					throw new ArgumentOutOfRangeException(nameof(logLevel), logLevel, null);
			}
		}
	}

	/// <summary>
#pragma warning disable 618
	/// Base class for <see cref="INHibernateLogger"/> implementations, provides <see cref="ITransitionalInternaLogger"/>
#pragma warning restore 618
	/// methods. It will be obsoleted once the old logger interfaces are dropped. <see cref="INHibernateLogger" />
	/// implementors using this base class will only need to cease using a base once it gets obsoleted.
	/// </summary>
	public abstract class NHibernateLoggerBase
	{
		private readonly INHibernateLogger _this;

		protected NHibernateLoggerBase()
		{
			_this = this as INHibernateLogger ??
				throw new InvalidOperationException("Concrete implementation must be an INHibernateLogger");
		}

		// Since v5.1
		[Obsolete("Please use IsErrorEnabled() INHibernateLogger extension method instead.")]
		public bool IsErrorEnabled => _this.IsErrorEnabled();
		// Since v5.1
		[Obsolete("Please use IsFatalEnabled() INHibernateLogger extension method instead.")]
		public bool IsFatalEnabled => _this.IsFatalEnabled();
		// Since v5.1
		[Obsolete("Please use IsDebugEnabled() INHibernateLogger extension method instead.")]
		public bool IsDebugEnabled => _this.IsDebugEnabled();
		// Since v5.1
		[Obsolete("Please use IsInfoEnabled() INHibernateLogger extension method instead.")]
		public bool IsInfoEnabled => _this.IsInfoEnabled();
		// Since v5.1
		[Obsolete("Please use IsWarnEnabled() INHibernateLogger extension method instead.")]
		public bool IsWarnEnabled => _this.IsWarnEnabled();

		// Since v5.1
		[Obsolete("Please use Error(string, params object[]) INHibernateLogger extension method instead.")]
		public void Error(object message) => _this.Error(message?.ToString());

		// Since v5.1
		[Obsolete("Please use Error(string, params object[]) INHibernateLogger extension method instead.")]
		public void Error(string message) => _this.Error(message, default(object[]));

		// Since v5.1
		[Obsolete("Please use Error(Exception, string, params object[]) INHibernateLogger extension method instead.")]
		public void Error(object message, Exception exception) => _this.Error(exception, message?.ToString());

		// Since v5.1
		[Obsolete("Please use Error(string, params object[]) INHibernateLogger extension method instead.")]
		public void ErrorFormat(string format, params object[] args) => _this.Error(format, args);

		// Since v5.1
		[Obsolete("Please use Fatal(string, params object[]) INHibernateLogger extension method instead.")]
		public void Fatal(object message) => _this.Fatal(message?.ToString());

		// Since v5.1
		[Obsolete("Please use Fatal(string, params object[]) INHibernateLogger extension method instead.")]
		public void Fatal(string message) => _this.Fatal(message, default(object[]));

		// Since v5.1
		[Obsolete("Please use Fatal(Exception, string, params object[]) INHibernateLogger extension method instead.")]
		public void Fatal(object message, Exception exception) => _this.Fatal(exception, message?.ToString());

		// Since v5.1
		[Obsolete("Please use Debug(string, params object[]) INHibernateLogger extension method instead.")]
		public void Debug(object message) => _this.Debug(message?.ToString());

		// Since v5.1
		[Obsolete("Please use Debug(string, params object[]) INHibernateLogger extension method instead.")]
		public void Debug(string message) => _this.Debug(message, default(object[]));

		// Since v5.1
		[Obsolete("Please use Debug(Exception, string, params object[]) INHibernateLogger extension method instead.")]
		public void Debug(object message, Exception exception) => _this.Debug(exception, message?.ToString());

		// Since v5.1
		[Obsolete("Please use Debug(string, params object[]) INHibernateLogger extension method instead.")]
		public void DebugFormat(string format, params object[] args) => _this.Debug(format, args);

		// Since v5.1
		[Obsolete("Please use Info(string, params object[]) INHibernateLogger extension method instead.")]
		public void Info(object message) => _this.Info(message?.ToString());

		// Since v5.1
		[Obsolete("Please use Info(string, params object[]) INHibernateLogger extension method instead.")]
		public void Info(string message) => _this.Info(message, default(object[]));

		// Since v5.1
		[Obsolete("Please use Info(Exception, string, params object[]) INHibernateLogger extension method instead.")]
		public void Info(object message, Exception exception) => _this.Info(exception, message?.ToString());

		// Since v5.1
		[Obsolete("Please use Info(string, params object[]) INHibernateLogger extension method instead.")]
		public void InfoFormat(string format, params object[] args) => _this.Info(format, args);

		// Since v5.1
		[Obsolete("Please use Warn(string, params object[]) INHibernateLogger extension method instead.")]
		public void Warn(object message) => _this.Warn(message?.ToString());

		// Since v5.1
		[Obsolete("Please use Warn(string, params object[]) INHibernateLogger extension method instead.")]
		public void Warn(string message) => _this.Warn(message, default(object[]));

		// Since v5.1
		[Obsolete("Please use Warn(Exception, string, params object[]) INHibernateLogger extension method instead.")]
		public void Warn(object message, Exception exception) => _this.Warn(exception, message?.ToString());

		// Since v5.1
		[Obsolete("Please use Warn(string, params object[]) INHibernateLogger extension method instead.")]
		public void WarnFormat(string format, params object[] args) => _this.Warn(format, args);
	}

	/// <summary>
	/// Factory building loggers which log nothing.
	/// </summary>
#pragma warning disable 618 // ILoggerFactory is obsolete
	public class NoLoggingLoggerFactory: INHibernateLoggerFactory, ILoggerFactory
#pragma warning restore 618
	{
		private static readonly INHibernateLogger Nologging = new NoLoggingInternalLogger();
		/// <inheritdoc />
		INHibernateLogger INHibernateLoggerFactory.LoggerFor(string keyName)
		{
			return Nologging;
		}

		/// <inheritdoc />
		INHibernateLogger INHibernateLoggerFactory.LoggerFor(System.Type type)
		{
			return Nologging;
		}

#pragma warning disable 618
		[Obsolete("Use this as an INHibernateLoggerFactory instead.")]
		public IInternalLogger LoggerFor(System.Type type)
		{
			return Nologging;
		}

		[Obsolete("Use this as an INHibernateLoggerFactory instead.")]
		public IInternalLogger LoggerFor(string keyName)
		{
			return Nologging;
		}
#pragma warning restore 618
	}

#pragma warning disable 618 // NHibernateLoggerBase is obsolete, to be removed in an upcoming major version
	/// <summary>
	/// Logger which logs nothing.
	/// </summary>
	public class NoLoggingInternalLogger: NHibernateLoggerBase, INHibernateLogger
#pragma warning restore 618
	{
		/// <inheritdoc />
		public void Log(InternalLogLevel logLevel, InternalLogValues state, Exception exception)
		{
		}

		/// <inheritdoc />
		public bool IsEnabled(InternalLogLevel logLevel)
		{
			return logLevel == InternalLogLevel.None;
		}
	}

#pragma warning disable 618 // ILoggerFactory is obsolete
	/// <summary>
	/// Reflection based log4net logger factory.
	/// </summary>
	public class Log4NetLoggerFactory: ILoggerFactory, INHibernateLoggerFactory
#pragma warning restore 618
	{
		private static readonly System.Type LogManagerType = System.Type.GetType("log4net.LogManager, log4net");
		private static readonly Func<Assembly, string, object> GetLoggerByNameDelegate;
		private static readonly Func<System.Type, object> GetLoggerByTypeDelegate;

		static Log4NetLoggerFactory()
		{
			GetLoggerByNameDelegate = GetGetLoggerByNameMethodCall();
			GetLoggerByTypeDelegate = GetGetLoggerMethodCall<System.Type>();
		}

#pragma warning disable 618
		[Obsolete("Use this as an INHibernateLoggerFactory instead.")]
		public IInternalLogger LoggerFor(string keyName)
		{
			INHibernateLoggerFactory nhFact = this;
			return nhFact.LoggerFor(keyName);
		}

		[Obsolete("Use this as an INHibernateLoggerFactory instead.")]
		public IInternalLogger LoggerFor(System.Type type)
		{
			INHibernateLoggerFactory nhFact = this;
			return nhFact.LoggerFor(type);
		}
#pragma warning restore 618

		INHibernateLogger INHibernateLoggerFactory.LoggerFor(string keyName)
		{
			return new Log4NetLogger(GetLoggerByNameDelegate(typeof(Log4NetLoggerFactory).Assembly, keyName));
		}

		INHibernateLogger INHibernateLoggerFactory.LoggerFor(System.Type type)
		{
			return new Log4NetLogger(GetLoggerByTypeDelegate(type));
		}

		private static Func<TParameter, object> GetGetLoggerMethodCall<TParameter>()
		{
			var method = LogManagerType.GetMethod("GetLogger", new[] { typeof(TParameter) });
			ParameterExpression resultValue;
			ParameterExpression keyParam = Expression.Parameter(typeof(TParameter), "key");
			MethodCallExpression methodCall = Expression.Call(null, method, resultValue = keyParam);
			return Expression.Lambda<Func<TParameter, object>>(methodCall, resultValue).Compile();
		}

		private static Func<Assembly, string, object> GetGetLoggerByNameMethodCall()
		{
			var method = LogManagerType.GetMethod("GetLogger", new[] {typeof(Assembly), typeof(string)});
			ParameterExpression nameParam = Expression.Parameter(typeof(string), "name");
			ParameterExpression repositoryAssemblyParam = Expression.Parameter(typeof(Assembly), "repositoryAssembly");
			MethodCallExpression methodCall = Expression.Call(null, method, repositoryAssemblyParam, nameParam);
			return Expression.Lambda<Func<Assembly, string, object>>(methodCall, repositoryAssemblyParam, nameParam).Compile();
		}
	}

#pragma warning disable 618 // NHibernateLoggerBase is obsolete, to be removed in a upcoming major version
	/// <summary>
	/// Reflection based log4net logger.
	/// </summary>
	public class Log4NetLogger: NHibernateLoggerBase, INHibernateLogger
#pragma warning restore 618
	{
		private static readonly System.Type ILogType = System.Type.GetType("log4net.ILog, log4net");
		private static readonly Func<object, bool> IsErrorEnabledDelegate;
		private static readonly Func<object, bool> IsFatalEnabledDelegate;
		private static readonly Func<object, bool> IsDebugEnabledDelegate;
		private static readonly Func<object, bool> IsInfoEnabledDelegate;
		private static readonly Func<object, bool> IsWarnEnabledDelegate;

		private static readonly Action<object, object> ErrorDelegate;
		private static readonly Action<object, object, Exception> ErrorExceptionDelegate;
		private static readonly Action<object, string, object[]> ErrorFormatDelegate;

		private static readonly Action<object, object> FatalDelegate;
		private static readonly Action<object, object, Exception> FatalExceptionDelegate;

		private static readonly Action<object, object> DebugDelegate;
		private static readonly Action<object, object, Exception> DebugExceptionDelegate;
		private static readonly Action<object, string, object[]> DebugFormatDelegate;

		private static readonly Action<object, object> InfoDelegate;
		private static readonly Action<object, object, Exception> InfoExceptionDelegate;
		private static readonly Action<object, string, object[]> InfoFormatDelegate;

		private static readonly Action<object, object> WarnDelegate;
		private static readonly Action<object, object, Exception> WarnExceptionDelegate;
		private static readonly Action<object, string, object[]> WarnFormatDelegate;

		private readonly object _logger;

		static Log4NetLogger()
		{
			IsErrorEnabledDelegate = DelegateHelper.BuildPropertyGetter<bool>(ILogType, "IsErrorEnabled");
			IsFatalEnabledDelegate = DelegateHelper.BuildPropertyGetter<bool>(ILogType, "IsFatalEnabled");
			IsDebugEnabledDelegate = DelegateHelper.BuildPropertyGetter<bool>(ILogType, "IsDebugEnabled");
			IsInfoEnabledDelegate = DelegateHelper.BuildPropertyGetter<bool>(ILogType, "IsInfoEnabled");
			IsWarnEnabledDelegate = DelegateHelper.BuildPropertyGetter<bool>(ILogType, "IsWarnEnabled");
			ErrorDelegate = DelegateHelper.BuildAction<object>(ILogType, "Error");
			ErrorExceptionDelegate = DelegateHelper.BuildAction<object, Exception>(ILogType, "Error");
			ErrorFormatDelegate = DelegateHelper.BuildAction<string, object[]>(ILogType, "ErrorFormat");

			FatalDelegate = DelegateHelper.BuildAction<object>(ILogType, "Fatal");
			FatalExceptionDelegate = DelegateHelper.BuildAction<object, Exception>(ILogType, "Fatal");

			DebugDelegate = DelegateHelper.BuildAction<object>(ILogType, "Debug");
			DebugExceptionDelegate = DelegateHelper.BuildAction<object, Exception>(ILogType, "Debug");
			DebugFormatDelegate = DelegateHelper.BuildAction<string, object[]>(ILogType, "DebugFormat");

			InfoDelegate = DelegateHelper.BuildAction<object>(ILogType, "Info");
			InfoExceptionDelegate = DelegateHelper.BuildAction<object, Exception>(ILogType, "Info");
			InfoFormatDelegate = DelegateHelper.BuildAction<string, object[]>(ILogType, "InfoFormat");

			WarnDelegate = DelegateHelper.BuildAction<object>(ILogType, "Warn");
			WarnExceptionDelegate = DelegateHelper.BuildAction<object, Exception>(ILogType, "Warn");
			WarnFormatDelegate = DelegateHelper.BuildAction<string, object[]>(ILogType, "WarnFormat");
		}

		/// <summary>
		/// Default constructor.
		/// </summary>
		/// <param name="logger">The <c>log4net.ILog</c> logger to use for logging.</param>
		public Log4NetLogger(object logger)
		{
			_logger = logger;
		}

		/// <inheritdoc />
		public void Log(InternalLogLevel logLevel, InternalLogValues state, Exception exception)
		{
			if (!IsEnabled(logLevel))
				return;

			switch (logLevel)
			{
				case InternalLogLevel.Debug:
				case InternalLogLevel.Trace:
					if (exception != null)
						DebugExceptionDelegate(_logger, state, exception);
					else if (state.Args?.Length > 0)
						DebugFormatDelegate(_logger, state.Format, state.Args);
					else
						DebugDelegate(_logger, state);
					break;
				case InternalLogLevel.Info:
					if (exception != null)
						InfoExceptionDelegate(_logger, state, exception);
					else if (state.Args?.Length > 0)
						InfoFormatDelegate(_logger, state.Format, state.Args);
					else
						InfoDelegate(_logger, state);
					break;
				case InternalLogLevel.Warn:
					if (exception != null)
						WarnExceptionDelegate(_logger, state, exception);
					else if (state.Args?.Length > 0)
						WarnFormatDelegate(_logger, state.Format, state.Args);
					else
						WarnDelegate(_logger, state);
					break;
				case InternalLogLevel.Error:
					if (exception != null)
						ErrorExceptionDelegate(_logger, state, exception);
					else if (state.Args?.Length > 0)
						ErrorFormatDelegate(_logger, state.Format, state.Args);
					else
						ErrorDelegate(_logger, state);
					break;
				case InternalLogLevel.Fatal:
					if (exception != null)
						FatalExceptionDelegate(_logger, state, exception);
					else
						FatalDelegate(_logger, state);
					break;
				case InternalLogLevel.None:
					break;
				default:
					throw new ArgumentOutOfRangeException(nameof(logLevel), logLevel, null);
			}
		}

		/// <inheritdoc />
		public bool IsEnabled(InternalLogLevel logLevel)
		{
			switch (logLevel)
			{
				case InternalLogLevel.Trace:
				case InternalLogLevel.Debug:
					return IsDebugEnabledDelegate(_logger);
				case InternalLogLevel.Info:
					return IsInfoEnabledDelegate(_logger);
				case InternalLogLevel.Warn:
					return IsWarnEnabledDelegate(_logger);
				case InternalLogLevel.Error:
					return IsErrorEnabledDelegate(_logger);
				case InternalLogLevel.Fatal:
					return IsFatalEnabledDelegate(_logger);
				case InternalLogLevel.None:
					return !IsFatalEnabledDelegate(_logger);
				default:
					throw new ArgumentOutOfRangeException(nameof(logLevel), logLevel, null);
			}
		}
		
		#region IInternalLogger re-implementations required for avoiding a breaking change
		// If some dependent library use the old internal logger, passing to it objects while using
		// formatters, we must then implement directly the call. The base impl would instead call
		// ToString on the object, preventing the formatters to work.

		void IInternalLogger.Fatal(object message)
		{
			FatalDelegate(_logger, message);
		}

		void IInternalLogger.Fatal(object message, Exception exception)
		{
			FatalExceptionDelegate(_logger, message, exception);
		}

		void IInternalLogger.Error(object message)
		{
			ErrorDelegate(_logger, message);
		}

		void IInternalLogger.Error(object message, Exception exception)
		{
			ErrorExceptionDelegate(_logger, message, exception);
		}

		void IInternalLogger.Warn(object message)
		{
			WarnDelegate(_logger, message);
		}

		void IInternalLogger.Warn(object message, Exception exception)
		{
			WarnExceptionDelegate(_logger, message, exception);
		}

		void IInternalLogger.Info(object message)
		{
			InfoDelegate(_logger, message);
		}

		void IInternalLogger.Info(object message, Exception exception)
		{
			InfoExceptionDelegate(_logger, message, exception);
		}

		void IInternalLogger.Debug(object message)
		{
			DebugDelegate(_logger, message);
		}

		void IInternalLogger.Debug(object message, Exception exception)
		{
			DebugExceptionDelegate(_logger, message, exception);
		}

		#endregion
	}

	/// <summary>
	/// Extensions method for logging.
	/// </summary>
	public static class NHibernateLoggerExtensions
	{
		public static bool IsDebugEnabled(this INHibernateLogger logger) => logger.IsEnabled(InternalLogLevel.Debug);
		public static bool IsInfoEnabled(this INHibernateLogger logger) => logger.IsEnabled(InternalLogLevel.Info);
		public static bool IsWarnEnabled(this INHibernateLogger logger) => logger.IsEnabled(InternalLogLevel.Warn);
		public static bool IsErrorEnabled(this INHibernateLogger logger) => logger.IsEnabled(InternalLogLevel.Error);
		public static bool IsFatalEnabled(this INHibernateLogger logger) => logger.IsEnabled(InternalLogLevel.Fatal);

		public static void Fatal(this INHibernateLogger logger, Exception exception, string format, params object[] args)
		{
			logger.Log(InternalLogLevel.Fatal, new InternalLogValues(format, args), exception);
		}

		public static void Fatal(this INHibernateLogger logger, string format, params object[] args)
		{
			logger.Log(InternalLogLevel.Fatal, new InternalLogValues(format, args), null);
		}

		public static void Error(this INHibernateLogger logger, Exception exception, string format, params object[] args)
		{
			logger.Log(InternalLogLevel.Error, new InternalLogValues(format, args), exception);
		}

		public static void Error(this INHibernateLogger logger, string format, params object[] args)
		{
			logger.Log(InternalLogLevel.Error, new InternalLogValues(format, args), null);
		}

		public static void Warn(this INHibernateLogger logger, Exception exception, string format, params object[] args)
		{
			logger.Log(InternalLogLevel.Warn, new InternalLogValues(format, args), exception);
		}

		public static void Warn(this INHibernateLogger logger, string format, params object[] args)
		{
			logger.Log(InternalLogLevel.Warn, new InternalLogValues(format, args), null);
		}

		public static void Info(this INHibernateLogger logger, Exception exception, string format, params object[] args)
		{
			logger.Log(InternalLogLevel.Info, new InternalLogValues(format, args), exception);
		}

		public static void Info(this INHibernateLogger logger, string format, params object[] args)
		{
			logger.Log(InternalLogLevel.Info, new InternalLogValues(format, args), null);
		}

		public static void Debug(this INHibernateLogger logger, Exception exception, string format, params object[] args)
		{
			logger.Log(InternalLogLevel.Debug, new InternalLogValues(format, args), exception);
		}

		public static void Debug(this INHibernateLogger logger, string format, params object[] args)
		{
			logger.Log(InternalLogLevel.Debug, new InternalLogValues(format, args), null);
		}
	}

	/// <summary>
	/// Data to log.
	/// </summary>
	public struct InternalLogValues
	{
		private readonly string _format;
		private readonly object[] _args;

		/// <summary>
		/// Default constructor.
		/// </summary>
		/// <param name="format">The message, eventually having format placeholders.</param>
		/// <param name="args">The formating arguments.</param>
		public InternalLogValues(string format, object[] args)
		{
			_format = format ?? "[Null]";
			_args = args;
		}

		/// <summary>
		/// The message, eventually having format placeholders.
		/// </summary>
		public string Format => _format;
		/// <summary>
		/// The formating arguments.
		/// </summary>
		public object[] Args => _args;

		/// <summary>
		/// The string representation of the log data.
		/// </summary>
		/// <returns></returns>
		public override string ToString()
		{
			return _args?.Length > 0 ? string.Format(_format, _args) : Format;
		}
	}

	/// <summary>Defines logging severity levels.</summary>
	public enum InternalLogLevel
	{
		Trace,
		Debug,
		Info,
		Warn,
		Error,
		Fatal,
		None,
	}
}

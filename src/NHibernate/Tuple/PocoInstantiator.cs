using System;
using System.Reflection;
using System.Runtime.Serialization;

using NHibernate.Bytecode;
using NHibernate.Mapping;
using NHibernate.Proxy;
using NHibernate.Util;

namespace NHibernate.Tuple
{
	/// <summary> Defines a POCO-based instantiator for use from the tuplizers.</summary>
	[Serializable]
	public class PocoInstantiator : IInstantiator, IDeserializationCallback
	{
		private static readonly INHibernateLogger log = NHibernateLogger.For(typeof(PocoInstantiator));

		private readonly SerializableSystemType _mappedClass;

		[NonSerialized]
		private IInstantiationOptimizer _optimizer;

		private readonly IProxyFactory _proxyFactory;

		private readonly bool _generateFieldInterceptionProxy;

		private readonly bool _embeddedIdentifier;

		[NonSerialized]
		private ConstructorInfo _constructor;

		private readonly SerializableSystemType _proxyInterface;

		public PocoInstantiator()
		{
		}

		public PocoInstantiator(Mapping.Component component, IInstantiationOptimizer optimizer)
		{
			_mappedClass = component.ComponentClass;
			_optimizer = optimizer;

			_proxyInterface = null;
			_embeddedIdentifier = false;

			try
			{
				_constructor = ReflectHelper.GetDefaultConstructor(_mappedClass.GetSystemType());
			}
			catch (PropertyNotFoundException)
			{
				log.Info("no default (no-argument) constructor for class: {0} (class must be instantiated by Interceptor)", _mappedClass.FullName);
				_constructor = null;
			}
		}

		public PocoInstantiator(PersistentClass persistentClass, IInstantiationOptimizer optimizer, IProxyFactory proxyFactory, bool generateFieldInterceptionProxy)
		{
			_mappedClass = persistentClass.MappedClass;
			_proxyInterface = persistentClass.ProxyInterface;
			_embeddedIdentifier = persistentClass.HasEmbeddedIdentifier;
			_optimizer = optimizer;
			_proxyFactory = proxyFactory;
			_generateFieldInterceptionProxy = generateFieldInterceptionProxy;

			try
			{
				_constructor = ReflectHelper.GetDefaultConstructor(_mappedClass.GetSystemType());
			}
			catch (PropertyNotFoundException)
			{
				log.Info("no default (no-argument) constructor for class: {0} (class must be instantiated by Interceptor)", _mappedClass.FullName);
				_constructor = null;
			}
		}

		#region IInstantiator Members

		public object Instantiate(object id)
		{
			bool useEmbeddedIdentifierInstanceAsEntity = _embeddedIdentifier && id != null && id.GetType().Equals(_mappedClass.GetSystemType());
			return useEmbeddedIdentifierInstanceAsEntity ? id : Instantiate();
		}

		public object Instantiate()
		{
			if (ReflectHelper.IsAbstractClass(_mappedClass.GetSystemType()))
			{
				throw new InstantiationException("Cannot instantiate abstract class or interface: ", _mappedClass.TryGetSystemType());
			}
			if (_generateFieldInterceptionProxy)
			{
				return _proxyFactory.GetFieldInterceptionProxy(GetInstance());
			}
			return GetInstance();
		}

		private object GetInstance()
		{
			if (_optimizer != null)
			{
				return _optimizer.CreateInstance();
			}
			if (_mappedClass.GetSystemType().IsValueType)
			{
				return Cfg.Environment.BytecodeProvider.ObjectsFactory.CreateInstance(_mappedClass.GetSystemType(), true);
			}
			if (_constructor == null)
			{
				throw new InstantiationException("No default constructor for entity: ", _mappedClass.TryGetSystemType());
			}
			try
			{
				return _constructor.Invoke(null);
			}
			catch (Exception e)
			{
				throw new InstantiationException("Could not instantiate entity: ", e, _mappedClass.TryGetSystemType());
			}
		}

		public bool IsInstance(object obj)
		{
			return _mappedClass.GetSystemType().IsInstanceOfType(obj) || (_proxyInterface != null && _proxyInterface.GetSystemType().IsInstanceOfType(obj)); //this one needed only for guessEntityMode()
		}

		#endregion

		#region IDeserializationCallback Members

		public void OnDeserialization(object sender)
		{
			if (_generateFieldInterceptionProxy)
			{
				throw new InvalidOperationException("IProxyFactory implementors are currently not serializable.");
			}

			_constructor = ReflectHelper.GetDefaultConstructor(_mappedClass.GetSystemType());
		}

		#endregion

		public void SetOptimizer(IInstantiationOptimizer optimizer)
		{
			_optimizer = optimizer;
		}
	}
}

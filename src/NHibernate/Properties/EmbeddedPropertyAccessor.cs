using System;
using System.Collections;
using System.Reflection;
using NHibernate.Engine;
using NHibernate.Util;

namespace NHibernate.Properties
{
	[Serializable]
	public class EmbeddedPropertyAccessor : IPropertyAccessor
	{
		#region IPropertyAccessor Members

		public IGetter GetGetter(System.Type theClass, string propertyName)
		{
			return new EmbeddedGetter(theClass);
		}

		public ISetter GetSetter(System.Type theClass, string propertyName)
		{
			return new EmbeddedSetter(theClass);
		}

		public bool CanAccessThroughReflectionOptimizer => false;

		#endregion

		[Serializable]
		public sealed class EmbeddedGetter : IGetter
		{
			private readonly SerializableSystemType _clazz;

			internal EmbeddedGetter(System.Type clazz)
			{
				this._clazz = clazz ?? throw new ArgumentNullException(nameof(clazz));
			}

			#region IGetter Members

			public object Get(object target)
			{
				return target;
			}

			public System.Type ReturnType => _clazz?.GetSystemType();

			public string PropertyName => null;

			public MethodInfo Method => null;

			public object GetForInsert(object owner, IDictionary mergeMap, ISessionImplementor session)
			{
				return Get(owner);
			}

			#endregion

			public override string ToString()
			{
				return string.Format("EmbeddedGetter({0})", _clazz.FullName);
			}
		}

		[Serializable]
		public sealed class EmbeddedSetter : ISetter
		{
			private readonly System.Type _clazz;

			internal EmbeddedSetter(System.Type clazz)
			{
				this._clazz = clazz ?? throw new ArgumentNullException(nameof(clazz));
			}

			#region ISetter Members

			public void Set(object target, object value)
			{
			}

			public string PropertyName => null;

			public MethodInfo Method => null;

			#endregion

			public override string ToString()
			{
				return string.Format("EmbeddedSetter({0})", _clazz.FullName);
			}
		}

	}
}

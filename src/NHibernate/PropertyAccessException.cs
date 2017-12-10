using System;
using System.Runtime.Serialization;
using System.Security;
using NHibernate.Util;

namespace NHibernate
{
	/// <summary>
	/// A problem occurred accessing a property of an instance of a persistent class by reflection
	/// </summary>
	[Serializable]
	public class PropertyAccessException : HibernateException, ISerializable
	{
		private readonly SerializableSystemType _persistentType;
		private readonly string _propertyName;
		private readonly bool _wasSetter;

		/// <summary>
		/// Initializes a new instance of the <see cref="PropertyAccessException"/> class.
		/// </summary>
		/// <param name="message">The message that describes the error. </param>
		/// <param name="innerException">
		/// The exception that is the cause of the current exception. If the innerException parameter 
		/// is not a null reference, the current exception is raised in a catch block that handles 
		/// the inner exception.
		/// </param>
		/// <param name="wasSetter">A <see cref="Boolean"/> indicating if this was a "setter" operation.</param>
		/// <param name="persistentType">The <see cref="System.Type"/> that NHibernate was trying find the Property or Field in.</param>
		/// <param name="propertyName">The mapped property name that was trying to be accessed.</param>
		public PropertyAccessException(Exception innerException, string message, bool wasSetter, System.Type persistentType,
									   string propertyName)
			: base(message, innerException)
		{
			_persistentType = persistentType;
			_wasSetter = wasSetter;
			_propertyName = propertyName;
		}

		public PropertyAccessException(Exception innerException, string message, bool wasSetter, System.Type persistentType)
			: base(message, innerException)
		{
			_persistentType = persistentType;
			_wasSetter = wasSetter;
		}

		/// <summary>
		/// Gets the <see cref="System.Type"/> that NHibernate was trying find the Property or Field in.
		/// </summary>
		public System.Type PersistentType => _persistentType?.TryGetSystemType();

		/// <summary>
		/// Gets a message that describes the current <see cref="PropertyAccessException"/>.
		/// </summary>
		/// <value>
		/// The error message that explains the reason for this exception and 
		/// information about the mapped property and its usage.
		/// </value>
		public override string Message
		{
			get
			{
				return base.Message + (_wasSetter ? " setter of " : " getter of ") +
					   (_persistentType == null ? "UnknownType" : _persistentType.FullName) +
					   (string.IsNullOrEmpty(_propertyName) ? string.Empty: "." + _propertyName);
			}
		}

		#region ISerializable Members

		/// <summary>
		/// Initializes a new instance of the <see cref="PropertyAccessException"/> class
		/// with serialized data.
		/// </summary>
		/// <param name="info">
		/// The <see cref="SerializationInfo"/> that holds the serialized object 
		/// data about the exception being thrown.
		/// </param>
		/// <param name="context">
		/// The <see cref="StreamingContext"/> that contains contextual information about the source or destination.
		/// </param>
		protected PropertyAccessException(SerializationInfo info, StreamingContext context) : base(info, context)
		{
			_propertyName = info.GetString("propertyName");
			_wasSetter = info.GetBoolean("wasSetter");

			foreach (SerializationEntry entry in info)
			{
				switch (entry.Name)
				{
					// TODO 6.0: remove "persistentType" deserialization
					case "persistentType":
						_persistentType = (System.Type) entry.Value;
						break;
					case "_persistentType":
						_persistentType = (SerializableSystemType) entry.Value;
						break;
				}
			}
		}

		/// <summary>
		/// Sets the serialization info for <see cref="PropertyAccessException"/> after 
		/// getting the info from the base Exception.
		/// </summary>
		/// <param name="info">
		/// The <see cref="SerializationInfo"/> that holds the serialized object 
		/// data about the exception being thrown.
		/// </param>
		/// <param name="context">
		/// The <see cref="StreamingContext"/> that contains contextual information about the source or destination.
		/// </param>
		[SecurityCritical]
		public override void GetObjectData(SerializationInfo info, StreamingContext context)
		{
			base.GetObjectData(info, context);
			info.AddValue("_persistentType", _persistentType);
			info.AddValue("propertyName", _propertyName);
			info.AddValue("wasSetter", _wasSetter);
		}

		#endregion
	}
}

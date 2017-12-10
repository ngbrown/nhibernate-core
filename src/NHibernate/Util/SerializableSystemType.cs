using System;
using System.Runtime.Serialization;
using System.Security;

namespace NHibernate.Util
{
	[Serializable]
	internal sealed class SerializableSystemType : ISerializable, IEquatable<SerializableSystemType>
	{
		[NonSerialized]
		private System.Type _type;

		private AssemblyQualifiedTypeName _typeName;

		/// <summary>
		/// Creates a new instance of <see cref="SerializableSystemType"/> if
		/// <paramref name="type"/> is not null, otherwise returns <c>null</c>.
		/// </summary>
		/// <param name="type">The <see cref="System.Type"/> being wrapped for serialization.</param>
		/// <returns>New instance of <see cref="SerializableSystemType"/> or <c>null</c>.</returns>
		public static SerializableSystemType Wrap(System.Type type)
		{
			return type == null ? null : new SerializableSystemType(type);
		}

		/// <summary>
		/// Creates a new <see cref="SerializableSystemType"/>
		/// </summary>
		/// <param name="type">The <see cref="System.Type"/> being wrapped for serialization.</param>
		private SerializableSystemType(System.Type type)
		{
			_type = type ?? throw new ArgumentNullException(nameof(type));
		}

		private SerializableSystemType(SerializationInfo info, StreamingContext context)
		{
			_typeName = info.GetValue<AssemblyQualifiedTypeName>("_typeName");
			if (_typeName == null)
				throw new InvalidOperationException("_typeName was null after deserialization");
			_type = _typeName.TypeFromAssembly(false);
		}

		/// <summary>
		/// Returns the wrapped type. Will throw if it was unable to load it after deserialization.
		/// </summary>
		/// <returns>The type that this class was initialized with or initialized after deserialization.</returns>
		public System.Type GetSystemType() => _type ?? throw new TypeLoadException("Could not load type " + _typeName + ".");

		/// <summary>
		/// Returns the wrapped type. Will return null if it was unable to load it after deserialization.
		/// </summary>
		/// <returns>The type that this class was initialized with, the type initialized after deserialization, or null if unable to load.</returns>
		public System.Type TryGetSystemType() => _type;

		public string FullName => _type?.FullName ?? _typeName.Type;

		public string AssemblyQualifiedName => _type?.AssemblyQualifiedName ?? _typeName.ToString();

		[SecurityCritical]
		public void GetObjectData(SerializationInfo info, StreamingContext context)
		{
			if (_typeName == null)
			{
				_typeName = new AssemblyQualifiedTypeName(_type.FullName, _type.Assembly.FullName);
			}

			info.AddValue("_typeName", _typeName);
		}

		public bool Equals(SerializableSystemType other)
		{
			return other != null &&
				(_type == null || other._type == null
					? Equals(_typeName, other._typeName)
					: Equals(_type, other._type));
		}

		public override bool Equals(object obj)
		{
			if (ReferenceEquals(null, obj)) return false;
			if (ReferenceEquals(this, obj)) return true;
			return obj is SerializableSystemType type && Equals(type);
		}

		public override int GetHashCode()
		{
			unchecked
			{
				return (FullName.GetHashCode() * 397) ^ (AssemblyQualifiedName?.GetHashCode() ?? 0);
			}
		}

		public static bool operator ==(SerializableSystemType left, SerializableSystemType right)
		{
			return Equals(left, right);
		}

		public static bool operator !=(SerializableSystemType left, SerializableSystemType right)
		{
			return !Equals(left, right);
		}

		public static explicit operator System.Type(SerializableSystemType serializableType)
		{
			return serializableType?.GetSystemType();
		}

		public static implicit operator SerializableSystemType(System.Type type)
		{
			return Wrap(type);
		}
	}
}

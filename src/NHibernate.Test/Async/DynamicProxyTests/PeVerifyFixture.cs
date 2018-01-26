﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by AsyncGenerator.
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------


using System;
using System.IO;
using System.Reflection;
using System.Reflection.Emit;
using NUnit.Framework;
using NHibernate.Proxy.DynamicProxy;

namespace NHibernate.Test.DynamicProxyTests
{
	using System.Threading.Tasks;
	[TestFixture]
	public class PeVerifyFixtureAsync
	{
		private static bool wasCalled;

		private const string assemblyName = "peVerifyAssembly";
		private const string assemblyFileName = "peVerifyAssembly.dll";

		[Test]
#if NETCOREAPP2_0
		[Ignore("This platform does not support saving dynamic assemblies.")]
#endif
		public Task VerifyClassWithPublicConstructorAsync()
		{
			try
			{
				var factory = new ProxyFactory(new SavingProxyAssemblyBuilder(assemblyName));
				var proxyType = factory.CreateProxyType(typeof(ClassWithPublicDefaultConstructor), null);

				wasCalled = false;
				Activator.CreateInstance(proxyType);

				Assert.That(wasCalled);
				return new PeVerifier(assemblyFileName).AssertIsValidAsync();
			}
			catch (Exception ex)
			{
				return Task.FromException<object>(ex);
			}
		}

		[Test]
#if NETCOREAPP2_0
		[Ignore("This platform does not support saving dynamic assemblies.")]
#endif
		public Task VerifyClassWithProtectedConstructorAsync()
		{
			try
			{
				var factory = new ProxyFactory(new SavingProxyAssemblyBuilder(assemblyName));
				var proxyType = factory.CreateProxyType(typeof(ClassWithProtectedDefaultConstructor), null);

				wasCalled = false;
				Activator.CreateInstance(proxyType);

				Assert.That(wasCalled);
				return new PeVerifier(assemblyFileName).AssertIsValidAsync();
			}
			catch (Exception ex)
			{
				return Task.FromException<object>(ex);
			}
		}

		#region PeVerifyTypes

		public class ClassWithPublicDefaultConstructor
		{
			public ClassWithPublicDefaultConstructor() { InitG<int>(1); }
			public ClassWithPublicDefaultConstructor(int unused) { }
			public virtual int Prop1 { get; set; }
			public virtual void InitG<T>(T value) { Init((int)(object)value); }
			public virtual void Init(int value) { Prop1 = value; if (Prop1 == 1) wasCalled = true; }
		}

		public class ClassWithProtectedDefaultConstructor
		{
			protected ClassWithProtectedDefaultConstructor() { wasCalled = true; }
		}

		public class ClassWithPrivateDefaultConstructor
		{
			private ClassWithPrivateDefaultConstructor() { wasCalled = true; }
		}

		public class ClassWithNoDefaultConstructor
		{
			public ClassWithNoDefaultConstructor(int unused) { wasCalled = true; }
			public ClassWithNoDefaultConstructor(string unused) { wasCalled = true; }
		}

		public class ClassWithInternalConstructor
		{
			internal ClassWithInternalConstructor() { wasCalled = true; }
		}

		#endregion

		#region ProxyFactory.IProxyAssemblyBuilder

		public class SavingProxyAssemblyBuilder : IProxyAssemblyBuilder
		{
			private string assemblyName;

			public SavingProxyAssemblyBuilder(string assemblyName)
			{
				this.assemblyName = assemblyName;
			}

			public AssemblyBuilder DefineDynamicAssembly(AppDomain appDomain, AssemblyName name)
			{
#if NETCOREAPP2_0
				throw new NotSupportedException("AppDomain.DefineDynamicModule not supported on this platform.");
#else
				AssemblyBuilderAccess access = AssemblyBuilderAccess.RunAndSave;
				return appDomain.DefineDynamicAssembly(new AssemblyName(assemblyName), access, TestContext.CurrentContext.TestDirectory);
#endif
			}

			public ModuleBuilder DefineDynamicModule(AssemblyBuilder assemblyBuilder, string moduleName)
			{
#if NETCOREAPP2_0
				throw new NotSupportedException("AssemblyBuilder.DefineDynamicModule not supported on this platform.");
#else
				return assemblyBuilder.DefineDynamicModule(moduleName, string.Format("{0}.mod", assemblyName), true);
#endif
			}

			public void Save(AssemblyBuilder assemblyBuilder)
			{
#if NETCOREAPP2_0
				throw new NotSupportedException("AssemblyBuilder.Save not supported on this platform.");
#else
				assemblyBuilder.Save(assemblyName + ".dll");
#endif
			}
		}

		#endregion
	}
}

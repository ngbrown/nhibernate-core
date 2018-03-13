﻿using System;
using System.Data.Common;
using System.Text.RegularExpressions;
using NHibernate.Cfg;
using NHibernate.Dialect;
using NHibernate.Driver;
using NHibernate.Util;
using NUnit.Framework;
using Environment = NHibernate.Cfg.Environment;

namespace NHibernate.Test.NHSpecificTest.NH3202
{
	[TestFixture]
	[Obsolete("Uses old driver")]
	public class FixtureObsolete : BugTestCase
	{
		protected override void Configure(Configuration configuration)
		{
			if (!(Dialect is MsSql2008Dialect))
				Assert.Ignore("Test is for MS SQL Server dialect only (custom dialect).");

			if (!ReflectHelper.ClassForName(cfg.GetProperty(Environment.ConnectionDriver)).IsSqlClientDriver())
				Assert.Ignore("Test is for MS SQL Server driver only (custom driver is used).");

			cfg.SetProperty(Environment.Dialect, typeof(OffsetStartsAtOneTestDialect).AssemblyQualifiedName);
			cfg.SetProperty(Environment.ConnectionDriver, typeof(OffsetTestObsoleteDriver).AssemblyQualifiedName);
		}

		private OffsetStartsAtOneTestDialect OffsetStartsAtOneTestDialect
		{
			get { return (OffsetStartsAtOneTestDialect)Sfi.Dialect; }
		}

		private OffsetTestObsoleteDriver CustomDriver
		{
			get { return (OffsetTestObsoleteDriver)Sfi.ConnectionProvider.Driver; }
		}

		protected override void OnSetUp()
		{
			CustomDriver.OffsetStartsAtOneTestDialect = OffsetStartsAtOneTestDialect;

			base.OnSetUp();

			using (var session = OpenSession())
			using (var transaction = session.BeginTransaction())
			{
				session.Save(new SequencedItem(1));
				session.Save(new SequencedItem(2));
				session.Save(new SequencedItem(3));

				session.Flush();
				transaction.Commit();
			}
		}

		protected override void OnTearDown()
		{
			using (ISession s = OpenSession())
			using (ITransaction t = s.BeginTransaction())
			{
				s.Delete("from SequencedItem");
				t.Commit();
			}
			base.OnTearDown();
		}


		[Test]
		public void OffsetNotStartingAtOneSetsParameterToSkipValue()
		{
			OffsetStartsAtOneTestDialect.ForceOffsetStartsAtOne = false;

			using (var session = OpenSession())
			{
				var item2 =
				session.QueryOver<SequencedItem>()
					.OrderBy(i => i.I).Asc
					.Take(1).Skip(2)
					.SingleOrDefault<SequencedItem>();

				Assert.That(CustomDriver.OffsetParameterValueFromCommand, Is.EqualTo(2));
			}
		}

		[Test]
		public void OffsetStartingAtOneSetsParameterToSkipValuePlusOne()
		{
			OffsetStartsAtOneTestDialect.ForceOffsetStartsAtOne = true;

			using (var session = OpenSession())
			{
				var item2 =
				session.QueryOver<SequencedItem>()
					.OrderBy(i => i.I).Asc
					.Take(1).Skip(2)
					.SingleOrDefault<SequencedItem>();

				Assert.That(CustomDriver.OffsetParameterValueFromCommand, Is.EqualTo(3));
			}
		}
	}


	[Obsolete("Uses old driver")]
	public class OffsetTestObsoleteDriver : SqlClientDriver
	{
		public OffsetStartsAtOneTestDialect OffsetStartsAtOneTestDialect;
		private int _offsetParameterIndex = 1;

		public int? OffsetParameterValueFromCommand { get; private set; }

		protected override void OnBeforePrepare(DbCommand command)
		{
			base.OnBeforePrepare(command);
			OffsetParameterValueFromCommand = null;

			bool hasLimit = new Regex(@"select\s+top").IsMatch(command.CommandText.ToLower());
			if (!hasLimit)
				return;

			OffsetParameterValueFromCommand = (int)command.Parameters[_offsetParameterIndex].Value;
		}
	}
}

// This file is part of the re-motion Core Framework (www.re-motion.org)
// Copyright (c) rubicon IT GmbH, www.rubicon.eu
// 
// The re-motion Core Framework is free software; you can redistribute it 
// and/or modify it under the terms of the GNU Lesser General Public License 
// as published by the Free Software Foundation; either version 2.1 of the 
// License, or (at your option) any later version.
// 
// re-motion is distributed in the hope that it will be useful, 
// but WITHOUT ANY WARRANTY; without even the implied warranty of 
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the 
// GNU Lesser General Public License for more details.
// 
// You should have received a copy of the GNU Lesser General Public License
// along with re-motion; if not, see http://www.gnu.org/licenses.
// 
using System;
using NUnit.Framework;
using Remotion.Development.UnitTesting.NUnit;
using Remotion.Utilities;

namespace Remotion.UnitTests.Utilities.ArgumentUtilityTests
{
  public enum TestEnum
  {
    Value1 = 1,
    Value2 = 2,
    Value3 = 3
  }

  [Flags]
  public enum TestFlags
  {
    Flag1 = 1,
    Flag2 = 2,
    Flag3 = 4,
    Flag13 = Flag1 | Flag3
  }

	[TestFixture]
	public class CheckValidEnumValue
	{
		[Test]
		public void Fail_UndefinedValue ()
		{
		  Assert.That(
		      () => ArgumentUtility.CheckValidEnumValue("arg", (TestEnum)4),
		      Throws.InstanceOf<ArgumentOutOfRangeException>());
		}

		[Test]
		public void Fail_PartiallyUndefinedFlags ()
		{
		  Assert.That(
		      () => ArgumentUtility.CheckValidEnumValue("arg", (TestFlags)(1 | 8)),
		      Throws.InstanceOf<ArgumentOutOfRangeException>());
		}

    [Test]
		public void Succeed_SingleValue ()
    {
      Enum result = ArgumentUtility.CheckValidEnumValue("arg", TestEnum.Value1);
      Assert.That(result, Is.EqualTo(TestEnum.Value1));
    }

	  [Test]
		public void Succeed_Flags ()
	  {
      Enum result = ArgumentUtility.CheckValidEnumValue("arg", TestFlags.Flag1 | TestFlags.Flag2);
      Assert.That(result, Is.EqualTo(TestFlags.Flag1 | TestFlags.Flag2));
	  }
	}

	[TestFixture]
	public class CheckValidEnumValueAndTypeAndNotNull
	{
		[Test]
		public void Fail_Null ()
		{
		  Assert.That(
		      () => ArgumentUtility.CheckValidEnumValueAndTypeAndNotNull<TestEnum>("arg", null),
		      Throws.InstanceOf<ArgumentNullException>());
		}

    [Test]
		public void Fail_UndefinedValue ()
		{
		  Assert.That(
		      () => ArgumentUtility.CheckValidEnumValueAndTypeAndNotNull<TestEnum>("arg", (TestEnum)4),
		      Throws.InstanceOf<ArgumentOutOfRangeException>());
		}

		[Test]
		public void Fail_PartiallyUndefinedFlags ()
		{
		  Assert.That(
		      () => ArgumentUtility.CheckValidEnumValueAndTypeAndNotNull<TestFlags>("arg", (TestFlags)(1 | 8)),
		      Throws.InstanceOf<ArgumentOutOfRangeException>());
		}

	  [Test]
	  public void Fail_WrongType ()
	  {
	    Assert.That(
	        () => ArgumentUtility.CheckValidEnumValueAndTypeAndNotNull<TestFlags>("arg", TestEnum.Value1),
	        Throws.ArgumentException
	            .With.ArgumentExceptionMessageEqualTo(
	                "Parameter 'arg' has type 'Remotion.UnitTests.Utilities.ArgumentUtilityTests.TestEnum' "
	                + "when type 'Remotion.UnitTests.Utilities.ArgumentUtilityTests.TestFlags' was expected.", "arg"));
	  }

	  [Test]
		public void Succeed_SingleValue ()
		{
      TestEnum result = ArgumentUtility.CheckValidEnumValueAndTypeAndNotNull<TestEnum>("arg", TestEnum.Value1);
      Assert.That(result, Is.EqualTo(TestEnum.Value1));
		}
		[Test]
		public void Succeed_Flags ()
		{
      TestFlags result = ArgumentUtility.CheckValidEnumValueAndTypeAndNotNull<TestFlags>("arg", TestFlags.Flag1 | TestFlags.Flag2);
		  Assert.That(result, Is.EqualTo(TestFlags.Flag1 | TestFlags.Flag2));
		}
	}

	[TestFixture]
	public class CheckValidEnumValueAndType
	{
		[Test]
		public void Succeed_Null ()
		{
      TestEnum? result = ArgumentUtility.CheckValidEnumValueAndType<TestEnum>("arg", null);
		  Assert.That(result, Is.Null);
		}

    [Test]
		public void Fail_UndefinedValue ()
		{
		  Assert.That(
		      () => ArgumentUtility.CheckValidEnumValueAndType<TestEnum>("arg", (TestEnum)4),
		      Throws.InstanceOf<ArgumentOutOfRangeException>());
		}

		[Test]
		public void Fail_PartiallyUndefinedFlags ()
		{
		  Assert.That(
		      () => ArgumentUtility.CheckValidEnumValueAndType<TestFlags>("arg", (TestFlags)(1 | 8)),
		      Throws.InstanceOf<ArgumentOutOfRangeException>());
		}

	  [Test]
	  public void Fail_WrongType ()
	  {
	    Assert.That(
	        () => ArgumentUtility.CheckValidEnumValueAndType<TestFlags>("arg", TestEnum.Value1),
	        Throws.ArgumentException
	            .With.ArgumentExceptionMessageEqualTo(
	                "Parameter 'arg' has type 'Remotion.UnitTests.Utilities.ArgumentUtilityTests.TestEnum' "
	                + "when type 'Remotion.UnitTests.Utilities.ArgumentUtilityTests.TestFlags' was expected.", "arg"));
	  }

	  [Test]
		public void Succeed_SingleValue ()
		{
      TestEnum? result = ArgumentUtility.CheckValidEnumValueAndType<TestEnum>("arg", TestEnum.Value1);
      Assert.That(result, Is.EqualTo(TestEnum.Value1));
		}
		[Test]
		public void Succeed_Flags ()
		{
      TestFlags? result = ArgumentUtility.CheckValidEnumValueAndType<TestFlags>("arg", TestFlags.Flag1 | TestFlags.Flag2);
		  Assert.That(result, Is.EqualTo(TestFlags.Flag1 | TestFlags.Flag2));
		}
	}
}

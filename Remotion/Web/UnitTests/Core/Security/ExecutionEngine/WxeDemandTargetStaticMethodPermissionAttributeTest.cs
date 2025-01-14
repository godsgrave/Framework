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
using Remotion.Web.Security.ExecutionEngine;
using Remotion.Web.UnitTests.Core.Security.Domain;

namespace Remotion.Web.UnitTests.Core.Security.ExecutionEngine
{
  [TestFixture]
  public class WxeDemandTargetStaticMethodPermissionAttributeTest
  {
    [Test]
    public void Initialize_WithMethodNameAndSecurableClass ()
    {
      WxeDemandTargetStaticMethodPermissionAttribute attribute = new WxeDemandTargetStaticMethodPermissionAttribute("Search", typeof(SecurableObject));

      Assert.That(attribute.MethodType, Is.EqualTo(MethodType.Static));
      Assert.That(attribute.MethodName, Is.EqualTo("Search"));
      Assert.That(attribute.SecurableClass, Is.SameAs(typeof(SecurableObject)));
    }

    [Test]
    public void Initialize_WithMethodNameEnum ()
    {
      WxeDemandTargetStaticMethodPermissionAttribute attribute = new WxeDemandTargetStaticMethodPermissionAttribute(SecurableObject.Method.Search);

      Assert.That(attribute.MethodType, Is.EqualTo(MethodType.Static));
      Assert.That(attribute.MethodName, Is.EqualTo("Search"));
      Assert.That(attribute.SecurableClass, Is.SameAs(typeof(SecurableObject)));
    }

    [Test]
    public void Initialize_WithMethodNameEnumAndSecurableClass ()
    {
      WxeDemandTargetStaticMethodPermissionAttribute attribute =
          new WxeDemandTargetStaticMethodPermissionAttribute(SecurableObject.Method.Search, typeof(DerivedSecurableObject));

      Assert.That(attribute.MethodType, Is.EqualTo(MethodType.Static));
      Assert.That(attribute.MethodName, Is.EqualTo("Search"));
      Assert.That(attribute.SecurableClass, Is.SameAs(typeof(DerivedSecurableObject)));
    }

    [Test]
    public void Initialize_WithMethodNameEnumNotNestedType ()
    {
      Assert.That(
          () => new WxeDemandTargetStaticMethodPermissionAttribute(MethodNameEnum.Show),
          Throws.ArgumentException
              .With.ArgumentExceptionMessageEqualTo(
                  "Enumerated type 'Remotion.Web.UnitTests.Core.Security.Domain.MethodNameEnum' is not declared as a nested type.", "methodNameEnum"));
    }

    [Test]
    public void Initialize_WithMethodNameEnumNotHavingValidDeclaringType ()
    {
      Assert.That(
          () => new WxeDemandTargetStaticMethodPermissionAttribute(SimpleType.MethodNameEnum.Show),
          Throws.ArgumentException
              .With.ArgumentExceptionMessageEqualTo(
                  "The declaring type of enumerated type 'Remotion.Web.UnitTests.Core.Security.Domain.SimpleType+MethodNameEnum' does not implement interface"
                  + " 'Remotion.Security.ISecurableObject'.", "methodNameEnum"));
    }

    [Test]
    public void Initialize_WithMethodNameEnumNotNestedTypeAndSecurableClass ()
    {
      Assert.That(
          () => new WxeDemandTargetStaticMethodPermissionAttribute(MethodNameEnum.Show, typeof(DerivedSecurableObject)),
          Throws.ArgumentException
              .With.ArgumentExceptionMessageEqualTo(
                  "Enumerated type 'Remotion.Web.UnitTests.Core.Security.Domain.MethodNameEnum' is not declared as a nested type.", "methodNameEnum"));
    }

    [Test]
    public void Initialize_WithMethodNameEnumNotHavingValidDeclaringTypeAndSecurableClass ()
    {
      Assert.That(
          () => new WxeDemandTargetStaticMethodPermissionAttribute(SimpleType.MethodNameEnum.Show, typeof(DerivedSecurableObject)),
          Throws.ArgumentException
              .With.ArgumentExceptionMessageEqualTo(
                  "The declaring type of enumerated type 'Remotion.Web.UnitTests.Core.Security.Domain.SimpleType+MethodNameEnum' does not implement interface"
                  + " 'Remotion.Security.ISecurableObject'.", "methodNameEnum"));
    }

    [Test]
    public void TestWithParameterNotOfNotMatchingType ()
    {
      Assert.That(
          () => new WxeDemandTargetStaticMethodPermissionAttribute(SecurableObject.Method.Show, typeof(OtherSecurableObject)),
          Throws.ArgumentException
              .With.ArgumentExceptionMessageEqualTo(
                  "Type 'Remotion.Web.UnitTests.Core.Security.Domain.OtherSecurableObject' cannot be assigned to the declaring type of enumerated type"
                  + " 'Remotion.Web.UnitTests.Core.Security.Domain.SecurableObject+Method'.", "securableClass"));
    }
  }
}

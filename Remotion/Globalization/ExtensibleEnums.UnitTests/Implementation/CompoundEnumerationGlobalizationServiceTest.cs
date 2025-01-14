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
using Moq;
using NUnit.Framework;
using Remotion.ExtensibleEnums;
using Remotion.Globalization.ExtensibleEnums.Implementation;
using Remotion.Globalization.ExtensibleEnums.UnitTests.TestDomain;

namespace Remotion.Globalization.ExtensibleEnums.UnitTests.Implementation
{
  [TestFixture]
  public class CompoundExtensibleEnumGlobalizationServiceTest
  {
    private CompoundExtensibleEnumGlobalizationService _service;
    private Mock<IExtensibleEnumGlobalizationService> _innerService1;
    private Mock<IExtensibleEnumGlobalizationService> _innerService2;
    private Mock<IExtensibleEnumGlobalizationService> _innerService3;

    [SetUp]
    public void SetUp ()
    {
      _innerService1 = new Mock<IExtensibleEnumGlobalizationService>(MockBehavior.Strict);
      _innerService2 = new Mock<IExtensibleEnumGlobalizationService>(MockBehavior.Strict);
      _innerService3 = new Mock<IExtensibleEnumGlobalizationService>(MockBehavior.Strict);

      _service = new CompoundExtensibleEnumGlobalizationService(new[] { _innerService1.Object, _innerService2.Object, _innerService3.Object });
    }

    [Test]
    public void Initialization ()
    {
      Assert.That(_service.ExtensibleEnumGlobalizationServices, Is.EqualTo(new[] { _innerService1.Object, _innerService2.Object, _innerService3.Object }));
    }

    [Test]
    public void TryGetTypeDisplayName_WithInnerServiceHavingResult_ReturnsResult ()
    {
      var enumValue = Color.Values.Red();
      string nullOutValue = null;
      var theNameOutValue = "The Name";

      var sequence = new MockSequence();
      _innerService1
          .InSequence(sequence)
          .Setup(
            mock => mock.TryGetExtensibleEnumValueDisplayName(
                enumValue,
                out nullOutValue))
          .Returns(false)
          .Verifiable();
      _innerService2
          .InSequence(sequence)
          .Setup(
            mock => mock.TryGetExtensibleEnumValueDisplayName(
                enumValue,
                out theNameOutValue))
          .Returns(true)
          .Verifiable();
      _innerService3
          .InSequence(sequence)
          .Setup(
            mock => mock.TryGetExtensibleEnumValueDisplayName(
                It.IsAny<IExtensibleEnum>(),
                out nullOutValue))
          .Verifiable();

      string value;
      var result = _service.TryGetExtensibleEnumValueDisplayName(enumValue, out value);

      Assert.That(result, Is.True);
      Assert.That(value, Is.EqualTo("The Name"));

      _innerService1.Verify();
      _innerService2.Verify();
      _innerService3.Verify(
          mock => mock.TryGetExtensibleEnumValueDisplayName(
                It.IsAny<IExtensibleEnum>(),
                out nullOutValue),
          Times.Never());
    }

    [Test]
    public void TryGetTypeDisplayName_WithoutInnerServiceHavingResult_ReturnsNull ()
    {
      var enumValue = Color.Values.Red();
      string outValue = null;

      var sequence = new MockSequence();
      _innerService1
          .InSequence(sequence)
          .Setup(
            mock => mock.TryGetExtensibleEnumValueDisplayName(
                enumValue,
                out outValue))
          .Returns(false)
          .Verifiable();
      _innerService2
          .InSequence(sequence)
          .Setup(
            mock => mock.TryGetExtensibleEnumValueDisplayName(
                enumValue,
                out outValue))
          .Returns(false)
          .Verifiable();
      _innerService3
          .InSequence(sequence)
          .Setup(
            mock => mock.TryGetExtensibleEnumValueDisplayName(
                enumValue,
                out outValue))
          .Returns(false)
          .Verifiable();

      string value;
      var result = _service.TryGetExtensibleEnumValueDisplayName(enumValue, out value);

      Assert.That(result, Is.False);
      Assert.That(value, Is.Null);

      _innerService1.Verify();
      _innerService2.Verify();
      _innerService3.Verify();
    }
  }
}

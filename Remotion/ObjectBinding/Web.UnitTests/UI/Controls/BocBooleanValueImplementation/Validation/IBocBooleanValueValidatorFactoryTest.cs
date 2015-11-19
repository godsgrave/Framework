﻿using System;
using NUnit.Framework;
using Remotion.ObjectBinding.Web.UI.Controls;
using Remotion.ObjectBinding.Web.UI.Controls.BocBooleanValueImplementation;
using Remotion.ObjectBinding.Web.UI.Controls.BocBooleanValueImplementation.Validation;
using Remotion.ServiceLocation;

namespace Remotion.ObjectBinding.Web.UnitTests.UI.Controls.BocBooleanValueImplementation.Validation
{
  [TestFixture]
  public class IBocBooleanValueValidatorFactoryTest
  {
    private DefaultServiceLocator _serviceLocator;

    [SetUp]
    public void SetUp ()
    {
      _serviceLocator = DefaultServiceLocator.Create();
    }

    [Test]
    public void GetInstance_Once ()
    {
      var instance = _serviceLocator.GetInstance<IBocBooleanValueValidatorFactory>();

      Assert.That (instance, Is.InstanceOf<CompoundValidatorFactory<IBocBooleanValue>>());
    }

    [Test]
    public void GetInstance_Twice_ReturnsSameInstance ()
    {
      var instance1 = _serviceLocator.GetInstance<IBocBooleanValueValidatorFactory>();
      var instance2 = _serviceLocator.GetInstance<IBocBooleanValueValidatorFactory>();

      Assert.That (instance1, Is.InstanceOf<CompoundValidatorFactory<IBocBooleanValue>>());
      Assert.That (instance1, Is.SameAs (instance2));
    }
  }
}
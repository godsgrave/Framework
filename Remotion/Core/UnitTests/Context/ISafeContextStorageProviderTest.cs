﻿// This file is part of the re-motion Core Framework (www.re-motion.org)
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
using System.Linq;
using NUnit.Framework;
using Remotion.Context;
using Remotion.ServiceLocation;

namespace Remotion.UnitTests.Context
{
  [TestFixture]
  public class ISafeContextStorageProviderTest
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
      var instance = _serviceLocator.GetInstance<ISafeContextStorageProvider>();

#if NETFRAMEWORK
      Assert.That(instance, Is.InstanceOf(typeof(CallContextStorageProvider)));
#else
// Ignore the obsolete warnings for AsyncLocalStorageProvider
#pragma warning disable 618
      Assert.That(instance, Is.InstanceOf(typeof(AsyncLocalStorageProvider)));
#pragma warning restore 618
#endif
    }

    [Test]
    public void GetInstance_Twice_ReturnsSameInstance ()
    {
      var instance1 = _serviceLocator.GetInstance<ISafeContextStorageProvider>();
      var instance2 = _serviceLocator.GetInstance<ISafeContextStorageProvider>();

      Assert.That(instance1, Is.SameAs(instance2));
    }
  }
}

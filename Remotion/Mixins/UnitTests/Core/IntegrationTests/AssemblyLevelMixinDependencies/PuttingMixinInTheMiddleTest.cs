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
using System;
using NUnit.Framework;

namespace Remotion.Mixins.UnitTests.Core.IntegrationTests.AssemblyLevelMixinDependencies
{
  [TestFixture]
  public class PuttingMixinInTheMiddleTest : AssemblyLevelMixinDependenciesTestBase
  {
    [Test]
    public void AssemblyLevelAttribute_CanBeUsedToPutAMixinInTheMiddle ()
    {
      PrepareMixinConfigurationWithAttributeDeclarations(
          new AdditionalMixinDependencyAttribute(typeof (C), typeof (M1), typeof (M3)),
          new AdditionalMixinDependencyAttribute(typeof (C), typeof (M3), typeof (M2)));

      var instance = ObjectFactory.Create<C>();

      var result = instance.M();

      Assert.That(result, Is.EqualTo("M1 M3 M2 C"));
    }

    public class C : IC
    {
      public virtual string M ()
      {
        return "C";
      }
    }

    public interface IC
    {
      string M ();
    }

    [Extends (typeof (C), AdditionalDependencies = new[] { typeof (M2) })]
    public class M1 : Mixin<C, IC>
    {
      [OverrideTarget]
      public string M ()
      {
        return "M1 " + Next.M();
      }
    }

    [Extends (typeof (C))]
    public class M2 : Mixin<C, IC>
    {
      [OverrideTarget]
      public string M ()
      {
        return "M2 " + Next.M();
      }
    }

    [Extends (typeof (C))]
    public class M3 : Mixin<C, IC>
    {
      [OverrideTarget]
      public string M ()
      {
        return "M3 " + Next.M();
      }
    }
  }
}
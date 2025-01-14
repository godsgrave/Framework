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
using System.Reflection;
using NUnit.Framework;
using Remotion.Development.UnitTesting;
using Remotion.Development.UnitTesting.NUnit;
using Remotion.Web.ExecutionEngine;
using Remotion.Web.UnitTests.Core.ExecutionEngine.TestFunctions;

namespace Remotion.Web.UnitTests.Core.ExecutionEngine
{

[TestFixture]
public class WxeMethodStepTest: WxeTest
{
  private TestFunction _function;
  private TestFunctionWithInvalidSteps _functionWithInvalidSteps;

  [SetUp]
  public override void SetUp ()
  {
    base.SetUp();

    _function = new TestFunction();
    _functionWithInvalidSteps = new TestFunctionWithInvalidSteps();
  }

  [Test]
  public void CheckCtorArgumentNotInstanceMember ()
  {
    Type functionType = typeof(TestFunctionWithInvalidSteps);
    MethodInfo step1 = functionType.GetMethod("InvalidStep1", BindingFlags.Static | BindingFlags.NonPublic);
    Assert.That(
        () => new WxeMethodStep(_functionWithInvalidSteps, step1),
        Throws.InstanceOf<WxeException>());
  }

  [Test]
  public void CheckCtorArgumentWrongParameterType ()
  {
    Type functionType = typeof(TestFunctionWithInvalidSteps);
    MethodInfo step2 = functionType.GetMethod("InvalidStep2", BindingFlags.Instance | BindingFlags.NonPublic);
    Assert.That(
        () => new WxeMethodStep(_functionWithInvalidSteps, step2),
        Throws.InstanceOf<WxeException>());
  }

  [Test]
  public void CheckCtorArgumentTooManyParameters ()
  {
    Type functionType = typeof(TestFunctionWithInvalidSteps);
    MethodInfo step3 = functionType.GetMethod("InvalidStep3", BindingFlags.Instance | BindingFlags.NonPublic);
    Assert.That(
        () => new WxeMethodStep(_functionWithInvalidSteps, step3),
        Throws.InstanceOf<WxeException>());
  }

  [Test]
  public void CheckCtorArgumentWrongStepListInstance ()
  {
    Type functionType = typeof(TestFunction);
    MethodInfo step1 = functionType.GetMethod("Step1", BindingFlags.Instance | BindingFlags.NonPublic);
    Assert.That(
        () => new WxeMethodStep(_functionWithInvalidSteps, step1),
        Throws.InstanceOf<WxeException>());
  }

  [Test]
  public void ExecuteMethodStep ()
  {
    Type functionType = typeof(TestFunction);
    MethodInfo step1 = functionType.GetMethod("Step1", BindingFlags.Instance | BindingFlags.NonPublic);
    WxeMethodStep methodStep = new WxeMethodStep(_function, step1);

    methodStep.Execute(CurrentWxeContext);

    Assert.That(_function.LastExecutedStepID, Is.EqualTo("1"));
  }

  [Test]
  public void MethodStepWithDelegate ()
  {
    WxeMethodStep methodStep = new WxeMethodStep(_function.PublicStepMethod);
    Assert.That(PrivateInvoke.GetNonPublicField(methodStep, "_methodName"), Is.EqualTo("PublicStepMethod"));
  }

  [Test]
  public void MethodStepWithDelegateWithContext ()
  {
    WxeMethodStep methodStep = new WxeMethodStep(_function.PublicStepMethodWithContext);
    Assert.That(PrivateInvoke.GetNonPublicField(methodStep, "_methodName"), Is.EqualTo("PublicStepMethodWithContext"));
  }

  [Test]
  public void MethodStepWithDelegateThrowsOnMultiDelegate ()
  {
    Action multiDelegate = _function.PublicStepMethod;
    multiDelegate += _function.PublicStepMethod;
    Assert.That(
        () => new WxeMethodStep(multiDelegate),
        Throws.ArgumentException
            .With.Message.Contains("The delegate must contain a single method."));
  }

  [Test]
  public void MethodStepWithDelegateThrowsOnInvalidTarget ()
  {
    Action action = InstanceMethodNotDeclaredOnWxeFunction;
    Assert.That(action.Target, Is.Not.Null.And.Not.AssignableTo<WxeStepList>());
    Assert.That(
        () => new WxeMethodStep(action),
        Throws.ArgumentException
            .With.ArgumentExceptionMessageEqualTo(
                "The delegate's target must be a non-null WxeStepList, but it was 'Remotion.Web.UnitTests.Core.ExecutionEngine.WxeMethodStepTest'. When used "
                + "within a WxeFunction, the delegate should be a method of the surrounding WxeFunction, and it must not be a closure.", "method"));
  }

  [Test]
  public void MethodStepWithDelegateThrowsOnNullTarget ()
  {
    Action action = StaticMethodNotDeclaredOnWxeFunction;
    Assert.That(action.Target, Is.Null);
    Assert.That(
        () => new WxeMethodStep(action),
        Throws.ArgumentException
            .With.ArgumentExceptionMessageEqualTo(
                "The delegate's target must be a non-null WxeStepList, but it was 'null'. When used "
                + "within a WxeFunction, the delegate should be a method of the surrounding WxeFunction, and it must not be a closure.", "method"));
  }

  [Test]
  public void ExecuteMethodStepWithDelegate ()
  {
    WxeMethodStep methodStep = new WxeMethodStep(_function.PublicStepMethod);

    Assert.That(_function.LastExecutedStepID, Is.Not.EqualTo("1"));

    methodStep.Execute(CurrentWxeContext);

    Assert.That(_function.LastExecutedStepID, Is.EqualTo("1"));
  }

  [Test]
  public void ExecuteMethodStepWithContext ()
  {
    Type functionType = typeof(TestFunction);
    MethodInfo step2 = functionType.GetMethod("Step2", BindingFlags.Instance | BindingFlags.NonPublic);
    WxeMethodStep methodStepWithContext = new WxeMethodStep(_function, step2);

    methodStepWithContext.Execute(CurrentWxeContext);

    Assert.That(_function.LastExecutedStepID, Is.EqualTo("2"));
    Assert.That(_function.WxeContextStep2, Is.SameAs(CurrentWxeContext));
  }

  [Test]
  public void ExecuteMethodStepWithDelegateWithContext ()
  {
    WxeMethodStep methodStepWithContext = new WxeMethodStep(_function.PublicStepMethodWithContext);

    methodStepWithContext.Execute(CurrentWxeContext);

    Assert.That(_function.LastExecutedStepID, Is.EqualTo("2"));
    Assert.That(_function.WxeContextStep2, Is.SameAs(CurrentWxeContext));
  }

  [Test]
  public void IsDirtyStateEnabled_PassesCallToBaseImplementation_WithParentFunctionHasDirtyStateEnabled_ReturnsTrue ()
  {
    var methodStep = new WxeMethodStep(_function.PublicStepMethod);
    methodStep.SetParentStep(_function);

    Assert.That(_function.IsDirtyStateEnabled, Is.True);

    Assert.That(methodStep.IsDirtyStateEnabled, Is.True);
  }

  [Test]
  public void IsDirtyStateEnabled_PassesCallToBaseImplementation_WithParentFunctionHasDirtyStateDisabled_ReturnsFalse ()
  {
    var methodStep = new WxeMethodStep(_function.PublicStepMethod);
    methodStep.SetParentStep(_function);

    _function.DisableDirtyState();
    Assert.That(_function.IsDirtyStateEnabled, Is.False);

    Assert.That(methodStep.IsDirtyStateEnabled, Is.False);
  }

  private void InstanceMethodNotDeclaredOnWxeFunction ()
  {
  }

  private static void StaticMethodNotDeclaredOnWxeFunction ()
  {
  }
}

}

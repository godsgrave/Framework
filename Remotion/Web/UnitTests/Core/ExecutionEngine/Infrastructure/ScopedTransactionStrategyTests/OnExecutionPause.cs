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
using Remotion.Web.ExecutionEngine;
using Remotion.Web.ExecutionEngine.Infrastructure;

namespace Remotion.Web.UnitTests.Core.ExecutionEngine.Infrastructure.ScopedTransactionStrategyTests
{
  [TestFixture]
  public class OnExecutionPause : ScopedTransactionStrategyTestBase
  {
    private ScopedTransactionStrategyBase _strategy;

    public override void SetUp ()
    {
      base.SetUp();
      _strategy = CreateScopedTransactionStrategy(true, NullTransactionStrategy.Null).Object;
    }

    [Test]
    public void Test ()
    {
      InvokeOnExecutionPlay(_strategy);

      var sequence = new MockSequence();
      ChildTransactionStrategyMock.InSequence(sequence).Setup(mock => mock.OnExecutionPause(Context, ExecutionListenerStub.Object)).Verifiable();
      ScopeMock.InSequence(sequence).Setup(mock => mock.Leave()).Verifiable();

      _strategy.OnExecutionPause(Context, ExecutionListenerStub.Object);

      VerifyAll();
      Assert.That(_strategy.Scope, Is.Null);
    }

    [Test]
    public void Test_WithNullScope ()
    {
      Assert.That(_strategy.Scope, Is.Null);
      Assert.That(
          () => _strategy.OnExecutionPause(Context, ExecutionListenerStub.Object),
          Throws.InvalidOperationException
              .With.Message.EqualTo("OnExecutionPause may not be invoked unless OnExecutionPlay was called first."));
    }

    [Test]
    public void Test_ChildStrategyThrows ()
    {
      var innerException = new ApplicationException("InnerListener Exception");

      InvokeOnExecutionPlay(_strategy);
      var sequence = new MockSequence();
      ChildTransactionStrategyMock.InSequence(sequence).Setup(mock => mock.OnExecutionPause(Context, ExecutionListenerStub.Object)).Throws(innerException).Verifiable();
      ScopeMock.InSequence(sequence).Setup(mock => mock.Leave()).Verifiable();

      try
      {
        _strategy.OnExecutionPause(Context, ExecutionListenerStub.Object);
        Assert.Fail("Expected Exception");
      }
      catch (ApplicationException actualException)
      {
        Assert.That(actualException, Is.SameAs(innerException));
      }

      VerifyAll();
      Assert.That(_strategy.Scope, Is.Null);
    }

    [Test]
    public void Test_ChildStrategyThrowsFatalException ()
    {
      var innerException = new WxeFatalExecutionException(new Exception("ChildStrategy Exception"), null);

      InvokeOnExecutionPlay(_strategy);
      ChildTransactionStrategyMock.Setup(mock => mock.OnExecutionPause(Context, ExecutionListenerStub.Object)).Throws(innerException).Verifiable();

      try
      {
        _strategy.OnExecutionPause(Context, ExecutionListenerStub.Object);
        Assert.Fail("Expected Exception");
      }
      catch (WxeFatalExecutionException actualException)
      {
        Assert.That(actualException, Is.SameAs(innerException));
      }

      VerifyAll();
      Assert.That(_strategy.Scope, Is.Not.Null);
    }

    [Test]
    public void Test_LeaveThrows ()
    {
      var innerException = new Exception("Leave Exception");

      InvokeOnExecutionPlay(_strategy);
      var sequence = new MockSequence();
      ChildTransactionStrategyMock.InSequence(sequence).Setup(mock => mock.OnExecutionPause(Context, ExecutionListenerStub.Object)).Verifiable();
      ScopeMock.InSequence(sequence).Setup(mock => mock.Leave()).Throws(innerException).Verifiable();

      try
      {
        _strategy.OnExecutionPause(Context, ExecutionListenerStub.Object);
        Assert.Fail("Expected Exception");
      }
      catch (WxeFatalExecutionException actualException)
      {
        Assert.That(actualException.InnerException, Is.SameAs(innerException));
      }

      VerifyAll();
      Assert.That(_strategy.Scope, Is.Not.Null);
    }

    [Test]
    public void Test_ChildStrategyThrows_And_LeaveThrows ()
    {
      var innerException = new Exception("InnerListener Exception");
      var outerException = new Exception("Leave Exception");

      InvokeOnExecutionPlay(_strategy);
      var sequence = new MockSequence();
      ChildTransactionStrategyMock.InSequence(sequence).Setup(mock => mock.OnExecutionPause(Context, ExecutionListenerStub.Object)).Throws(innerException).Verifiable();
      ScopeMock.InSequence(sequence).Setup(mock => mock.Leave()).Throws(outerException).Verifiable();

      try
      {
        _strategy.OnExecutionPause(Context, ExecutionListenerStub.Object);
        Assert.Fail("Expected Exception");
      }
      catch (WxeFatalExecutionException actualException)
      {
        Assert.That(actualException.InnerException, Is.SameAs(innerException));
        Assert.That(actualException.OuterException, Is.SameAs(outerException));
      }

      VerifyAll();
      Assert.That(_strategy.Scope, Is.Not.Null);
    }
  }
}

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
using Remotion.Data.DomainObjects.DataManagement.Commands.EndPointModifications;
using Remotion.Data.DomainObjects.DataManagement.RelationEndPoints;
using Remotion.Data.DomainObjects.UnitTests.DataManagement.RelationEndPoints;
using Remotion.Data.DomainObjects.UnitTests.EventReceiver;
using Remotion.Data.DomainObjects.UnitTests.TestDomain;
using Remotion.Data.DomainObjects.UnitTests.UnitTesting;
using Remotion.Development.UnitTesting.NUnit;

namespace Remotion.Data.DomainObjects.UnitTests.DataManagement.Commands.EndPointModifications
{
  [TestFixture]
  public class DomainObjectCollectionEndPointRemoveCommandTest : DomainObjectCollectionEndPointModificationCommandTestBase
  {
    private Order _removedRelatedObject;
    private DomainObjectCollectionEndPointRemoveCommand _command;

    public override void SetUp ()
    {
      base.SetUp();

      _removedRelatedObject = DomainObjectIDs.Order1.GetObject<Order>(Transaction);

      _command = new DomainObjectCollectionEndPointRemoveCommand(
          CollectionEndPoint, _removedRelatedObject, CollectionDataMock.Object, EndPointProviderStub.Object, TransactionEventSinkMock.Object);
    }

    [Test]
    public void Initialization ()
    {
      Assert.That(_command.ModifiedEndPoint, Is.SameAs(CollectionEndPoint));
      Assert.That(_command.OldRelatedObject, Is.SameAs(_removedRelatedObject));
      Assert.That(_command.NewRelatedObject, Is.Null);
      Assert.That(_command.ModifiedCollectionEventRaiser, Is.SameAs(CollectionEndPoint.Collection));
      Assert.That(_command.ModifiedCollectionData, Is.SameAs(CollectionDataMock.Object));
    }

    [Test]
    public void Initialization_FromNullEndPoint ()
    {
      var endPoint = new NullDomainObjectCollectionEndPoint(Transaction, RelationEndPointID.Definition);
      Assert.That(
          () => new DomainObjectCollectionEndPointRemoveCommand(
              endPoint,
              _removedRelatedObject,
              CollectionDataMock.Object,
              EndPointProviderStub.Object,
              TransactionEventSinkMock.Object),
          Throws.ArgumentException
              .With.ArgumentExceptionMessageEqualTo(
                  "Modified end point is null, a NullEndPointModificationCommand is needed.",
                  "modifiedEndPoint"));
    }

    [Test]
    public void Begin ()
    {
      var sequence = new MockSequence();
      CollectionMockEventReceiver
          .InSequence(sequence)
          .SetupRemoving(CollectionEndPoint.Collection, _removedRelatedObject)
          .WithCurrentTransaction(Transaction)
          .Verifiable();
      TransactionEventSinkMock
          .InSequence(sequence)
          .Setup(mock => mock.RaiseRelationChangingEvent(DomainObject, CollectionEndPoint.Definition, _removedRelatedObject, null))
          .Verifiable();

      _command.Begin();

      CollectionMockEventReceiver.Verify();
      TransactionEventSinkMock.Verify();
    }

    [Test]
    public void End ()
    {
      var sequence = new MockSequence();
      TransactionEventSinkMock
          .InSequence(sequence)
          .Setup(mock => mock.RaiseRelationChangedEvent(DomainObject, CollectionEndPoint.Definition, _removedRelatedObject, null))
          .Verifiable();
      CollectionMockEventReceiver
          .InSequence(sequence)
          .SetupRemoved(CollectionEndPoint.Collection, _removedRelatedObject)
          .WithCurrentTransaction(Transaction)
          .Verifiable();

      _command.End();

      TransactionEventSinkMock.Verify();
      CollectionMockEventReceiver.Verify();
    }

    [Test]
    public void Perform ()
    {
      CollectionDataMock.Reset();
      CollectionDataMock.Setup(mock => mock.Remove(_removedRelatedObject)).Returns(true).Verifiable();

      _command.Perform();

      CollectionDataMock.Verify();

      CollectionMockEventReceiver.Verify(mock => mock.Removing(It.IsAny<object>(), It.IsAny<DomainObjectCollectionChangeEventArgs>()), Times.Never());
      CollectionMockEventReceiver.Verify(mock => mock.Removed(It.IsAny<object>(), It.IsAny<DomainObjectCollectionChangeEventArgs>()), Times.Never());
      Assert.That(CollectionEndPoint.HasBeenTouched, Is.True);
    }

    [Test]
    public void ExpandToAllRelatedObjects ()
    {
      var removedEndPointID = RelationEndPointObjectMother.CreateRelationEndPointID(_removedRelatedObject.ID, "Customer");
      var removedEndPoint = (IObjectEndPoint)DataManager.GetRelationEndPointWithoutLoading(removedEndPointID);
      Assert.That(removedEndPoint, Is.Not.Null);

      EndPointProviderStub.Setup(stub => stub.GetRelationEndPointWithLazyLoad(removedEndPoint.ID)).Returns(removedEndPoint);

      var bidirectionalModification = _command.ExpandToAllRelatedObjects();

      // DomainObject.Orders.Remove (_removedRelatedObject)
      var steps = bidirectionalModification.GetNestedCommands();
      Assert.That(steps.Count, Is.EqualTo(2));

      // _removedRelatedObject.Customer = null
      Assert.That(steps[0], Is.InstanceOf(typeof(RealObjectEndPointRegistrationCommandDecorator)));
      var setCustomerCommand = ((ObjectEndPointSetCommand)((RealObjectEndPointRegistrationCommandDecorator)steps[0]).DecoratedCommand);
      Assert.That(setCustomerCommand.ModifiedEndPoint, Is.SameAs(removedEndPoint));
      Assert.That(setCustomerCommand.OldRelatedObject, Is.SameAs(DomainObject));
      Assert.That(setCustomerCommand.NewRelatedObject, Is.Null);

      // DomainObject.Orders.Remove (_removedRelatedObject)
      Assert.That(steps[1], Is.SameAs(_command));
    }
  }
}

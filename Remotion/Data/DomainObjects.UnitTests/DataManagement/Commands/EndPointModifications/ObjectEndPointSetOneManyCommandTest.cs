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
using Remotion.Data.DomainObjects.DataManagement;
using Remotion.Data.DomainObjects.DataManagement.Commands.EndPointModifications;
using Remotion.Data.DomainObjects.DataManagement.RelationEndPoints;
using Remotion.Data.DomainObjects.Mapping;
using Remotion.Data.DomainObjects.UnitTests.DataManagement.RelationEndPoints;
using Remotion.Data.DomainObjects.UnitTests.TestDomain;
using Remotion.Development.UnitTesting.NUnit;

namespace Remotion.Data.DomainObjects.UnitTests.DataManagement.Commands.EndPointModifications
{
  [TestFixture]
  public class ObjectEndPointSetOneManyCommandTest : ObjectEndPointSetCommandTestBase
  {
    private OrderItem _domainObject;
    private Order _oldRelatedObject;
    private Order _newRelatedObject;

    private RelationEndPointID _endPointID;
    private RealObjectEndPoint _endPoint;

    private ObjectEndPointSetCommand _command;

    public override void SetUp ()
    {
      base.SetUp();

      _domainObject = DomainObjectIDs.OrderItem1.GetObject<OrderItem>();
      _oldRelatedObject = _domainObject.Order;
      _newRelatedObject = DomainObjectIDs.Order3.GetObject<Order>();

      _endPointID = RelationEndPointID.Resolve(_domainObject, oi => oi.Order);
      _endPoint = (RealObjectEndPoint)RelationEndPointObjectMother.CreateObjectEndPoint(_endPointID, _oldRelatedObject.ID);

      _command = new ObjectEndPointSetOneManyCommand(_endPoint, _newRelatedObject, OppositeObjectSetter, EndPointProviderStub.Object, TransactionEventSinkWithMock.Object);
    }

    [Test]
    public void Initialization ()
    {
      Assert.That(_command.ModifiedEndPoint, Is.SameAs(_endPoint));
      Assert.That(_command.OldRelatedObject, Is.SameAs(_oldRelatedObject));
      Assert.That(_command.NewRelatedObject, Is.SameAs(_newRelatedObject));
    }

    [Test]
    public void Initialization_FromNullEndPoint ()
    {
      var endPoint = new Mock<IRealObjectEndPoint>();
      endPoint.Setup(stub => stub.IsNull).Returns(true);
      Assert.That(
          () => new ObjectEndPointSetOneManyCommand(endPoint.Object, _newRelatedObject, OppositeObjectSetter, EndPointProviderStub.Object, TransactionEventSinkWithMock.Object),
          Throws.ArgumentException
              .With.ArgumentExceptionMessageEqualTo(
                  "Modified end point is null, a NullEndPointModificationCommand is needed.",
                  "modifiedEndPoint"));
    }

    [Test]
    public void Initialization_Unidirectional ()
    {
      var definition = MappingConfiguration.Current.GetTypeDefinition(typeof(Client)).GetMandatoryRelationEndPointDefinition(typeof(Client).FullName + ".ParentClient");
      var relationEndPointID = RelationEndPointID.Create(DomainObjectIDs.Client1.GetObject<Client>().ID, definition);
      var endPoint = (IRealObjectEndPoint)TestableClientTransaction.DataManager.GetRelationEndPointWithLazyLoad(relationEndPointID);
      Assert.That(
          () => new ObjectEndPointSetOneManyCommand(endPoint, Client.NewObject(), mi => { }, EndPointProviderStub.Object, TransactionEventSinkWithMock.Object),
          Throws.ArgumentException
              .With.ArgumentExceptionMessageEqualTo(
                  "EndPoint 'Remotion.Data.DomainObjects.UnitTests.TestDomain.Client.ParentClient' "
                  + "is from a unidirectional relation - use a ObjectEndPointSetUnidirectionalCommand instead.",
                  "modifiedEndPoint"));
    }

    [Test]
    public void Initialization_Bidirectional_OneOne ()
    {
      var definition = MappingConfiguration.Current.GetTypeDefinition(typeof(OrderTicket)).GetMandatoryRelationEndPointDefinition(typeof(OrderTicket).FullName + ".Order");
      var relationEndPointID = RelationEndPointID.Create(DomainObjectIDs.OrderTicket1.GetObject<OrderTicket>().ID, definition);
      var endPoint = (IRealObjectEndPoint)TestableClientTransaction.DataManager.GetRelationEndPointWithLazyLoad(relationEndPointID);
      Assert.That(
          () => new ObjectEndPointSetOneManyCommand(endPoint, Order.NewObject(), mi => { }, EndPointProviderStub.Object, TransactionEventSinkWithMock.Object),
          Throws.ArgumentException
              .With.ArgumentExceptionMessageEqualTo(
                  "EndPoint 'Remotion.Data.DomainObjects.UnitTests.TestDomain.OrderTicket.Order' "
                  + "is from a 1:1 relation - use a ObjectEndPointSetOneOneCommand instead.",
                  "modifiedEndPoint"));
    }

    [Test]
    public void Initialization_Same ()
    {
      var endPoint = (IRealObjectEndPoint)RelationEndPointObjectMother.CreateObjectEndPoint(_endPointID, _oldRelatedObject.ID);
      Assert.That(
          () => new ObjectEndPointSetOneManyCommand(endPoint, _oldRelatedObject, mi => { }, EndPointProviderStub.Object, TransactionEventSinkWithMock.Object),
          Throws.ArgumentException
              .With.ArgumentExceptionMessageEqualTo(
                  "New related object for EndPoint "
                  + "'Remotion.Data.DomainObjects.UnitTests.TestDomain.OrderItem.Order' is the same as its old value - use a ObjectEndPointSetSameCommand instead.",
                  "newRelatedObject"));
    }

    [Test]
    public void Perform ()
    {
      Assert.That(OppositeObjectSetterCalled, Is.False);
      Assert.That(_endPoint.HasBeenTouched, Is.False);

      _command.Perform();

      Assert.That(OppositeObjectSetterCalled, Is.True);
      Assert.That(OppositeObjectSetterObject, Is.SameAs(_newRelatedObject));
      Assert.That(_endPoint.HasBeenTouched, Is.True);
    }

    [Test]
    public virtual void Begin ()
    {
      TransactionEventSinkWithMock
          .Setup(mock => mock.RaiseRelationChangingEvent(_endPoint.GetDomainObject(), _endPoint.Definition, _oldRelatedObject, _newRelatedObject))
          .Verifiable();

      _command.Begin();

      TransactionEventSinkWithMock.Verify();
    }

    [Test]
    public virtual void End ()
    {
      TransactionEventSinkWithMock
          .Setup(mock => mock.RaiseRelationChangedEvent(_endPoint.GetDomainObject(), _endPoint.Definition, _oldRelatedObject, _newRelatedObject))
          .Verifiable();

      _command.End();

      TransactionEventSinkWithMock.Verify();
    }

    [Test]
    public void ExpandToAllRelatedObjects ()
    {
      // Scenario: orderItem.Order = newOrder;

      var oldRelatedEndPointID = RelationEndPointID.Resolve(_oldRelatedObject, o => o.OrderItems);
      var oldRelatedEndPointMock = new Mock<ICollectionEndPoint<ICollectionEndPointData>>(MockBehavior.Strict);

      var newRelatedEndPointID = RelationEndPointID.Resolve(_newRelatedObject, o => o.OrderItems);
      var newRelatedEndPointMock = new Mock<ICollectionEndPoint<ICollectionEndPointData>>(MockBehavior.Strict);

      EndPointProviderStub.Setup(stub => stub.GetRelationEndPointWithLazyLoad(oldRelatedEndPointID)).Returns(oldRelatedEndPointMock.Object);
      EndPointProviderStub.Setup(stub => stub.GetRelationEndPointWithLazyLoad(newRelatedEndPointID)).Returns(newRelatedEndPointMock.Object);

      // oldOrder.OrderItems.Remove (orderItem)
      var fakeRemoveCommand = new Mock<IDataManagementCommand>();
      fakeRemoveCommand.Setup(stub => stub.GetAllExceptions()).Returns(new Exception[0]);
      oldRelatedEndPointMock.Setup(mock => mock.CreateRemoveCommand(_domainObject)).Returns(fakeRemoveCommand.Object).Verifiable();

      // newOrder.OrderItems.Add (orderItem);
      var fakeAddCommand = new Mock<IDataManagementCommand>();
      fakeAddCommand.Setup(stub => stub.GetAllExceptions()).Returns(new Exception[0]);
      newRelatedEndPointMock.Setup(mock => mock.CreateAddCommand(_domainObject)).Returns(fakeAddCommand.Object).Verifiable();

      var bidirectionalModification = _command.ExpandToAllRelatedObjects();

      oldRelatedEndPointMock.Verify();
      newRelatedEndPointMock.Verify();

      var steps = bidirectionalModification.GetNestedCommands();
      Assert.That(steps.Count, Is.EqualTo(3));

      // orderItem.Order = newOrder;
      Assert.That(steps[0], Is.SameAs(_command));

      // newOrder.OrderItems.Add (orderItem);
      Assert.That(steps[1], Is.SameAs(fakeAddCommand.Object));

      // oldOrder.OrderItems.Remove (orderItem)
      Assert.That(steps[2], Is.SameAs(fakeRemoveCommand.Object));
    }
  }
}

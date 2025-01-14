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
using Remotion.Data.DomainObjects.DataManagement.Commands;
using Remotion.Data.DomainObjects.DataManagement.RelationEndPoints;
using Remotion.Data.DomainObjects.Mapping;
using Remotion.Data.DomainObjects.UnitTests.TestDomain;
using Remotion.Development.UnitTesting.NUnit;

namespace Remotion.Data.DomainObjects.UnitTests.DataManagement.RelationEndPoints
{
  [TestFixture]
  public class RelationEndPointManagerTest : ClientTransactionBaseTest
  {
    private RelationEndPointManager _relationEndPointManager;

    public override void SetUp ()
    {
      base.SetUp();

      _relationEndPointManager = (RelationEndPointManager)DataManagerTestHelper.GetRelationEndPointManager(TestableClientTransaction.DataManager);
    }

    [Test]
    public void RegisterEndPointsForDataContainer_Existing_RegistersRealObjectEndPoints ()
    {
      var endPointID = RelationEndPointObjectMother.CreateRelationEndPointID(DomainObjectIDs.OrderTicket1, "Order");
      var dataContainer = RelationEndPointTestHelper.CreateExistingForeignKeyDataContainer(endPointID, DomainObjectIDs.Order3);
      var foreignKeyProperty = GetPropertyDefinition(typeof(OrderTicket), "Order");

      _relationEndPointManager.RegisterEndPointsForDataContainer(dataContainer);

      var endPoint = (RealObjectEndPoint)_relationEndPointManager.RelationEndPoints[endPointID];
      Assert.That(endPoint, Is.Not.Null);
      Assert.That(endPoint.PropertyDefinition, Is.EqualTo(foreignKeyProperty));
      Assert.That(endPoint.OppositeObjectID, Is.EqualTo(DomainObjectIDs.Order3));
    }

    [Test]
    public void RegisterEndPointsForDataContainer_Existing_RegistersOppositeVirtualObjectEndPoints ()
    {
      var endPointID = RelationEndPointObjectMother.CreateRelationEndPointID(DomainObjectIDs.OrderTicket1, "Order");
      var dataContainer = RelationEndPointTestHelper.CreateExistingForeignKeyDataContainer(endPointID, DomainObjectIDs.Order3);

      _relationEndPointManager.RegisterEndPointsForDataContainer(dataContainer);

      var oppositeID = RelationEndPointObjectMother.CreateRelationEndPointID(DomainObjectIDs.Order3, "OrderTicket");
      var oppositeEndPoint = (IVirtualObjectEndPoint)_relationEndPointManager.RelationEndPoints[oppositeID];

      Assert.That(oppositeEndPoint, Is.Not.Null);
      Assert.That(oppositeEndPoint.OppositeObjectID, Is.EqualTo(DomainObjectIDs.OrderTicket1));
    }

    [Test]
    public void RegisterEndPointsForDataContainer_Existing_RegistersNoOppositeNullObjectEndPoints ()
    {
      var endPointID = RelationEndPointObjectMother.CreateRelationEndPointID(DomainObjectIDs.OrderTicket1, "Order");
      var dataContainer = RelationEndPointTestHelper.CreateExistingForeignKeyDataContainer(endPointID, null);

      _relationEndPointManager.RegisterEndPointsForDataContainer(dataContainer);

      var oppositeEndPointDefinition = endPointID.Definition.GetOppositeEndPointDefinition();
      var expectedID = RelationEndPointID.Create(null, oppositeEndPointDefinition);

      Assert.That(_relationEndPointManager.RelationEndPoints[expectedID], Is.Null);
    }

    [Test]
    public void RegisterEndPointsForDataContainer_Existing_RegistersNoCollectionEndPoints ()
    {
      var endPointID = RelationEndPointObjectMother.CreateRelationEndPointID(DomainObjectIDs.Order1, "OrderItems");
      var dataContainer = RelationEndPointTestHelper.CreateExistingDataContainer(endPointID);

      _relationEndPointManager.RegisterEndPointsForDataContainer(dataContainer);

      Assert.That(_relationEndPointManager.RelationEndPoints[endPointID], Is.Null);
    }

    [Test]
    public void RegisterEndPointsForDataContainer_Existing_RegistersNoVirtualObjectEndPoints ()
    {
      var endPointID = RelationEndPointObjectMother.CreateRelationEndPointID(DomainObjectIDs.Order1, "OrderTicket");
      var dataContainer = RelationEndPointTestHelper.CreateExistingDataContainer(endPointID);

      _relationEndPointManager.RegisterEndPointsForDataContainer(dataContainer);

      Assert.That(_relationEndPointManager.RelationEndPoints[endPointID], Is.Null);
    }

    [Test]
    public void RegisterEndPointsForDataContainer_New_RegistersVirtualObjectEndPoints ()
    {
      var endPointID = RelationEndPointObjectMother.CreateRelationEndPointID(DomainObjectIDs.Order1, "OrderTicket");
      var dataContainer = RelationEndPointTestHelper.CreateNewDataContainer(endPointID);

      _relationEndPointManager.RegisterEndPointsForDataContainer(dataContainer);

      var objectEndPoint = (IVirtualObjectEndPoint)_relationEndPointManager.RelationEndPoints[endPointID];
      Assert.That(objectEndPoint, Is.Not.Null);
      Assert.That(objectEndPoint.IsDataComplete, Is.True);
      Assert.That(objectEndPoint.OppositeObjectID, Is.Null);
    }

    [Test]
    public void RegisterEndPointsForDataContainer_New_RegistersRealObjectEndPoints ()
    {
      var endPointID = RelationEndPointObjectMother.CreateRelationEndPointID(DomainObjectIDs.OrderTicket1, "Order");
      var dataContainer = RelationEndPointTestHelper.CreateNewDataContainer(endPointID);
      var foreignKeyProperty = GetPropertyDefinition(typeof(OrderTicket), "Order");

      _relationEndPointManager.RegisterEndPointsForDataContainer(dataContainer);

      var objectEndPoint = (RealObjectEndPoint)_relationEndPointManager.RelationEndPoints[endPointID];
      Assert.That(objectEndPoint.PropertyDefinition, Is.EqualTo(foreignKeyProperty));
    }

    [Test]
    public void RegisterEndPointsForDataContainer_New_RegistersDomainObjectCollectionEndPoints ()
    {
      var endPointID = RelationEndPointObjectMother.CreateRelationEndPointID(DomainObjectIDs.Order1, "OrderItems");
      var dataContainer = RelationEndPointTestHelper.CreateNewDataContainer(endPointID);

      _relationEndPointManager.RegisterEndPointsForDataContainer(dataContainer);

      var collectionEndPoint = (IDomainObjectCollectionEndPoint)_relationEndPointManager.RelationEndPoints[endPointID];
      Assert.That(collectionEndPoint, Is.Not.Null);
      Assert.That(collectionEndPoint.IsDataComplete, Is.True);
      Assert.That(collectionEndPoint.Collection, Is.Empty);
    }

    [Test]
    public void CreateUnregisterCommandForDataContainer_Existing_IncludesRealObjectEndPoints_IgnoresVirtualEndPoints ()
    {
      var realEndPointID = RelationEndPointObjectMother.CreateRelationEndPointID(DomainObjectIDs.Order1, "Customer");
      var dataContainer = RelationEndPointTestHelper.CreateExistingForeignKeyDataContainer(realEndPointID, DomainObjectIDs.Order3);
      _relationEndPointManager.RegisterEndPointsForDataContainer(dataContainer);
      var realEndPoint = _relationEndPointManager.RelationEndPoints[realEndPointID];

      var virtualObjectEndPointID = RelationEndPointObjectMother.CreateRelationEndPointID(DomainObjectIDs.Order1, "OrderItems");
      var virtualObjectEndPointStub = new Mock<IVirtualObjectEndPoint>();
      virtualObjectEndPointStub.Setup(stub => stub.ID).Returns(virtualObjectEndPointID);
      RelationEndPointManagerTestHelper.AddEndPoint(_relationEndPointManager, virtualObjectEndPointStub.Object);

      var collectionEndPointID = RelationEndPointObjectMother.CreateRelationEndPointID(DomainObjectIDs.Order1, "OrderTicket");
      var collectionEndPointStub = new Mock<ICollectionEndPoint<ICollectionEndPointData>>();
      collectionEndPointStub.Setup(stub => stub.ID).Returns(collectionEndPointID);
      RelationEndPointManagerTestHelper.AddEndPoint(_relationEndPointManager, collectionEndPointStub.Object);

      var command = _relationEndPointManager.CreateUnregisterCommandForDataContainer(dataContainer);

      Assert.That(command, Is.TypeOf<UnregisterEndPointsCommand>());
      Assert.That(((UnregisterEndPointsCommand)command).RegistrationAgent, Is.SameAs(_relationEndPointManager.RegistrationAgent));
      Assert.That(((UnregisterEndPointsCommand)command).Map, Is.SameAs(_relationEndPointManager.RelationEndPoints));
      Assert.That(((UnregisterEndPointsCommand)command).EndPoints, Has.Member(realEndPoint));
      Assert.That(((UnregisterEndPointsCommand)command).EndPoints, Has.No.Member(virtualObjectEndPointStub.Object));
      Assert.That(((UnregisterEndPointsCommand)command).EndPoints, Has.No.Member(collectionEndPointStub.Object));
    }

    [Test]
    public void CreateUnregisterCommandForDataContainer_New_IncludesRealObjectEndPoints_IncludesVirtualEndPoints ()
    {
      var realEndPointID = RelationEndPointObjectMother.CreateRelationEndPointID(DomainObjectIDs.Order1, "Customer");
      var dataContainer = RelationEndPointTestHelper.CreateNewDataContainer(realEndPointID);
      _relationEndPointManager.RegisterEndPointsForDataContainer(dataContainer);
      var realEndPoint = _relationEndPointManager.RelationEndPoints[realEndPointID];

      var virtualObjectEndPointID = RelationEndPointObjectMother.CreateRelationEndPointID(DomainObjectIDs.Order1, "OrderItems");
      var virtualObjectEndPoint = _relationEndPointManager.RelationEndPoints[virtualObjectEndPointID];
      Assert.That(virtualObjectEndPoint, Is.Not.Null);

      var collectionEndPointID = RelationEndPointObjectMother.CreateRelationEndPointID(DomainObjectIDs.Order1, "OrderTicket");
      var collectionEndPoint = _relationEndPointManager.RelationEndPoints[collectionEndPointID];
      Assert.That(collectionEndPoint, Is.Not.Null);

      var command = _relationEndPointManager.CreateUnregisterCommandForDataContainer(dataContainer);

      Assert.That(command, Is.TypeOf<UnregisterEndPointsCommand>());
      Assert.That(((UnregisterEndPointsCommand)command).RegistrationAgent, Is.SameAs(_relationEndPointManager.RegistrationAgent));
      Assert.That(((UnregisterEndPointsCommand)command).Map, Is.SameAs(_relationEndPointManager.RelationEndPoints));
      Assert.That(((UnregisterEndPointsCommand)command).EndPoints, Has.Member(realEndPoint));
      Assert.That(((UnregisterEndPointsCommand)command).EndPoints, Has.Member(virtualObjectEndPoint));
      Assert.That(((UnregisterEndPointsCommand)command).EndPoints, Has.Member(collectionEndPoint));
    }

    [Test]
    public void CreateUnregisterCommandForDataContainer_IgnoresNonRegisteredEndPoints ()
    {
      var dataContainer = DataContainer.CreateNew(DomainObjectIDs.Order1);
      var command = _relationEndPointManager.CreateUnregisterCommandForDataContainer(dataContainer);

      Assert.That(command, Is.TypeOf<UnregisterEndPointsCommand>());
      Assert.That(((UnregisterEndPointsCommand)command).RegistrationAgent, Is.SameAs(_relationEndPointManager.RegistrationAgent));
      Assert.That(((UnregisterEndPointsCommand)command).Map, Is.SameAs(_relationEndPointManager.RelationEndPoints));
      Assert.That(((UnregisterEndPointsCommand)command).EndPoints, Is.Empty);
    }

    [Test]
    public void CreateUnregisterCommandForDataContainer_WithUnidirectionalEndPoints ()
    {
      var endPointID = RelationEndPointObjectMother.CreateRelationEndPointID(DomainObjectIDs.Location1, "Client");
      var dataContainer = RelationEndPointTestHelper.CreateNewDataContainer(endPointID);
      _relationEndPointManager.RegisterEndPointsForDataContainer(dataContainer);
      var unidirectionalEndPoint = (RealObjectEndPoint)_relationEndPointManager.RelationEndPoints[endPointID];
      Assert.That(unidirectionalEndPoint, Is.Not.Null);

      var command = _relationEndPointManager.CreateUnregisterCommandForDataContainer(dataContainer);

      Assert.That(command, Is.TypeOf<UnregisterEndPointsCommand>());
      Assert.That(((UnregisterEndPointsCommand)command).RegistrationAgent, Is.SameAs(_relationEndPointManager.RegistrationAgent));
      Assert.That(((UnregisterEndPointsCommand)command).Map, Is.SameAs(_relationEndPointManager.RelationEndPoints));
      Assert.That(((UnregisterEndPointsCommand)command).EndPoints, Has.Member(unidirectionalEndPoint));
    }

    [Test]
    public void CreateUnregisterCommandForDataContainer_WithUnregisterableEndPoint ()
    {
      var dataContainer = DataContainer.CreateForExisting(DomainObjectIDs.Order1, null, pd => pd.DefaultValue);
      var endPoint = new Mock<IRealObjectEndPoint>();
      endPoint.Setup(stub => stub.ID).Returns(RelationEndPointID.Create(dataContainer.ID, typeof(Order), "Customer"));
      endPoint.Setup(stub => stub.Definition).Returns(endPoint.Object.ID.Definition);
      endPoint.Setup(stub => stub.HasChanged).Returns(true);
      RelationEndPointManagerTestHelper.AddEndPoint(_relationEndPointManager, endPoint.Object);

      var command = _relationEndPointManager.CreateUnregisterCommandForDataContainer(dataContainer);

      Assert.That(command, Is.TypeOf<ExceptionCommand>());
      Assert.That(((ExceptionCommand)command).Exception, Is.TypeOf<InvalidOperationException>());
      Assert.That(((ExceptionCommand)command).Exception.Message, Is.EqualTo(
          "The relations of object 'Order|5682f032-2f0b-494b-a31c-c97f02b89c36|System.Guid' cannot be unloaded.\r\n"
          + "Relation end-point "
          + "'Order|5682f032-2f0b-494b-a31c-c97f02b89c36|System.Guid/Remotion.Data.DomainObjects.UnitTests.TestDomain.Order.Customer' has changed. "
          + "Only unchanged relation end-points can be unregistered."));
    }

    [Test]
    public void CreateUnregisterCommandForDataContainer_WithUnregisterableEndPoint_DueToChangedOpposite ()
    {
      var dataContainer = DataContainer.CreateForExisting(DomainObjectIDs.Order1, null, pd => pd.DefaultValue);
      var endPoint = new Mock<IRealObjectEndPoint>();
      endPoint.Setup(stub => stub.ID).Returns(RelationEndPointID.Create(dataContainer.ID, typeof(Order), "Customer"));
      endPoint.Setup(stub => stub.Definition).Returns(endPoint.Object.ID.Definition);
      endPoint.Setup(stub => stub.HasChanged).Returns(false);
      endPoint.Setup(stub => stub.OppositeObjectID).Returns(DomainObjectIDs.Customer1);
      RelationEndPointManagerTestHelper.AddEndPoint(_relationEndPointManager, endPoint.Object);

      var oppositeEndPoint = new Mock<IVirtualEndPoint>();
      oppositeEndPoint.Setup(stub => stub.ID).Returns(RelationEndPointID.Create(DomainObjectIDs.Customer1, typeof(Customer), "Orders"));
      oppositeEndPoint.Setup(stub => stub.Definition).Returns(oppositeEndPoint.Object.ID.Definition);
      oppositeEndPoint.Setup(stub => stub.HasChanged).Returns(true);
      RelationEndPointManagerTestHelper.AddEndPoint(_relationEndPointManager, oppositeEndPoint.Object);

      var command = _relationEndPointManager.CreateUnregisterCommandForDataContainer(dataContainer);

      Assert.That(command, Is.TypeOf<ExceptionCommand>());
      Assert.That(((ExceptionCommand)command).Exception, Is.TypeOf<InvalidOperationException>());
      Assert.That(((ExceptionCommand)command).Exception.Message, Is.EqualTo(
          "The relations of object 'Order|5682f032-2f0b-494b-a31c-c97f02b89c36|System.Guid' cannot be unloaded.\r\n"
          + "The opposite relation property 'Remotion.Data.DomainObjects.UnitTests.TestDomain.Customer.Orders' of relation end-point "
          + "'Order|5682f032-2f0b-494b-a31c-c97f02b89c36|System.Guid/Remotion.Data.DomainObjects.UnitTests.TestDomain.Order.Customer' has changed. "
          + "Non-virtual end-points that are part of changed relations cannot be unloaded."));
    }

    [Test]
    public void CreateUnregisterCommandForDataContainer_WithMultipleUnregisterableEndPoints ()
    {
      var dataContainer = DataContainer.CreateNew(DomainObjectIDs.Order1);
      var endPoint1 = new Mock<IVirtualObjectEndPoint>();
      endPoint1.Setup(stub => stub.ID).Returns(RelationEndPointID.Create(dataContainer.ID, typeof(Order), "OrderTicket"));
      endPoint1.Setup(stub => stub.Definition).Returns(endPoint1.Object.ID.Definition);
      endPoint1.Setup(stub => stub.OppositeObjectID).Returns(DomainObjectIDs.OrderTicket1);
      RelationEndPointManagerTestHelper.AddEndPoint(_relationEndPointManager, endPoint1.Object);

      var endPoint2 = new Mock<IRealObjectEndPoint>();
      endPoint2.Setup(stub => stub.ID).Returns(RelationEndPointID.Create(dataContainer.ID, typeof(Order), "Customer"));
      endPoint2.Setup(stub => stub.Definition).Returns(endPoint2.Object.ID.Definition);
      endPoint2.Setup(stub => stub.OriginalOppositeObjectID).Returns(DomainObjectIDs.Customer1);
      RelationEndPointManagerTestHelper.AddEndPoint(_relationEndPointManager, endPoint2.Object);

      var command = _relationEndPointManager.CreateUnregisterCommandForDataContainer(dataContainer);

      Assert.That(command, Is.TypeOf<ExceptionCommand>());
      Assert.That(
          ((ExceptionCommand)command).Exception.Message,
          Is.EqualTo(
              "The relations of object 'Order|5682f032-2f0b-494b-a31c-c97f02b89c36|System.Guid' cannot be unloaded.\r\n"
              + "Relation end-point "
              + "'Order|5682f032-2f0b-494b-a31c-c97f02b89c36|System.Guid/Remotion.Data.DomainObjects.UnitTests.TestDomain.Order.OrderTicket' "
              + "would leave a dangling reference.\r\n"
              + "Relation end-point "
              + "'Order|5682f032-2f0b-494b-a31c-c97f02b89c36|System.Guid/Remotion.Data.DomainObjects.UnitTests.TestDomain.Order.Customer' "
              + "would leave a dangling reference."));
    }

    [Test]
    public void CreateUnloadVirtualEndPointsCommand ()
    {
      var endPointID1 = RelationEndPointID.Create(DomainObjectIDs.Order1, typeof(Order), "OrderItems");
      var endPointID2 = RelationEndPointID.Create(DomainObjectIDs.Order3, typeof(Order), "OrderItems");

      var endPointStub1 = new Mock<IVirtualEndPoint>();
      endPointStub1.Setup(stub => stub.ID).Returns(endPointID1);
      endPointStub1.Setup(stub => stub.CanBeMarkedIncomplete).Returns(true);
      RelationEndPointManagerTestHelper.AddEndPoint(_relationEndPointManager, endPointStub1.Object);

      var endPointStub2 = new Mock<IVirtualEndPoint>();
      endPointStub2.Setup(stub => stub.ID).Returns(endPointID2);
      endPointStub2.Setup(stub => stub.CanBeMarkedIncomplete).Returns(true);
      RelationEndPointManagerTestHelper.AddEndPoint(_relationEndPointManager, endPointStub2.Object);

      var result = _relationEndPointManager.CreateUnloadVirtualEndPointsCommand(new[] { endPointID1, endPointID2 });

      Assert.That(
          result,
          Is.TypeOf<UnloadVirtualEndPointsCommand>()
              .With.Property("VirtualEndPoints").EqualTo(new[] { endPointStub1.Object, endPointStub2.Object })
              .And.Property("RelationEndPointMap").SameAs(_relationEndPointManager.RelationEndPoints)
              .And.Property("RegistrationAgent").SameAs(_relationEndPointManager.RegistrationAgent));
    }

    [Test]
    public void CreateUnloadVirtualEndPointsCommand_NonLoadedEndPoints ()
    {
      var endPointID = RelationEndPointID.Create(DomainObjectIDs.Order1, typeof(Order), "OrderItems");

      var result = _relationEndPointManager.CreateUnloadVirtualEndPointsCommand(new[] { endPointID });

      Assert.That(result, Is.TypeOf<NopCommand>());
    }

    [Test]
    public void CreateUnloadVirtualEndPointsCommand_SomeNonLoadedEndPoints ()
    {
      var endPointID1 = RelationEndPointID.Create(DomainObjectIDs.Order1, typeof(Order), "OrderItems");
      var endPointID2 = RelationEndPointID.Create(DomainObjectIDs.Order3, typeof(Order), "OrderItems");

      var endPointStub2 = new Mock<IVirtualEndPoint>();
      endPointStub2.Setup(stub => stub.ID).Returns(endPointID2);
      endPointStub2.Setup(stub => stub.CanBeMarkedIncomplete).Returns(true);
      RelationEndPointManagerTestHelper.AddEndPoint(_relationEndPointManager, endPointStub2.Object);

      var result = _relationEndPointManager.CreateUnloadVirtualEndPointsCommand(new[] { endPointID1, endPointID2 });

      Assert.That(result, Is.TypeOf<UnloadVirtualEndPointsCommand>().With.Property("VirtualEndPoints").EqualTo(new[] { endPointStub2.Object }));
    }

    [Test]
    public void CreateUnloadVirtualEndPointsCommand_NonUnloadableEndPoints ()
    {
      var endPointID1 = RelationEndPointID.Create(DomainObjectIDs.Order1, typeof(Order), "OrderItems");
      var endPointID2 = RelationEndPointID.Create(DomainObjectIDs.Order3, typeof(Order), "OrderItems");
      var endPointID3 = RelationEndPointID.Create(DomainObjectIDs.Order4, typeof(Order), "OrderItems");

      var endPointStub1 = new Mock<IVirtualEndPoint>();
      endPointStub1.Setup(stub => stub.ID).Returns(endPointID1);
      endPointStub1.Setup(stub => stub.CanBeMarkedIncomplete).Returns(false);
      RelationEndPointManagerTestHelper.AddEndPoint(_relationEndPointManager, endPointStub1.Object);

      var endPointStub2 = new Mock<IVirtualEndPoint>();
      endPointStub2.Setup(stub => stub.ID).Returns(endPointID2);
      endPointStub2.Setup(stub => stub.CanBeMarkedIncomplete).Returns(false);
      RelationEndPointManagerTestHelper.AddEndPoint(_relationEndPointManager, endPointStub2.Object);

      var endPointStub3 = new Mock<IVirtualEndPoint>();
      endPointStub3.Setup(stub => stub.ID).Returns(endPointID3);
      endPointStub3.Setup(stub => stub.CanBeMarkedIncomplete).Returns(true);
      RelationEndPointManagerTestHelper.AddEndPoint(_relationEndPointManager, endPointStub3.Object);

      var result = _relationEndPointManager.CreateUnloadVirtualEndPointsCommand(new[] { endPointID1, endPointID2, endPointID3 });

      Assert.That(result, Is.TypeOf<CompositeCommand>());
      var exceptionCommands = ((CompositeCommand)result).GetNestedCommands();
      Assert.That(exceptionCommands[0], Is.TypeOf<ExceptionCommand>());
      Assert.That(((ExceptionCommand)exceptionCommands[0]).Exception, Is.TypeOf<InvalidOperationException>().With.Message.EqualTo(
          "The end point with ID "
          + "'Order|5682f032-2f0b-494b-a31c-c97f02b89c36|System.Guid/Remotion.Data.DomainObjects.UnitTests.TestDomain.Order.OrderItems' "
          + "has been changed. Changed end points cannot be unloaded."));
      Assert.That(exceptionCommands[1], Is.TypeOf<ExceptionCommand>());
      Assert.That(((ExceptionCommand)exceptionCommands[1]).Exception, Is.TypeOf<InvalidOperationException>().With.Message.EqualTo(
          "The end point with ID "
          + "'Order|83445473-844a-4d3f-a8c3-c27f8d98e8ba|System.Guid/Remotion.Data.DomainObjects.UnitTests.TestDomain.Order.OrderItems' "
          + "has been changed. Changed end points cannot be unloaded."));
    }

    [Test]
    public void CreateUnloadVirtualEndPointsCommand_NonVirtualIDs ()
    {
      var endPointID = RelationEndPointID.Create(DomainObjectIDs.OrderItem1, typeof(OrderItem), "Order");

      Assert.That(
          () => _relationEndPointManager.CreateUnloadVirtualEndPointsCommand(new[] { endPointID }),
          Throws.ArgumentException.With.ArgumentExceptionMessageEqualTo(
              "The given end point ID "
              + "'OrderItem|2f4d42c7-7ffa-490d-bfcd-a9101bbf4e1a|System.Guid/Remotion.Data.DomainObjects.UnitTests.TestDomain.OrderItem.Order' "
              + "does not denote a virtual end-point.", "endPointIDs"));
    }

    [Test]
    public void GetRelationEndPointWithoutLoading_NullRealObjectEndPoint ()
    {
      var endPointDefinition =
          Configuration.GetTypeDefinition(typeof(OrderTicket)).GetRelationEndPointDefinition(typeof(OrderTicket).FullName + ".Order");
      var relationEndPointID = RelationEndPointID.Create(null, endPointDefinition);

      var result = _relationEndPointManager.GetRelationEndPointWithoutLoading(relationEndPointID);

      Assert.That(result, Is.TypeOf(typeof(NullRealObjectEndPoint)));
      Assert.That(result.Definition, Is.EqualTo(endPointDefinition));
      Assert.That(result.ID, Is.EqualTo(relationEndPointID));
    }

    [Test]
    public void GetRelationEndPointWithoutLoading_NullVirtualObjectEndPoint ()
    {
      var endPointDefinition =
          Configuration.GetTypeDefinition(typeof(Order)).GetRelationEndPointDefinition(typeof(Order).FullName + ".OrderTicket");
      var relationEndPointID = RelationEndPointID.Create(null, endPointDefinition);

      var result = _relationEndPointManager.GetRelationEndPointWithoutLoading(relationEndPointID);

      Assert.That(result, Is.TypeOf(typeof(NullVirtualObjectEndPoint)));
      Assert.That(result.Definition, Is.EqualTo(endPointDefinition));
      Assert.That(result.ID, Is.EqualTo(relationEndPointID));
    }

    [Test]
    public void GetRelationEndPointWithoutLoading_NullDomainObjectCollectionEndPoint ()
    {
      var endPointDefinition =
          Configuration.GetTypeDefinition(typeof(Order)).GetRelationEndPointDefinition(typeof(Order).FullName + ".OrderItems");
      var relationEndPointID = RelationEndPointID.Create(null, endPointDefinition);

      var result = _relationEndPointManager.GetRelationEndPointWithoutLoading(relationEndPointID);

      Assert.That(result, Is.TypeOf(typeof(NullDomainObjectCollectionEndPoint)));
      Assert.That(result.Definition, Is.EqualTo(endPointDefinition));
      Assert.That(result.ID, Is.EqualTo(relationEndPointID));
    }

    [Test]
    public void GetRelationEndPointWithoutLoading_NullVirtualCollectionEndPoint ()
    {
      var endPointDefinition =
          Configuration.GetTypeDefinition(typeof(Product)).GetRelationEndPointDefinition(typeof(Product).FullName + ".Reviews");
      var relationEndPointID = RelationEndPointID.Create(null, endPointDefinition);

      var result = _relationEndPointManager.GetRelationEndPointWithoutLoading(relationEndPointID);

      Assert.That(result, Is.TypeOf(typeof(NullVirtualCollectionEndPoint)));
      Assert.That(result.Definition, Is.EqualTo(endPointDefinition));
      Assert.That(result.ID, Is.EqualTo(relationEndPointID));
    }

    [Test]
    public void GetRelationEndPointWithoutLoading_EndPointRegistered ()
    {
      var endPointID = RelationEndPointID.Create(DomainObjectIDs.Order1, typeof(Order), "OrderItems");
      var endPointStub = new Mock<IRelationEndPoint>();
      endPointStub.Setup(stub => stub.ID).Returns(endPointID);

      RelationEndPointManagerTestHelper.AddEndPoint(_relationEndPointManager, endPointStub.Object);

      var result = _relationEndPointManager.GetRelationEndPointWithoutLoading(endPointID);

      Assert.That(result, Is.SameAs(endPointStub.Object));
    }

    [Test]
    public void GetRelationEndPointWithoutLoading_EndPointNotRegistered ()
    {
      var endPointID = RelationEndPointID.Create(DomainObjectIDs.Order1, typeof(Order), "OrderItems");
      var endPointStub = new Mock<IRelationEndPoint>();
      endPointStub.Setup(stub => stub.ID).Returns(endPointID);

      var result = _relationEndPointManager.GetRelationEndPointWithoutLoading(endPointID);

      Assert.That(result, Is.Null);
    }

    [Test]
    public void GetRelationEndPointWithLazyLoad_NullObjectEndPoint ()
    {
      var endPointDefinition =
          Configuration.GetTypeDefinition(typeof(Order)).GetRelationEndPointDefinition(typeof(Order).FullName + ".OrderTicket");
      var relationEndPointID = RelationEndPointID.Create(null, endPointDefinition);

      var result = _relationEndPointManager.GetRelationEndPointWithLazyLoad(relationEndPointID);

      Assert.That(result, Is.TypeOf(typeof(NullVirtualObjectEndPoint)));
      Assert.That(result.Definition, Is.EqualTo(endPointDefinition));
    }

    [Test]
    public void GetRelationEndPointWithLazyLoad_NullDomainObjectCollectionEndPoint ()
    {
      var endPointDefinition =
          Configuration.GetTypeDefinition(typeof(Order)).GetRelationEndPointDefinition(typeof(Order).FullName + ".OrderItems");
      var relationEndPointID = RelationEndPointID.Create(null, endPointDefinition);

      var result = _relationEndPointManager.GetRelationEndPointWithLazyLoad(relationEndPointID);

      Assert.That(result, Is.TypeOf(typeof(NullDomainObjectCollectionEndPoint)));
      Assert.That(result.Definition, Is.EqualTo(endPointDefinition));
    }

    [Test]
    public void GetRelationEndPointWithLazyLoad_NullVirtualCollectionEndPoint ()
    {
      var endPointDefinition =
          Configuration.GetTypeDefinition(typeof(Product)).GetRelationEndPointDefinition(typeof(Product).FullName + ".Reviews");
      var relationEndPointID = RelationEndPointID.Create(null, endPointDefinition);

      var result = _relationEndPointManager.GetRelationEndPointWithLazyLoad(relationEndPointID);

      Assert.That(result, Is.TypeOf(typeof(NullVirtualCollectionEndPoint)));
      Assert.That(result.Definition, Is.EqualTo(endPointDefinition));
    }

    [Test]
    public void GetRelationEndPointWithLazyLoad_DoesNotSupportAnonymousEndPoints ()
    {
      var client = DomainObjectIDs.Client2.GetObject<Client>();
      var parentClientEndPointDefinition = client.ID.ClassDefinition.GetRelationEndPointDefinition(typeof(Client) + ".ParentClient");
      IRelationEndPoint unidirectionalEndPoint =
          _relationEndPointManager.GetRelationEndPointWithLazyLoad(RelationEndPointID.Create(client.ID, parentClientEndPointDefinition));

      Client parentClient = client.ParentClient;
      Assert.That(parentClient, Is.Not.Null);

      var anonymousEndPointDefinition = unidirectionalEndPoint.Definition.GetOppositeEndPointDefinition();
      Assert.That(
          () => _relationEndPointManager.GetRelationEndPointWithLazyLoad(RelationEndPointID.Create(parentClient.ID, anonymousEndPointDefinition)),
          Throws.ArgumentException
              .With.ArgumentExceptionMessageEqualTo("GetRelationEndPointWithLazyLoad cannot be called for anonymous end points.", "endPointID"));
    }

    [Test]
    public void GetRelationEndPointWithLazyLoad_EndPointAlreadyAvailable ()
    {
      var endPointID = RelationEndPointObjectMother.CreateRelationEndPointID(DomainObjectIDs.Order1, "OrderTicket");
      var endPointMock = new Mock<IRelationEndPoint>(MockBehavior.Strict);
      endPointMock.Setup(stub => stub.ID).Returns(endPointID);
      endPointMock.Setup(mock => mock.EnsureDataComplete()).Verifiable();

      RelationEndPointManagerTestHelper.AddEndPoint(_relationEndPointManager, endPointMock.Object);

      Assert.That(_relationEndPointManager.RelationEndPoints[endPointID], Is.SameAs(endPointMock.Object));

      var result = _relationEndPointManager.GetRelationEndPointWithLazyLoad(endPointID);

      Assert.That(result, Is.SameAs(endPointMock.Object));
      Assert.That(_relationEndPointManager.RelationEndPoints[endPointID], Is.SameAs(endPointMock.Object));
    }

    [Test]
    public void GetRelationEndPointWithLazyLoad_RegistersCollectionEndPoint_ButDoesNotLoadItsContents ()
    {
      _relationEndPointManager.ClientTransaction.EnsureDataAvailable(DomainObjectIDs.Order1); // preload Order1 before lazily loading its virtual end point

      var orderItemsEndPointID = RelationEndPointObjectMother.CreateRelationEndPointID(DomainObjectIDs.Order1, "OrderItems");
      Assert.That(_relationEndPointManager.RelationEndPoints[orderItemsEndPointID], Is.Null);

      var endPoint = _relationEndPointManager.GetRelationEndPointWithLazyLoad(orderItemsEndPointID);

      Assert.That(endPoint, Is.Not.Null);
      Assert.That(endPoint.IsDataComplete, Is.False);
      Assert.That(_relationEndPointManager.RelationEndPoints[orderItemsEndPointID], Is.SameAs(endPoint));
    }

    [Test]
    public void GetRelationEndPointWithLazyLoad_RegistersVirtualObjectEndPoint_ButDoesNotLoadItsContents ()
    {
      _relationEndPointManager.ClientTransaction.EnsureDataAvailable(DomainObjectIDs.Order1); // preload Order1 before lazily loading its virtual end point

      var orderTicketEndPointID = RelationEndPointObjectMother.CreateRelationEndPointID(DomainObjectIDs.Order1, "OrderTicket");
      Assert.That(_relationEndPointManager.RelationEndPoints[orderTicketEndPointID], Is.Null);

      var endPoint = _relationEndPointManager.GetRelationEndPointWithLazyLoad(orderTicketEndPointID);

      Assert.That(endPoint, Is.Not.Null);
      Assert.That(endPoint.IsDataComplete, Is.False);
      Assert.That(_relationEndPointManager.RelationEndPoints[orderTicketEndPointID], Is.SameAs(endPoint));
    }

    [Test]
    public void GetRelationEndPointWithLazyLoad_RegistersVirtualObjectEndPointWithNull ()
    {
      _relationEndPointManager.ClientTransaction.EnsureDataAvailable(DomainObjectIDs.Employee1); // preload Employee before lazily loading its virtual end point

      var endPointID = RelationEndPointObjectMother.CreateRelationEndPointID(DomainObjectIDs.Employee1, "Computer");
      Assert.That(_relationEndPointManager.RelationEndPoints[endPointID], Is.Null);

      var endPoint = _relationEndPointManager.GetRelationEndPointWithLazyLoad(endPointID);

      Assert.That(endPoint, Is.Not.Null);
      Assert.That(_relationEndPointManager.RelationEndPoints[endPointID], Is.SameAs(endPoint));
      Assert.That(((IVirtualObjectEndPoint)endPoint).OppositeObjectID, Is.Null);
    }

    [Test]
    public void GetRelationEndPointWithLazyLoad_LoadsData_OfObjectsWithRealEndPointNotYetRegistered ()
    {
      var locationEndPointID = RelationEndPointObjectMother.CreateRelationEndPointID(DomainObjectIDs.Location1, "Client");
      Assert.That(locationEndPointID.Definition.IsVirtual, Is.False);
      Assert.That(TestableClientTransaction.DataManager.DataContainers[DomainObjectIDs.Location1], Is.Null);

      var result = _relationEndPointManager.GetRelationEndPointWithLazyLoad(locationEndPointID);
      Assert.That(result, Is.Not.Null);

      Assert.That(TestableClientTransaction.DataManager.DataContainers[DomainObjectIDs.Location1], Is.Not.Null);
    }

    [Test]
    public void GetRelationEndPointWithLazyLoad_DoesNotLoadData_OfObjectsWithVirtualEndPointNotYetRegistered ()
    {
      var endPointID = RelationEndPointObjectMother.CreateRelationEndPointID(DomainObjectIDs.Order1, "OrderTicket");
      Assert.That(endPointID.Definition.IsVirtual, Is.True);
      Assert.That(TestableClientTransaction.DataManager.DataContainers[DomainObjectIDs.Order1], Is.Null);

      var result = _relationEndPointManager.GetRelationEndPointWithLazyLoad(endPointID);
      Assert.That(result, Is.Not.Null);

      Assert.That(TestableClientTransaction.DataManager.DataContainers[DomainObjectIDs.Order1], Is.Null);
    }

    [Test]
    public void GetOrCreateVirtualEndPoint_AlreadyAvailable ()
    {
      var endPointID = RelationEndPointObjectMother.CreateRelationEndPointID(DomainObjectIDs.Order1, "OrderTicket");
      var endPointMock = new Mock<IVirtualEndPoint>(MockBehavior.Strict);
      endPointMock.Setup(stub => stub.ID).Returns(endPointID);
      endPointMock.Setup(mock => mock.EnsureDataComplete()).Verifiable();

      RelationEndPointManagerTestHelper.AddEndPoint(_relationEndPointManager, endPointMock.Object);

      Assert.That(_relationEndPointManager.RelationEndPoints[endPointID], Is.SameAs(endPointMock.Object));

      var result = _relationEndPointManager.GetOrCreateVirtualEndPoint(endPointID);

      Assert.That(result, Is.SameAs(endPointMock.Object));
      Assert.That(_relationEndPointManager.RelationEndPoints[endPointID], Is.SameAs(endPointMock.Object));
    }

    [Test]
    public void GetOrCreateVirtualEndPoint_NewlyCreated_ObjectEndPoint ()
    {
      var endPointID = RelationEndPointObjectMother.CreateRelationEndPointID(DomainObjectIDs.Order1, "OrderTicket");
      Assert.That(_relationEndPointManager.RelationEndPoints[endPointID], Is.Null);

      var result = _relationEndPointManager.GetOrCreateVirtualEndPoint(endPointID);

      Assert.That(result, Is.Not.Null.And.AssignableTo<IVirtualObjectEndPoint>());
      Assert.That(_relationEndPointManager.RelationEndPoints[endPointID], Is.SameAs(result));
      Assert.That(result.IsDataComplete, Is.False);
    }

    [Test]
    public void GetOrCreateVirtualEndPoint_NewlyCreated_CollectionEndPoint ()
    {
      var endPointID = RelationEndPointObjectMother.CreateRelationEndPointID(DomainObjectIDs.Order1, "OrderItems");
      Assert.That(_relationEndPointManager.RelationEndPoints[endPointID], Is.Null);

      var result = _relationEndPointManager.GetOrCreateVirtualEndPoint(endPointID);

      Assert.That(result, Is.Not.Null.And.AssignableTo<ICollectionEndPoint<ICollectionEndPointData>>());
      Assert.That(_relationEndPointManager.RelationEndPoints[endPointID], Is.SameAs(result));
      Assert.That(result.IsDataComplete, Is.False);
    }

    [Test]
    public void GetOrCreateVirtualEndPoint_Null_ObjectEndPoint ()
    {
      var endPointID = RelationEndPointID.Create(null, GetEndPointDefinition(typeof(Order), "OrderTicket"));
      Assert.That(_relationEndPointManager.RelationEndPoints[endPointID], Is.Null);

      var result = _relationEndPointManager.GetOrCreateVirtualEndPoint(endPointID);

      Assert.That(result, Is.TypeOf<NullVirtualObjectEndPoint>());
    }

    [Test]
    public void GetOrCreateVirtualEndPoint_Null_DomainObjectCollectionEndPoint ()
    {
      var endPointID = RelationEndPointID.Create(null, GetEndPointDefinition(typeof(Order), "OrderItems"));
      Assert.That(_relationEndPointManager.RelationEndPoints[endPointID], Is.Null);

      var result = _relationEndPointManager.GetOrCreateVirtualEndPoint(endPointID);

      Assert.That(result, Is.TypeOf<NullDomainObjectCollectionEndPoint>());
    }

    [Test]
    public void GetOrCreateVirtualEndPoint_Null_VirtualCollectionEndPoint ()
    {
      var endPointID = RelationEndPointID.Create(null, GetEndPointDefinition(typeof(Product), "Reviews"));
      Assert.That(_relationEndPointManager.RelationEndPoints[endPointID], Is.Null);

      var result = _relationEndPointManager.GetOrCreateVirtualEndPoint(endPointID);

      Assert.That(result, Is.TypeOf<NullVirtualCollectionEndPoint>());
    }

    [Test]
    public void GetOrCreateVirtualEndPoint_DoesNotSupportAnonymousEndPoints ()
    {
      var client = DomainObjectIDs.Client2.GetObject<Client>();
      var parentClientEndPointDefinition = client.ID.ClassDefinition.GetRelationEndPointDefinition(typeof(Client) + ".ParentClient");
      IRelationEndPoint unidirectionalEndPoint =
          _relationEndPointManager.GetRelationEndPointWithLazyLoad(RelationEndPointID.Create(client.ID, parentClientEndPointDefinition));

      Client parentClient = client.ParentClient;
      Assert.That(parentClient, Is.Not.Null);

      var anonymousEndPointDefinition = unidirectionalEndPoint.Definition.GetOppositeEndPointDefinition();
      Assert.That(
          () => _relationEndPointManager.GetOrCreateVirtualEndPoint(RelationEndPointID.Create(parentClient.ID, anonymousEndPointDefinition)),
          Throws.ArgumentException
              .With.ArgumentExceptionMessageEqualTo("GetOrCreateVirtualEndPoint cannot be called for anonymous end points.", "endPointID"));
    }

    [Test]
    public void GetOrCreateVirtualEndPoint_NonVirtualEndPoint ()
    {
      var endPointID = RelationEndPointID.Create(DomainObjectIDs.OrderItem1, typeof(OrderItem), "Order");
      Assert.That(
          () => _relationEndPointManager.GetOrCreateVirtualEndPoint(endPointID),
          Throws.ArgumentException
              .With.ArgumentExceptionMessageEqualTo("GetOrCreateVirtualEndPoint cannot be called for non-virtual end points.", "endPointID"));
    }

    [Test]
    public void CommitAllEndPoints_CommitsEndPoints ()
    {
      RelationEndPointID endPointID = RelationEndPointObjectMother.CreateRelationEndPointID(DomainObjectIDs.Customer1, "Orders");
      var endPointMock = new Mock<IRelationEndPoint>(MockBehavior.Strict);
      endPointMock.Setup(stub => stub.ID).Returns(endPointID);
      endPointMock.Setup(mock => mock.Commit()).Verifiable();

      RelationEndPointManagerTestHelper.AddEndPoint(_relationEndPointManager, endPointMock.Object);

      _relationEndPointManager.CommitAllEndPoints();

      endPointMock.Verify();
    }

    [Test]
    public void RollbackAllEndPoints_RollsbackEndPoints ()
    {
      RelationEndPointID endPointID = RelationEndPointObjectMother.CreateRelationEndPointID(DomainObjectIDs.Customer1, "Orders");
      var endPointMock = new Mock<IRelationEndPoint>(MockBehavior.Strict);
      endPointMock.Setup(stub => stub.ID).Returns(endPointID);
      endPointMock.Setup(mock => mock.Rollback()).Verifiable();

      RelationEndPointManagerTestHelper.AddEndPoint(_relationEndPointManager, endPointMock.Object);

      _relationEndPointManager.RollbackAllEndPoints();

      endPointMock.Verify();
    }

    [Test]
    public void Reset_RemovesEndPoints ()
    {
      var endPointID = RelationEndPointObjectMother.CreateRelationEndPointID(DomainObjectIDs.Customer1, "Orders");
      var endPointMock = new Mock<IRelationEndPoint>(MockBehavior.Strict);
      endPointMock.Setup(stub => stub.ID).Returns(endPointID);
      endPointMock.Setup(mock => mock.Rollback()).Verifiable();
      RelationEndPointManagerTestHelper.AddEndPoint(_relationEndPointManager, endPointMock.Object);
      Assert.That(_relationEndPointManager.RelationEndPoints, Is.Not.Empty.And.Member(endPointMock.Object));

      _relationEndPointManager.Reset();

      endPointMock.Verify();
      Assert.That(_relationEndPointManager.RelationEndPoints, Is.Empty);
    }

    [Test]
    public void Reset_RaisesUnregisteringEvents ()
    {
      var endPointID = RelationEndPointObjectMother.CreateRelationEndPointID(DomainObjectIDs.Customer1, "Orders");
      var endPointStub = new Mock<IRelationEndPoint>();
      endPointStub.Setup(stub => stub.ID).Returns(endPointID);
      RelationEndPointManagerTestHelper.AddEndPoint(_relationEndPointManager, endPointStub.Object);

      var listenerMock = ClientTransactionTestHelperWithMocks.CreateAndAddListenerMock(_relationEndPointManager.ClientTransaction);

      _relationEndPointManager.Reset();

      listenerMock.Verify(mock => mock.RelationEndPointMapUnregistering(_relationEndPointManager.ClientTransaction, endPointID), Times.AtLeastOnce());
    }
  }
}

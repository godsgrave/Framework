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
using Remotion.Data.DomainObjects.Queries;
using Remotion.Data.DomainObjects.Security.UnitTests.TestDomain;
using Remotion.Development.Data.UnitTesting.DomainObjects;
using Remotion.Security;

namespace Remotion.Data.DomainObjects.Security.UnitTests.SecurityClientTransactionExtensionTests
{
  [TestFixture]
  public class FilterQueryResultTest : TestBase
  {
    private TestHelper _testHelper;
    private IClientTransactionExtension _extension;

    public override void SetUp ()
    {
      base.SetUp();

      _testHelper = new TestHelper();
      _extension = new SecurityClientTransactionExtension();

      _testHelper.SetupSecurityIoCConfiguration();
    }

    public override void TearDown ()
    {
      _testHelper.TearDownSecurityIoCConfiguration();

      base.TearDown();
    }

    [Test]
    public void Test_WithNullValue_ReturnsNullValue ()
    {
      var query = new Mock<IQuery>();
      var queryResult = new QueryResult<DomainObject>(query.Object, new DomainObject[] { null });

      var finalResult = _extension.FilterQueryResult(_testHelper.Transaction, queryResult);

      _testHelper.VerifyAll();
      Assert.That(finalResult, Is.SameAs(queryResult));
    }

    [Test]
    public void Test_WithOneAllowedObject_ReturnsObject ()
    {
      SecurableObject allowedObject = _testHelper.CreateSecurableObject();
      var query = new Mock<IQuery>();
      var queryResult = new QueryResult<DomainObject>(query.Object, new DomainObject[] { allowedObject });
      _testHelper.ExpectObjectSecurityStrategyHasAccess(new MockSequence(), allowedObject, GeneralAccessTypes.Find, true);

      var finalResult = _extension.FilterQueryResult(_testHelper.Transaction, queryResult);

      _testHelper.VerifyAll();
      Assert.That(finalResult, Is.SameAs(queryResult));
    }

    [Test]
    public void Test_WithNoObjects_ReturnsNoObjects ()
    {
      _extension = new SecurityClientTransactionExtension();
      var query = new Mock<IQuery>();
      var queryResult = new QueryResult<DomainObject>(query.Object, new DomainObject[0]);

      var finalResult = _extension.FilterQueryResult(_testHelper.Transaction, queryResult);

      _testHelper.VerifyAll();
      Assert.That(finalResult.ToArray(), Is.Empty);
    }

    [Test]
    public void Test_WithOneDeniedObject_ReturnsEmptyResult ()
    {
      SecurableObject deniedObject = _testHelper.CreateSecurableObject();
      var query = new Mock<IQuery>();
      var queryResult = new QueryResult<DomainObject>(query.Object, new DomainObject[] { deniedObject });
      _testHelper.ExpectObjectSecurityStrategyHasAccess(new MockSequence(), deniedObject, GeneralAccessTypes.Find, false);

      var finalResult = _extension.FilterQueryResult(_testHelper.Transaction, queryResult);

      _testHelper.VerifyAll();
      Assert.That(finalResult.ToArray(), Is.Empty);
    }

    [Test]
    public void Test_WithOneAllowedAndOneDeniedObject_ReturnsOnlyAllowedObject ()
    {
      SecurableObject allowedObject = _testHelper.CreateSecurableObject();
      SecurableObject deniedObject = _testHelper.CreateSecurableObject();
      var query = new Mock<IQuery>();
      var queryResult = new QueryResult<DomainObject>(query.Object, new DomainObject[] { deniedObject, allowedObject });
      _testHelper.ExpectObjectSecurityStrategyHasAccess(new MockSequence(), deniedObject, GeneralAccessTypes.Find, false);
      _testHelper.ExpectObjectSecurityStrategyHasAccess(new MockSequence(), allowedObject, GeneralAccessTypes.Find, true);

      var finalResult = _extension.FilterQueryResult(_testHelper.Transaction, queryResult);

      _testHelper.VerifyAll();
      Assert.That(finalResult.ToArray(), Is.EqualTo(new[] { allowedObject }));
    }

    [Test]
    public void Test_WithNonSecurableObject_ReturnsObjects ()
    {
      NonSecurableObject nonSecurableObject = _testHelper.CreateNonSecurableObject();
      var query = new Mock<IQuery>();
      var queryResult = new QueryResult<DomainObject>(query.Object, new DomainObject[] { nonSecurableObject });

      var finalResult = _extension.FilterQueryResult(_testHelper.Transaction, queryResult);

      _testHelper.VerifyAll();
      Assert.That(finalResult.ToArray(), Is.EqualTo(new[] { nonSecurableObject }));
    }

    [Test]
    public void Test_WithinSecurityFreeSection_ReturnsQueryResultUnchanged ()
    {
      SecurableObject allowedObject = _testHelper.CreateSecurableObject();
      SecurableObject deniedObject = _testHelper.CreateSecurableObject();
      var query = new Mock<IQuery>();
      var queryResult = new QueryResult<DomainObject>(query.Object, new DomainObject[] { allowedObject, deniedObject });

      QueryResult<DomainObject> finalResult;
      using (SecurityFreeSection.Activate())
      {
        finalResult = _extension.FilterQueryResult(_testHelper.Transaction, queryResult);
      }

      _testHelper.VerifyAll();
      Assert.That(finalResult, Is.SameAs(queryResult));
    }

    [Test]
    public void Test_RecursiveSecurity_ChecksAccessOnNestedCall ()
    {
      SecurableObject allowedObject = _testHelper.CreateSecurableObject();
      SecurableObject deniedObject = _testHelper.CreateSecurableObject();
      IQuery nestedQuery = _testHelper.CreateSecurableObjectQuery();
      var nestedQueryResult = new QueryResult<DomainObject>(nestedQuery, new DomainObject[] { allowedObject, deniedObject });
      HasAccessDelegate hasAccess = delegate
      {
        var filteredResult = _extension.FilterQueryResult(_testHelper.Transaction, nestedQueryResult);
        Assert.That(filteredResult.ToArray(), Is.EqualTo(new[] { allowedObject }));
        return true;
      };
      _testHelper.ExpectObjectSecurityStrategyHasAccess(new MockSequence(), allowedObject, GeneralAccessTypes.Find, true);
      _testHelper.ExpectObjectSecurityStrategyHasAccess(new MockSequence(), deniedObject, GeneralAccessTypes.Find, false);

      SecurableObject securableObject = _testHelper.CreateSecurableObject();
      IQuery query = _testHelper.CreateSecurableObjectQuery();
      var queryResult = new QueryResult<DomainObject>(query, new DomainObject[] { securableObject });
      _testHelper.ExpectObjectSecurityStrategyHasAccess(securableObject, GeneralAccessTypes.Find, hasAccess);

      var finalResult = _extension.FilterQueryResult(_testHelper.Transaction, queryResult);

      _testHelper.VerifyAll();

      Assert.That(finalResult.ToArray(), Is.EqualTo(new[] { securableObject }));
    }

    [Test]
    public void Test_AccessedViaDomainObject ()
    {
      _testHelper.AddExtension(_extension);
      _testHelper.ExpectSecurityProviderGetAccess(SecurityContext.CreateStateless(typeof(SecurableObject)), GeneralAccessTypes.Find);

      _testHelper.Transaction.QueryManager.GetCollection(_testHelper.CreateSecurableObjectQuery());

      _testHelper.VerifyAll();
    }

    [Test]
    public void Test_WithSubtransaction_EventIsIgnored ()
    {
      SecurableObject deniedObject = _testHelper.CreateSecurableObject();
      var query = new Mock<IQuery>();
      var queryResult = new QueryResult<DomainObject>(query.Object, new DomainObject[] { deniedObject });

      var subTransaction = _testHelper.Transaction.CreateSubTransaction();
      var finalResult = _extension.FilterQueryResult(subTransaction, queryResult);
      subTransaction.Discard();

      _testHelper.VerifyAll();
      Assert.That(finalResult, Is.SameAs(queryResult));
    }

    [Test]
    public void Test_WithSubtransaction_ViaDomainObjectQuery_EventIsExecutedInRoot ()
    {
      _testHelper.AddExtension(_extension);
      _testHelper.ExpectSecurityProviderGetAccess(SecurityContext.CreateStateless(typeof(SecurableObject)), GeneralAccessTypes.Find);

      _testHelper.Transaction.QueryManager.GetCollection(_testHelper.CreateSecurableObjectQuery());

      _testHelper.VerifyAll();
    }

    [Test]
    public void Test_WithActiveTransactionMatchingTransactionPassedAsArgument_DoesNotCreateScope ()
    {
      SecurableObject securableObject = _testHelper.CreateSecurableObject();
      var query = new Mock<IQuery>();
      var queryResult = new QueryResult<DomainObject>(query.Object, new DomainObject[] { securableObject });

      using (var scope = _testHelper.Transaction.EnterNonDiscardingScope())
      {
        _testHelper.ExpectObjectSecurityStrategyHasAccessWithMatchingScope(securableObject, scope);

        _extension.FilterQueryResult(_testHelper.Transaction, queryResult);
      }

      _testHelper.VerifyAll();
    }

    [Test]
    public void Test_WithActiveTransactionNotMatchingTransactionPassedAsArgument_CreatesScope ()
    {
      SecurableObject securableObject = _testHelper.CreateSecurableObject();
      var query = new Mock<IQuery>();
      var queryResult = new QueryResult<DomainObject>(query.Object, new DomainObject[] { securableObject });
      _testHelper.ExpectObjectSecurityStrategyHasAccess(new MockSequence(), securableObject, GeneralAccessTypes.Find, true);

      using (ClientTransaction.CreateRootTransaction().EnterDiscardingScope())
      {
        _extension.FilterQueryResult(_testHelper.Transaction, queryResult);
      }

      _testHelper.VerifyAll();
    }

    [Test]
    public void Test_WithInactiveTransaction_CreatesScope ()
    {
      SecurableObject securableObject = _testHelper.CreateSecurableObject();
      var query = new Mock<IQuery>();
      var queryResult = new QueryResult<DomainObject>(query.Object, new DomainObject[] { securableObject });
      _testHelper.ExpectObjectSecurityStrategyHasAccess(new MockSequence(), securableObject, GeneralAccessTypes.Find, true);

      using (_testHelper.Transaction.EnterNonDiscardingScope())
      {
        using (ClientTransactionTestHelper.MakeInactive(_testHelper.Transaction))
        {
          _extension.FilterQueryResult(_testHelper.Transaction, queryResult);
        }
      }
      _testHelper.VerifyAll();
    }
  }
}

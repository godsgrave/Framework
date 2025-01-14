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
using Remotion.Utilities;

namespace Remotion.Data.DomainObjects
{
  /// <summary>
  /// Defines useful extension methods working on <see cref="ClientTransaction"/> instances.
  /// </summary>
  public static class ClientTransactionExtensions
  {
    /// <summary>
    /// Executes the specified delegate in the context of the <see cref="ClientTransaction"/>, returning the result of the delegate. While the
    /// delegate is being executed, the <see cref="ClientTransaction"/> is made the <see cref="ClientTransaction.Current"/> transaction and 
    /// <see cref="ClientTransaction.ActiveTransaction"/>, as if <see cref="ClientTransaction.EnterNonDiscardingScope"/> had been called.
    /// </summary>
    /// <typeparam name="T">The type of the value returned by the delegate.</typeparam>
    /// <param name="clientTransaction">The <see cref="ClientTransaction"/> in whose context to execute the delegate.</param>
    /// <param name="func">The delegate to be executed.</param>
    /// <returns>The result of <paramref name="func"/>.</returns>
    public static T ExecuteInScope<T> (this ClientTransaction clientTransaction, Func<T> func)
    {
      ArgumentUtility.CheckNotNull("clientTransaction", clientTransaction);
      ArgumentUtility.CheckNotNull("func", func);

      using (EnterScopeOnDemand(clientTransaction))
      {
        return func();
      }
    }

    /// <summary>
    /// Executes the specified delegate in the context of the <see cref="ClientTransaction"/>. While the
    /// delegate is being executed, the <see cref="ClientTransaction"/> is made the <see cref="ClientTransaction.Current"/> transaction and 
    /// <see cref="ClientTransaction.ActiveTransaction"/>, as if <see cref="ClientTransaction.EnterNonDiscardingScope"/> had been called.
    /// </summary>
    /// <param name="clientTransaction">The <see cref="ClientTransaction"/> in whose context to execute the delegate.</param>
    /// <param name="action">The delegate to be executed.</param>
    public static void ExecuteInScope (this ClientTransaction clientTransaction, Action action)
    {
      ArgumentUtility.CheckNotNull("clientTransaction", clientTransaction);
      ArgumentUtility.CheckNotNull("action", action);

      using (EnterScopeOnDemand(clientTransaction))
      {
        action();
      }
    }

    private static IDisposable? EnterScopeOnDemand (ClientTransaction clientTransaction)
    {
      if (clientTransaction.ActiveTransaction != clientTransaction)
        return clientTransaction.EnterNonDiscardingScope();

      if (ClientTransaction.Current != clientTransaction)
        return clientTransaction.EnterNonDiscardingScope();

      return null;
    }

    /// <summary>
    /// Returns whether at least one <see cref="DomainObject"/> in the <see cref="ClientTransaction"/> has been changed.
    /// </summary>
    /// <returns><see langword="true"/> if at least one <see cref="DomainObject"/> in the <see cref="ClientTransaction"/> has been changed; otherwise, <see langword="false"/>.</returns>
    public static bool HasChanged (this ClientTransaction clientTransaction)
    {
      ArgumentUtility.CheckNotNull("clientTransaction", clientTransaction);

      // Place tests in order of probability to reduce number of checks required until a match for a typical usage scenario
      return clientTransaction.HasObjectsWithState(state => state.IsChanged || state.IsNew || state.IsDeleted);
    }
  }
}

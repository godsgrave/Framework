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
using JetBrains.Annotations;
using Remotion.ObjectBinding.Web.Development.WebTesting.ControlObjects;
using Remotion.Utilities;
using Remotion.Web.Development.WebTesting;
using Remotion.Web.Development.WebTesting.ControlObjects;
using Remotion.Web.Development.WebTesting.ControlSelection;
using Remotion.Web.Development.WebTesting.ScreenshotCreation;
using Remotion.Web.Development.WebTesting.ScreenshotCreation.Fluent;

namespace Remotion.ObjectBinding.Web.Development.WebTesting.ScreenshotCreation.BocList
{
  /// <summary>
  /// Marker class for the screenshot fluent API. Represents a <see cref="BocListControlObjectBase{TRowControlObject,TCellControlObject}"/> cell.
  /// </summary>
  public class ScreenshotBocListCell<TList, TRow, TCell> : IControlHost, ISelfResolvable
      where TList : BocListControlObjectBase<TRow, TCell>, IControlObjectWithRows<TRow>
      where TRow : ControlObject, IControlObjectWithCells<TCell>
      where TCell : ControlObject
  {
    private readonly IFluentScreenshotElement<ScreenshotBocList<TList, TRow, TCell>> _fluentList;
    private readonly IFluentScreenshotElement<TCell> _fluentCell;

    public ScreenshotBocListCell (
        [NotNull] IFluentScreenshotElement<ScreenshotBocList<TList, TRow, TCell>> fluentList,
        [NotNull] IFluentScreenshotElement<TCell> fluentCell)
    {
      ArgumentUtility.CheckNotNull ("fluentList", fluentList);
      ArgumentUtility.CheckNotNull ("fluentCell", fluentCell);

      _fluentList = fluentList;
      _fluentCell = fluentCell;
    }

    public IFluentScreenshotElement<TCell> FluentCell
    {
      get { return _fluentCell; }
    }

    public IFluentScreenshotElement<ScreenshotBocList<TList, TRow, TCell>> FluentList
    {
      get { return _fluentList; }
    }

    public TCell Cell
    {
      get { return _fluentCell.Target; }
    }

    public TList List
    {
      get { return _fluentList.Target.List; }
    }

    /// <inheritdoc />
    public TControlObject GetControl<TControlObject> (IControlSelectionCommand<TControlObject> controlSelectionCommand)
        where TControlObject : ControlObject
    {
      ArgumentUtility.CheckNotNull ("controlSelectionCommand", controlSelectionCommand);

      return _fluentCell.Target.Children.GetControl (controlSelectionCommand);
    }

    /// <inheritdoc />
    public TControlObject GetControlOrNull<TControlObject> (IControlOptionalSelectionCommand<TControlObject> controlSelectionCommand)
        where TControlObject : ControlObject
    {
      ArgumentUtility.CheckNotNull ("controlSelectionCommand", controlSelectionCommand);

      return _fluentCell.Target.Children.GetControlOrNull (controlSelectionCommand);
    }

    /// <inheritdoc />
    public bool HasControl (IControlExistsCommand controlSelectionCommand)
    {
      ArgumentUtility.CheckNotNull ("controlSelectionCommand", controlSelectionCommand);

      return _fluentCell.Target.Children.HasControl (controlSelectionCommand);
    }

    /// <inheritdoc />
    public ResolvedScreenshotElement ResolveBrowserCoordinates ()
    {
      return _fluentCell.ResolveBrowserCoordinates();
    }

    /// <inheritdoc />
    public ResolvedScreenshotElement ResolveDesktopCoordinates (IBrowserContentLocator locator)
    {
      ArgumentUtility.CheckNotNull ("locator", locator);

      return _fluentCell.ResolveDesktopCoordinates (locator);
    }
  }
}
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
using Remotion.Web.ExecutionEngine;

namespace Remotion.Web.Development.WebTesting.TestSite.DirtyStates
{
  public partial class DirtyStateTest : WxePage
  {
    private bool IsDirtyFromPageState
    {
      get { return (bool?)ViewState[nameof(IsDirtyFromPageState)] == true;}
      set { ViewState[nameof(IsDirtyFromPageState)] = value; }
    }

    protected override void OnPreRender (EventArgs e)
    {
      IsDirty = IsDirtyFromPageState;
      RegisterControlForClientSideDirtyStateTracking(TextField.ClientID);

      base.OnPreRender(e);
    }

    protected override bool IsAbortConfirmationEnabled => false;

    protected void SetPageDirtyButton_OnClick (object sender, EventArgs e)
    {
      IsDirtyFromPageState = true;
    }

    protected void SetCurrentFunctionDirtyButton_OnClick (object sender, EventArgs e)
    {
      CurrentFunction.IsDirty = true;
    }

    protected void ExecuteSubFunctionButton_OnClick (object sender, EventArgs e)
    {
      if (!IsReturningPostBack)
        ExecuteFunction(new DirtyStateSubFunction(), WxeCallArguments.Default);
    }

    protected void ExecuteNextStepButton_OnClick (object sender, EventArgs e)
    {
      ExecuteNextStep();
    }

    protected void CancelExecutingFunctionButton_OnClick (object sender, EventArgs e)
    {
      throw new WxeUserCancelException();
    }


    protected void DisableDirtyStateOnCurrentPageButton_OnClick (object sender, EventArgs e)
    {
      EnableDirtyState = false;
    }

    protected void ExecuteSubFunctionWithDisabledDirtyStateButton_OnClick (object sender, EventArgs e)
    {
      if (!IsReturningPostBack)
      {
        var dirtyStateSubFunction = new DirtyStateSubFunction();
        dirtyStateSubFunction.DisableDirtyState();
        ExecuteFunction(dirtyStateSubFunction, WxeCallArguments.Default);
      }
    }
  }
}

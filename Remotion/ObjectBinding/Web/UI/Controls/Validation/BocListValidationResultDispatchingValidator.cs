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
using System.Linq;
using System.Web.UI;
using System.Web.UI.WebControls;
using JetBrains.Annotations;
using Remotion.FunctionalProgramming;
using Remotion.ObjectBinding.Validation;
using Remotion.Utilities;

namespace Remotion.ObjectBinding.Web.UI.Controls.Validation
{
  public sealed class BocListValidationResultDispatchingValidator : BaseValidator, IBusinessObjectBoundEditableWebControlValidationResultDispatcher
  {
    public BocListValidationResultDispatchingValidator ()
    {
    }

    public void DispatchValidationFailures (IBusinessObjectValidationResult validationResult)
    {
      ArgumentUtility.CheckNotNull("validationResult", validationResult);

      var bocListControl = GetControlToValidate();

      var validatorsMatchingToControls = EnumerableUtility.SelectRecursiveDepthFirst(
              bocListControl as Control,
              child => child.Controls.Cast<Control>().Where(item => !(item is INamingContainer)))
          .OfType<IBusinessObjectBoundEditableWebControlValidationResultDispatcher>();

      foreach (var validator in validatorsMatchingToControls)
        validator.DispatchValidationFailures(validationResult);
    }


    protected override bool EvaluateIsValid ()
    {
      // TODO RM-6056: Shows a validation error if IBusinessObjectValidationResult returned messages for this control.
      return true;
    }

    protected override bool ControlPropertiesValid ()
    {
      string controlToValidate = ControlToValidate;
      if (string.IsNullOrEmpty(controlToValidate))
        return base.ControlPropertiesValid();
      else
        return NamingContainer.FindControl(controlToValidate) != null;
    }

    [NotNull]
    private BocList GetControlToValidate ()
    {
      var control = NamingContainer.FindControl(ControlToValidate);
      var bocListControl = control as BocList;
      if (bocListControl == null)
      {
        throw new InvalidOperationException(
            $"'{nameof(BocListValidationResultDispatchingValidator)}' may only be applied to controls of type '{nameof(BocList)}'.");
      }

      return bocListControl;
    }
  }
}

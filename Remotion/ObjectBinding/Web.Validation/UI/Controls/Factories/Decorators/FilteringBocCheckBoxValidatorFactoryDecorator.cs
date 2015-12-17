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
using System.Web.UI.WebControls;
using Remotion.ObjectBinding.Web.UI.Controls;
using Remotion.ObjectBinding.Web.UI.Controls.BocBooleanValueImplementation;
using Remotion.ObjectBinding.Web.UI.Controls.BocBooleanValueImplementation.Validation;
using Remotion.Utilities;

namespace Remotion.ObjectBinding.Web.Validation.UI.Controls.Factories.Decorators
{
  /// <summary>
  /// Implements <see cref="IBocCheckBoxValidatorFactory"/> inteface and removes all validators not required when writing the value back into the control.
  /// This allows fluent validation to validate the business object in a domain context.
  /// </summary>
  /// <seealso cref="IBocCheckBoxValidatorFactory"/>
  public class FilteringBocCheckBoxValidatorFactoryDecorator : FilteringValidatorFactoryDecorator<IBocCheckBox>, IBocCheckBoxValidatorFactory
  {
    public FilteringBocCheckBoxValidatorFactoryDecorator (IBocValidatorFactory<IBocCheckBox> innerFactory)
        : base (innerFactory)
    {
    }

    public override bool UseValidator (IBocCheckBox control, BaseValidator validator)
    {
      ArgumentUtility.CheckNotNull ("control", control);
      ArgumentUtility.CheckNotNull ("validator", validator);

      return true;
    }
  }
}
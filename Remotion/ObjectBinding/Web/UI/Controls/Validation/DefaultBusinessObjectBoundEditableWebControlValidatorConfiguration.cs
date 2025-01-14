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
using Remotion.ServiceLocation;
using Remotion.Utilities;

namespace Remotion.ObjectBinding.Web.UI.Controls.Validation
{
  /// <summary>
  /// Default implementation of the <see cref="IBusinessObjectBoundEditableWebControlValidatorConfiguration"/> interface.
  /// </summary>
  /// <remarks>
  /// Enables optional validators for unbound business object controls.
  /// </remarks>
  [ImplementationFor(typeof(IBusinessObjectBoundEditableWebControlValidatorConfiguration), RegistrationType = RegistrationType.Single, Position = Position, Lifetime = LifetimeKind.Singleton)]
  public class DefaultBusinessObjectBoundEditableWebControlValidatorConfiguration : IBusinessObjectBoundEditableWebControlValidatorConfiguration
  {
    public const int Position = 0;

    public DefaultBusinessObjectBoundEditableWebControlValidatorConfiguration ()
    {
    }

    public bool AreOptionalValidatorsEnabled (IBusinessObjectBoundEditableWebControl control)
    {
      ArgumentUtility.CheckNotNull("control", control);

      if (control.DataSource == null)
        return true;

      if (control.Property == null)
        return true;

      return false;
    }
  }
}

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

namespace Remotion.ExtensibleEnums
{
  /// <summary>
  /// Defines extensions for <see cref="IExtensibleEnum"/>.
  /// </summary>
  public static class ExtensibleEnumExtensions
  {
    /// <summary>
    /// Gets the localized name of the value represented by this <see cref="IExtensibleEnum"/> value.
    /// </summary>
    [Obsolete("Retrieve IExtensibleEnumerationGlobalizationService from IoC container and use IExtensibleEnumerationGlobalizationService.GetExtensibleEnumerationValueDisplayName (...). (Version 1.13.223.0)", true)]
    public static string GetLocalizedName (this IExtensibleEnum extensibleEnum)
    {
      throw new NotSupportedException("Retrieve IExtensibleEnumerationGlobalizationService from IoC container and use IExtensibleEnumerationGlobalizationService.GetExtensibleEnumerationValueDisplayName (...). (Version 1.13.223.0)");
    }
  }
}

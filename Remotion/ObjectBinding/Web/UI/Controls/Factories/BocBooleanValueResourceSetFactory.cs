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
using Remotion.Globalization;
using Remotion.ObjectBinding.Web.UI.Controls.BocBooleanValueImplementation;
using Remotion.ObjectBinding.Web.UI.Controls.BocBooleanValueImplementation.Rendering;
using Remotion.ServiceLocation;
using Remotion.Utilities;
using Remotion.Web;
using Remotion.Web.Globalization;
using Remotion.Web.UI;

namespace Remotion.ObjectBinding.Web.UI.Controls.Factories
{
  /// <summary>
  /// Responsible for creating the <see cref="BocBooleanValue"/> <see cref="BocBooleanValueResourceSet"/>.
  /// </summary>
  [ImplementationFor(typeof(IBocBooleanValueResourceSetFactory), Lifetime = LifetimeKind.Singleton)]
  public class BocBooleanValueResourceSetFactory : IBocBooleanValueResourceSetFactory
  {
    private const string c_trueIcon = "sprite.svg#CheckBoxTrue";
    private const string c_falseIcon = "sprite.svg#CheckBoxFalse";
    private const string c_nullIcon = "sprite.svg#CheckBoxNull";
    private const string c_defaultResourceGroup = "default";

    private readonly IResourceUrlFactory _resourceUrlFactory;

    public BocBooleanValueResourceSetFactory (IResourceUrlFactory resourceUrlFactory)
    {
      ArgumentUtility.CheckNotNull("resourceUrlFactory", resourceUrlFactory);

      _resourceUrlFactory = resourceUrlFactory;
    }

    public BocBooleanValueResourceSet CreateResourceSet (IBocBooleanValue control)
    {
      ArgumentUtility.CheckNotNull("control", control);

      return control.CreateResourceSet() ?? CreateDefaultResourceSet(control);
    }

    private BocBooleanValueResourceSet CreateDefaultResourceSet (IBocBooleanValue control)
    {
      IResourceManager resourceManager = control.GetResourceManager();

      BocBooleanValueResourceSet resourceSet = new BocBooleanValueResourceSet(
          c_defaultResourceGroup,
          GetResourceUrl(c_trueIcon),
          GetResourceUrl(c_falseIcon),
          GetResourceUrl(c_nullIcon),
          resourceManager.GetText(BocBooleanValue.ResourceIdentifier.TrueDescription),
          resourceManager.GetText(BocBooleanValue.ResourceIdentifier.FalseDescription),
          resourceManager.GetText(BocBooleanValue.ResourceIdentifier.NullDescription)
          );

      return resourceSet;
    }

    private string GetResourceUrl (string icon)
    {
      return _resourceUrlFactory.CreateThemedResourceUrl(typeof(HtmlHeadAppenderExtensions), ResourceType.Image, icon).GetUrl();
    }
  }
}

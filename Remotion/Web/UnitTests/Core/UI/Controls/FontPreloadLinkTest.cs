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
using Remotion.Web.UI.Controls;

namespace Remotion.Web.UnitTests.Core.UI.Controls
{
  [TestFixture]
  public class FontPreloadLinkTest
  {
    private HtmlHelper _htmlHelper;

    [SetUp]
    public void SetUp ()
    {
      _htmlHelper = new HtmlHelper();
    }

    [Test]
    public void Render ()
    {
      var resourceUrl = new Mock<IResourceUrl>();
      resourceUrl.Setup(stub => stub.GetUrl()).Returns("myfont.ttf");

      var javaScriptInclude = new FontPreloadLink(resourceUrl.Object, "font-type");

      javaScriptInclude.Render(_htmlHelper.Writer);

      var document = _htmlHelper.GetResultDocument();
      var element = _htmlHelper.GetAssertedChildElement(document, "link", 0);
      _htmlHelper.AssertAttribute(element, "rel", "preload");
      _htmlHelper.AssertAttribute(element, "as", "font");
      _htmlHelper.AssertAttribute(element, "type", "font-type");
      _htmlHelper.AssertAttribute(element, "href", "myfont.ttf");
    }
  }
}

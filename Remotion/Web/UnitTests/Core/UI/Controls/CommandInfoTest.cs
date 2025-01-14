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
using System.IO;
using System.Web.UI;
using NUnit.Framework;
using Remotion.Web.Contracts.DiagnosticMetadata;
using Remotion.Web.UI.Controls;
using Remotion.Web.UI.Controls.Rendering;

namespace Remotion.Web.UnitTests.Core.UI.Controls
{
  [TestFixture]
  public class CommandInfoTest
  {
    [Test]
    public void CreateForNone_WithoutTabIndex ()
    {
      var commandInfo = CommandInfo.CreateForNone(false);

      Assert.That(commandInfo.Title, Is.Null);
      Assert.That(commandInfo.AccessKey, Is.Null);
      Assert.That(commandInfo.OnClick, Is.Null);
      Assert.That(commandInfo.Href, Is.Null);
      Assert.That(commandInfo.Target, Is.Null);
      Assert.That(commandInfo.TabIndex, Is.Null);
    }

    [Test]
    public void CreateForNone_WithTabIndex ()
    {
      var commandInfo = CommandInfo.CreateForNone(true);

      Assert.That(commandInfo.Title, Is.Null);
      Assert.That(commandInfo.AccessKey, Is.Null);
      Assert.That(commandInfo.OnClick, Is.Null);
      Assert.That(commandInfo.Href, Is.Null);
      Assert.That(commandInfo.Target, Is.Null);
      Assert.That(commandInfo.TabIndex, Is.EqualTo(0));
    }

    [Test]
    public void CreateForPostBack ()
    {
      var commandInfo = CommandInfo.CreateForPostBack("TheTitle", "A", "ClickHandler");

      Assert.That(commandInfo.Title, Is.EqualTo("TheTitle"));
      Assert.That(commandInfo.AccessKey, Is.EqualTo("A"));
      Assert.That(commandInfo.OnClick, Is.EqualTo("ClickHandler"));
      Assert.That(commandInfo.Href, Is.EqualTo("#"));
      Assert.That(commandInfo.Target, Is.Null);
    }

    [Test]
    public void CreateForPostBack_OnClickOnly ()
    {
      var commandInfo = CommandInfo.CreateForPostBack(null, null, "ClickHandler");

      Assert.That(commandInfo.Title, Is.Null);
      Assert.That(commandInfo.AccessKey, Is.Null);
      Assert.That(commandInfo.OnClick, Is.EqualTo("ClickHandler"));
      Assert.That(commandInfo.Href, Is.EqualTo("#"));
      Assert.That(commandInfo.Target, Is.Null);
    }

    [Test]
    public void CreateForLink ()
    {
      var commandInfo = CommandInfo.CreateForLink("TheTitle", "A", "Url", "TheTarget", "ClickHandler");

      Assert.That(commandInfo.Title, Is.EqualTo("TheTitle"));
      Assert.That(commandInfo.AccessKey, Is.EqualTo("A"));
      Assert.That(commandInfo.OnClick, Is.EqualTo("ClickHandler"));
      Assert.That(commandInfo.Href, Is.EqualTo("Url"));
      Assert.That(commandInfo.Target, Is.EqualTo("TheTarget"));
    }

    [Test]
    public void CreateForLink_HrefOnly ()
    {
      var commandInfo = CommandInfo.CreateForLink(null, null, "Url", null, null);

      Assert.That(commandInfo.Title, Is.Null);

      Assert.That(commandInfo.OnClick, Is.Null);
      Assert.That(commandInfo.Href, Is.EqualTo("Url"));
      Assert.That(commandInfo.Target, Is.Null);
    }

    [Test]
    public void AddAttributesToRender_None_WithoutTabIndex ()
    {
      var commandInfo = CommandInfo.CreateForNone(false);

      var stringWriter = new StringWriter();
      var htmlTextWriter = new HtmlTextWriter(stringWriter);

      commandInfo.AddAttributesToRender(htmlTextWriter, RenderingFeatures.Default);
      htmlTextWriter.RenderBeginTag(HtmlTextWriterTag.A);
      htmlTextWriter.RenderEndTag();

      var result = stringWriter.ToString();

      Assert.That(result, Is.EqualTo("<a></a>"));
    }

    [Test]
    public void AddAttributesToRender_None_WithTabIndex ()
    {
      var commandInfo = CommandInfo.CreateForNone(true);

      var stringWriter = new StringWriter();
      var htmlTextWriter = new HtmlTextWriter(stringWriter);

      commandInfo.AddAttributesToRender(htmlTextWriter, RenderingFeatures.Default);
      htmlTextWriter.RenderBeginTag(HtmlTextWriterTag.A);
      htmlTextWriter.RenderEndTag();

      var result = stringWriter.ToString();

      Assert.That(result, Is.EqualTo("<a tabindex=\"0\"></a>"));
    }

    [Test]
    public void AddAttributesToRender ()
    {
      var commandInfo = CommandInfo.CreateForLink("TheTitle", "A", "Url", "TheTarget", "ClickHandler");

      var stringWriter = new StringWriter();
      var htmlTextWriter = new HtmlTextWriter(stringWriter);

      commandInfo.AddAttributesToRender(htmlTextWriter, RenderingFeatures.Default);

      htmlTextWriter.RenderBeginTag(HtmlTextWriterTag.A);
      htmlTextWriter.RenderEndTag();

      var result = stringWriter.ToString();

      Assert.That(result, Does.Contain("title=\"TheTitle\""));
      Assert.That(result, Does.Contain("accesskey=\"A\""));
      Assert.That(result, Does.Contain("onclick=\"ClickHandler\""));
      Assert.That(result, Does.Contain("href=\"Url\""));
      Assert.That(result, Does.Contain("target=\"TheTarget\""));
    }

    [Test]
    public void AddAttributesToRender_HrefOnly ()
    {
      var commandInfo = CommandInfo.CreateForLink(null, null, "Url", null, null);

      var stringWriter = new StringWriter();
      var htmlTextWriter = new HtmlTextWriter(stringWriter);

      commandInfo.AddAttributesToRender(htmlTextWriter, RenderingFeatures.Default);

      htmlTextWriter.RenderBeginTag(HtmlTextWriterTag.A);
      htmlTextWriter.RenderEndTag();

      var result = stringWriter.ToString();

      Assert.That(result, Does.Not.Contains("title="));
      Assert.That(result, Does.Not.Contains("accesskey="));
      Assert.That(result, Does.Not.Contains("onclick="));
      Assert.That(result, Does.Contain("href=\"Url\""));
      Assert.That(result, Does.Not.Contains("target="));
    }

    [Test]
    public void AddAttributesToRender_OnClickOnly ()
    {
      var commandInfo = CommandInfo.CreateForPostBack(null, null, "ClickHandler");

      var stringWriter = new StringWriter();
      var htmlTextWriter = new HtmlTextWriter(stringWriter);

      commandInfo.AddAttributesToRender(htmlTextWriter, RenderingFeatures.Default);

      htmlTextWriter.RenderBeginTag(HtmlTextWriterTag.A);
      htmlTextWriter.RenderEndTag();

      var result = stringWriter.ToString();

      Assert.That(result, Does.Not.Contains("title="));
      Assert.That(result, Does.Not.Contains("accesskey="));
      Assert.That(result, Does.Contain("onclick=\"ClickHandler\""));
      Assert.That(result, Does.Contain("href=\"#\""));
      Assert.That(result, Does.Not.Contains("target="));
    }

    [Test]
    public void AddAttributesToRender_EncodesTitle ()
    {
      var commandInfo = CommandInfo.CreateForLink("TheTitle\"Space'", null, "Url", "TheTarget", "ClickHandler");

      var stringWriter = new StringWriter();
      var htmlTextWriter = new HtmlTextWriter(stringWriter);

      commandInfo.AddAttributesToRender(htmlTextWriter, RenderingFeatures.Default);

      htmlTextWriter.RenderBeginTag(HtmlTextWriterTag.A);
      htmlTextWriter.RenderEndTag();

      var result = stringWriter.ToString();

      Assert.That(result, Does.Contain("title=\"TheTitle&quot;Space&#39;\""));
    }

    [Test]
    public void AddAttributesToRender_EncodesAccesskey ()
    {
      var commandInfo = CommandInfo.CreateForLink("TheTitle\"Space'", "\'", "Url", "TheTarget", "ClickHandler");

      var stringWriter = new StringWriter();
      var htmlTextWriter = new HtmlTextWriter(stringWriter);

      commandInfo.AddAttributesToRender(htmlTextWriter, RenderingFeatures.Default);

      htmlTextWriter.RenderBeginTag(HtmlTextWriterTag.A);
      htmlTextWriter.RenderEndTag();

      var result = stringWriter.ToString();

      Assert.That(result, Does.Contain("accesskey=\"&#39;"));
    }

    [Test]
    public void AddAttributesToRender_EncodesOnClick ()
    {
      var commandInfo = CommandInfo.CreateForLink("TheTitle", null, "Url", "TheTarget", "ClickHandler\"Space'");

      var stringWriter = new StringWriter();
      var htmlTextWriter = new HtmlTextWriter(stringWriter);

      commandInfo.AddAttributesToRender(htmlTextWriter, RenderingFeatures.Default);

      htmlTextWriter.RenderBeginTag(HtmlTextWriterTag.A);
      htmlTextWriter.RenderEndTag();

      var result = stringWriter.ToString();

      Assert.That(result, Does.Contain("onclick=\"ClickHandler&quot;Space&#39;\""));
    }

    [Test]
    public void AddAttributesToRender_EncodesHref ()
    {
      var commandInfo = CommandInfo.CreateForLink("TheTitle", null, "Url\"Space'", "TheTarget", "ClickHandler");

      var stringWriter = new StringWriter();
      var htmlTextWriter = new HtmlTextWriter(stringWriter);

      commandInfo.AddAttributesToRender(htmlTextWriter, RenderingFeatures.Default);

      htmlTextWriter.RenderBeginTag(HtmlTextWriterTag.A);
      htmlTextWriter.RenderEndTag();

      var result = stringWriter.ToString();

      Assert.That(result, Does.Contain("href=\"Url&quot;Space&#39;\""));
    }

    [Test]
    public void AddAttributesToRender_DoesNotEncodeTarget ()
    {
      var commandInfo = CommandInfo.CreateForLink("TheTitle", null, "Url", "TheTarget\"Space'", "ClickHandler");

      var stringWriter = new StringWriter();
      var htmlTextWriter = new HtmlTextWriter(stringWriter);

      commandInfo.AddAttributesToRender(htmlTextWriter, RenderingFeatures.Default);

      htmlTextWriter.RenderBeginTag(HtmlTextWriterTag.A);
      htmlTextWriter.RenderEndTag();

      var result = stringWriter.ToString();

      Assert.That(result, Does.Contain("target=\"TheTarget\"Space'\""));
    }

    [Test]
    public void AddDiagnosticMetadataAttributes_FromCreateForLink ()
    {
      var commandInfo = CommandInfo.CreateForLink("TheTitle", null, "http://localhost/My.wxe", "TheTarget", "javascript:Foo();");

      var stringWriter = new StringWriter();
      var htmlTextWriter = new HtmlTextWriter(stringWriter);
      commandInfo.AddAttributesToRender(htmlTextWriter, RenderingFeatures.WithDiagnosticMetadata);

      htmlTextWriter.RenderBeginTag(HtmlTextWriterTag.A);
      htmlTextWriter.RenderEndTag();

      var result = stringWriter.ToString();

      Assert.That(result, Does.Contain(DiagnosticMetadataAttributes.ControlType + "=\"Command\""));
      Assert.That(result, Does.Contain(DiagnosticMetadataAttributes.TriggersPostBack + "=\"false\""));
      Assert.That(result, Does.Contain(DiagnosticMetadataAttributes.TriggersNavigation + "=\"true\""));
    }

    [Test]
    public void AddDiagnosticMetadataAttributes_FromCreateForLink_WithDoPostBack ()
    {
      var commandInfo = CommandInfo.CreateForLink("TheTitle", null, "#", "TheTarget", "FrontGarbage__doPostBackBackGarbage");

      var stringWriter = new StringWriter();
      var htmlTextWriter = new HtmlTextWriter(stringWriter);
      commandInfo.AddAttributesToRender(htmlTextWriter, RenderingFeatures.WithDiagnosticMetadata);

      htmlTextWriter.RenderBeginTag(HtmlTextWriterTag.A);
      htmlTextWriter.RenderEndTag();

      var result = stringWriter.ToString();

      Assert.That(result, Does.Contain(DiagnosticMetadataAttributes.ControlType + "=\"Command\""));
      Assert.That(result, Does.Contain(DiagnosticMetadataAttributes.TriggersPostBack + "=\"true\""));
      Assert.That(result, Does.Contain(DiagnosticMetadataAttributes.TriggersNavigation + "=\"false\""));
    }

    [Test]
    public void AddDiagnosticMetadataAttributes_FromCreateForLink_WithPureJavaScript ()
    {
      var commandInfo = CommandInfo.CreateForLink("TheTitle", null, "#", "TheTarget", "javascript:Foo();");

      var stringWriter = new StringWriter();
      var htmlTextWriter = new HtmlTextWriter(stringWriter);
      commandInfo.AddAttributesToRender(htmlTextWriter, RenderingFeatures.WithDiagnosticMetadata);

      htmlTextWriter.RenderBeginTag(HtmlTextWriterTag.A);
      htmlTextWriter.RenderEndTag();

      var result = stringWriter.ToString();

      Assert.That(result, Does.Contain(DiagnosticMetadataAttributes.ControlType + "=\"Command\""));
      Assert.That(result, Does.Contain(DiagnosticMetadataAttributes.TriggersPostBack + "=\"false\""));
      Assert.That(result, Does.Contain(DiagnosticMetadataAttributes.TriggersNavigation + "=\"false\""));
    }

    [Test]
    public void AddDiagnosticMetadataAttributes_FromCreateForPostBack_WithDoPostBack ()
    {
      var commandInfo = CommandInfo.CreateForPostBack("TheTitle", null, "Garbage__doPostBackGarbage");

      var stringWriter = new StringWriter();
      var htmlTextWriter = new HtmlTextWriter(stringWriter);
      commandInfo.AddAttributesToRender(htmlTextWriter, RenderingFeatures.WithDiagnosticMetadata);

      htmlTextWriter.RenderBeginTag(HtmlTextWriterTag.A);
      htmlTextWriter.RenderEndTag();

      var result = stringWriter.ToString();

      Assert.That(result, Does.Contain(DiagnosticMetadataAttributes.ControlType + "=\"Command\""));
      Assert.That(result, Does.Contain(DiagnosticMetadataAttributes.TriggersPostBack + "=\"true\""));
      Assert.That(result, Does.Contain(DiagnosticMetadataAttributes.TriggersNavigation + "=\"false\""));
    }

    [Test]
    public void AddDiagnosticMetadataAttributes_FromCreateForPostBack_WithPureJavascript ()
    {
      var commandInfo = CommandInfo.CreateForPostBack("TheTitle", null, "foo();");

      var stringWriter = new StringWriter();
      var htmlTextWriter = new HtmlTextWriter(stringWriter);
      commandInfo.AddAttributesToRender(htmlTextWriter, RenderingFeatures.WithDiagnosticMetadata);

      htmlTextWriter.RenderBeginTag(HtmlTextWriterTag.A);
      htmlTextWriter.RenderEndTag();

      var result = stringWriter.ToString();

      Assert.That(result, Does.Contain(DiagnosticMetadataAttributes.ControlType + "=\"Command\""));
      Assert.That(result, Does.Contain(DiagnosticMetadataAttributes.TriggersPostBack + "=\"false\""));
      Assert.That(result, Does.Contain(DiagnosticMetadataAttributes.TriggersNavigation + "=\"false\""));
    }

    [Test]
    public void AddDiagnosticMetadataAttributes_FromCreateForNone ()
    {
      var commandInfo = CommandInfo.CreateForNone(false);

      var stringWriter = new StringWriter();
      var htmlTextWriter = new HtmlTextWriter(stringWriter);
      commandInfo.AddAttributesToRender(htmlTextWriter, RenderingFeatures.WithDiagnosticMetadata);

      htmlTextWriter.RenderBeginTag(HtmlTextWriterTag.A);
      htmlTextWriter.RenderEndTag();

      var result = stringWriter.ToString();

      Assert.That(result, Does.Contain(DiagnosticMetadataAttributes.ControlType + "=\"Command\""));
      Assert.That(result, Does.Contain(DiagnosticMetadataAttributes.IsDisabled + "=\"true\""));
      Assert.That(result, Does.Not.Contains(DiagnosticMetadataAttributes.TriggersPostBack));
      Assert.That(result, Does.Not.Contains(DiagnosticMetadataAttributes.TriggersNavigation));
    }
  }
}

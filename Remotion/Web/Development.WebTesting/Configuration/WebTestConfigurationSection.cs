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
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using Remotion.Utilities;

namespace Remotion.Web.Development.WebTesting.Configuration
{
  /// <summary>
  /// Loads the app.config and validates the parameter. Serves as DTO Object to transfer the app.config settings to the finer grained config classes.
  /// </summary>
  public class WebTestConfigurationSection : ConfigurationSection
  {
    private static readonly Lazy<WebTestConfigurationSection> s_current;

    private readonly ConfigurationPropertyCollection _properties;
    private readonly ConfigurationProperty _browserProperty;
    private readonly ConfigurationProperty _searchTimeoutProperty;
    private readonly ConfigurationProperty _retryIntervalProperty;
    private readonly ConfigurationProperty _webApplicationRootProperty;
    private readonly ConfigurationProperty _screenshotDirectoryProperty;
    private readonly ConfigurationProperty _logsDirectoryProperty;
    private readonly ConfigurationProperty _closeBrowserWindowsOnSetUpAndTearDownProperty;
    private readonly ConfigurationProperty _hostingProperty;

    static WebTestConfigurationSection ()
    {
      s_current = new Lazy<WebTestConfigurationSection> (
          () =>
          {
            var configuration = (WebTestConfigurationSection) ConfigurationManager.GetSection ("remotion.webTesting");
            Assertion.IsNotNull (configuration, "Configuration section 'remotion.webTesting' missing.");
            return configuration;
          });
    }

    private WebTestConfigurationSection ()
    {
      _browserProperty = new ConfigurationProperty (
          "browser",
          typeof (string),
          null,
          null,
          new StringValidator (minLength: 1),
          ConfigurationPropertyOptions.IsRequired);
      _searchTimeoutProperty = new ConfigurationProperty ("searchTimeout", typeof (TimeSpan), null, ConfigurationPropertyOptions.IsRequired);
      _retryIntervalProperty = new ConfigurationProperty ("retryInterval", typeof (TimeSpan), null, ConfigurationPropertyOptions.IsRequired);
      _webApplicationRootProperty = new ConfigurationProperty (
          "webApplicationRoot",
          typeof (string),
          null,
          null,
          new StringValidator (minLength: 1),
          ConfigurationPropertyOptions.IsRequired);
      _screenshotDirectoryProperty = new ConfigurationProperty ("screenshotDirectory", typeof (string));
      _logsDirectoryProperty = new ConfigurationProperty ("logsDirectory", typeof (string), ".");
      _closeBrowserWindowsOnSetUpAndTearDownProperty = new ConfigurationProperty ("closeBrowserWindowsOnSetUpAndTearDown", typeof (bool), false);
      _hostingProperty = new ConfigurationProperty ("hosting", typeof (ProviderSettings));
      
      _properties = new ConfigurationPropertyCollection
                    {
                        _browserProperty,
                        _searchTimeoutProperty,
                        _retryIntervalProperty,
                        _webApplicationRootProperty,
                        _screenshotDirectoryProperty,
                        _logsDirectoryProperty,
                        _closeBrowserWindowsOnSetUpAndTearDownProperty,
                        _hostingProperty
                    };
    }

    /// <summary>
    /// Internal method to access app.config webtesting section values. External projects needing to access configuration should use <see cref="WebTestConfigurationFactory"/>
    /// </summary>
    internal static WebTestConfigurationSection Current
    {
      get { return s_current.Value; }
    }

    protected override ConfigurationPropertyCollection Properties
    {
      get { return _properties; }
    }

    /// <summary>
    /// Browser in which the web tests are run.
    /// </summary>
    public string BrowserName
    {
      get { return (string) this [_browserProperty]; }
    }

    /// <summary>
    /// Specifies how long the Coypu engine should maximally search for a web element or try to interact with a web element before it fails.
    /// </summary>
    public TimeSpan SearchTimeout
    {
      get { return (TimeSpan) this [_searchTimeoutProperty]; }
    }

    /// <summary>
    /// Whenever the element to be interacted with is not ready, visible or otherwise not present, the Coypu engine automatically retries the action
    /// after the given <see cref="RetryInterval"/> until the <see cref="SearchTimeout"/> has been reached.
    /// </summary>
    public TimeSpan RetryInterval
    {
      get { return (TimeSpan) this [_retryIntervalProperty]; }
    }

    /// <summary>
    /// URL to which the web application under test has been published.
    /// </summary>
    public string WebApplicationRoot
    {
      get { return (string) this [_webApplicationRootProperty]; }
    }

    /// <summary>
    /// Absolute or relative path to the screenshot directory. The web testing framework automatically takes two screenshots (one of the whole desktop
    /// and one of the browser window) in case a web test failed.
    /// </summary>
    public string ScreenshotDirectory
    {
      get { return Path.GetFullPath ((string) this [_screenshotDirectoryProperty]); }
    }

    /// <summary>
    /// Absolute or relative path to the logs directory. Some web driver implementations write log files for debugging reasons.
    /// </summary>
    public string LogsDirectory
    {
      get { return (string) this [_logsDirectoryProperty]; }
    }

    /// <summary>
    /// Some Selenium web driver implementations may become confused when searching for windows if there are other browser windows present. Typically
    /// you want to turn this auto-close option on when running web tests, on developer machines, however, this may unexpectedly close important
    /// browser windows, which is why the default value is set to <see langword="false" />.
    /// </summary>
    public bool CloseBrowserWindowsOnSetUpAndTearDown
    {
      get { return (bool) this [_closeBrowserWindowsOnSetUpAndTearDownProperty]; }
    }

    public ProviderSettings HostingProviderSettings
    {
      get { return (ProviderSettings) this [_hostingProperty]; }
    }
  }
}
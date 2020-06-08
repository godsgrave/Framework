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
using System.Collections.Generic;
using System.Reflection;
using Remotion.Utilities;

namespace Remotion.Reflection.TypeDiscovery.AssemblyLoading
{
  /// <summary>
  /// Filters the assemblies loaded during type discovery by name, excluding those whose names resemble system assemblies or assemblies
  /// generated by the mixin engine. In addition, assemblies that have the <see cref="NonApplicationAssemblyAttribute"/> defined are also excluded.
  /// Note that assemblies have to be loaded in memory in order to check whether the attribute is present. This will also lock the assembly files.
  /// </summary>
  /// <remarks>
  /// The name-based filtering by default excludes the following assembly name patterns:
  /// <list type="bullet">
  ///   <item>mscorlib</item>
  ///   <item>System</item>
  ///   <item>System\..*</item>
  ///   <item>Microsoft\..*</item>
  ///   <item>Remotion\..*\.Generated\..*</item>
  /// </list>
  /// This list might change in the future, and it can be extended via <see cref="AddIgnoredAssembly"/>.
  /// </remarks>
  /// <threadsafety static="true" instance="true" />
  public class ApplicationAssemblyLoaderFilter : IAssemblyLoaderFilter
  {
    /// <summary>
    /// Returns the global instance of this filter. There is only this one instance of the filter in an application.
    /// </summary>
    public static readonly ApplicationAssemblyLoaderFilter Instance = new ApplicationAssemblyLoaderFilter();

    private static string MakeMatchExpression (IEnumerable<string> assemblyMatchStrings)
    {
      ArgumentUtility.CheckNotNull ("assemblyMatchStrings", assemblyMatchStrings);

      return "^((" + string.Join (")|(", assemblyMatchStrings) + "))$";
    }

    //TODO RM-7434: Mark with MemberNotNull once supported by msbuild
    private List<string> _nonApplicationAssemblyNames = default!;

    private RegexAssemblyLoaderFilter? _assemblyNameFilter;
    private readonly object _assemblyNameFilterLock = new object();

    private ApplicationAssemblyLoaderFilter ()
    {
      Reset();
    }

    /// <summary>
    /// Resets the name filter to the default names, removing any that have been added via <see cref="AddIgnoredAssembly"/>.
    /// </summary>
    public void Reset ()
    {
      lock (_assemblyNameFilterLock)
      {
        _nonApplicationAssemblyNames = new List<string> (
            new[]
            {
                @"mscorlib",
                @"System",
                @"System\..*",
                @"Microsoft\..*",
                @"Remotion\..*\.Generated\..*",
                @"TypePipe_.*Generated.*",
            });
        _assemblyNameFilter = null;
      }
    }

    /// <summary>
    /// Gets the regular expression string applied to assembly names in order to check whether they are system assemblies.
    /// </summary>
    /// <value>The system assembly regular expression string.</value>
    public string SystemAssemblyMatchExpression
    {
      get { return GetAssemblyNameFilter().MatchExpressionString; }
    }

    /// <summary>
    /// Adds the given regular expression to the list of assembly names to be excluded.
    /// </summary>
    /// <param name="simpleNameRegularExpression">A regular expression matching the simple names of assemblies to be excluded.</param>
    public void AddIgnoredAssembly (string simpleNameRegularExpression)
    {
      ArgumentUtility.CheckNotNullOrEmpty ("simpleNameRegularExpression", simpleNameRegularExpression);
      lock (_assemblyNameFilterLock)
      {
        _nonApplicationAssemblyNames.Add (simpleNameRegularExpression);
        _assemblyNameFilter = null;
      }
    }

    /// <summary>
    /// Determines whether the assembly of the given name should be considered for inclusion by the <see cref="FilteringAssemblyLoader"/>.
    /// An assembly is only considered for inclusion if it is not identified as a system assembly by matching its name against the 
    /// <see cref="SystemAssemblyMatchExpression"/>.
    /// </summary>
    /// <param name="assemblyName">The name of the assembly to be checked.</param>
    /// <returns>
    /// 	<see langword="true"/> unless the <paramref name="assemblyName"/> matches the <see cref="SystemAssemblyMatchExpression"/>.
    /// </returns>
    /// <remarks>This is the first step of a two-step filtering protocol. Assemblies accepted by this method can still be filtered via 
    /// <see cref="ShouldIncludeAssembly"/>.</remarks>
    public bool ShouldConsiderAssembly (AssemblyName assemblyName)
    {
      ArgumentUtility.CheckNotNull ("assemblyName", assemblyName);
      return !GetAssemblyNameFilter().ShouldConsiderAssembly (assemblyName);
    }

    /// <summary>
    /// Determines whether the given assembly should be included in the list of assemblies returned by the <see cref="FilteringAssemblyLoader"/>.
    /// An assembly is only included if it does not have the <see cref="NonApplicationAssemblyAttribute"/> defined.
    /// </summary>
    /// <param name="assembly">The assembly to be checked.</param>
    /// <returns>
    /// 	<see langword="true"/> unless the <paramref name="assembly"/> has the <see cref="NonApplicationAssemblyAttribute"/> defined.
    /// </returns>
    /// <remarks>This is the second step of a two-step filtering protocol. Only assemblies not rejected by <see cref="ShouldConsiderAssembly"/> are
    /// passed on to this step.</remarks>
    public bool ShouldIncludeAssembly (Assembly assembly)
    {
      ArgumentUtility.CheckNotNull ("assembly", assembly);
      return !assembly.IsDefined (typeof (NonApplicationAssemblyAttribute), false);
    }

    private RegexAssemblyLoaderFilter GetAssemblyNameFilter ()
    {
      lock (_assemblyNameFilterLock)
      {
        if (_assemblyNameFilter == null)
        {
          string matchExpression = MakeMatchExpression (_nonApplicationAssemblyNames);
          _assemblyNameFilter = new RegexAssemblyLoaderFilter (matchExpression, RegexAssemblyLoaderFilter.MatchTargetKind.SimpleName);
        }
        return _assemblyNameFilter;
      }
    }
  }
}

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
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using Remotion.Utilities;

namespace Remotion.Data.DomainObjects.DomainImplementation.Transport
{
  /// <summary>
  /// Represents an import strategy for <see cref="DomainObject"/> instances using binary serialization. This matches <see cref="BinaryExportStrategy"/>.
  /// </summary>
  public class BinaryImportStrategy : IImportStrategy
  {
    public static readonly BinaryImportStrategy Instance = new BinaryImportStrategy();

    public IEnumerable<TransportItem> Import (Stream inputStream)
    {
      ArgumentUtility.CheckNotNull("inputStream", inputStream);

      var formatter = new BinaryFormatter();
      try
      {
        KeyValuePair<string, Dictionary<string, object?>>[] deserializedData = PerformDeserialization(inputStream, formatter);
        TransportItem[] transportedObjects = GetTransportItems(deserializedData);
        return transportedObjects;
      }
      catch (Exception ex)
      {
        throw new TransportationException("Invalid data specified: " + ex.Message, ex);
      }
    }

    protected virtual KeyValuePair<string, Dictionary<string, object?>>[] PerformDeserialization (Stream stream, BinaryFormatter formatter)
    {
      ArgumentUtility.CheckNotNull("stream", stream);
      ArgumentUtility.CheckNotNull("formatter", formatter);

#pragma warning disable SYSLIB0011
      return (KeyValuePair<string, Dictionary<string, object?>>[])formatter.Deserialize(stream);
#pragma warning restore SYSLIB0011
    }

    private TransportItem[] GetTransportItems (KeyValuePair<string, Dictionary<string, object?>>[] deserializedData)
    {
      return Array.ConvertAll(deserializedData, pair => new TransportItem(ObjectID.Parse(pair.Key), pair.Value));
    }
  }
}

﻿using Newtonsoft.Json;
using System;

namespace NHSD.GPITF.BuyingCatalog.Tests
{
  /// <summary>
  /// Reference Article http://www.codeproject.com/KB/tips/SerializedObjectCloner.aspx
  /// Provides a method for performing a deep copy of an object.
  /// Binary Serialization is used to perform the copy.
  /// 
  /// Stolen from:
  ///   https://stackoverflow.com/questions/78536/deep-cloning-objects
  /// This uses Newtonsoft JSON serialiser as it is a bit faster
  /// </summary>
  public static class ObjectCloner
  {
    /// <summary>
    /// Perform a deep Copy of the object, using Json as a serialisation method. NOTE: Private members are not cloned using this method.
    /// </summary>
    /// <typeparam name="T">The type of object being copied.</typeparam>
    /// <param name="source">The object instance to copy.</param>
    /// <returns>The copied object.</returns>
    public static T Clone<T>(this T source)
    {
      // Don't serialize a null object, simply return the default for that object
      if (Object.ReferenceEquals(source, null))
      {
        return default(T);
      }

      // initialize inner objects individually
      // for example in default constructor some list property initialized with some values,
      // but in 'source' these items are cleaned -
      // without ObjectCreationHandling.Replace default constructor values will be added to result
      var deserializeSettings = new JsonSerializerSettings { ObjectCreationHandling = ObjectCreationHandling.Replace };

      return JsonConvert.DeserializeObject<T>(JsonConvert.SerializeObject(source), deserializeSettings);
    }
  }
}

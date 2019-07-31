﻿using NHSD.GPITF.BuyingCatalog.Models;
using System.Collections.Generic;

namespace NHSD.GPITF.BuyingCatalog.Interfaces
{
#pragma warning disable CS1591
  public interface IClaimsDatastore<T> where T : ClaimsBase
  {
    T ById(string id);
    IEnumerable<T> BySolution(string solutionId);
    void Delete(T claim);
  }
#pragma warning restore CS1591
}

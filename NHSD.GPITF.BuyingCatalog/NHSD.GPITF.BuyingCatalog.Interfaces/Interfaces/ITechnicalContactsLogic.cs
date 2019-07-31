﻿using NHSD.GPITF.BuyingCatalog.Models;
using System.Collections.Generic;

namespace NHSD.GPITF.BuyingCatalog.Interfaces
{
#pragma warning disable CS1591
  public interface ITechnicalContactsLogic
  {
    IEnumerable<TechnicalContacts> BySolution(string solutionId);
    void Delete(TechnicalContacts techCont);
  }
#pragma warning restore CS1591
}

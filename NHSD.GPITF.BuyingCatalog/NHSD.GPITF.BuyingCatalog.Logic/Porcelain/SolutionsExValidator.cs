﻿using FluentValidation;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using NHSD.GPITF.BuyingCatalog.Interfaces;
using NHSD.GPITF.BuyingCatalog.Interfaces.Porcelain;
using NHSD.GPITF.BuyingCatalog.Models;
using NHSD.GPITF.BuyingCatalog.Models.Porcelain;
using System;
using System.Collections.Generic;
using System.Linq;

namespace NHSD.GPITF.BuyingCatalog.Logic.Porcelain
{
  public sealed class SolutionsExValidator : ValidatorBase<SolutionEx>, ISolutionsExValidator
  {
    private readonly ISolutionsExDatastore _datastore;
    private readonly ISolutionsValidator _solutionsValidator;

    public SolutionsExValidator(
      IHttpContextAccessor context,
      ILogger<SolutionsExValidator> logger,
      ISolutionsExDatastore datastore,
      ISolutionsValidator solutionsValidator) :
      base(context, logger)
    {
      _datastore = datastore;
      _solutionsValidator = solutionsValidator;
    }

    public void ClaimedCapabilityMustBelongToSolution()
    {
      RuleFor(x => x)
        .Must(soln =>
        {
          return soln.ClaimedCapability.All(cc => cc.SolutionId == soln.Solution.Id);
        })
        .WithMessage("ClaimedCapability must belong to solution");
    }

    public void ClaimedStandardMustBelongToSolution()
    {
      RuleFor(x => x)
        .Must(soln =>
        {
          return soln.ClaimedStandard.All(cs => cs.SolutionId == soln.Solution.Id);
        })
        .WithMessage("ClaimedStandard must belong to solution");
    }

    public void TechnicalContactMustBelongToSolution()
    {
      RuleFor(x => x)
        .Must(soln =>
        {
          return soln.TechnicalContact.All(tc => tc.SolutionId == soln.Solution.Id);
        })
        .WithMessage("TechnicalContact must belong to solution");
    }

    public void CheckUpdateAllowed()
    {
      RuleFor(x => x)
        .Must(newSolnEx =>
        {
          var oldSolnEx = _datastore.BySolution(newSolnEx.Solution.Id);
          return MustBePendingToChangeClaimedCapability(oldSolnEx, newSolnEx);
        })
        .WithMessage("Must Be Pending To Change Claimed Capability");

      RuleFor(x => x)
        .Must(newSolnEx =>
        {
          var oldSolnEx = _datastore.BySolution(newSolnEx.Solution.Id);
          return MustBePendingToChangeClaimedStandard(oldSolnEx, newSolnEx);
        })
        .WithMessage("Must Be Pending To Change Claimed Standard");
    }

    private static bool MustBePendingToChangeClaim<T>(
      SolutionStatus newSolnStatus,
      IEnumerable<T> oldItems,
      IEnumerable<T> newItems,
      IEqualityComparer<T> comparer,
      Action onError
      ) where T : IHasId
    {
      var newNotOld = newItems.Except(oldItems, comparer).ToList();
      var oldNotNew = oldItems.Except(newItems, comparer).ToList();
      var same = !newNotOld.Any() && !oldNotNew.Any();

      if (same)
      {
        // no add/remove
        return true;
      }

      if ((oldNotNew.Any() || newNotOld.Any()) &&
        !IsPendingForClaims(newSolnStatus))
      {
        // Can only add/remove Claim while pending
        onError();
        return false;
      }

      return true;
    }

    // can only add/remove ClaimedCapability while pending
    public bool MustBePendingToChangeClaimedCapability(SolutionEx oldSolnEx, SolutionEx newSolnEx)
    {
      var same = MustBePendingToChangeClaim(
        newSolnEx.Solution.Status,
        oldSolnEx.ClaimedCapability,
        newSolnEx.ClaimedCapability,
        new CapabilitiesImplementedComparer(),
        () =>
        {
          // Can only add/remove ClaimedCapability while pending
          var msg = new { ErrorMessage = nameof(MustBePendingToChangeClaimedCapability), ExistingValue = oldSolnEx };
          _logger.LogError(JsonConvert.SerializeObject(msg));
        });

      return same;
    }

    // can only add/remove ClaimedStandard while pending
    public bool MustBePendingToChangeClaimedStandard(SolutionEx oldSolnEx, SolutionEx newSolnEx)
    {
      var same = MustBePendingToChangeClaim(
        newSolnEx.Solution.Status,
        oldSolnEx.ClaimedStandard,
        newSolnEx.ClaimedStandard,
        new StandardsApplicableComparer(),
        () =>
        {
          // Can only add/remove ClaimedStandard while pending
          var msg = new { ErrorMessage = nameof(MustBePendingToChangeClaimedStandard), ExistingValue = oldSolnEx };
          _logger.LogError(JsonConvert.SerializeObject(msg));
        });

      return same;
    }

    // check every ClaimedCapability
    // check every ClaimedStandard
    // check every ClaimedCapabilityEvidence
    // check every ClaimedStandardEvidence
    // check every ClaimedCapabilityReview
    // check every ClaimedStandardReview

    private static bool IsPendingForClaims(SolutionStatus status)
    {
      return status == SolutionStatus.Draft ||
        status == SolutionStatus.Registered ||
        status == SolutionStatus.CapabilitiesAssessment ||
        status == SolutionStatus.StandardsCompliance;
    }
  }
}

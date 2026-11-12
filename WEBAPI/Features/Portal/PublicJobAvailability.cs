using System.Linq.Expressions;
using WEBAPI.Entities;
using WEBAPI.Entities.Enums;

namespace WEBAPI.Features.Portal;

public static class PublicJobAvailability
{
    public static Expression<Func<RecruitmentBatch, bool>> AcceptsApplications(DateTime now) =>
        batch => batch.Status == BatchStatus.Open && batch.EndDate >= now &&
            (batch.ApplicationDeadlineAt == null || now < batch.ApplicationDeadlineAt) &&
            batch.BatchStages.Any(stage => stage.IsActive) &&
            !batch.BatchStages.Any(stage => stage.LifecycleStatus != BatchStageLifecycleStatus.Inactive);
}

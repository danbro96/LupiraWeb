using JasperFx.Events.Projections;
using Marten;
using Marten.Events.Projections;

namespace LupiraWeb.Domain;

public static class MartenRegistrations
{
    public static StoreOptions UseLupiraProjections(this StoreOptions opts)
    {
        opts.Schema.For<MyInfo>();

        opts.Projections.Snapshot<Skill>(SnapshotLifecycle.Inline);
        opts.Projections.Snapshot<Engagement>(SnapshotLifecycle.Inline);
        opts.Projections.Snapshot<Project>(SnapshotLifecycle.Inline);
        opts.Projections.Snapshot<MediaAsset>(SnapshotLifecycle.Inline);
        opts.Projections.Snapshot<Artifact>(SnapshotLifecycle.Inline);
        opts.Projections.Snapshot<Goal>(SnapshotLifecycle.Inline);

        opts.Projections.Add<EngagementTitleHistoryProjection>(ProjectionLifecycle.Inline);
        opts.Projections.Add<SkillTimelineProjection>(ProjectionLifecycle.Inline);
        opts.Projections.Add<SkillMaturityProjection>(ProjectionLifecycle.Inline);
        opts.Projections.Add<SkillAdjacencyProjection>(ProjectionLifecycle.Inline);
        opts.Projections.Add<ProjectMediaProjection>(ProjectionLifecycle.Inline);
        opts.Projections.Add<SkillMediaGalleryProjection>(ProjectionLifecycle.Inline);
        opts.Projections.Add<ProjectArtifactsProjection>(ProjectionLifecycle.Inline);
        opts.Projections.Add<SkillArtifactsProjection>(ProjectionLifecycle.Inline);
        opts.Projections.Add<EngagementArtifactsProjection>(ProjectionLifecycle.Inline);
        opts.Projections.Add<SkillGoalIndexProjection>(ProjectionLifecycle.Inline);
        opts.Projections.Add<ExperienceProjection>(ProjectionLifecycle.Inline);

        return opts;
    }
}

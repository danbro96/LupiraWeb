using Marten.Events.Projections;

namespace LupiraWeb.Server.Domain;

public sealed class ProjectArtifactRow
{
    public Guid Id { get; set; }
    public Guid ProjectId { get; set; }
    public Guid ArtifactId { get; set; }
    public DateTimeOffset AddedAt { get; set; }
}

public sealed class ProjectArtifactsProjection : MultiStreamProjection<ProjectArtifactRow, Guid>
{
    public ProjectArtifactsProjection()
    {
        Identity<ArtifactLinkedToProject>(e => ArtifactLinkIds.Compose(e.ProjectId, e.ArtifactId));
        Identity<ArtifactUnlinked>(e =>
            e.TargetKind == ArtifactTargetKind.Project
                ? ArtifactLinkIds.Compose(e.TargetId, e.ArtifactId)
                : Guid.Empty);
    }

    public ProjectArtifactRow Create(ArtifactLinkedToProject e) => new()
    {
        Id = ArtifactLinkIds.Compose(e.ProjectId, e.ArtifactId),
        ProjectId = e.ProjectId,
        ArtifactId = e.ArtifactId,
        AddedAt = e.OccurredAt,
    };

    public bool ShouldDelete(ArtifactUnlinked e) =>
        e.TargetKind == ArtifactTargetKind.Project;
}

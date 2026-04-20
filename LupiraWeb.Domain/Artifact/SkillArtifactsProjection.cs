using Marten.Events.Projections;

namespace LupiraWeb.Domain;

public sealed class SkillArtifactRow
{
    public Guid Id { get; set; }
    public Guid SkillId { get; set; }
    public Guid ArtifactId { get; set; }
    public ArtifactRole Role { get; set; }
    public DateTimeOffset AddedAt { get; set; }
}

public sealed class SkillArtifactsProjection : MultiStreamProjection<SkillArtifactRow, Guid>
{
    public SkillArtifactsProjection()
    {
        Identity<ArtifactLinkedToSkill>(e => ArtifactLinkIds.Compose(e.SkillId, e.ArtifactId));
        Identity<ArtifactUnlinked>(e =>
            e.TargetKind == ArtifactTargetKind.Skill
                ? ArtifactLinkIds.Compose(e.TargetId, e.ArtifactId)
                : Guid.Empty);
    }

    public SkillArtifactRow Create(ArtifactLinkedToSkill e) => new()
    {
        Id = ArtifactLinkIds.Compose(e.SkillId, e.ArtifactId),
        SkillId = e.SkillId,
        ArtifactId = e.ArtifactId,
        Role = e.Role,
        AddedAt = e.OccurredAt,
    };

    public bool ShouldDelete(ArtifactUnlinked e) =>
        e.TargetKind == ArtifactTargetKind.Skill;
}

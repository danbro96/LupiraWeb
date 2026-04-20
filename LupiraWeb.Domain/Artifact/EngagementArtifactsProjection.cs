using Marten.Events.Projections;

namespace LupiraWeb.Domain;

public sealed class EngagementArtifactRow
{
    public Guid Id { get; set; }
    public Guid EngagementId { get; set; }
    public Guid ArtifactId { get; set; }
    public DateTimeOffset AddedAt { get; set; }
}

public sealed class EngagementArtifactsProjection : MultiStreamProjection<EngagementArtifactRow, Guid>
{
    public EngagementArtifactsProjection()
    {
        Identity<ArtifactLinkedToEngagement>(e => ArtifactLinkIds.Compose(e.EngagementId, e.ArtifactId));
        Identity<ArtifactUnlinked>(e =>
            e.TargetKind == ArtifactTargetKind.Engagement
                ? ArtifactLinkIds.Compose(e.TargetId, e.ArtifactId)
                : Guid.Empty);
    }

    public EngagementArtifactRow Create(ArtifactLinkedToEngagement e) => new()
    {
        Id = ArtifactLinkIds.Compose(e.EngagementId, e.ArtifactId),
        EngagementId = e.EngagementId,
        ArtifactId = e.ArtifactId,
        AddedAt = e.OccurredAt,
    };

    public bool ShouldDelete(ArtifactUnlinked e) =>
        e.TargetKind == ArtifactTargetKind.Engagement;
}

internal static class ArtifactLinkIds
{
    public static Guid Compose(Guid a, Guid b)
    {
        var ab = a.ToByteArray();
        var bb = b.ToByteArray();
        var combined = new byte[16];
        for (var i = 0; i < 16; i++) combined[i] = (byte)(ab[i] ^ bb[i]);
        return new Guid(combined);
    }
}

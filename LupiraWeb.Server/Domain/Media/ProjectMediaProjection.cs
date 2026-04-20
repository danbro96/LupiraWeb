using Marten.Events.Projections;

namespace LupiraWeb.Server.Domain;

public sealed class ProjectMediaRow
{
    public Guid Id { get; set; }
    public Guid ProjectId { get; set; }
    public Guid MediaId { get; set; }
    public MediaRole Role { get; set; }
    public DateTimeOffset AddedAt { get; set; }
}

public sealed class ProjectMediaProjection : MultiStreamProjection<ProjectMediaRow, Guid>
{
    public ProjectMediaProjection()
    {
        Identity<MediaLinkedToProject>(e => ComposeId(e.ProjectId, e.MediaId));
        Identity<MediaUnlinked>(e =>
            e.TargetKind == MediaTargetKind.Project
                ? ComposeId(e.TargetId, e.MediaId)
                : Guid.Empty);
    }

    public ProjectMediaRow Create(MediaLinkedToProject e) => new()
    {
        Id = ComposeId(e.ProjectId, e.MediaId),
        ProjectId = e.ProjectId,
        MediaId = e.MediaId,
        Role = e.Role,
        AddedAt = e.OccurredAt,
    };

    public bool ShouldDelete(MediaUnlinked e) =>
        e.TargetKind == MediaTargetKind.Project;

    private static Guid ComposeId(Guid a, Guid b)
    {
        var ab = a.ToByteArray();
        var bb = b.ToByteArray();
        var combined = new byte[16];
        for (var i = 0; i < 16; i++) combined[i] = (byte)(ab[i] ^ bb[i]);
        return new Guid(combined);
    }
}

using Marten.Events.Projections;

namespace LupiraWeb.Domain;

public sealed class SkillMediaGalleryRow
{
    public Guid Id { get; set; }
    public Guid SkillId { get; set; }
    public Guid MediaId { get; set; }
    public string? Note { get; set; }
    public DateTimeOffset AddedAt { get; set; }
}

public sealed class SkillMediaGalleryProjection : MultiStreamProjection<SkillMediaGalleryRow, Guid>
{
    public SkillMediaGalleryProjection()
    {
        Identity<MediaLinkedToSkill>(e => ComposeId(e.SkillId, e.MediaId));
        Identity<MediaUnlinked>(e =>
            e.TargetKind == MediaTargetKind.Skill
                ? ComposeId(e.TargetId, e.MediaId)
                : Guid.Empty);
    }

    public SkillMediaGalleryRow Create(MediaLinkedToSkill e) => new()
    {
        Id = ComposeId(e.SkillId, e.MediaId),
        SkillId = e.SkillId,
        MediaId = e.MediaId,
        Note = e.Note,
        AddedAt = e.OccurredAt,
    };

    public bool ShouldDelete(MediaUnlinked e) =>
        e.TargetKind == MediaTargetKind.Skill;

    private static Guid ComposeId(Guid a, Guid b)
    {
        var ab = a.ToByteArray();
        var bb = b.ToByteArray();
        var combined = new byte[16];
        for (var i = 0; i < 16; i++) combined[i] = (byte)(ab[i] ^ bb[i]);
        return new Guid(combined);
    }
}

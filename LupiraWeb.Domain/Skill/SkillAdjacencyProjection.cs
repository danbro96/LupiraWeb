using Marten.Events.Projections;

namespace LupiraWeb.Domain;

public sealed class SkillAdjacencyRow
{
    public Guid Id { get; set; }
    public Guid SkillA { get; set; }
    public Guid SkillB { get; set; }
    public int Count { get; set; }
    public DateOnly? FirstSeen { get; set; }
    public DateOnly? LastSeen { get; set; }
}

public sealed class SkillAdjacencyProjection : MultiStreamProjection<SkillAdjacencyRow, Guid>
{
    public SkillAdjacencyProjection()
    {
        Identity<SkillsCombined>(e => PairId(e.SkillId, e.OtherSkillId));
    }

    public SkillAdjacencyRow Create(SkillsCombined e)
    {
        var (a, b) = SortPair(e.SkillId, e.OtherSkillId);
        return new SkillAdjacencyRow
        {
            Id = PairId(a, b),
            SkillA = a,
            SkillB = b,
            Count = e.IsPrimary ? 1 : 0,
            FirstSeen = e.IsPrimary ? e.OccurredOn : null,
            LastSeen = e.IsPrimary ? e.OccurredOn : null,
        };
    }

    public void Apply(SkillsCombined e, SkillAdjacencyRow row)
    {
        if (!e.IsPrimary) return;
        row.Count++;
        if (row.FirstSeen is null || e.OccurredOn < row.FirstSeen)
            row.FirstSeen = e.OccurredOn;
        if (row.LastSeen is null || e.OccurredOn > row.LastSeen)
            row.LastSeen = e.OccurredOn;
    }

    private static (Guid, Guid) SortPair(Guid a, Guid b) =>
        a.CompareTo(b) < 0 ? (a, b) : (b, a);

    private static Guid PairId(Guid a, Guid b)
    {
        var (x, y) = SortPair(a, b);
        var xb = x.ToByteArray();
        var yb = y.ToByteArray();
        var combined = new byte[16];
        for (int i = 0; i < 16; i++) combined[i] = (byte)(xb[i] ^ yb[i]);
        return new Guid(combined);
    }
}

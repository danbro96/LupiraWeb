using Marten.Events.Aggregation;

namespace LupiraWeb.Domain;

public sealed class SkillTimelineEntry
{
    public required string Kind { get; set; }
    public DateOnly OccurredOn { get; set; }
    public SkillContextKind? ContextKind { get; set; }
    public Guid? ContextId { get; set; }
    public string? ContextLabel { get; set; }
    public Intensity? Intensity { get; set; }
    public Maturity? Maturity { get; set; }
    public Guid? OtherSkillId { get; set; }
    public string? Note { get; set; }
}

public sealed class SkillTimeline
{
    public Guid Id { get; set; }
    public string Name { get; set; } = "";
    public List<SkillTimelineEntry> Entries { get; set; } = new();
}

public sealed class SkillTimelineProjection : SingleStreamProjection<SkillTimeline, Guid>
{
    public SkillTimeline Create(SkillRegistered e) => new()
    {
        Id = e.SkillId,
        Name = e.Name,
        Entries = new List<SkillTimelineEntry>(),
    };

    public void Apply(SkillRenamed e, SkillTimeline t) => t.Name = e.NewName;

    public void Apply(SkillLearned e, SkillTimeline t) =>
        t.Entries.Add(CreateEntry("Learned", e.OccurredOn, e.Context, maturity: e.InitialMaturity));

    public void Apply(SkillApplied e, SkillTimeline t) =>
        t.Entries.Add(CreateEntry("Applied", e.OccurredOn, e.Context, intensity: e.Intensity));

    public void Apply(SkillDeepened e, SkillTimeline t) =>
        t.Entries.Add(CreateEntry("Deepened", e.OccurredOn, e.Context, maturity: e.ToMaturity, note: e.Note));

    public void Apply(SkillTaught e, SkillTimeline t) =>
        t.Entries.Add(CreateEntry("Taught", e.OccurredOn, e.Context, note: e.Audience));

    public void Apply(SkillReferenced e, SkillTimeline t) =>
        t.Entries.Add(CreateEntry("Referenced", e.OccurredOn, e.Context, note: e.Note));

    public void Apply(SkillsCombined e, SkillTimeline t)
    {
        if (!e.IsPrimary) return;
        t.Entries.Add(new SkillTimelineEntry
        {
            Kind = "Combined",
            OccurredOn = e.OccurredOn,
            OtherSkillId = e.OtherSkillId,
            ContextKind = e.Context.Kind,
            ContextId = ContextIdFrom(e.Context),
            ContextLabel = e.Context.ExternalLabel,
        });
    }

    private static SkillTimelineEntry CreateEntry(
        string kind,
        DateOnly occurredOn,
        SkillEdgeContext context,
        Intensity? intensity = null,
        Maturity? maturity = null,
        string? note = null) => new()
    {
        Kind = kind,
        OccurredOn = occurredOn,
        ContextKind = context.Kind,
        ContextId = ContextIdFrom(context),
        ContextLabel = context.ExternalLabel,
        Intensity = intensity,
        Maturity = maturity,
        Note = note,
    };

    private static Guid? ContextIdFrom(SkillEdgeContext ctx) => ctx.Kind switch
    {
        SkillContextKind.InEngagement => ctx.EngagementId,
        SkillContextKind.InProject => ctx.ProjectId,
        _ => null,
    };
}

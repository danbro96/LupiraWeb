using Marten.Events.Projections;

namespace LupiraWeb.Server.Domain;

public class EngagementTitleHistoryRow
{
    public Guid Id { get; set; }
    public Guid EngagementId { get; set; }
    public string Text { get; set; } = "";
    public DateOnly From { get; set; }
    public DateOnly? To { get; set; }
}

public class EngagementTitleHistoryProjection : MultiStreamProjection<EngagementTitleHistoryRow, Guid>
{
    public EngagementTitleHistoryProjection()
    {
        Identity<TitleAssumed>(e => e.TitleId);
        Identity<TitleRevised>(e => e.TitleId);
        Identity<TitleRetired>(e => e.TitleId);
    }

    public EngagementTitleHistoryRow Create(TitleAssumed e) => new()
    {
        Id = e.TitleId,
        EngagementId = e.EngagementId,
        Text = e.Text,
        From = e.EffectiveFrom,
    };

    public void Apply(TitleRevised e, EngagementTitleHistoryRow row) => row.Text = e.NewText;

    public void Apply(TitleRetired e, EngagementTitleHistoryRow row) => row.To = e.EffectiveTo;
}

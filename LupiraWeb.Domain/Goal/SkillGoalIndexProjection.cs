using Marten.Events.Projections;

namespace LupiraWeb.Domain;

public sealed class SkillGoalIndexRow
{
    public Guid Id { get; set; }
    public Guid SkillId { get; set; }
    public Guid GoalId { get; set; }
    public GoalStatus Status { get; set; }
}

public sealed class SkillGoalIndexProjection : MultiStreamProjection<SkillGoalIndexRow, Guid>
{
    public SkillGoalIndexProjection()
    {
        Identity<GoalSet>(e => e.SkillId is Guid sid ? Compose(sid, e.GoalId) : Guid.Empty);
        Identity<GoalAchieved>(e => e.GoalId);
        Identity<GoalAbandoned>(e => e.GoalId);
    }

    public SkillGoalIndexRow? Create(GoalSet e)
    {
        if (e.SkillId is not Guid skillId) return null;
        return new SkillGoalIndexRow
        {
            Id = Compose(skillId, e.GoalId),
            SkillId = skillId,
            GoalId = e.GoalId,
            Status = GoalStatus.Active,
        };
    }

    public void Apply(GoalAchieved e, SkillGoalIndexRow row) => row.Status = GoalStatus.Achieved;
    public void Apply(GoalAbandoned e, SkillGoalIndexRow row) => row.Status = GoalStatus.Abandoned;

    private static Guid Compose(Guid a, Guid b)
    {
        var ab = a.ToByteArray();
        var bb = b.ToByteArray();
        var combined = new byte[16];
        for (var i = 0; i < 16; i++) combined[i] = (byte)(ab[i] ^ bb[i]);
        return new Guid(combined);
    }
}

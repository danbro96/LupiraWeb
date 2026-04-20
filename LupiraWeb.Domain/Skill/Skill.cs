namespace LupiraWeb.Domain;

public enum SkillCategory
{
    Language,
    Framework,
    Tool,
    Platform,
    Method,
    Domain,
    Other,
}

public class Skill
{
    public Guid Id { get; set; }
    public int Version { get; set; }

    public string Name { get; set; } = "";
    public SkillCategory Category { get; set; }
    public List<string> Aliases { get; set; } = new();
    public Guid? ParentSkillId { get; set; }
    public bool Retired { get; set; }

    public DateOnly? FirstLearnedOn { get; set; }
    public Maturity CurrentMaturity { get; set; } = Maturity.Aware;

    public void Apply(SkillRegistered e)
    {
        Id = e.SkillId;
        Name = e.Name;
        Category = e.Category;
        Aliases = e.Aliases?.ToList() ?? new();
        ParentSkillId = e.ParentSkillId;
    }

    public void Apply(SkillRenamed e) => Name = e.NewName;
    public void Apply(SkillCategoryChanged e) => Category = e.NewCategory;

    public void Apply(SkillAliasAdded e)
    {
        if (!Aliases.Contains(e.Alias))
            Aliases.Add(e.Alias);
    }

    public void Apply(SkillReparented e) => ParentSkillId = e.NewParentSkillId;
    public void Apply(SkillRetired e) => Retired = true;

    public void Apply(SkillLearned e)
    {
        FirstLearnedOn ??= e.OccurredOn;
        CurrentMaturity = e.InitialMaturity;
    }

    public void Apply(SkillDeepened e) => CurrentMaturity = e.ToMaturity;
}

namespace LupiraWeb.Server.Domain;

public record SkillRegistered(
    Guid SkillId,
    string Name,
    SkillCategory Category,
    IReadOnlyList<string>? Aliases,
    Guid? ParentSkillId);

public record SkillRenamed(Guid SkillId, string NewName);

public record SkillCategoryChanged(Guid SkillId, SkillCategory NewCategory);

public record SkillAliasAdded(Guid SkillId, string Alias);

public record SkillReparented(Guid SkillId, Guid? NewParentSkillId);

public record SkillRetired(Guid SkillId);

using LupiraWeb.Server.Data.Repositories;
using LupiraWeb.Server.Endpoints.Resume.Dtos;
using Microsoft.AspNetCore.Http.HttpResults;
using EngagementDocument = LupiraWeb.Server.Domain.Engagement;
using ProjectDocument = LupiraWeb.Server.Domain.Project;
using SkillDocument = LupiraWeb.Server.Domain.Skill;

namespace LupiraWeb.Server.Endpoints.Resume;

public class ResumeHandler(
    IMyInfoRepository myInfoRepository,
    IEngagementRepository engagementRepository,
    IProjectRepository projectRepository,
    ISkillRepository skillRepository)
{
    public async Task<Results<Ok<MyInfo>, NotFound>> GetMeAsync(CancellationToken ct)
    {
        var myInfo = await myInfoRepository.GetAsync(ct);
        if (myInfo is null)
            return TypedResults.NotFound();
        return TypedResults.Ok(MyInfo.From(myInfo));
    }

    public async Task<Ok<IReadOnlyList<Engagement>>> GetEngagementsAsync(CancellationToken ct)
    {
        var engagements = await engagementRepository.ListAsync(ct);
        var ctx = await BuildContextAsync(ct);
        return TypedResults.Ok<IReadOnlyList<Engagement>>(
            engagements.Select(e => ToEngagementDto(e, ctx)).ToList());
    }

    public async Task<Results<Ok<Engagement>, NotFound>> GetEngagementAsync(Guid id, CancellationToken ct)
    {
        var engagement = await engagementRepository.GetAsync(id, ct);
        if (engagement is null)
            return TypedResults.NotFound();

        var ctx = await BuildContextAsync(ct);
        return TypedResults.Ok(ToEngagementDto(engagement, ctx));
    }

    public async Task<Ok<IReadOnlyList<Project>>> GetProjectsAsync(CancellationToken ct)
    {
        var projects = await projectRepository.ListAsync(ct);
        var ctx = await BuildContextAsync(ct);
        return TypedResults.Ok<IReadOnlyList<Project>>(
            projects.Select(p => ToProjectDto(p, ctx)).ToList());
    }

    public async Task<Results<Ok<Project>, NotFound>> GetProjectAsync(Guid id, CancellationToken ct)
    {
        var project = await projectRepository.GetAsync(id, ct);
        if (project is null)
            return TypedResults.NotFound();

        var ctx = await BuildContextAsync(ct);
        return TypedResults.Ok(ToProjectDto(project, ctx));
    }

    public async Task<Ok<IReadOnlyList<Skill>>> GetSkillsAsync(CancellationToken ct)
    {
        var skills = await skillRepository.ListAsync(ct);
        return TypedResults.Ok<IReadOnlyList<Skill>>(
            skills.Select(Skill.From).ToList());
    }

    private async Task<ResumeContext> BuildContextAsync(CancellationToken ct)
    {
        var skills = (await skillRepository.ListAsync(ct)).ToDictionary(s => s.Id);
        var projects = (await projectRepository.ListAsync(ct)).ToList();
        var engagements = (await engagementRepository.ListAsync(ct)).ToDictionary(e => e.Id);
        return new ResumeContext(skills, projects, engagements);
    }

    private static Engagement ToEngagementDto(EngagementDocument e, ResumeContext ctx)
    {
        var skills = e.SkillIds
            .Where(ctx.SkillsById.ContainsKey)
            .Select(id => Skill.From(ctx.SkillsById[id]));
        var projects = ctx.Projects
            .Where(p => p.EngagementId == e.Id)
            .Select(p => ToProjectDto(p, ctx));
        return Engagement.From(e, skills, projects);
    }

    private static Project ToProjectDto(ProjectDocument p, ResumeContext ctx)
    {
        var skills = p.SkillIds
            .Where(ctx.SkillsById.ContainsKey)
            .Select(id => Skill.From(ctx.SkillsById[id]));
        string? engagementInstitution = null;
        if (p.EngagementId is Guid eid
            && ctx.EngagementsById.TryGetValue(eid, out var eng))
        {
            engagementInstitution = eng.Institution;
        }
        return Project.From(p, skills, engagementInstitution);
    }

    private sealed record ResumeContext(
        IReadOnlyDictionary<Guid, SkillDocument> SkillsById,
        IReadOnlyList<ProjectDocument> Projects,
        IReadOnlyDictionary<Guid, EngagementDocument> EngagementsById);
}

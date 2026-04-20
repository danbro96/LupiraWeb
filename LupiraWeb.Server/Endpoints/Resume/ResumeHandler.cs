using LupiraWeb.Server.Data.Repositories;
using LupiraWeb.Server.Endpoints.Resume.Dtos;
using Microsoft.AspNetCore.Http.HttpResults;

namespace LupiraWeb.Server.Endpoints.Resume;

public class ResumeHandler(
    IMyInfoRepository myInfoRepository,
    IEmploymentRepository employmentRepository,
    IProjectRepository projectRepository,
    ISkillRepository skillRepository)
{
    private readonly IMyInfoRepository _myInfoRepository = myInfoRepository;
    private readonly IEmploymentRepository _employmentRepository = employmentRepository;
    private readonly IProjectRepository _projectRepository = projectRepository;
    private readonly ISkillRepository _skillRepository = skillRepository;

    public async Task<Results<Ok<MyInfo>, NotFound>> GetMeAsync(CancellationToken ct)
    {
        var myInfo = await _myInfoRepository.GetAsync(ct);

        if (myInfo is null)
            return TypedResults.NotFound();

        return TypedResults.Ok(MyInfo.From(myInfo));
    }

    public async Task<Ok<IReadOnlyList<Employment>>> GetEmploymentsAsync(CancellationToken ct)
    {
        var employments = await _employmentRepository.ListAsync(ct);

        return TypedResults.Ok<IReadOnlyList<Employment>>(
            employments.Select(Employment.From).ToList());
    }

    public async Task<Results<Ok<Employment>, NotFound>> GetEmploymentAsync(Guid id, CancellationToken ct)
    {
        var employment = await _employmentRepository.GetAsync(id, ct);

        if (employment is null)
            return TypedResults.NotFound();

        return TypedResults.Ok(Employment.From(employment));
    }

    public async Task<Ok<IReadOnlyList<Project>>> GetProjectsAsync(CancellationToken ct)
    {
        var projects = await _projectRepository.ListAsync(ct);

        return TypedResults.Ok<IReadOnlyList<Project>>(
            projects.Select(Project.From).ToList());
    }

    public async Task<Results<Ok<Project>, NotFound>> GetProjectAsync(Guid id, CancellationToken ct)
    {
        var project = await _projectRepository.GetAsync(id, ct);

        if (project is null)
            return TypedResults.NotFound();

        return TypedResults.Ok(Project.From(project));
    }

    public async Task<Ok<IReadOnlyList<Skill>>> GetSkillsAsync(CancellationToken ct)
    {
        var skills = await _skillRepository.ListAsync(ct);

        return TypedResults.Ok<IReadOnlyList<Skill>>(
            skills.Select(Skill.From).ToList());
    }
}

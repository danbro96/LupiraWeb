using LupiraWeb.Server.Data.Entities;
using LupiraWeb.Server.Data.Repositories;
using LupiraWeb.Server.Endpoints.Resume;
using LupiraWeb.Server.Endpoints.Resume.Dtos;
using Microsoft.AspNetCore.Http.HttpResults;
using NSubstitute;
using Xunit;

namespace LupiraWeb.Server.Tests.Resume;

public class ResumeHandlerTests
{
    private static ResumeHandler CreateHandler(
        IMyInfoRepository? myInfoRepository = null,
        IEmploymentRepository? employmentRepository = null,
        IProjectRepository? projectRepository = null,
        ISkillRepository? skillRepository = null) =>
        new(
            myInfoRepository ?? Substitute.For<IMyInfoRepository>(),
            employmentRepository ?? Substitute.For<IEmploymentRepository>(),
            projectRepository ?? Substitute.For<IProjectRepository>(),
            skillRepository ?? Substitute.For<ISkillRepository>());

    [Fact]
    public async Task GetMeAsync_returns_NotFound_when_repository_is_empty()
    {
        var myInfoRepository = Substitute.For<IMyInfoRepository>();
        myInfoRepository.GetAsync(Arg.Any<CancellationToken>()).Returns((MyInfoEntity?)null);
        var handler = CreateHandler(myInfoRepository: myInfoRepository);

        var result = await handler.GetMeAsync(CancellationToken.None);

        Assert.IsType<NotFound>(result.Result);
    }

    [Fact]
    public async Task GetMeAsync_returns_Ok_with_dto_when_present()
    {
        var myInfoRepository = Substitute.For<IMyInfoRepository>();
        myInfoRepository.GetAsync(Arg.Any<CancellationToken>()).Returns(new MyInfoEntity
        {
            Id = MyInfoEntity.SingletonId,
            FullName = "Daniel Broström",
            Email = "daniel.brostrom@strivo.se",
        });
        var handler = CreateHandler(myInfoRepository: myInfoRepository);

        var result = await handler.GetMeAsync(CancellationToken.None);

        var ok = Assert.IsType<Ok<MyInfo>>(result.Result);
        Assert.Equal("Daniel Broström", ok.Value!.FullName);
        Assert.Equal("daniel.brostrom@strivo.se", ok.Value.Email);
    }

    [Fact]
    public async Task GetEmploymentsAsync_returns_mapped_list()
    {
        var employmentRepository = Substitute.For<IEmploymentRepository>();
        employmentRepository.ListAsync(Arg.Any<CancellationToken>()).Returns(new List<EmploymentEntity>
        {
            new()
            {
                Id = Guid.NewGuid(),
                Company = "Strivo",
                Title = "Consultant",
                StartDate = new DateOnly(2023, 1, 1),
            },
        });
        var handler = CreateHandler(employmentRepository: employmentRepository);

        var result = await handler.GetEmploymentsAsync(CancellationToken.None);

        Assert.Single(result.Value!);
        Assert.Equal("Strivo", result.Value![0].Company);
    }

    [Fact]
    public async Task GetEmploymentAsync_returns_NotFound_when_missing()
    {
        var employmentRepository = Substitute.For<IEmploymentRepository>();
        employmentRepository.GetAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns((EmploymentEntity?)null);
        var handler = CreateHandler(employmentRepository: employmentRepository);

        var result = await handler.GetEmploymentAsync(Guid.NewGuid(), CancellationToken.None);

        Assert.IsType<NotFound>(result.Result);
    }

    [Fact]
    public async Task GetEmploymentAsync_returns_Ok_when_found()
    {
        var id = Guid.NewGuid();
        var employmentRepository = Substitute.For<IEmploymentRepository>();
        employmentRepository.GetAsync(id, Arg.Any<CancellationToken>()).Returns(new EmploymentEntity
        {
            Id = id,
            Company = "Strivo",
            Title = "Consultant",
            StartDate = new DateOnly(2023, 1, 1),
        });
        var handler = CreateHandler(employmentRepository: employmentRepository);

        var result = await handler.GetEmploymentAsync(id, CancellationToken.None);

        var ok = Assert.IsType<Ok<Employment>>(result.Result);
        Assert.Equal(id, ok.Value!.Id);
    }

    [Fact]
    public async Task GetProjectAsync_returns_NotFound_when_missing()
    {
        var projectRepository = Substitute.For<IProjectRepository>();
        projectRepository.GetAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns((ProjectEntity?)null);
        var handler = CreateHandler(projectRepository: projectRepository);

        var result = await handler.GetProjectAsync(Guid.NewGuid(), CancellationToken.None);

        Assert.IsType<NotFound>(result.Result);
    }

    [Fact]
    public async Task GetSkillsAsync_returns_mapped_list()
    {
        var skillRepository = Substitute.For<ISkillRepository>();
        skillRepository.ListAsync(Arg.Any<CancellationToken>()).Returns(new List<SkillEntity>
        {
            new() { Id = Guid.NewGuid(), Name = "C#", Category = SkillCategory.Language },
            new() { Id = Guid.NewGuid(), Name = ".NET", Category = SkillCategory.Framework },
        });
        var handler = CreateHandler(skillRepository: skillRepository);

        var result = await handler.GetSkillsAsync(CancellationToken.None);

        Assert.Equal(2, result.Value!.Count);
        Assert.Contains(result.Value!, s => s.Name == "C#");
    }
}

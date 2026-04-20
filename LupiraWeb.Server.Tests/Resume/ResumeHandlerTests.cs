using LupiraWeb.Server.Data.Repositories;
using LupiraWeb.Domain;
using LupiraWeb.Server.Endpoints.Resume;
using Microsoft.AspNetCore.Http.HttpResults;
using NSubstitute;
using Xunit;
using Dtos = LupiraWeb.Server.Endpoints.Resume.Dtos;
using EngagementDocument = LupiraWeb.Domain.Engagement;
using ProjectDocument = LupiraWeb.Domain.Project;
using SkillDocument = LupiraWeb.Domain.Skill;
using MyInfoDocument = LupiraWeb.Domain.MyInfo;

namespace LupiraWeb.Server.Tests.Resume;

public class ResumeHandlerTests
{
    private static ResumeHandler CreateHandler(
        IMyInfoRepository? myInfoRepository = null,
        IEngagementRepository? engagementRepository = null,
        IProjectRepository? projectRepository = null,
        ISkillRepository? skillRepository = null) =>
        new(
            myInfoRepository ?? Substitute.For<IMyInfoRepository>(),
            engagementRepository ?? StubRepo<IEngagementRepository, EngagementDocument>(),
            projectRepository ?? StubRepo<IProjectRepository, ProjectDocument>(),
            skillRepository ?? StubSkillRepo());

    private static IEngagementRepository StubEngagementRepo(IReadOnlyList<EngagementDocument> list)
    {
        var repo = Substitute.For<IEngagementRepository>();
        repo.ListAsync(Arg.Any<CancellationToken>()).Returns(list);
        return repo;
    }

    private static T StubRepo<T, TDoc>() where T : class
    {
        var repo = Substitute.For<T>();
        // List methods default to returning empty arrays via NSubstitute defaults for Task<IReadOnlyList<T>>,
        // but some paths expect explicit stubs; the handler tolerates empty lists.
        return repo;
    }

    private static ISkillRepository StubSkillRepo()
    {
        var repo = Substitute.For<ISkillRepository>();
        repo.ListAsync(Arg.Any<CancellationToken>()).Returns(Array.Empty<SkillDocument>());
        return repo;
    }

    [Fact]
    public async Task GetMeAsync_returns_NotFound_when_repository_is_empty()
    {
        var myInfoRepository = Substitute.For<IMyInfoRepository>();
        myInfoRepository.GetAsync(Arg.Any<CancellationToken>()).Returns((MyInfoDocument?)null);
        var handler = CreateHandler(myInfoRepository: myInfoRepository);

        var result = await handler.GetMeAsync(CancellationToken.None);

        Assert.IsType<NotFound>(result.Result);
    }

    [Fact]
    public async Task GetMeAsync_returns_Ok_with_dto_when_present()
    {
        var myInfoRepository = Substitute.For<IMyInfoRepository>();
        myInfoRepository.GetAsync(Arg.Any<CancellationToken>()).Returns(new MyInfoDocument
        {
            Id = MyInfoDocument.SingletonId,
            FullName = "Daniel Broström",
            Email = "daniel.brostrom@strivo.se",
        });
        var handler = CreateHandler(myInfoRepository: myInfoRepository);

        var result = await handler.GetMeAsync(CancellationToken.None);

        var ok = Assert.IsType<Ok<Dtos.MyInfo>>(result.Result);
        Assert.Equal("Daniel Broström", ok.Value!.FullName);
        Assert.Equal("daniel.brostrom@strivo.se", ok.Value.Email);
    }

    [Fact]
    public async Task GetEngagementsAsync_returns_mapped_list()
    {
        var id = Guid.NewGuid();
        var engagement = new EngagementDocument
        {
            Id = id,
            Kind = EngagementKind.Employment,
            Institution = "Strivo",
            Start = new DateOnly(2023, 1, 1),
            Titles = new List<TitleEpoch>
            {
                new() { TitleId = Guid.NewGuid(), Text = "Consultant", From = new DateOnly(2023, 1, 1) },
            },
        };
        var handler = CreateHandler(engagementRepository: StubEngagementRepo(new[] { engagement }));

        var result = await handler.GetEngagementsAsync(CancellationToken.None);

        Assert.Single(result.Value!);
        Assert.Equal("Strivo", result.Value![0].Institution);
        Assert.Equal("Consultant", result.Value[0].Title);
    }

    [Fact]
    public async Task GetEngagementAsync_returns_NotFound_when_missing()
    {
        var repo = Substitute.For<IEngagementRepository>();
        repo.GetAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns((EngagementDocument?)null);
        var handler = CreateHandler(engagementRepository: repo);

        var result = await handler.GetEngagementAsync(Guid.NewGuid(), CancellationToken.None);

        Assert.IsType<NotFound>(result.Result);
    }

    [Fact]
    public async Task GetEngagementAsync_returns_Ok_when_found()
    {
        var id = Guid.NewGuid();
        var engagement = new EngagementDocument
        {
            Id = id,
            Kind = EngagementKind.Employment,
            Institution = "Strivo",
            Start = new DateOnly(2023, 1, 1),
            Titles = new List<TitleEpoch>
            {
                new() { TitleId = Guid.NewGuid(), Text = "Consultant", From = new DateOnly(2023, 1, 1) },
            },
        };
        var repo = Substitute.For<IEngagementRepository>();
        repo.GetAsync(id, Arg.Any<CancellationToken>()).Returns(engagement);
        repo.ListAsync(Arg.Any<CancellationToken>()).Returns(new[] { engagement });
        var handler = CreateHandler(engagementRepository: repo);

        var result = await handler.GetEngagementAsync(id, CancellationToken.None);

        var ok = Assert.IsType<Ok<Dtos.Engagement>>(result.Result);
        Assert.Equal(id, ok.Value!.Id);
        Assert.Equal("Consultant", ok.Value.Title);
    }

    [Fact]
    public async Task GetProjectAsync_returns_NotFound_when_missing()
    {
        var repo = Substitute.For<IProjectRepository>();
        repo.GetAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns((ProjectDocument?)null);
        var handler = CreateHandler(projectRepository: repo);

        var result = await handler.GetProjectAsync(Guid.NewGuid(), CancellationToken.None);

        Assert.IsType<NotFound>(result.Result);
    }

    [Fact]
    public async Task GetSkillsAsync_returns_mapped_list()
    {
        var skillRepository = Substitute.For<ISkillRepository>();
        skillRepository.ListAsync(Arg.Any<CancellationToken>()).Returns(new List<SkillDocument>
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

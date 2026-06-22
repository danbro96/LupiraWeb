using LupiraWeb.Server.Contracts;
using LupiraWeb.Server.Data.Repositories;
using LupiraWeb.Server.Endpoints.Resume;
using LupiraWeb.Server.Integration.CareerApi.Dtos;
using Microsoft.AspNetCore.Http.HttpResults;
using NSubstitute;
using Xunit;
using Dtos = LupiraWeb.Server.Endpoints.Resume.Dtos;

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
            engagementRepository ?? Substitute.For<IEngagementRepository>(),
            projectRepository ?? Substitute.For<IProjectRepository>(),
            skillRepository ?? StubSkillRepo());

    private static IEngagementRepository StubEngagementRepo(IReadOnlyList<CareerEngagementDto> list)
    {
        var repo = Substitute.For<IEngagementRepository>();
        repo.ListAsync(Arg.Any<CancellationToken>()).Returns(list);
        return repo;
    }

    private static ISkillRepository StubSkillRepo()
    {
        var repo = Substitute.For<ISkillRepository>();
        repo.ListAsync(Arg.Any<CancellationToken>()).Returns(Array.Empty<CareerSkillDto>());
        return repo;
    }

    private static CareerEngagementDto Engagement(Guid id) => new(
        id,
        EngagementKind.Employment,
        Guid.NewGuid(),
        "Strivo",
        new DateOnly(2023, 1, 1),
        null,
        null,
        null,
        "Consultant",
        [new CareerTitleEpochDto(Guid.NewGuid(), "Consultant", new DateOnly(2023, 1, 1), null)],
        []);

    [Fact]
    public async Task GetMeAsync_returns_NotFound_when_repository_is_empty()
    {
        var myInfoRepository = Substitute.For<IMyInfoRepository>();
        myInfoRepository.GetAsync(Arg.Any<CancellationToken>()).Returns((OwnerInfo?) null);
        var handler = CreateHandler(myInfoRepository: myInfoRepository);

        var result = await handler.GetMeAsync(CancellationToken.None);

        Assert.IsType<NotFound>(result.Result);
    }

    [Fact]
    public async Task GetMeAsync_returns_Ok_with_dto_when_present()
    {
        var myInfoRepository = Substitute.For<IMyInfoRepository>();
        myInfoRepository.GetAsync(Arg.Any<CancellationToken>()).Returns(new OwnerInfo(
            Guid.NewGuid(),
            "Daniel Broström",
            "daniel.brostrom@strivo.se",
            null, null, null, null, null, null));
        var handler = CreateHandler(myInfoRepository: myInfoRepository);

        var result = await handler.GetMeAsync(CancellationToken.None);

        var ok = Assert.IsType<Ok<Dtos.MyInfo>>(result.Result);
        Assert.Equal("Daniel Broström", ok.Value!.FullName);
        Assert.Equal("daniel.brostrom@strivo.se", ok.Value.Email);
    }

    [Fact]
    public async Task GetEngagementsAsync_returns_mapped_list()
    {
        var handler = CreateHandler(engagementRepository: StubEngagementRepo([Engagement(Guid.NewGuid())]));

        var result = await handler.GetEngagementsAsync(CancellationToken.None);

        Assert.Single(result.Value!);
        Assert.Equal("Strivo", result.Value![0].Institution);
        Assert.Equal("Consultant", result.Value[0].Title);
    }

    [Fact]
    public async Task GetEngagementAsync_returns_NotFound_when_missing()
    {
        var repo = Substitute.For<IEngagementRepository>();
        repo.GetAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns((CareerEngagementDto?) null);
        var handler = CreateHandler(engagementRepository: repo);

        var result = await handler.GetEngagementAsync(Guid.NewGuid(), CancellationToken.None);

        Assert.IsType<NotFound>(result.Result);
    }

    [Fact]
    public async Task GetEngagementAsync_returns_Ok_when_found()
    {
        var id = Guid.NewGuid();
        var engagement = Engagement(id);
        var repo = Substitute.For<IEngagementRepository>();
        repo.GetAsync(id, Arg.Any<CancellationToken>()).Returns(engagement);
        repo.ListAsync(Arg.Any<CancellationToken>()).Returns([engagement]);
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
        repo.GetAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns((CareerProjectDto?) null);
        var handler = CreateHandler(projectRepository: repo);

        var result = await handler.GetProjectAsync(Guid.NewGuid(), CancellationToken.None);

        Assert.IsType<NotFound>(result.Result);
    }

    [Fact]
    public async Task GetSkillsAsync_returns_mapped_list()
    {
        var skillRepository = Substitute.For<ISkillRepository>();
        skillRepository.ListAsync(Arg.Any<CancellationToken>()).Returns(new List<CareerSkillDto>
        {
            new(Guid.NewGuid(), "C#", SkillCategory.Language, [], null, false, null, Maturity.Aware),
            new(Guid.NewGuid(), ".NET", SkillCategory.Framework, [], null, false, null, Maturity.Aware),
        });
        var handler = CreateHandler(skillRepository: skillRepository);

        var result = await handler.GetSkillsAsync(CancellationToken.None);

        Assert.Equal(2, result.Value!.Count);
        Assert.Contains(result.Value!, s => s.Name == "C#");
    }
}

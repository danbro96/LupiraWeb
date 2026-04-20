using System.Net;
using System.Net.Http.Json;
using LupiraWeb.Domain;
using LupiraWeb.Server.Endpoints.Goals.Dtos;
using LupiraWeb.Server.Tests.Resume;
using Xunit;

namespace LupiraWeb.Server.Tests.Goals;

public class GoalsEndpointsIntegrationTests : IClassFixture<ResumeTestFactory>
{
    private readonly ResumeTestFactory _factory;

    public GoalsEndpointsIntegrationTests(ResumeTestFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task Set_then_get_roundtrips_metadata()
    {
        var client = _factory.CreateClient();

        var setResponse = await client.PostAsJsonAsync("/api/goals",
            new SetGoalRequest
            {
                SkillId = ResumeTestFactory.SeededSkillId,
                TargetMaturity = Maturity.Expert,
                Deadline = new DateOnly(2026, 12, 31),
                Motivation = "Push C# skills deeper",
            });
        Assert.Equal(HttpStatusCode.OK, setResponse.StatusCode);
        var set = await setResponse.Content.ReadFromJsonAsync<SetGoalResponse>();
        Assert.NotNull(set);

        var getResponse = await client.GetAsync($"/api/goals/{set!.GoalId}");
        var goal = await getResponse.Content.ReadFromJsonAsync<GoalDto>();
        Assert.NotNull(goal);
        Assert.Equal(GoalStatus.Active, goal!.Status);
        Assert.Equal(Maturity.Expert, goal.TargetMaturity);
        Assert.Equal(ResumeTestFactory.SeededSkillId, goal.SkillId);
    }

    [Fact]
    public async Task Record_progress_appends_to_progress_log()
    {
        var client = _factory.CreateClient();
        var goalId = await SetSkillGoalAsync(client);

        var progressResponse = await client.PostAsJsonAsync(
            $"/api/goals/{goalId}/progress",
            new RecordProgressRequest { Note = "Studied 2 hours" });
        Assert.Equal(HttpStatusCode.NoContent, progressResponse.StatusCode);

        var goal = await (await client.GetAsync($"/api/goals/{goalId}"))
            .Content.ReadFromJsonAsync<GoalDto>();
        Assert.Single(goal!.Progress);
        Assert.Equal("Studied 2 hours", goal.Progress[0].Note);
    }

    [Fact]
    public async Task Achieving_goal_marks_it_achieved()
    {
        var client = _factory.CreateClient();
        var goalId = await SetSkillGoalAsync(client);

        var achieveResponse = await client.PostAsJsonAsync(
            $"/api/goals/{goalId}/achieve",
            new AchieveGoalRequest { AchievedOn = new DateOnly(2026, 6, 1) });
        Assert.Equal(HttpStatusCode.NoContent, achieveResponse.StatusCode);

        var goal = await (await client.GetAsync($"/api/goals/{goalId}"))
            .Content.ReadFromJsonAsync<GoalDto>();
        Assert.Equal(GoalStatus.Achieved, goal!.Status);
        Assert.NotNull(goal.ResolvedAt);
    }

    [Fact]
    public async Task Double_resolution_is_rejected()
    {
        var client = _factory.CreateClient();
        var goalId = await SetSkillGoalAsync(client);

        await client.PostAsJsonAsync($"/api/goals/{goalId}/achieve",
            new AchieveGoalRequest { AchievedOn = new DateOnly(2026, 6, 1) });

        var secondResponse = await client.PostAsJsonAsync(
            $"/api/goals/{goalId}/abandon",
            new AbandonGoalRequest
            {
                AbandonedOn = new DateOnly(2026, 7, 1),
                Reason = "changed mind",
            });
        Assert.Equal(HttpStatusCode.BadRequest, secondResponse.StatusCode);
    }

    [Fact]
    public async Task Abandoning_goal_records_reason()
    {
        var client = _factory.CreateClient();
        var goalId = await SetSkillGoalAsync(client);

        var abandonResponse = await client.PostAsJsonAsync(
            $"/api/goals/{goalId}/abandon",
            new AbandonGoalRequest
            {
                AbandonedOn = new DateOnly(2026, 7, 1),
                Reason = "priorities shifted",
            });
        Assert.Equal(HttpStatusCode.NoContent, abandonResponse.StatusCode);

        var goal = await (await client.GetAsync($"/api/goals/{goalId}"))
            .Content.ReadFromJsonAsync<GoalDto>();
        Assert.Equal(GoalStatus.Abandoned, goal!.Status);
        Assert.Equal("priorities shifted", goal.ResolutionReason);
    }

    [Fact]
    public async Task Get_returns_404_for_unknown_id()
    {
        var client = _factory.CreateClient();
        var response = await client.GetAsync($"/api/goals/{Guid.NewGuid()}");
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    private static async Task<Guid> SetSkillGoalAsync(HttpClient client)
    {
        var response = await client.PostAsJsonAsync("/api/goals",
            new SetGoalRequest
            {
                SkillId = ResumeTestFactory.SeededSkillId,
                TargetMaturity = Maturity.Expert,
                Motivation = "test",
            });
        response.EnsureSuccessStatusCode();
        var set = await response.Content.ReadFromJsonAsync<SetGoalResponse>();
        return set!.GoalId;
    }
}

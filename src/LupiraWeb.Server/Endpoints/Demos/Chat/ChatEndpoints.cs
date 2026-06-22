using LupiraWeb.Server.Endpoints.Demos.Chat.Dtos;

namespace LupiraWeb.Server.Endpoints.Demos.Chat;

public static class ChatEndpoints
{
    public static IEndpointRouteBuilder MapChatEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/demos/chat").WithTags("Demos.Chat");

        group.MapPost("/messages",
                (ChatRequest body, ChatHandler handler, CancellationToken ct)
                    => handler.SendAsync(body, ct))
            .WithName("DemoChatSendMessage");

        return app;
    }
}

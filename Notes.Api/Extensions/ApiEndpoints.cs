using Notes.Api.Features.CreateNote;

namespace Notes.Api.Extensions;

public static class ApiEndpoints
{
    public static void MapApiEndpoints(this WebApplication app)
    {
        app.MapPost("/notes", CreateNoteEndpoint.Execute)
            .WithTags("Notes");
    }
}

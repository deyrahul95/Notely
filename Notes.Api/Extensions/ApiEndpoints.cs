using Notes.Api.Features.CreateNote;
using Notes.Api.Features.GetNotes;

namespace Notes.Api.Extensions;

public static class ApiEndpoints
{
    public static void MapApiEndpoints(this WebApplication app)
    {
        var notesGroup = app.MapGroup("/notes").WithTags("Notes");

        // notesGroup.MapGet("/", GetNotesEndpoint.Execute);
        notesGroup.MapPost("/", CreateNoteEndpoint.Execute);
    }
}

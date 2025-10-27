using Microsoft.AspNetCore.Mvc;
using Notes.Api.Data;

namespace Notes.Api.Features.CreateNote;

internal static class CreateNoteEndpoint
{
    public record CreateNoteRequest(string Title, string Content);
    public record CreateNoteResponse(
        Guid Id,
        string Title,
        string Content,
        DateTime CreatedAtUtc,
        List<TagResponse> Tags);
    public record TagResponse(
        Guid Id,
        string Name,
        string Color,
        DateTime CreatedAtUtc);

    public static async Task<IResult> Execute(
        [FromBody] CreateNoteRequest request,
        NoteDbContext dbContext,
        ILogger<Program> logger)
    {
        try
        {
            logger.LogInformation(
                "[CreateNoteEndpoint] Execution started for request: {@Request}",
                request);

            var note = new Note()
            {
                Id = Guid.CreateVersion7(),
                Title = request.Title,
                Content = request.Content,
                CreatedAtUtc = DateTime.UtcNow,
                LastUpdatedAtUtc = DateTime.UtcNow,
            };

            dbContext.Notes.Add(note);
            await dbContext.SaveChangesAsync();

            var response = new CreateNoteResponse(
                Id: note.Id,
                Title: note.Title,
                Content: note.Content,
                CreatedAtUtc: note.CreatedAtUtc,
                Tags: []);

            logger.LogInformation(
                "[CreateNoteEndpoint] Execution completed for request: {@Request}",
                request);

            return Results.Created($"notes/{note.Id}", response);
        }
        catch (Exception ex)
        {
            logger.LogError(
                exception: ex,
                message: "[CreateNoteEndpoint] Failed to create note. Error: {@Message}",
                ex.Message);
            return Results.Problem("An error occurred while creating the note!");
        }
    }
}

using Microsoft.AspNetCore.Mvc;
using Notely.Shared.DTOs;
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

    public static async Task<IResult> Execute(
        [FromBody] CreateNoteRequest request,
        NoteDbContext dbContext,
        IHttpClientFactory httpClientFactory,
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

            var tags = await AnalyzeNoteForTags(
                note: note,
                httpClientFactory: httpClientFactory,
                logger: logger);

            var response = new CreateNoteResponse(
                Id: note.Id,
                Title: note.Title,
                Content: note.Content,
                CreatedAtUtc: note.CreatedAtUtc,
                Tags: tags);

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

    private static async Task<List<TagResponse>> AnalyzeNoteForTags(
        Note note,
        IHttpClientFactory httpClientFactory,
        ILogger<Program> logger)
    {
        try
        {
            var analyzedNoteRequest = new AnalyzeNoteRequest(
                NoteId: note.Id,
                Title: note.Title,
                Content: note.Content);

            var client = httpClientFactory.CreateClient("TagsApi");
            var response = await client.PostAsJsonAsync(
                requestUri: "tags/analyze",
                value: analyzedNoteRequest);

            if (response.IsSuccessStatusCode == false)
            {
                logger.LogWarning(
                    "Analyzed response status code does not indicate success. Status Code: {@StatusCode}, Response: {@Response}",
                    response.StatusCode,
                    response);
                return [];
            }

            var tags = await response.Content.ReadFromJsonAsync<AnalyzeNoteResponse>();
            return tags?.Tags ?? [];
        }
        catch (Exception ex)
        {
            logger.LogError(
                exception: ex,
                message: "[CreateNoteEndpoint] Failed to analyzed note for tags. Note: {@Note}, Error: {@Message}",
                note,
                ex.Message);
            throw;
        }
    }
}

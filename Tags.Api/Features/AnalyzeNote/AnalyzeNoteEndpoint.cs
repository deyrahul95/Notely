using Microsoft.AspNetCore.Mvc;
using Notely.Shared.DTOs;
using Tags.Api.Data;

namespace Tags.Api.Features.AnalyzeNote;

internal static class AnalyzeNoteEndpoint
{
    public static async Task<IResult> Execute(
        [FromBody] AnalyzeNoteRequest request,
        TagDbContext dbContext,
        ILogger<Program> logger)
    {
        try
        {
            logger.LogInformation(
                "[AnalyzeNoteEndpoint] Execution started for request: {@Request}",
                request);

            var tags = AnalyzeContentForTags(request.Title, request.Content);
            var tagEntities = tags.Select(tag => new Tag()
            {
                Id = Guid.CreateVersion7(),
                Name = tag.Name,
                Color = tag.Color,
                NoteId = request.NoteId,
                CreatedAtUtc = DateTime.UtcNow,
            }).ToList();

            dbContext.Tags.AddRange(tagEntities);
            await dbContext.SaveChangesAsync();

            var response = new AnalyzeNoteResponse(NoteId: request.NoteId, Tags: tags);

            logger.LogInformation(
                "[CreateNoteEndpoint] Execution completed for request: {@Request}",
                request);

            return Results.Ok(response);
        }
        catch (Exception ex)
        {
            logger.LogError(
                ex,
                "[AnalyzeNoteEndpoint] Failed to analyze note. Id: {@NoteId}, Error: {@Message}",
                request.NoteId,
                ex.Message);
            return Results.Problem("An error occurred while analysing the note.");
        }
    }

    private static List<TagResponse> AnalyzeContentForTags(string title, string content)
    {
        var tags = new List<TagResponse>();
        var allText = $"{title} {content}".ToLowerInvariant();

        // Simple keyword-based tagging (replace with LLM in the future)
        var tagKeywords = new Dictionary<string, (string name, string color)>{
            { "work", ("Work", "#3B82F6") },
            { "personal", ("Personal", "#10B981") },
            { "important", ("Important", "#EF4444") },
            { "urgent", ("Urgent", "#F59E0B") },
            { "idea", ("Idea", "#8B5CF6") },
            { "meeting", ("Meeting", "#06B6D4") },
            { "project", ("Project", "#84CC16") },
            { "todo", ("Todo", "#F97316") },
            { "reminder", ("Reminder", "#EC4899") },
            { "note", ("Note", "#6B7280") }
        };

        foreach (var keyword in tagKeywords)
        {
            if (allText.Contains(keyword.Key))
            {
                tags.Add(new TagResponse(
                    Id: Guid.NewGuid(),
                    Name: keyword.Value.name,
                    Color: keyword.Value.color,
                    CreateAtUtc: DateTime.UtcNow
                ));
            }
        }

        // If no tags found, add a default 'General' tag
        if (tags.Count == 0)
        {
            tags.Add(new TagResponse(
                Id: Guid.NewGuid(),
                Name: "General",
                Color: "#6B7280",
                CreateAtUtc: DateTime.UtcNow
            ));
        }

        return tags;
    }
}

using System.Linq.Expressions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Notes.Api.Data;

namespace Notes.Api.Features.GetNotes;

internal class GetNotesEndpoint
{
    private static readonly Expression<Func<Note, NoteResponse>> ToResponse =
        note => new NoteResponse(
            Id: note.Id,
            Title: note.Title,
            Content: note.Content,
            LastUpdatedAtUtc: note.LastUpdatedAtUtc);

    public record GetNotesRequest(int PageNumber, int PageSize);
    public record NoteResponse(Guid Id, string Title, string Content, DateTime LastUpdatedAtUtc);
    public record GetNotesResponse(int Total, int PageNumber, int PageSize, List<NoteResponse> Items);

    public static async Task<IResult> Execute(
        [FromQuery] GetNotesRequest request,
        NoteDbContext dbContext,
        ILogger<Program> logger)
    {
        try
        {
            logger.LogInformation(
                "[GetNotesEndpoint] Execution started for request {@Request}.",
                request);

            var countTask = dbContext.Notes.CountAsync();

            var notesTask = dbContext.Notes.AsNoTracking()
                                             .OrderByDescending(n => n.LastUpdatedAtUtc)
                                             .Skip(request.PageNumber * request.PageSize)
                                             .Take(request.PageSize)
                                             .Select(ToResponse)
                                             .ToListAsync();

            await Task.WhenAll(countTask, notesTask);

            logger.LogInformation(
                "[GetNotesEndpoint] Found {Count} number of notes on page {PageNumber}. Total: {Total}",
                notesTask.Result.Count,
                request.PageNumber,
                countTask.Result);

            var response = new GetNotesResponse(
                Total: countTask.Result,
                PageNumber: request.PageNumber,
                PageSize: request.PageSize,
                Items: notesTask.Result);

            logger.LogInformation(
                "[GetNotesEndpoint] Execution completed for request {@Request}.",
                request);
            return Results.Ok(response);
        }
        catch (Exception ex)
        {
            logger.LogError(
                 exception: ex,
                 message: "[GetNotesEndpoint] Failed to retrieve notes. Error: {@Message}",
                 ex.Message);
            return Results.Problem("An error occurred while retrieving the notes!");
        }
    }
}
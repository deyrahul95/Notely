using Microsoft.EntityFrameworkCore;
using Tags.Api.Data;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

builder.Services.AddDbContext<TagDbContext>(option =>
    option.UseNpgsql(builder.Configuration.GetConnectionString("notely-tags")));
builder.EnrichNpgsqlDbContext<TagDbContext>();

builder.Services.AddOpenApi();

var app = builder.Build();

app.MapDefaultEndpoints();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();

    // Apply EF migrations only in development
    using var scope = app.Services.CreateScope();
    var dbContext = scope.ServiceProvider.GetRequiredService<TagDbContext>();
    dbContext.Database.Migrate();
}

app.UseHttpsRedirection();

app.Run();

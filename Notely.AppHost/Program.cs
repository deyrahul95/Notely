var builder = DistributedApplication.CreateBuilder(args);

builder.AddProject<Projects.Notes_Api>("notes-api");

builder.AddProject<Projects.Tags_Api>("tags-api");

builder.Build().Run();

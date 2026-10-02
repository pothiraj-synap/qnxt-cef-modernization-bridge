
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Modern.Core.Csharp.Api; // Binds the namespace where CefOutboxProcessor lives

var builder = WebApplication.CreateBuilder(args);

// 1. Register Web API Controllers to manage incoming HTTP client requests
builder.Services.AddControllers();
//will be removed
//builder.Services.AddOpenApi();
// 2. Senior Architecture Pattern: Register your custom QNXT CEF Outbox Background Processor.
// This activates the background thread loop to continuously monitor the queue without blocking the Web API.
builder.Services.AddHostedService<CefOutboxProcessor>();

var app = builder.Build();
//will be removed
// Configure the HTTP request pipeline.
//if (app.Environment.IsDevelopment())
//{
   // app.MapOpenApi();
//}

// 3. Configure the HTTP request processing pipeline
app.UseRouting();
app.UseAuthorization();

// 4. Map the API routes to your controllers seamlessly
app.MapControllers();
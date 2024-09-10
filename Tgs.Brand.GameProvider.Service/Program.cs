using Microsoft.EntityFrameworkCore;
using OpenTelemetry.Exporter;
using OpenTelemetry.Instrumentation.AspNetCore;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Tgs.Brand.GameProvider.Repositories.DependencyInjection;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration
    .AddJsonFile("/config/appsettings.json", true)
    .AddEnvironmentVariables();

builder.Services
    .AddBrandGameProviderCore()
    .AddBrandGameProviderRepositories((_, dbOptionsBuilder) => dbOptionsBuilder
        .UseNpgsql("name=Tgs"))
    .AddRedis((options, sp) =>
    {
        var config = sp.GetRequiredService<IConfiguration>();

        options.ConnectionString = config.GetConnectionString("Redis")!;
    });

builder.Logging
    .AddOpenTelemetry(options =>
    {
        options.IncludeFormattedMessage = true;
        options.ParseStateValues = true;
        _ = options.AddFluentdForwardExporter(fluentOptions =>
        {
            var nodeIp = builder.Configuration.GetValue<string>("Otel:NodeIp");
            fluentOptions.Host = nodeIp!;
            fluentOptions.Tag = builder.Configuration.GetValue<string>("Otel:ServiceName")!;
            fluentOptions.UseMessagePack();
        });
    });

builder.Services
    .AddOptions<OtlpExporterOptions>()
    .Configure<IConfiguration>((options, conifg) =>
    {
        var nodeIp = conifg.GetValue<string>("NODE_IP");

        if (!string.IsNullOrWhiteSpace(nodeIp))
            options.Endpoint = new Uri($"http://{nodeIp}:4317");

        conifg.GetSection("Opentelemetry:Tracing").Bind(options);
    });

builder.Services
    .AddOpenTelemetry()
    .ConfigureResource(_ => _.AddService(builder.Configuration.GetValue<string>("Otel:ServiceName")))
    .WithTracing(tracingBuilder => tracingBuilder
        .AddSource("Tgs.*")
        .AddSource("TGS.*")
        .AddAspNetCoreInstrumentation()
        .AddHttpClientInstrumentation()
        .AddEntityFrameworkCoreInstrumentation(options => options.SetDbStatementForText = true)
        .AddProfileViewExporter()
        .AddOtlpExporter(options => options.Endpoint = builder.Configuration.GetValue<Uri>("Otel:Collector")))
    .Services
    .Configure<AspNetCoreInstrumentationOptions>(options => options.Filter =
        httpContext => !httpContext.Request.Path.HasValue ||
                       (!httpContext.Request.Path.StartsWithSegments("/swagger") &&
                        !httpContext.Request.Path.StartsWithSegments("/healthz") &&
                        !httpContext.Request.Path.StartsWithSegments("/profiler/") &&
                        !new[] { "html", "ico", "png", "jpg" }.Any(
                            ext => httpContext.Request.Path.Value.EndsWith($".{ext}"))));

builder.Services.AddProfileViewer();
builder.Services.AddHealthChecks();
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

app.UseSwaggerUI();
app.MapControllers();
app.MapSwagger();
app.MapHealthChecks("/healthz");

app.Run();
using System.Globalization;
using LoraStatsNet.Database;
using LoraStatsNet.Services;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Localization;

var builder = WebApplication.CreateBuilder(args);
var configuration = new Configuration(builder.Configuration);

builder.Logging.AddSimpleConsole(options => { options.SingleLine = true; options.TimestampFormat = "HH:mm:ss "; });
builder.Services.AddDbContext<LoraStatsNetDb>();
builder.Services.AddDataProtection().PersistKeysToFileSystem(new DirectoryInfo(Path.Combine(configuration.DataDir, "./keys/")));
builder.Services.AddControllers(options => { options.SuppressImplicitRequiredAttributeForNonNullableReferenceTypes = true; });
builder.Services.AddSingleton(configuration);
builder.Services.AddFileLoggerProvider(configuration);
builder.Services.AddRazorPages();

builder.Services.AddSingleton<MeshCrypto>();
builder.Services.AddHostedService<MQTTService>();
builder.Services.AddScoped<MQTTWorker>();
if (configuration.Liam)
{
	builder.Services.AddHostedService<LiamService>();
	builder.Services.AddScoped<LiamWorker>();
}

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
	await scope.ServiceProvider.GetRequiredService<LoraStatsNetDb>().InitializeAsync();
}
app.UseStatusCodePages();
app.UseExceptionHandler("/Error");
app.UseMiddleware<ExceptionLoggingMiddleware>();
app.UseForwardedHeaders(new ForwardedHeadersOptions { ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto });
app.UseMiddleware<RequestLoggingMiddleware>();
app.UseMiddleware<RequestFilteringMiddleware>();
var cultureInfo = new CultureInfo("pl-PL");
cultureInfo.DateTimeFormat.ShortDatePattern = "yyyy-MM-dd";
cultureInfo.DateTimeFormat.LongTimePattern = "HH:mm:ss";
var cultureInfos = new[] { cultureInfo };
app.UseRequestLocalization(new RequestLocalizationOptions { DefaultRequestCulture = new RequestCulture(cultureInfo), SupportedCultures = cultureInfos, SupportedUICultures = cultureInfos });
app.UseHttpsRedirection();
app.UseStaticFiles();
app.MapControllers();
app.MapRazorPages();

await app.RunAsync();

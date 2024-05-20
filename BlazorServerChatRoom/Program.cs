using BlazorServerChatRoom.Components;
using BlazorServerChatRoom.Hubs;
using BlazorServerChatRoom.Services;
using Microsoft.AspNetCore.ResponseCompression;
using MudBlazor;
using MudBlazor.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddSingleton<ChatRoomManager>();

/*  Intentionally setting the KeepAliveInterval to longer than the ServerTimeout in Chat.razor.
    I could not figure out any other way to get the server to timeout with inactivity.
    If you don't specify a KeepAliveInterval, the default is 15 seconds. I think this was the
    cause of keeping the server alive regardless of length of inactivity.
 */

builder.Services.AddSignalR().AddHubOptions<ChatRoomHub>(options =>
{
    options.KeepAliveInterval = TimeSpan.FromMinutes(15);
});


builder.Services.AddMudServices();
builder.Services.AddMudServices(config =>
{
    config.SnackbarConfiguration.PositionClass = Defaults.Classes.Position.TopCenter;
    config.SnackbarConfiguration.PreventDuplicates = false;
    config.SnackbarConfiguration.NewestOnTop = false;
    config.SnackbarConfiguration.ShowCloseIcon = true;
    config.SnackbarConfiguration.VisibleStateDuration = 3000;
    config.SnackbarConfiguration.HideTransitionDuration = 500;
    config.SnackbarConfiguration.ShowTransitionDuration = 500;
    config.SnackbarConfiguration.SnackbarVariant = Variant.Filled;
});

builder.Services.AddResponseCompression(opts =>
{
    opts.MimeTypes = ResponseCompressionDefaults.MimeTypes.Concat(["application/octet-stream"]);
});

builder.Services.AddHttpClient();

var app = builder.Build();
app.UseResponseCompression();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseStaticFiles();
app.UseAntiforgery();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();
app.MapHub<ChatRoomHub>("/chatroomhub");

app.Run();

using System.Security.Claims;
using System.Text.Json;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OAuth;
using Zealot.Shared.Database;
using Zealot.Shared.Services;
using Zealot.Shared.Services.Interfaces;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages();
builder.Services.AddDbContext<BotDbContext>();
builder.Services.AddScoped<IGuildDataService, GuildDataService>();
builder.Services.AddHttpClient();
builder.Services
    .AddAuthentication(options =>
    {
        options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = "Discord";
    })
    .AddCookie(options =>
    {
        options.LoginPath = "/Login";
        options.LogoutPath = "/Logout";
        options.AccessDeniedPath = "/Login";
    })
    .AddOAuth("Discord", options =>
    {
        options.ClientId = builder.Configuration["Discord:ClientId"] ?? string.Empty;
        options.ClientSecret = builder.Configuration["Discord:ClientSecret"] ?? string.Empty;
        options.CallbackPath = "/signin-discord";

        options.AuthorizationEndpoint = "https://discord.com/oauth2/authorize";
        options.TokenEndpoint = "https://discord.com/api/oauth2/token";
        options.UserInformationEndpoint = "https://discord.com/api/users/@me";

        options.Scope.Add("identify");
        options.Scope.Add("guilds");
        options.SaveTokens = true;

        options.Events = new OAuthEvents
        {
            OnCreatingTicket = async context =>
            {
                using var request = new HttpRequestMessage(HttpMethod.Get, context.Options.UserInformationEndpoint);
                request.Headers.Authorization = new("Bearer", context.AccessToken);

                using var response = await context.Backchannel.SendAsync(
                    request,
                    HttpCompletionOption.ResponseHeadersRead,
                    context.HttpContext.RequestAborted);

                response.EnsureSuccessStatusCode();

                await using var stream = await response.Content.ReadAsStreamAsync(context.HttpContext.RequestAborted);
                using var user = await JsonDocument.ParseAsync(stream, cancellationToken: context.HttpContext.RequestAborted);
                var root = user.RootElement;
                static string? GetString(JsonElement element, string propertyName)
                {
                    return element.TryGetProperty(propertyName, out var property)
                        ? property.GetString()
                        : null;
                }

                var discordId = GetString(root, "id");
                var username = GetString(root, "username") ?? "Discord User";
                var globalName = GetString(root, "global_name") ?? username;
                var avatarHash = GetString(root, "avatar");

                if (!string.IsNullOrWhiteSpace(discordId))
                {
                    context.Identity?.AddClaim(new Claim(ClaimTypes.NameIdentifier, discordId));
                    context.Identity?.AddClaim(new Claim("discord:id", discordId));
                }

                context.Identity?.AddClaim(new Claim(ClaimTypes.Name, globalName));
                context.Identity?.AddClaim(new Claim("discord:username", username));

                if (!string.IsNullOrWhiteSpace(discordId) && !string.IsNullOrWhiteSpace(avatarHash))
                {
                    context.Identity?.AddClaim(new Claim(
                        "discord:avatar",
                        $"https://cdn.discordapp.com/avatars/{discordId}/{avatarHash}.png?size=128"));
                }
            }
        };
    });

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapStaticAssets();
app.MapRazorPages()
   .WithStaticAssets();

app.Run();

using System.Collections.Immutable;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text.Json;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Zealot.Shared.Services.Interfaces;

namespace Zealot.Web.Pages;

[Authorize]
public class DashboardModel : PageModel
{
    private const ulong Administrator = 0x8;
    private const ulong ManageGuild = 0x20;

    private readonly IGuildDataService _guildDataService;
    private readonly IHttpClientFactory _httpClientFactory;

    public DashboardModel(
        IGuildDataService guildDataService,
        IHttpClientFactory httpClientFactory)
    {
        _guildDataService = guildDataService;
        _httpClientFactory = httpClientFactory;
    }

    public string DisplayName { get; private set; } = "Discord User";
    public string DiscordId { get; private set; } = "Unknown";
    public string? AvatarUrl { get; private set; }
    public string Initials { get; private set; } = "Z";

    public IReadOnlyList<GuildViewModel> Guilds { get; private set; }
        = ImmutableList<GuildViewModel>.Empty;

    public async Task OnGetAsync()
    {
        DisplayName = User.Identity?.Name ?? "Discord User";

        DiscordId = User.FindFirstValue("discord:id")
            ?? User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? "Unknown";

        AvatarUrl = User.FindFirstValue("discord:avatar");
        Initials = BuildInitials(DisplayName);

        var accessToken = await HttpContext.GetTokenAsync("access_token");

        if (string.IsNullOrWhiteSpace(accessToken))
        {
            return;
        }

        var client = _httpClientFactory.CreateClient();

        var request = new HttpRequestMessage(
            HttpMethod.Get,
            "https://discord.com/api/v10/users/@me/guilds");

        request.Headers.Authorization =
            new AuthenticationHeaderValue("Bearer", accessToken);

        var response = await client.SendAsync(request);

        response.EnsureSuccessStatusCode();

        var guildsJson =
            await response.Content.ReadFromJsonAsync<JsonElement[]>();

        if (guildsJson is null || guildsJson.Length == 0)
        {
            return;
        }

        var guildIds = guildsJson
            .Select(g => ulong.Parse(g.GetProperty("id").GetString()!))
            .ToList();

        var databaseGuilds =
            await _guildDataService.GetGuildsByIdsAsync(guildIds);

        var guildLookup = databaseGuilds.ToDictionary(g => g.GuildId);

        Guilds =
        [
            .. guildsJson
                .Where(g =>
                {
                    var owner = g.GetProperty("owner").GetBoolean();

                    var permissions =
                        ulong.Parse(g.GetProperty("permissions").GetString()!);

                    return owner
                        || (permissions & Administrator) == Administrator
                        || (permissions & ManageGuild) == ManageGuild;
                })
                .Where(g =>
                {
                    var guildId =
                        ulong.Parse(g.GetProperty("id").GetString()!);

                    return guildLookup.ContainsKey(guildId);
                })
                .Select(g =>
                {
                    var guildId =
                        ulong.Parse(g.GetProperty("id").GetString()!);

                    var owner =
                        g.GetProperty("owner").GetBoolean();

                    var permissions =
                        ulong.Parse(g.GetProperty("permissions").GetString()!);

                    var canManage =
                        owner
                        || (permissions & Administrator) == Administrator
                        || (permissions & ManageGuild) == ManageGuild;

                    var dbGuild = guildLookup[guildId];

                    string? iconUrl = dbGuild.IconUrl;

                    if (string.IsNullOrWhiteSpace(iconUrl)
                        && g.TryGetProperty("icon", out var iconProperty))
                    {
                        var iconHash = iconProperty.GetString();

                        if (!string.IsNullOrWhiteSpace(iconHash))
                        {
                            iconUrl =
                                $"https://cdn.discordapp.com/icons/{guildId}/{iconHash}.png";
                        }
                    }

                    return new GuildViewModel(
                        guildId,
                        dbGuild.Name,
                        dbGuild.TotalMembers,
                        iconUrl,
                        canManage);
                })
                .OrderBy(g => g.Name)
        ];

        Console.WriteLine("Guilds available to user:");

        foreach (var guild in Guilds)
        {
            Console.WriteLine(
                $"- {guild.Name} ({guild.MemberCount} members)");
        }
    }

    private static string BuildInitials(string name)
    {
        var words = name.Split(
            ' ',
            StringSplitOptions.RemoveEmptyEntries |
            StringSplitOptions.TrimEntries);

        if (words.Length == 0)
        {
            return "Z";
        }

        return string.Concat(
            words.Take(2)
                .Select(word => char.ToUpperInvariant(word[0])));
    }
}

public sealed record GuildViewModel(
    ulong GuildId,
    string Name,
    int MemberCount,
    string? IconUrl,
    bool CanManage);
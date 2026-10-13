using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using FluentAssertions;
using UrlShortener.Api.Models;

namespace UrlShortener.Tests.Integration;

public class LinkSecurityTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly CustomWebApplicationFactory _factory;

    public LinkSecurityTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
    }

    private async Task<(HttpClient client, string token)> CreateAuthenticatedClientAsync()
    {
        var client = _factory.CreateClient();
        var email = $"user_{Guid.NewGuid()}@example.com";

        var registerResponse = await client.PostAsJsonAsync("/api/Auth/register", new
        {
            email,
            password = "Password123!"
        });

        var body = await registerResponse.Content.ReadFromJsonAsync<AuthResponse>();
        client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", body!.Token);

        return (client, body.Token);
    }

    [Fact]
    public async Task CreateLink_WithoutToken_Returns401()
    {
        var client = _factory.CreateClient();
        var response = await client.PostAsJsonAsync("/api/Links", new { originalUrl = "https://google.com" });
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetLinks_WithoutToken_Returns401()
    {
        var client = _factory.CreateClient();
        var response = await client.GetAsync("/api/Links");
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task CreateLink_WithToken_Returns201()
    {
        var (client, _) = await CreateAuthenticatedClientAsync();

        var response = await client.PostAsJsonAsync("/api/Links", new { originalUrl = "https://google.com" });

        response.StatusCode.Should().Be(HttpStatusCode.Created);
    }

    [Fact]
    public async Task User_CannotDeleteAnotherUsersLink_Returns404()
    {
        var (clientA, _) = await CreateAuthenticatedClientAsync();
        var createResponse = await clientA.PostAsJsonAsync("/api/Links", new { originalUrl = "https://google.com" });
        var created = await createResponse.Content.ReadFromJsonAsync<CreateLinkResponse>();

        var (clientB, _) = await CreateAuthenticatedClientAsync();
        var deleteResponse = await clientB.DeleteAsync($"/api/Links/{created!.Id}");

        deleteResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task User_OnlySeesOwnLinks()
    {
        var (clientA, _) = await CreateAuthenticatedClientAsync();
        await clientA.PostAsJsonAsync("/api/Links", new { originalUrl = "https://user-a-only.com" });

        var (clientB, _) = await CreateAuthenticatedClientAsync();
        var listResponse = await clientB.GetAsync("/api/Links");
        var list = await listResponse.Content.ReadFromJsonAsync<PagedResponse<LinkListItemResponse>>();

        list!.Items.Should().NotContain(l => l.OriginalUrl.Contains("user-a-only"));
    }

    [Fact]
    public async Task CreateLink_JavaScriptUrl_Returns400()
    {
        var (client, _) = await CreateAuthenticatedClientAsync();

        var response = await client.PostAsJsonAsync("/api/Links", new
        {
            originalUrl = "javascript:alert(1)"
        });

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Redirect_ExpiredLink_Returns404()
    {
        var (client, _) = await CreateAuthenticatedClientAsync();

        var response = await client.PostAsJsonAsync("/api/Links", new
        {
            originalUrl = "https://google.com",
            expiresAt = DateTime.UtcNow.AddSeconds(1)
        });

        var created = await response.Content.ReadFromJsonAsync<CreateLinkResponse>();

        await Task.Delay(1500);

        var redirectClient = _factory.CreateClient(
            new Microsoft.AspNetCore.Mvc.Testing.WebApplicationFactoryClientOptions
            {
                AllowAutoRedirect = false
            });
        var redirectResponse = await redirectClient.GetAsync($"/{created!.ShortCode}");

        redirectResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}
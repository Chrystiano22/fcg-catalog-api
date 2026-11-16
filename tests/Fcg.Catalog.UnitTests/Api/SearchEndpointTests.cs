using System.Net;
using System.Net.Http.Headers;

namespace Fcg.Catalog.UnitTests.Api;

public sealed class SearchEndpointTests : IClassFixture<ApiWebApplicationFactory>
{
    private readonly HttpClient client;
    public SearchEndpointTests(ApiWebApplicationFactory factory) => client = factory.CreateClient();

    [Fact]
    public async Task AnonymousSearchIsUnauthorized() =>
        Assert.Equal(HttpStatusCode.Unauthorized, (await client.GetAsync("/jogos/busca?q=aventura")).StatusCode);

    [Fact]
    public async Task DisabledSearchReturnsServiceUnavailable()
    {
        using var request = new HttpRequestMessage(HttpMethod.Get, "/jogos/busca?q=aventura");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", TestJwtTokenFactory.CreateUserToken());
        Assert.Equal(HttpStatusCode.ServiceUnavailable, (await client.SendAsync(request)).StatusCode);
    }
}

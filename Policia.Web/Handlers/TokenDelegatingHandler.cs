using Blazored.LocalStorage;
using System.Net.Http.Headers;

namespace Policia.Web.Handlers;

/// <summary>
/// Intercepta CADA petición HTTP y agrega automáticamente el token JWT
/// desde LocalStorage. Así ninguna página necesita hacerlo manualmente.
/// </summary>
public class TokenDelegatingHandler : DelegatingHandler
{
    private readonly ILocalStorageService _localStorage;

    public TokenDelegatingHandler(ILocalStorageService localStorage)
        : base(new HttpClientHandler())
    {
        _localStorage = localStorage;
    }

    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request, CancellationToken cancellationToken)
    {
        try
        {
            var token = await _localStorage.GetItemAsync<string>("token_policial");
            if (!string.IsNullOrEmpty(token))
            {
                // Agrega el header SOLO a esta petición (no al HttpClient global)
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
            }
        }
        catch
        {
            // Si LocalStorage no está disponible, la petición continúa sin token
        }

        return await base.SendAsync(request, cancellationToken);
    }
}

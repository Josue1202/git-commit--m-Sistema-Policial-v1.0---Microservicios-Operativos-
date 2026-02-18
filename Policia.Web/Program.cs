using Blazored.LocalStorage;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Policia.Web;
using Policia.Web.Handlers;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

var apiUrl = builder.Configuration["ApiUrl"] ?? "https://localhost:7059";

// 1. LocalStorage (para guardar/leer token JWT)
builder.Services.AddBlazoredLocalStorage();

// 2. Handler que intercepta peticiones HTTP y agrega el token automáticamente
builder.Services.AddScoped<TokenDelegatingHandler>();

// 3. HttpClient configurado con el handler (ya no necesita headers manuales)
builder.Services.AddScoped(sp =>
{
    var handler = sp.GetRequiredService<TokenDelegatingHandler>();
    return new HttpClient(handler) { BaseAddress = new Uri(apiUrl) };
});

await builder.Build().RunAsync();
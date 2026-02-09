using Blazored.LocalStorage;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Policia.Web;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

// --- MODIFICACIÓN DE EMERGENCIA ---

// 1. IGNORAMOS el appsettings por un segundo.
// Ponemos la dirección DIRECTA al puerto SEGURO (HTTPS) del Gateway.
// (Confirma que 7059 es tu puerto https en el launchSettings.json del Gateway)
var apiUrl = "https://localhost:7059";

// 2. Configuramos el cliente con esa URL fija
builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(apiUrl) });

// 3. Activamos LocalStorage
builder.Services.AddBlazoredLocalStorage();
// -----------------------------------------------------

await builder.Build().RunAsync();
using Ocelot.DependencyInjection;
using Ocelot.Middleware;
// NUEVO: Librerías para entender la seguridad y los Tokens
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// ==============================================================================
// ZONA NUEVA: CONFIGURACIÓN DE SEGURIDAD (EL SCANNER DE PLACAS)
// ==============================================================================
// Aquí le enseñamos al Gateway a leer el Token antes de pasarlo.

// 1. Definimos la clave secreta (Debe ser la MISMA que usaste en Identity)
// OJO: Si esta clave no coincide, el sistema rechazará todas las placas.
var secretKey = builder.Configuration["Jwt:Key"] ?? "SISTEMA_POLICIAL_SECRETO_S2_JOSUE_2026_CLAVE_SUPER_SEGURA";

// 2. Registramos el servicio de autenticación
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer("Bearer", options => // <--- "Bearer" coincide con lo que pusiste en ocelot.json
{
    options.RequireHttpsMetadata = false;
    options.SaveToken = true;
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        // Convertimos la clave secreta a bytes para compararla
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey)),

        // En el Gateway a veces es mejor relajar estas validaciones y dejar que
        // el Microservicio final haga el chequeo estricto.
        ValidateIssuer = false,
        ValidateAudience = false
    };
});
// ==============================================================================


// --- 1. CONFIGURACIÓN DE CORS (PERMISO DE FRONTERA) ---
// ¿POR QUÉ? Porque el Frontend (Blazor) vive en un "país" diferente (puerto 7036)
// y el Gateway vive en otro (puerto 5107). Si no ponemos esto, el navegador bloquea el ataque.
builder.Services.AddCors(options =>
{
    // Creamos una política llamada "PermitirFrontend"
    options.AddPolicy("PermitirFrontend", policy =>
    {
        // A. ORIGEN PERMITIDO: Solo dejamos pasar peticiones que vengan de ESTA URL.
        // (Es la URL https que vimos en tu foto de perfiles de inicio)
        policy.WithOrigins("https://localhost:7036")

              // B. CUALQUIER MÉTODO: Dejamos pasar GET, POST, PUT, DELETE, etc.
              .AllowAnyMethod()

              // C. CUALQUIER CABECERA: Dejamos pasar Tokens de Autorización y JSONs.
              .AllowAnyHeader();
    });
});

// --- 2. CARGAMOS LA CONFIGURACIÓN DE OCELOT (TU MAPA) ---
// Le decimos al sistema: "Lee el archivo ocelot.json donde están las rutas de los patrulleros".
builder.Configuration.AddJsonFile("ocelot.json", optional: false, reloadOnChange: true);

// --- 3. INYECTAMOS LOS SERVICIOS DE OCELOT ---
// "Prepara las herramientas internas de Ocelot para que funcionen".
builder.Services.AddOcelot(builder.Configuration);

// Agregamos controladores (por si el Gateway tuviera lógica propia, aunque Ocelot no la suele necesitar)
builder.Services.AddControllers();

// Configuración de OpenAPI (Swagger)
builder.Services.AddOpenApi();

var app = builder.Build();

// --- 4. ACTIVAMOS EL CORS (¡IMPORTANTE!) ---
// OJO TÁCTICO: Esto TIENE que ir ANTES de UseOcelot.
// ¿Por qué? Porque primero revisamos el pasaporte (CORS) antes de dejarlo entrar al sistema (Ocelot).
app.UseCors("PermitirFrontend");

// NUEVO: ACTIVAMOS LA AUTENTICACIÓN
// Esto enciende el scanner. Sin esto, la configuración de arriba no sirve.
app.UseAuthentication();

// --- 5. ACTIVAMOS OCELOT (EL TRÁFICO) ---
// El 'await' es importante porque Ocelot trabaja asíncrono para no trabar el servidor.
// Aquí es donde el Gateway empieza a redirigir las llamadas a RRHH, Logística, etc.
await app.UseOcelot();

// Configuración para entorno de desarrollo (Swagger)
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
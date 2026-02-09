using Microsoft.AspNetCore.Authentication.JwtBearer; // <--- 1. HERRAMIENTA NUEVA: Para entender Tokens
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens; // <--- 2. HERRAMIENTA NUEVA: Para validar firmas
using Policia.RRHH.API.Models;
using System.Text; // <--- 3. HERRAMIENTA NUEVA: Para leer texto
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

// --- 1. CONEXIÓN A BD (MODIFICADO PARA ASEGURAR QUE FUNCIONE) ---
// Le agregué la parte de "GetConnectionString" para que sea igual al de Logística y no falle.
builder.Services.AddDbContext<BdRrhhContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("CadenaConexion")));


// --- 2. EVITAR CICLOS INFINITOS ---
builder.Services.AddControllers().AddJsonOptions(x =>
{
    // Esta línea corta el cable rojo: Ignora los ciclos repetidos
    x.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
});


// --- ZONA DE SEGURIDAD (CÓDIGO NUEVO AGREGADO) ---

// A. "Oye sistema, activa el servicio de AUTENTICACIÓN (Revisión de Identidad)"
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        // B. "Aquí están las REGLAS para los guardias de RRHH:"
        options.TokenValidationParameters = new TokenValidationParameters
        {
            // 1. ¿Validar quién emitió el token? (Debe ser Policia.Identity)
            ValidateIssuer = true,

            // 2. ¿Validar para quién es? (Debe ser Policia.Sistemas)
            ValidateAudience = true,

            // 3. ¿Validar si ya venció? (Si expiró, no entra)
            ValidateLifetime = true,

            // 4. ¿Validar la Firma Secreta? (CRUCIAL: Revisa la clave maestra)
            ValidateIssuerSigningKey = true,

            // C. LEEMOS LOS DATOS DEL ARCHIVO appsettings.json
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]))
        };
    });

// --- FIN ZONA DE SEGURIDAD ---


builder.Services.AddOpenApi();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();


// --- ACTIVAR LOS GUARDIAS EN ORDEN (NUEVO) ---

// D. PRIMERO: Authentication (¿Quién eres? Muestra tu placa)
app.UseAuthentication();

// E. SEGUNDO: Authorization (¿Tienes permiso para pasar?)
app.UseAuthorization();

app.MapControllers();

app.Run();
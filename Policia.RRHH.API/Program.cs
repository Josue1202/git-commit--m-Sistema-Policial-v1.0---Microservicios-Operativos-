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


// --- INICIO DE LA ZONA DE SEGURIDAD (CÓDIGO NUEVO AGREGADO) ---

// A. LEVANTAMOS EL SERVICIO DE AUTENTICACIÓN
// Le decimos al sistema: "Oye, prepárate para revisar credenciales usando JWT".
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        // B. DEFINIMOS LAS REGLAS DEL GUARDIA
        // Aquí le decimos al guardia QUÉ debe mirar con lupa en el Token.
        options.TokenValidationParameters = new TokenValidationParameters
        {
            // 1. ¿Validar el Emisor (Issuer)?
            // "Sí. Revisa que el token diga 'Policia.Identity'. Si dice otra cosa, es falso."
            ValidateIssuer = true,

            // 2. ¿Validar la Audiencia (Audience)?
            // "Sí. Revisa que el token sea para 'Policia.Sistemas'. Si es para otra app, no entra."
            ValidateAudience = true,

            // 3. ¿Validar la Duración (Lifetime)?
            // "Sí. Fíjate en la fecha de vencimiento. Si ya pasó, no lo dejes entrar."
            ValidateLifetime = true,

            // 4. ¿Validar la Firma (SigningKey)? ¡LA MÁS IMPORTANTE!
            // "Sí. Verifica que la firma digital coincida con nuestra CLAVE SECRETA."
            ValidateIssuerSigningKey = true,

            // C. LEEMOS LOS VALORES REALES DESDE appsettings.json
            // Aquí conectamos con el archivo de configuración para sacar la clave secreta y los nombres.
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]))
        };
    });

// --- FIN DE LA ZONA DE SEGURIDAD --


builder.Services.AddOpenApi();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

// --- ACTIVACIÓN DE LOS FILTROS (CÓDIGO NUEVO AGREGADO) ---

// D. ACTIVAMOS AL GUARDIA (Authentication)
// OJO: Esto DEBE ir antes de Authorization.
// Significa: "¿Quién eres? Déjame ver tu placa y verificar que no sea falsa".
app.UseAuthentication();

// E. ACTIVAMOS LOS PERMISOS (Authorization) - (YA LO TENÍAS, PERO AHORA FUNCIONA CON EL DE ARRIBA)
// Significa: "Ya sé quién eres. Ahora voy a ver si tienes permiso para entrar aquí".
app.UseAuthorization();

app.MapControllers();

app.Run();
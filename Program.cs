using Kiosco.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Kiosco.Service;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Registrar AuthService
builder.Services.AddTransient<AuthService>();



// Configurar DbContext
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Leer la clave secreta desde appsettings.json
var jwtSecret = builder.Configuration["Jwt:Secret"];
if (string.IsNullOrEmpty(jwtSecret))
{
    throw new InvalidOperationException("La clave secreta JWT no está configurada en appsettings.json.");
}

// Configurar JWT
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = builder.Configuration["Jwt:Issuer"],
        ValidAudience = builder.Configuration["Jwt:Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Secret"]))
    };
});

// Habilitar autorización
builder.Services.AddAuthorization();



// Configurar CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin() // Permite cualquier origen
              .AllowAnyMethod() // Permite cualquier método HTTP (GET, POST, etc.)
              .AllowAnyHeader(); // Permite cualquier encabezado
    });
});

builder.Services.AddTransient<PasswordMigrationService>();
builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Obtener el servicio de migración
using (var scope = app.Services.CreateScope())
{
    var passwordMigrationService = scope.ServiceProvider.GetRequiredService<PasswordMigrationService>();

    // Ejecutar la migración solo si hay contraseñas pendientes
    var pendingUsers = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>()
        .Users.Any(u => !u.PasswordHash.StartsWith("$2"));

    if (pendingUsers)
    {
        passwordMigrationService.MigratePasswords();
    }
}

app.UseStaticFiles();

// Configurar el pipeline de solicitudes HTTP
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage(); // Muestra detalles del error en desarrollo
}

app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Kiosco API v1");
    c.RoutePrefix = "swagger"; // Asegura que Swagger esté accesible en /swagger
});


// Usar CORS
app.UseCors("AllowAll");

app.UseHttpsRedirection();

app.UseAuthentication(); // Asegúrate de que esta línea esté antes de UseAuthorization
app.UseAuthorization();

app.MapControllers();

app.Run();
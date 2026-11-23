using Microting.EntityFrameworkCore.MySql; // <-- AÑADE ESTA LÍNEA
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using ProyectoFinal.API.Converters;
using ProyectoFinal.Domain.Interfaces;
using ProyectoFinal.Persistence.Contexts;
using ProyectoFinal.Persistence.Importers;
using ProyectoFinal.Persistence.Repositories;
using System.Text;
using Microsoft.Extensions.DependencyInjection;

var builder = WebApplication.CreateBuilder(args);

//builder.Services.AddControllers();
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        // Le decimos a .NET que si recibe un Vehiculo abstracto, por defecto cree un Coche 
        options.JsonSerializerOptions.Converters.Add(new VehiculoJsonConverter());
        //options.JsonSerializerOptions.Converters.Add(new ProyectoFinal.API.Converters.VehiculoJsonConverter());
    });

builder.Services.AddEndpointsApiExplorer();

//builder.Services.AddSwaggerGen();
builder.Services.AddSwaggerGen(options =>
{
    options.AddSecurityDefinition("bearer", new Microsoft.OpenApi.OpenApiSecurityScheme
    {
        Type = Microsoft.OpenApi.SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = Microsoft.OpenApi.ParameterLocation.Header,
        Description = "Introduce el token JWT aquí."
    });

    options.AddSecurityRequirement(document => new Microsoft.OpenApi.OpenApiSecurityRequirement
    {
        [new Microsoft.OpenApi.OpenApiSecuritySchemeReference("bearer", document)] = []
    });
});



// 1. Inyección de Dependencias (Open/Closed)
// Configuración de la persistencia (OCP)
var persistenceMode = builder.Configuration["PersistenceMode"];

if (persistenceMode == "Memory")
{
    builder.Services.AddSingleton<IVehiculoRepository, InMemoryVehiculoRepository>();
}
else if (persistenceMode == "MySQL")
{
    // Registramos el DbContext con la cadena de conexión
    var connectionString = builder.Configuration.GetConnectionString("MySqlConnection");
    builder.Services.AddDbContext<AppDbContext>(options =>
        options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString)));

    // Registramos el repositorio de EF Core
    builder.Services.AddScoped<IVehiculoRepository, EfVehiculoRepository>();
}
else
{
    throw new NotImplementedException("Modo de persistencia no implementado aún.");
}


builder.Services.AddTransient<IDataImporter, VehicleSpecificationImporter>();

// 2. Configuración de JWT
var jwtKey = builder.Configuration["Jwt:Key"]!;
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
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
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey))
        };
    });

var app = builder.Build();

// Aplicar migraciones automáticamente al arrancar (solo si usamos MySQL)
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    dbContext.Database.Migrate(); // Esto crea las tablas si no existen
}


if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthentication(); // Importante para leer el Token
app.UseAuthorization();  // Importante para [Authorize]
app.MapControllers();

app.Run();

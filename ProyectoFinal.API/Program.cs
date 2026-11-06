using ProyectoFinal.API.Converters;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using ProyectoFinal.Domain.Interfaces;
using ProyectoFinal.Persistence.Repositories;
using ProyectoFinal.Persistence.Importers;
using System.Text;


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
var persistenceMode = builder.Configuration["PersistenceMode"];
if (persistenceMode == "Memory")
{
    builder.Services.AddSingleton<IVehiculoRepository, InMemoryVehiculoRepository>();
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

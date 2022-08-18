using Polly;
using Polly.Timeout;
using System.Text;
using TalkBack.Common.MassTransit;
using TalkBack.Common.MongoDB;
using TalkBack.Login.Service;
using TalkBack.Login.Service.Clients;
using TalkBack.Login.Service.Entities;
using TalkBack.Login.Service.Services.PasswordHash;
using TalkBack.Login.Service.Services.TokenGenerators;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddMongo(builder.Configuration)
                .AddMongoRepository<LoginUser>("users")
                .AddMassTransitWithRabbitMq(builder.Configuration);

// Inter-service Http communication
builder.Services.AddCatalogClient();

builder.Services.AddSingleton<IPasswordHasher, PasswordHasher>();
builder.Services.AddScoped<IJwtTokenGenerator, JwtTokenGenerator>();
builder.Services.Configure<JwtSettings>(builder.Configuration.GetSection("JwtSettings"));


builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();

    // CORS settings
    const string AllowedOriginSetting = "AllowedOrigin";
    app.UseCors(b =>
    {
        b.WithOrigins(builder.Configuration[AllowedOriginSetting])
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
}

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
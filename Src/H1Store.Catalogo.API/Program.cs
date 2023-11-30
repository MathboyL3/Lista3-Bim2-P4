using H1Store.Catalogo.Application.AutoMapper;
using H1Store.Catalogo.Application.Interfaces;
using H1Store.Catalogo.Application.Services;
using H1Store.Catalogo.Data.Providers.MongoDb.Interfaces;
using H1Store.Catalogo.Data.Providers.MongoDb;
using H1Store.Catalogo.Data.Repository;
using H1Store.Catalogo.Domain.Interfaces;
using H1Store.Catalogo.Data.Providers.MongoDb.Configuration;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.DependencyInjection;
using H1Store.Catalogo.Data.AutoMapper;
using H1Store.Catalogo.Infra.EmailService;
using Microsoft.Extensions.Configuration;
using H1Store.Catalogo.Infra.Autenticacao.Models;
using H1Store.Catalogo.Infra.Autenticacao;
using Microsoft.AspNetCore.Cors.Infrastructure;
using H1Store.Catalogo.Application;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Microsoft.OpenApi.Models;
using System.Reflection;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(sa => {
    sa.SwaggerDoc("v1", new OpenApiInfo {
        Title = "Api lista 3",
        Version = "v1"
    });

    sa.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme {
        Description = "Autenticação via JWT. \r\n\r\n Use 'Bearer' [token]. \r\n\r\nExemplo: 'Bearer tokenmaissegurodomundo'",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer",
    });

    sa.AddSecurityRequirement(new OpenApiSecurityRequirement{
        {
            new OpenApiSecurityScheme(){
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                },
                Scheme = "oauth2",
                Name = "Bearer",
                In = ParameterLocation.Header,
            }, new List<string>()
        }
        
    });

});

builder.Services.Configure<MongoDbSettings>(
	builder.Configuration.GetSection("MongoDbSettings"));

builder.Services.AddSingleton<IMongoDbSettings>(serviceProvider =>
	   serviceProvider.GetRequiredService<IOptions<MongoDbSettings>>().Value);

builder.Services.AddAutoMapper(typeof(DomainToApplication), typeof(ApplicationToDomain));
builder.Services.AddAutoMapper(typeof(DomainToCollection), typeof(CollectionToDomain));

builder.Services.AddScoped(typeof(IMongoRepository<>), typeof(MongoRepository<>));

builder.Services.AddScoped<IProdutoRepository, ProdutoRepository>();
builder.Services.AddScoped<IProdutoService, ProdutoService>();

builder.Services.AddScoped<IFornecedorRepository, FornecedorRepository>();
builder.Services.AddScoped<IFornecedorService, FornecedorService>();

builder.Services.AddScoped<ICategoriaRepository, CategoriaRepository>();
builder.Services.AddScoped<ICategoriaService, CategoriaService>();

builder.Services.Configure<Token>(
	builder.Configuration.GetSection("token"));


builder.Services.AddScoped<ITokenService, TokenService>();

builder.Services.AddScoped<IUsuarioRepository, UsuarioRepository>();
builder.Services.AddScoped<IUsuarioService, UsuarioService>();

builder.Services.Configure<EmailConfig>(
	builder.Configuration.GetSection("EmailConfig"));

builder.Services.Configure<EmailMasterReport>(
    builder.Configuration.GetSection("ReportEmail"));

builder.Services.AddCors(x => x.AddPolicy("All", new CorsPolicyBuilder()
	.AllowAnyHeader()
	.AllowAnyMethod()
	.AllowAnyOrigin()
	.Build()
	));

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,

            ValidIssuer = builder.Configuration.GetSection("token:issuer").Get<string>(),
            ValidAudience = builder.Configuration.GetSection("token:audience").Get<string>(),
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration.GetSection("token:secret").Get<string>()))
        };
    });

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
	app.UseSwagger();
	app.UseSwaggerUI();
}

app.UseCors("All");

app.UseHttpsRedirection();

app.UseAuthentication();

app.UseRouting();

app.UseAuthorization();

app.MapControllers();

app.Run();

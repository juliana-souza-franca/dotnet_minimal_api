using System;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using minimal_api.Dominio.DTOs;
using minimal_api.Dominio.Entidade;
using minimal_api.Dominio.Enuns;
using minimal_api.Dominio.interfaces;
using minimal_api.Dominio.ModelViews;
using minimal_api.Dominio.Servico;
using minimal_api.Infratrutura.Db;
using minimal_api.Infratrutura.interfaces;

namespace minimal_api
{
    public class Startup
    {
        private readonly string _key;
        private readonly string _issuer;

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
            _key = configuration["Jwt:key"] ?? throw new ArgumentNullException("Jwt:key não encontrado.");
            _issuer = configuration["Jwt:Issuer"] ?? "meu-sistema-api";
        }

        public IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            // JWT
            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateLifetime = true,
                    ValidateAudience = false,
                    ValidateIssuer = true,
                    ValidIssuer = _issuer,
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_key))
                };
            });

            
            services.AddScoped<IAdministradorServico, AdministradorServico>();
            services.AddScoped<IVeiculoServico, VeiculoServico>();

            
            services.AddEndpointsApiExplorer();
            services.AddSwaggerGen(options =>
            {
                options.SwaggerDoc("v1", new OpenApiInfo
                {
                    Title = "API de Gestão de Administradores e Veículos",
                    Version = "v1"
                });

                options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    Name = "Authorization",
                    Type = SecuritySchemeType.Http,
                    Scheme = "bearer",
                    BearerFormat = "JWT",
                    In = ParameterLocation.Header,
                    Description = "Informe o token JWT: <token>."
                });

                options.AddSecurityRequirement(new OpenApiSecurityRequirement
                {
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference
                            {
                                Type = ReferenceType.SecurityScheme,
                                Id = "Bearer"
                            },
                            Scheme = "Bearer",
                            Name = "Authorization",
                            In = ParameterLocation.Header
                        },
                        new List<string>()
                    }
                });
            });

            
            var connectionString = Configuration.GetConnectionString("mysql");
            services.AddDbContext<DbContexto>(options =>
                options.UseMySql(connectionString, new MySqlServerVersion(new Version(8, 0, 36)))
            );

            services.Configure<Microsoft.AspNetCore.Http.Json.JsonOptions>(options =>
            {
                options.SerializerOptions.Converters.Add(new JsonStringEnumConverter());
            });

            services.AddAuthorization();
        }

        public void Configure(IApplicationBuilder app)
        {
            app.UseSwagger();
            app.UseSwaggerUI(sw =>
{
                sw.SwaggerEndpoint("/swagger/v1/swagger.json", "API de Gestão de Administradores e Veículos v1");
                sw.RoutePrefix = "swagger";
            });

            app.UseRouting();
            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                
                endpoints.MapGet("/", () => Results.Json(new { Mensagem = "Bem-vindo à API!" }))
         .AllowAnonymous();

                
                string GerarTokenJwt(Administrador administrador)
                {
                    var claims = new List<Claim>
                    {
                        new Claim("Email", administrador.Email),
                        new Claim("Perfil", administrador.Perfil.ToString()),
                        new Claim(ClaimTypes.Role, administrador.Perfil)
                    };

                    var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_key));
                    var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

                    var token = new JwtSecurityToken(
                        issuer: _issuer,
                        audience: _issuer,
                        claims: claims,
                        expires: DateTime.UtcNow.AddDays(1),
                        signingCredentials: credentials
                    );

                    return new JwtSecurityTokenHandler().WriteToken(token);
                }

                ErrorDeValidacao ValidaAdministradorDTO(AdministradorDTO dto)
                {
                    var validacao = new ErrorDeValidacao { Mensagens = new List<string>() };
                    if (string.IsNullOrEmpty(dto.Email)) validacao.Mensagens.Add("Email obrigatório");
                    if (string.IsNullOrEmpty(dto.Senha)) validacao.Mensagens.Add("Senha obrigatória");
                    if (dto.Perfil == null) validacao.Mensagens.Add("Perfil obrigatório");
                    return validacao;
                }

                ErrorDeValidacao ValidaVeiculoDTO(VeiculoDTO dto)
                {
                    var validacao = new ErrorDeValidacao { Mensagens = new List<string>() };
                    if (string.IsNullOrEmpty(dto.Nome)) validacao.Mensagens.Add("Nome obrigatório");
                    if (string.IsNullOrEmpty(dto.Marca)) validacao.Mensagens.Add("Marca obrigatória");
                    if (dto.Ano < 1950) validacao.Mensagens.Add("Ano deve ser >= 1950");
                    return validacao;
                }

                
                endpoints.MapPost("/administradores/login", ([FromBody] LoginDTO dto, IAdministradorServico svc) =>
                {
                    var adm = svc.Login(dto);
                    if (adm == null) return Results.Unauthorized();

                    var token = GerarTokenJwt(adm);
                    return Results.Ok(new AdmLogado { Email = adm.Email, Perfil = adm.Perfil, Token = token });
                }).AllowAnonymous();

                endpoints.MapPost("/administradores", ([FromBody] AdministradorDTO dto, IAdministradorServico svc) =>
                {
                    var validacao = ValidaAdministradorDTO(dto);
                    if (validacao.Mensagens.Count > 0) return Results.BadRequest(validacao);

                    var adm = new Administrador { Email = dto.Email, Senha = dto.Senha, Perfil = dto.Perfil?.ToString() ?? Perfil.Editor.ToString() };
                    svc.Incluir(adm);
                    return Results.Created($"/administradores/{adm.Id}", adm);
                }).RequireAuthorization(new AuthorizeAttribute { Roles = "Adm" });

                endpoints.MapGet("/administradores", (IAdministradorServico svc) =>
                {
                    var adms = svc.Todos(null)
                        .Select(a => new AdministradorModelView { Id = a.Id, Email = a.Email, Perfil = a.Perfil })
                        .ToList();
                    return Results.Ok(adms);
                }).RequireAuthorization(new AuthorizeAttribute { Roles = "Adm" });

               
                endpoints.MapPost("/veiculos", ([FromBody] VeiculoDTO dto, IVeiculoServico svc) =>
                {
                    var validacao = ValidaVeiculoDTO(dto);
                    if (validacao.Mensagens.Count > 0) return Results.BadRequest(validacao);

                    var veiculo = new Veiculo { Nome = dto.Nome, Marca = dto.Marca, Ano = dto.Ano };
                    svc.Incluir(veiculo);
                    return Results.Created($"/veiculos/{veiculo.Id}", veiculo);
                }).RequireAuthorization(new AuthorizeAttribute { Roles = "Adm,Editor" });

                endpoints.MapGet("/veiculos", (IVeiculoServico svc) => Results.Ok(svc.Todos(null)))
                         .RequireAuthorization(new AuthorizeAttribute { Roles = "Adm,Editor" });

                endpoints.MapGet("/veiculos/{id}", (int id, IVeiculoServico svc) =>
                {
                    var veiculo = svc.BucaPorId(id);
                    return veiculo == null ? Results.NotFound() : Results.Ok(veiculo);
                }).RequireAuthorization(new AuthorizeAttribute { Roles = "Adm,Editor" });
            });
        }
    }
}

using minimal_api.Infratrutura.Db;
using minimal_api.Dominio.DTOs;
using Microsoft.EntityFrameworkCore;
using minimal_api.Infratrutura.interfaces;
using minimal_api.Dominio.Servico;
using Microsoft.AspNetCore.Mvc;
using minimal_api.Dominio.ModelViews;
using minimal_api.Dominio.interfaces;
using minimal_api.Dominio.Entidade;
using minimal_api.Dominio.Enuns;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.ComponentModel.DataAnnotations;
using System.Security.Cryptography;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Models;
using Microsoft.AspNetCore.Authorization;

#region Builder
var builder = WebApplication.CreateBuilder(args);

var key = builder.Configuration["Jwt:key"];

if (string.IsNullOrEmpty(key))
{
    var keyBytes = RandomNumberGenerator.GetBytes(32);
    key = Convert.ToBase64String(keyBytes);
    Console.ForegroundColor = ConsoleColor.Yellow;
    Console.WriteLine($"[INFO] JWT Key gerada: {key}");
    Console.ResetColor();
}

var issuer = builder.Configuration["Jwt:Issuer"] ?? "meu-sistema-api";
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme= JwtBearerDefaults.AuthenticationScheme;
}).AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateLifetime = true,
        ValidateAudience = false,
        ValidateIssuer = true,
        ValidIssuer = issuer,
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key!))

    };
});

builder.Services.AddScoped<IAdministradorServico, AdministradorServico>();
builder.Services.AddScoped<IVeiculoServico, VeiculoServico>();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(Options =>
{
    Options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "API de Gestão de Administradores e Veículos",
        Version = "v1"
    });


    Options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Informe o token JWT: <token>."

    });

    Options.AddSecurityRequirement(new OpenApiSecurityRequirement
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



builder.Services.AddDbContext<DbContexto>(options =>
    options.UseMySql(builder.Configuration.GetConnectionString("mysql"),
    ServerVersion.AutoDetect(builder.Configuration.GetConnectionString("mysql")))
);

builder.Services.AddDbContext<DbContexto>(options =>
{
    var connectionString = builder.Configuration.GetConnectionString("mysql");
    options.UseMySql(connectionString, new MySqlServerVersion(new Version(8, 0, 36)));
});

builder.Services.Configure<Microsoft.AspNetCore.Http.Json.JsonOptions>(options =>
{
    options.SerializerOptions.Converters.Add(new JsonStringEnumConverter());
});

builder.Services.AddAuthorization();

var app = builder.Build();
#endregion

#region Home
app.MapGet("/", () => Results.Json(new Home())).AllowAnonymous().WithTags("Home");
#endregion

#region Administradores

string GerarTokenJwt(Administrador administrador, string key, string issuer)
{
    if (string.IsNullOrEmpty(key))
        return string.Empty;

    var claims = new List<Claim>
    {
        new Claim("Email", administrador.Email),
        new Claim("Perfil", administrador.Perfil.ToString()),
        new Claim(ClaimTypes.Role, administrador.Perfil)
    };

    var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key));
    var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

    var token = new JwtSecurityToken(
        issuer: issuer,                       
        audience: issuer,                     
        claims: claims,
        expires: DateTime.UtcNow.AddDays(1),
        signingCredentials: credentials
    );

    return new JwtSecurityTokenHandler().WriteToken(token);
}



ErrorDeValidacao ValidaAdministradorDTO(AdministradorDTO administradorDTO)
{
    var validacao = new ErrorDeValidacao { Mensagens = new List<string>() };

    if (string.IsNullOrEmpty(administradorDTO.Email))
        validacao.Mensagens.Add("O campo 'Email' é obrigatório.");

    if (string.IsNullOrEmpty(administradorDTO.Senha))
        validacao.Mensagens.Add("O campo 'Senha' é obrigatório.");

    if (administradorDTO.Perfil == null)
        validacao.Mensagens.Add("O campo 'Perfil' é obrigatório.");

    return validacao;
}


app.MapPost("/administradores/login", ([FromBody] LoginDTO loginDTO, IAdministradorServico AdministradorServico) =>
{
     var adm = AdministradorServico.Login(loginDTO); 

    if (adm != null)
    {
        string token = GerarTokenJwt(adm, key!, issuer); // 👈 passe o issuer
        return Results.Ok(new AdmLogado
        {
            Email = adm.Email,
            Perfil = adm.Perfil,
            Message = "Login realizado com sucesso.",
            Token = token
        });
    }
    else
    {
        return Results.Json(
            new { message = "Usuário ou senha inválidos." },
            statusCode: StatusCodes.Status401Unauthorized
        );
    }
}).AllowAnonymous().WithTags("Administradores");

app.MapPost("/administradores", ([FromBody] AdministradorDTO administradorDTO, IAdministradorServico administradorServico) =>
{
    var validacao = ValidaAdministradorDTO(administradorDTO);

    if (validacao.Mensagens.Count > 0)
        return Results.BadRequest(validacao);

    var administrador = new Administrador
    {
        Email = administradorDTO.Email,
        Senha = administradorDTO.Senha,
        Perfil = administradorDTO.Perfil?.ToString() ?? Perfil.Editor.ToString()
    };

    administradorServico.Incluir(administrador);

    return Results.Created($"/administradores/{administrador.Id}", new AdministradorModelView
    {
            Email = administrador.Email,
            Id = administrador.Id,
            Perfil = administrador.Perfil?.ToString() ?? Perfil.Editor.ToString()

        });
}).RequireAuthorization()
.RequireAuthorization(new AuthorizeAttribute { Roles = "Adm" })
.WithTags("Administradores");


app.MapGet("/administradores", ([FromQuery] int? pagina, IAdministradorServico administradorServico) =>
{

    var adms = new List<AdministradorModelView>();
    var administrador = administradorServico.Todos(pagina);

    foreach (var adm in administrador)
    {
        adms.Add(new AdministradorModelView
        {
            Email = adm.Email,
            Id = adm.Id,
            Perfil = adm.Perfil?.ToString() ?? Perfil.Editor.ToString()

        });
    }

    return Results.Ok(adms);
}).RequireAuthorization()
.RequireAuthorization(new AuthorizeAttribute { Roles = "Adm" })
.WithTags("Administradores");


app.MapGet("/administradores/{id}", ([FromRoute] int id, IAdministradorServico administradorServico) =>
{

    var admin = administradorServico.BucaPorId(id);

        
        if (admin == null)
        {
            return Results.NotFound(new { mensagem = $"Administrador com ID {id} não encontrado." });
        }

        
        var adminModel = new AdministradorModelView
        {
            Id = admin.Id,
            Email = admin.Email,
            Perfil = admin.Perfil?.ToString() ?? Perfil.Editor.ToString()
        };

        return Results.Ok(adminModel);

    }).RequireAuthorization()
    .RequireAuthorization(new AuthorizeAttribute { Roles = "Adm" })
    .WithTags("Administradores");

#endregion

#region  Veiculos

ErrorDeValidacao validaDTO(VeiculoDTO veiculoDTO)
{
    var validacao = new ErrorDeValidacao
    { Mensagens = new List<string>() 
    };
        

    if (string.IsNullOrEmpty(veiculoDTO.Nome))
    {
        validacao.Mensagens.Add("O campo 'Nome' é obrigatório.");
    }

    if (string.IsNullOrEmpty(veiculoDTO.Marca))
    {
        validacao.Mensagens.Add("O campo 'Marca' é obrigatório.");
    }

    if (veiculoDTO.Ano == 0)
    {
        validacao.Mensagens.Add("O campo 'Ano' é obrigatório.");
    }

    if (veiculoDTO.Ano < 1950)
    {
        validacao.Mensagens.Add("O campo 'Ano' deve ser maior ou igual a 1950.");
    }
    return validacao;
}

app.MapPost("/veiculos", ([FromBody] VeiculoDTO veiculoDTO, IVeiculoServico veiculoServico) =>
{
    var validacao = validaDTO(veiculoDTO);
    
    if (validacao.Mensagens.Count > 0)
    {
        return Results.BadRequest(validacao);
    }

    var veiculo = new Veiculo
    {
        Nome = veiculoDTO.Nome,
        Marca = veiculoDTO.Marca,
        Ano = veiculoDTO.Ano
    };
    veiculoServico.Incluir(veiculo);

    return Results.Created($"/veiculo/{veiculo.Id}", veiculo);
}).RequireAuthorization()
.RequireAuthorization(new AuthorizeAttribute { Roles = "Adm, Editor" })
.WithTags("Veiculos"); ;


app.MapGet("/veiculos", ([FromQuery] int? pagina, IVeiculoServico veiculoServico) =>
{
    var veiculos = veiculoServico.Todos(pagina);

    return Results.Ok(veiculos);
}).RequireAuthorization()
.RequireAuthorization(new AuthorizeAttribute { Roles = "Adm, Editor" })
.WithTags("Veiculos");


app.MapGet("/veiculos/{id}", ([FromRoute] int id, IVeiculoServico veiculoServico) =>
{
    var veiculo = veiculoServico.BucaPorId(id);

    if (veiculo == null)
    {
        return Results.NotFound(new { mensagem = $"Veículo com ID {id} não encontrado." });
    }

    return Results.Ok(veiculo);
}).RequireAuthorization()
.RequireAuthorization(new AuthorizeAttribute { Roles = "Adm, Editor" })
.WithTags("Veiculos");

app.MapPut("/veiculos/{id}", ([FromRoute] int id, VeiculoDTO veiculoDTO, IVeiculoServico veiculoServico) =>
{

    var veiculo = veiculoServico.BucaPorId(id);

    if (veiculo == null)
    {
        return Results.NotFound(new { mensagem = $"Nenhum veículo encontrado com o ID {id}." });
    }

    var validacao = validaDTO(veiculoDTO);

    if (validacao.Mensagens.Count > 0)
    {
        return Results.BadRequest(validacao);
    }

    veiculo.Nome = veiculoDTO.Nome;
    veiculo.Marca = veiculoDTO.Marca;
    veiculo.Ano = veiculoDTO.Ano;

    veiculoServico.Atualizar(veiculo);
    return Results.Ok(new { mensagem = $"Veículo atualizado com sucesso." });

}).RequireAuthorization()
.RequireAuthorization(new AuthorizeAttribute { Roles = "Adm"})
.WithTags("Veiculos");

app.MapDelete("/veiculos/{id}", ([FromRoute] int id, IVeiculoServico veiculoServico) =>
{
    var veiculo = veiculoServico.BucaPorId(id);

    if (veiculo == null)
    {
        return Results.NotFound(new { mensagem = $"Nenhum veículo encontrado com o ID {id}." });
    }


    veiculoServico.Apagar(veiculo);
    return Results.Ok(new { mensagem = $"O veículo foi removido com sucesso." });

}).RequireAuthorization()
.RequireAuthorization(new AuthorizeAttribute { Roles = "Adm"})
.WithTags("Veiculos");

#endregion

#region APP
app.UseSwagger();
app.UseSwaggerUI();

app.UseAuthentication();
app.UseAuthorization();

app.Run();
#endregion

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

app.MapPost("/login", (minimal_api.Dominio.DTOs.LoginDTO loginDTO) =>
{
    if (loginDTO.Email == "administrador@contateste.com" && loginDTO.Senha == "12345@Ju")
    {
        return Results.Ok("Login realizado com sucesso.");
    }
    else
    {
        return Results.Json(
            new { message = "Usuário ou senha inválidos." },
            statusCode: StatusCodes.Status401Unauthorized
        );
    }
});

app.Run();


### Pré prepara para inicio do projeto
- Microsoft .NET: https://dotnet.microsoft.com/pt-br/download
- MySQL: https://www.oracle.com/mysql/technologies/mysql-enterprise-edition-downloads.html#windows
- VSCode
- .NET INSTALL TOOL dentro do vscode
- C# dentro do vscode
- Entity Framework:
<b> https://www.nuget.org/packages/Microsoft.EntityFrameworkCore/9.0.1
<b> https://www.nuget.org/packages/Microsoft.EntityFrameworkCore.Design/9.0.1
<b> https://www.nuget.org/packages/Microsoft.EntityFrameworkCore.Tools/9.0.1
<b> https://www.nuget.org/packages/Pomelo.EntityFrameworkCore.MySql
<b> https://www.nuget.org/packages/Swashbuckle.AspNetCore
<b> https://www.nuget.org/packages/System.Text.Json/9.0.4
<b> https://www.nuget.org/packages/Microsoft.AspNetCore.Authentication.JwtBearer/9.0.6

 
 ### Criar um projeto
 comando:<b>dotnet new web -o miminal-api

 ### Executar o projeto
 comando: 
 - dotnet watch run
 - dotnet run 


### Ignorar algumas pastas
link: https://www.toptal.com/developers/gitignore/

### String para conexão mysql
link: https://www.connectionstrings.com/mysql/

### Migrations
comando: <b>dotnet ef --version
<b>dotnet tool install --global dotnet-ef

### Criar arquivo de migração
<b>dotnet ef migrations add AdministradorMigration
<b>dotnet ef migrations add VeiculosMigration

### Para executar no banco de dados
comando: <b> dotnet ef database update
comando para acessar o BD:<b> mysql -u root -p
<b>"/c/Program Files/MySQL/MySQL Server 8.0/bin/mysql.exe" -u root -p
comando colocar nome database: USE minimal_api;
comando para visualizar a tabela: SHOW TABLES;
comando no terminal dentro do MySQL selecionar uma tabela: SELECT * FROM administradores;
comando no termonal dentro do Mysql para mostrar a estrutura: desc Veiculos;

### Para criar os dados no banco de dados (inseirir os dados no banco de dados )
comando para criar: <b> dotnet ef migrations add SeedAdministrador
comando para remover: <b> dotnet ef migrations remove
comando para atualizar banco de dados: <b> dotnet ef database update

### JWT
https://www.jwt.io/











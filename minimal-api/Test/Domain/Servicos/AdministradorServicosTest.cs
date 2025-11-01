using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using minimal_api.Dominio.Entidade;
using minimal_api.Dominio.Servico;
using minimal_api.Infratrutura.Db;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;
using System.Linq;
using System.Reflection;

namespace Test.Domain.Servicos
{
    [TestClass]
    public sealed class AdministradorServicosTest
    {

        private DbContexto CriarContextoTeste()
        {
            var assemblyPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)!;
            var path = Path.GetFullPath(Path.Combine(assemblyPath ?? "", "..", "..", ".."));
           
            var builder = new ConfigurationBuilder()
                .SetBasePath(path)
                .AddJsonFile("appsettingsDevelopment.json", optional: false, reloadOnChange: true);

            var configuration = builder.Build();

            var connectionString = configuration.GetConnectionString("mysql");

            var options = new DbContextOptionsBuilder<DbContexto>() 
                .UseMySql(connectionString, ServerVersion.AutoDetect(connectionString))
                .Options;

            return new DbContexto(options);
        }

        public DbSet<Administrador> Administradores { get; set; } = default!;
        public DbSet<Veiculo> Veiculos{ get; set; } = default!;

        [TestMethod]
        public void TestSalvarAdministrador()
        {
            // Arrange 
            using var context = CriarContextoTeste();
            var servico = new AdministradorServico(context);
            context.Database.ExecuteSqlRaw("TRUNCATE TABLE Administradores");

            var adm = new Administrador();
            adm.Id = 1;
            adm.Email = "teste@contatest.com";
            adm.Senha = "12345@test";
            adm.Perfil = "Adm";
            
            var administradorServico = new AdministradorServico(context);

            // Act 
            administradorServico.Incluir(adm);
            context.SaveChanges();
           var admBD = administradorServico.BucaPorId(adm.Id);
            

            // Assert 
            Assert.AreEqual(1, administradorServico.Todos(1).Count());
            Assert.AreEqual(1, admBD!.Id);
            Assert.AreEqual("teste@contatest.com", admBD.Email);
            Assert.AreEqual("12345@test", admBD.Senha);
            Assert.AreEqual("Adm", admBD.Perfil);
           
        }
    }
}

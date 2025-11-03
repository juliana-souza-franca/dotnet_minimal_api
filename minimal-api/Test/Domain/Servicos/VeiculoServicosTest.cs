using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using minimal_api.Dominio.Entidade;
using minimal_api.Dominio.Servico;
using minimal_api.Infratrutura.Db;
using MySqlConnector;
using System;
using System.IO;
using System.Linq;
using System.Reflection;

namespace Test.Domain.Servicos
{
    [TestClass]
    public sealed class VeiculoServicosTest
    {
        private DbContexto _context = null!;
        private VeiculoServico _servico = null!;
        private string _nomeBancoTemporario = null!;

        // Retorna a string de conexão sem o database, mantendo user e password
        private string ObterConnectionStringBase()
        {
            var assemblyPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)!;
            var path = Path.GetFullPath(Path.Combine(assemblyPath, "..", "..", ".."));

            var builder = new ConfigurationBuilder()
                .SetBasePath(path)
                .AddJsonFile("appsettingsDevelopment.json", optional: false, reloadOnChange: true);

            var configuration = builder.Build();
            var connectionString = configuration.GetConnectionString("mysql");

            if (string.IsNullOrWhiteSpace(connectionString))
                throw new InvalidOperationException("String de conexão 'mysql' não encontrada");

            // Usa MySqlConnectionStringBuilder para preservar usuário e senha
            var builderConn = new MySqlConnectionStringBuilder(connectionString);
            builderConn.Database = ""; // remove database, mantém Server, User e Password
            return builderConn.ConnectionString;
        }

        private DbContexto CriarContextoTemporario()
        {
            _nomeBancoTemporario = "teste_" + Guid.NewGuid().ToString("N");
            var connBase = ObterConnectionStringBase();
            var connCriar = $"{connBase}Database={_nomeBancoTemporario};";

            var options = new DbContextOptionsBuilder<DbContexto>()
                .UseMySql(connCriar, ServerVersion.AutoDetect(connCriar))
                .Options;

            // Cria o banco temporário
            using (var tmpContext = new DbContexto(options))
            {
                tmpContext.Database.EnsureCreated();
            }

            return new DbContexto(options);
        }

        [TestInitialize]
        public void Init()
        {
            _context = CriarContextoTemporario();
            _servico = new VeiculoServico(_context);

            // Garante que o banco está vazio
            Assert.AreEqual(0, _context.Veiculos.Count(), "Banco não está limpo antes do teste!");
        }

        [TestCleanup]
        public void Cleanup()
        {
            try
            {
                _context?.Dispose();
            }
            catch { }

            if (!string.IsNullOrEmpty(_nomeBancoTemporario))
            {
                try
                {
                    var connBase = ObterConnectionStringBase();
                    var conn = $"{connBase}Database=information_schema;";
                    var options = new DbContextOptionsBuilder<DbContexto>()
                        .UseMySql(conn, ServerVersion.AutoDetect(conn))
                        .Options;

                    using var cleanupContext = new DbContexto(options);
                    cleanupContext.Database.ExecuteSql($"DROP DATABASE IF EXISTS `{_nomeBancoTemporario}`;");
                }
                catch { }
                finally
                {
                    _nomeBancoTemporario = null!;
                }
            }
        }

        [TestMethod]
        public void TestSalvarVeiculo()
        {
            var veiculo = new Veiculo { Nome = "Civic", Marca = "Honda", Ano = 2025 };
            _servico.Incluir(veiculo);
            _context.SaveChanges();

            // Garante que EF Core vai buscar do banco real
            _context.ChangeTracker.Clear();

            var veiculoBD = _servico.BucaPorId(veiculo.Id);
            Assert.IsNotNull(veiculoBD);
            Assert.AreEqual("Civic", veiculoBD!.Nome);

            var todos = _servico.Todos();
            Assert.AreEqual(1, todos.Count(), "Deve haver apenas 1 veículo no banco após salvar.");
        }

        [TestMethod]
        public void TestAtualizarVeiculo()
        {
            var veiculo = new Veiculo { Nome = "Corolla", Marca = "Toyota", Ano = 2024 };
            _servico.Incluir(veiculo);
            _context.SaveChanges();

            veiculo.Nome = "Corolla Cross";
            _servico.Atualizar(veiculo);
            _context.SaveChanges();

            _context.ChangeTracker.Clear();

            var veiculoBD = _servico.BucaPorId(veiculo.Id);
            Assert.IsNotNull(veiculoBD);
            Assert.AreEqual("Corolla Cross", veiculoBD!.Nome);
        }

        [TestMethod]
        public void TestApagarVeiculo()
        {
            var veiculo = new Veiculo { Nome = "Fusion", Marca = "Ford", Ano = 2020 };
            _servico.Incluir(veiculo);
            _context.SaveChanges();

            _servico.Apagar(veiculo);
            _context.SaveChanges();

            _context.ChangeTracker.Clear();

            var veiculoBD = _servico.BucaPorId(veiculo.Id);
            Assert.IsNull(veiculoBD);

            var todos = _servico.Todos();
            Assert.AreEqual(0, todos.Count(), "Todos os veículos devem ter sido apagados.");
        }
    }
}

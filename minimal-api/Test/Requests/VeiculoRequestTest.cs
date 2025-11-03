using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using minimal_api.Dominio.Entidade;
using Test.Mock;
using System.Linq;

namespace Test.Requests
{
    [TestClass]
    public sealed class VeiculoRequestTest
    {
        private VeiculoServicoMockTest _veiculoMock = null!;

        [TestInitialize]
        public void Setup()
        {
            _veiculoMock = new VeiculoServicoMockTest();
            _veiculoMock.Setup(); 
        }

        [TestMethod]
        public void TestIncluirVeiculo()
        {
            // Arrange
            var novoVeiculo = new Veiculo { Id = 100, Nome = "Palio", Marca = "Fiat", Ano = 2025 };
            var servico = _veiculoMock.VeiculoServicoMock.Object;

            // Act
            servico.Incluir(novoVeiculo);

            // Assert
            Assert.IsNotNull(servico.BucaPorId(100));
            Assert.AreEqual("Palio", servico.BucaPorId(100)!.Nome);
            Assert.AreEqual("Fiat", servico.BucaPorId(100)!.Marca);
            Assert.AreEqual(2025, servico.BucaPorId(100)!.Ano);

            _veiculoMock.VeiculoServicoMock.Verify(s => s.Incluir(It.Is<Veiculo>(v => v.Id == 100)), Times.Once);
        }

        [TestMethod]
        public void TestApagarVeiculo()
        {
            // Arrange
            var veiculo = new Veiculo { Id = 101, Nome = "Corsa", Marca = "Chevrolet", Ano = 2023 };
            _veiculoMock.VeiculosMock.Add(veiculo);
            var servico = _veiculoMock.VeiculoServicoMock.Object;

            // Act
            servico.Apagar(veiculo);

            // Assert
            Assert.IsNull(servico.BucaPorId(101));
            _veiculoMock.VeiculoServicoMock.Verify(s => s.Apagar(It.Is<Veiculo>(v => v.Id == 101)), Times.Once);
        }

        [TestMethod]
        public void TestObterTodosVeiculos()
        {
            // Arrange
            _veiculoMock.VeiculosMock.AddRange(new[]
            {
                new Veiculo { Id = 1, Nome = "Civic", Marca = "Honda", Ano = 2025 },
                new Veiculo { Id = 2, Nome = "Corolla", Marca = "Toyota", Ano = 2024 }
            });
            var servico = _veiculoMock.VeiculoServicoMock.Object;

            // Act
            var resultado = servico.Todos();

            // Assert
            Assert.IsNotNull(resultado);
            Assert.AreEqual(2, resultado.Count());
            Assert.AreEqual("Civic", resultado.ElementAt(0).Nome);
            Assert.AreEqual("Corolla", resultado.ElementAt(1).Nome);
        }

        [TestMethod]
        public void TestBuscarVeiculoPorId()
        {
            // Arrange
            var veiculo = new Veiculo { Id = 5, Nome = "HB20", Marca = "Hyundai", Ano = 2022 };
            _veiculoMock.VeiculosMock.Add(veiculo);
            var servico = _veiculoMock.VeiculoServicoMock.Object;

            // Act
            var resultado = servico.BucaPorId(5);

            // Assert
            Assert.IsNotNull(resultado);
            Assert.AreEqual("HB20", resultado!.Nome);
            Assert.AreEqual("Hyundai", resultado.Marca);
            Assert.AreEqual(2022, resultado.Ano);
        }
    }
}

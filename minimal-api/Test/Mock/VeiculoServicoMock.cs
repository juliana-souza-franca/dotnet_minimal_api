using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using minimal_api.Dominio.Entidade;
using minimal_api.Dominio.interfaces;
using System.Collections.Generic;
using System.Linq;

namespace Test.Mock
{
    [TestClass]
    public class VeiculoServicoMockTest
    {
        
        public Mock<IVeiculoServico> VeiculoServicoMock { get; private set; } = null!;
        public List<Veiculo> VeiculosMock { get; private set; } = null!;
        
        [TestInitialize]
        public void Setup()
        {
            VeiculosMock = new List<Veiculo>();
            VeiculoServicoMock = new Mock<IVeiculoServico>();

            // Configura métodos mockados
            VeiculoServicoMock
                .Setup(s => s.Todos(It.IsAny<int?>(), It.IsAny<string?>(), It.IsAny<string?>()))
                .Returns(() => VeiculosMock);

            VeiculoServicoMock
                .Setup(s => s.BucaPorId(It.IsAny<int>()))
                .Returns((int id) => VeiculosMock.FirstOrDefault(v => v.Id == id));

            VeiculoServicoMock
                .Setup(s => s.Incluir(It.IsAny<Veiculo>()))
                .Callback<Veiculo>(v => VeiculosMock.Add(v));

            VeiculoServicoMock
                .Setup(s => s.Apagar(It.IsAny<Veiculo>()))
                .Callback<Veiculo>(v => VeiculosMock.RemoveAll(x => x.Id == v.Id));
        }

        [TestMethod]
        public void DeveRetornarVeiculoMockadoPorId()
        {
            var veiculo = new Veiculo { Id = 1, Nome = "Civic", Marca = "Honda", Ano = 2025 };
            VeiculosMock.Add(veiculo);

            var servico = VeiculoServicoMock.Object;
            var resultado = servico.BucaPorId(1);

            Assert.IsNotNull(resultado);
            Assert.AreEqual("Civic", resultado!.Nome);
            Assert.AreEqual("Honda", resultado.Marca);
            Assert.AreEqual(2025, resultado.Ano);
        }

        [TestMethod]
        public void DeveRetornarListaDeVeiculosMockada()
        {
            VeiculosMock.AddRange(new List<Veiculo>
            {
                new Veiculo { Id = 1, Nome = "Civic", Marca = "Honda", Ano = 2025 },
                new Veiculo { Id = 2, Nome = "Corolla", Marca = "Toyota", Ano = 2024 }
            });

            var servico = VeiculoServicoMock.Object;
            var resultado = servico.Todos();

            Assert.IsNotNull(resultado);
            Assert.AreEqual(2, resultado.Count());
            Assert.AreEqual("Corolla", resultado[1].Nome);
        }

        [TestMethod]
        public void DeveChamarMetodoIncluir()
        {
            var novoVeiculo = new Veiculo { Id = 3, Nome = "Fusion", Marca = "Ford", Ano = 2023 };
            var servico = VeiculoServicoMock.Object;

            servico.Incluir(novoVeiculo);

            VeiculoServicoMock.Verify(s => s.Incluir(It.Is<Veiculo>(v => v.Id == 3)), Times.Once);
            Assert.AreEqual(1, VeiculosMock.Count());
        }

        [TestMethod]
        public void DeveChamarMetodoApagar()
        {
            var veiculo = new Veiculo { Id = 10, Nome = "HB20", Marca = "Hyundai", Ano = 2022 };
            VeiculosMock.Add(veiculo);

            var servico = VeiculoServicoMock.Object;
            servico.Apagar(veiculo);

            VeiculoServicoMock.Verify(s => s.Apagar(It.Is<Veiculo>(v => v.Id == 10)), Times.Once);
            Assert.AreEqual(0, VeiculosMock.Count());
        }
    }
}

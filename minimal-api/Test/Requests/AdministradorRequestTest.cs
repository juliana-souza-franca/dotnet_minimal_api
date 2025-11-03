using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using minimal_api.Dominio.Entidade;
using Test.Helpers;

namespace Test.Requests
{
    [TestClass]
    public sealed class AdministradorRequestTest
    {
        [TestInitialize]
        public void SetupAmbiente()
        {
            Setup.Initialize();
            Setup.LimparMocks();
        }

        [TestMethod]
        public void TestIncluirAdministrador()
        {
            var novoAdministrador = new Administrador
            {
                Id = 2,
                Email = "novo@mock.com",
                Senha = "abc123",
                Perfil = "Editor"
            };

            Setup.AdministradorServicoMock
                .Setup(s => s.Incluir(It.IsAny<Administrador>()))
                .Returns((Administrador adm) => adm);

            var servico = Setup.AdministradorServicoMock.Object;
            var resultado = servico.Incluir(novoAdministrador);

            Assert.IsNotNull(resultado);
            Assert.AreEqual("novo@mock.com", resultado.Email);
            Assert.AreEqual("Editor", resultado.Perfil);

            Setup.AdministradorServicoMock.Verify(s => s.Incluir(It.IsAny<Administrador>()), Times.Once);
        }

        [TestMethod]
        public void TestBuscarAdministradorPorId()
        {
            var admEsperado = new Administrador
            {
                Id = 1,
                Email = "teste@mock.com",
                Senha = "123",
                Perfil = "Adm"
            };

            Setup.AdministradorServicoMock
                .Setup(s => s.BucaPorId(1))
                .Returns(admEsperado);

            var servico = Setup.AdministradorServicoMock.Object;
            var resultado = servico.BucaPorId(1);

            Assert.IsNotNull(resultado);
            Assert.AreEqual("teste@mock.com", resultado.Email);
            Assert.AreEqual("Adm", resultado.Perfil);
        }
    }
}

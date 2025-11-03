using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using minimal_api.Dominio.Entidade;
using minimal_api.Dominio.interfaces;
using minimal_api.Infratrutura.interfaces;

namespace Test.Mock
{
    [TestClass]
    public class AdministradorServicoMockTest
    {
        private Mock<IAdministradorServico> _mockServico = null!;

        [TestInitialize]
        public void SetupAmbiente()
        {
            _mockServico = new Mock<IAdministradorServico>();
        }

        [TestMethod]
        public void DeveRetornarAdministradorMockado()
        {
            var admEsperado = new Administrador
            {
                Id = 1,
                Email = "teste@mock.com",
                Senha = "123",
                Perfil = "Adm"
            };

            _mockServico.Setup(s => s.BucaPorId(1)).Returns(admEsperado);

            var resultado = _mockServico.Object.BucaPorId(1);

            Assert.IsNotNull(resultado);
            Assert.AreEqual("teste@mock.com", resultado.Email);
            Assert.AreEqual("Adm", resultado.Perfil);
        }

        [TestMethod]
        public void DeveIncluirAdministradorMockado()
        {
            var novoAdm = new Administrador
            {
                Id = 2,
                Email = "novo@mock.com",
                Senha = "abc123",
                Perfil = "Editor"
            };

            _mockServico.Setup(s => s.Incluir(It.IsAny<Administrador>())).Returns((Administrador adm) => adm);

            var resultado = _mockServico.Object.Incluir(novoAdm);

            Assert.IsNotNull(resultado);
            Assert.AreEqual("novo@mock.com", resultado.Email);
            Assert.AreEqual("Editor", resultado.Perfil);

            _mockServico.Verify(s => s.Incluir(It.IsAny<Administrador>()), Times.Once);
        }
    }
}

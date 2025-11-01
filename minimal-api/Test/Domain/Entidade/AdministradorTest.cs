using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using minimal_api.Dominio.Entidade;

namespace Test.Domain.Entidade
{
     [TestClass]
public sealed class AdministradorTest
{
    [TestMethod]
    public void TestGetSettingPropriedades()
        {
            // Arrange
            var adm = new Administrador();

            // Act
            adm.Id = 1;
            adm.Email = "teste@contatest.com";
            adm.Senha = "12345@test";
            adm.Perfil = "Adm";

            //Assert
            Assert.AreEqual(1, adm.Id);
            Assert.AreEqual("teste@contatest.com", adm.Email);
            Assert.AreEqual("12345@test", adm.Senha);
            Assert.AreEqual("Adm", adm.Perfil);

            
        }
}
}
 


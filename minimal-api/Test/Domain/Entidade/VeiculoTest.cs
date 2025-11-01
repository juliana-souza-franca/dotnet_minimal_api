using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using minimal_api.Dominio.Entidade;

namespace Test.Domain.Entidade
{
    [TestClass]
    public sealed class VeiculoTest
    {
        [TestMethod]
        public void TestGetSettingPropriedades()
        {
            // Arrange
            var v = new Veiculo();

            // Act
            v.Id = 1;
            v.Nome = "Civic";
            v.Marca = "Honda";
            v.Ano = 2025;

            //Assert
            Assert.AreEqual(1, v.Id);
            Assert.AreEqual("Civic", v.Nome);
            Assert.AreEqual("Honda", v.Marca);
            Assert.AreEqual(2025, v.Ano);


        }
    }
}
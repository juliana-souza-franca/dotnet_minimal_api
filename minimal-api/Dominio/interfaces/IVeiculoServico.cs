using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using minimal_api.Dominio.Entidade;

namespace minimal_api.Dominio.interfaces
{
    public interface IVeiculoServico
    {
        List<Veiculo> Todos(int? pagina = 1, string? nome = null, string? marca = null);

        Veiculo? BucaPorId(int id);
        void Incluir(Veiculo veiculo);
        void Atualizar(Veiculo veiculo);
       void Apagar(Veiculo veiculo);
    }
}
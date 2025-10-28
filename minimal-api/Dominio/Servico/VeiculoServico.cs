using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using minimal_api.Dominio.Entidade;
using minimal_api.Dominio.interfaces;
using minimal_api.Infratrutura.Db;

namespace minimal_api.Dominio.Servico
{
    public class VeiculoServico : IVeiculoServico
    {
         private readonly DbContexto _contexto;
        public VeiculoServico(DbContexto contexto)
        {
                _contexto = contexto;
        }
        public void Apagar(Veiculo veiculo)
        {
            _contexto.Veiculos.Remove(veiculo);
            _contexto.SaveChanges();
           
        }

        public void Atualizar(Veiculo veiculo)
        {
            _contexto.Veiculos.Update(veiculo);
             _contexto.SaveChanges();
           
        }

        public Veiculo? BucaPorId(int id)
        {
            return _contexto.Veiculos.Where(v => v.Id == id).FirstOrDefault();
        }

        public void Incluir(Veiculo veiculo)
        {
            _contexto.Veiculos.Add(veiculo);
            _contexto.SaveChanges();
        }

       public List<Veiculo> Todos(int? pagina = 1, string? nome = null, string? marca = null)
        {
        IQueryable<Veiculo> vehicle = _contexto.Veiculos;

            if (!string.IsNullOrEmpty(nome))
            {
                vehicle = vehicle.Where(v => v.Nome.ToLower().Contains(nome.ToLower()));
            }

            if (!string.IsNullOrEmpty(marca))
            {
                vehicle = vehicle.Where(v => v.Marca.ToLower().Contains(marca.ToLower()));
            }

            int tamanhoPagina = 10;
            int paginaAtual = pagina ?? 1; 
            vehicle = vehicle.Skip((paginaAtual - 1) * tamanhoPagina).Take(tamanhoPagina);

            return vehicle.ToList();
        }
    }
}
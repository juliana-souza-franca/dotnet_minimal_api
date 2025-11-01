using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using minimal_api.Dominio.DTOs;
using minimal_api.Dominio.Entidade;
using minimal_api.Infratrutura.Db;
using minimal_api.Infratrutura.interfaces;

namespace minimal_api.Dominio.Servico
{
    public class AdministradorServico : IAdministradorServico
    {
        private readonly DbContexto _contexto;
       public AdministradorServico(DbContexto contexto)
       {
            _contexto = contexto;
       }

        public Administrador? BucaPorId(int id)
        {
            return _contexto.Administradores.Where(v => v.Id == id).FirstOrDefault();
        }

        public Administrador Incluir(Administrador administrador)
        {
            _contexto.Administradores.Add(administrador);
            _contexto.SaveChanges();
            return administrador; 
        }

        public Administrador? Login(LoginDTO loginDTO)
        {
            var adm = _contexto.Administradores.Where(a => a.Email == loginDTO.Email && a.Senha == loginDTO.Senha).FirstOrDefault();
             return adm;
            
        }

        public List<Administrador> Todos(int? pagina)
        {
            IQueryable<Administrador> adm = _contexto.Administradores;


            int tamanhoPagina = 10;
            int paginaAtual = pagina ?? 1; 
            adm = adm.Skip((paginaAtual - 1) * tamanhoPagina).Take(tamanhoPagina);

            return adm.ToList();


        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using minimal_api.Dominio.DTOs;
using minimal_api.Dominio.Entidade;


namespace minimal_api.Infratrutura.interfaces
{
    public interface IAdministradorServico
  {
      Administrador? BucaPorId(int id);
      Administrador? Login(LoginDTO loginDTO);
      Administrador Incluir(Administrador administrador);
      List <Administrador> Todos(int? pagina);
    }
}
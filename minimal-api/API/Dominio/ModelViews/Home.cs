using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace minimal_api.Dominio.ModelViews
{
    public struct Home
    {
        public string Message { get => "Seja bem-vindo à API de Veículos - Minimal API!"; }
        public string Doc { get => "/swagger"; }
        
    }
}
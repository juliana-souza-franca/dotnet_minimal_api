using System.Net.Http;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using minimal_api;
using minimal_api.Dominio.interfaces;
using minimal_api.Infratrutura.interfaces;

namespace Test.Helpers
{
    public static class Setup
    {
        // WebApplicationFactory e Client
        public static WebApplicationFactory<Startup> AppFactory { get; private set; } = null!;
        public static HttpClient Client { get; private set; } = null!;

       
        public static Mock<IAdministradorServico> AdministradorServicoMock { get; private set; } = null!;
        public static Mock<IVeiculoServico> VeiculoServicoMock { get; private set; } = null!;

        
        public static void Initialize()
        {
            
            AdministradorServicoMock = new Mock<IAdministradorServico>();
            VeiculoServicoMock = new Mock<IVeiculoServico>();

           
            AppFactory = new WebApplicationFactory<Startup>()
                .WithWebHostBuilder(builder =>
                {
                    builder.ConfigureServices(services =>
                    {
                      
                        services.AddSingleton(AdministradorServicoMock.Object);
                        services.AddSingleton(VeiculoServicoMock.Object);
                    });
                });

            Client = AppFactory.CreateClient();
        }

        
        public static void LimparMocks()
        {
            AdministradorServicoMock?.Invocations.Clear();
            VeiculoServicoMock?.Invocations.Clear();
        }
    }
}

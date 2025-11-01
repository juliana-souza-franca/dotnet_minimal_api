using System;
using Microsoft.EntityFrameworkCore;
using minimal_api.Dominio.Entidade;

namespace minimal_api.Infratrutura.Db
{

    public class DbContexto : DbContext
    {
        public DbSet<Administrador> Administradores { get; set; } = default!;
        public DbSet<Veiculo> Veiculos { get; set; } = default!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Administrador>().HasData(
                new Administrador
                { 
                    Id= 1,
                    Email = "administrador@contateste.com",
                    Senha = "12345@Ju",
                    Perfil= "Adm"
                }
            );
        }
        public DbContexto(DbContextOptions<DbContexto> options) : base(options)
        {
        }

    }

    public class DbContextoFactory : Microsoft.EntityFrameworkCore.Design.IDesignTimeDbContextFactory<DbContexto>
    {
        public DbContexto CreateDbContext(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<DbContexto>();
            optionsBuilder.UseMySql(
                "server=127.0.0.1;port=3306;database=minimal_api;user=root;password=230277;",
                new MySqlServerVersion(new Version(8, 0, 36))
            );

            return new DbContexto(optionsBuilder.Options);
        }
    }
}


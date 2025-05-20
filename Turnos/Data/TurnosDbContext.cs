using Microsoft.AspNetCore.DataProtection.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.Configuration;
using Turnos.Common;
using Turnos.Data.Auth;
using Turnos.Data.Configurations;
using AppRoles = Turnos.Common.Roles;

namespace Turnos.Data;
public sealed class TurnosDbContext : IdentityDbContext<User, Role, Guid>, IDataProtectionKeyContext {

    private readonly IConfiguration _configuration;

    public DbSet<DataProtectionKey> DataProtectionKeys => Set<DataProtectionKey>();

    public DbSet<Alumno> Alumnos => Set<Alumno>();
    public DbSet<Personal> Personals => Set<Personal>();

    public DbSet<Fila> Filas => Set<Fila>();
    public DbSet<Turno> Turnos => Set<Turno>();

    public TurnosDbContext(IConfiguration configuration, DbContextOptions<TurnosDbContext> options) : base(options) {
        _configuration = configuration;
    }

    protected override void OnModelCreating(ModelBuilder builder) {

        builder.HasDefaultSchema(DbConstants.Schema);

        base.OnModelCreating(builder);

        builder.ApplyConfigurationsFromAssembly(typeof(TurnosDbContext).Assembly,
            t => t.IsClass && !t.IsAbstract && t.Namespace == typeof(IDbConfiguration<>).Namespace);

    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder) {

        optionsBuilder.UseNpgsql(_configuration.GetConnectionString("turnos"),
                sql => {
                    sql.MigrationsAssembly(typeof(TurnosDbContext).Assembly)
                       .EnableRetryOnFailure(3)
                       .CommandTimeout(30);

                    sql.MapEnum<EstadoFila>("estado_fila", DbConstants.Schema);
                    sql.MapEnum<EstadoTurno>("estado_turno", DbConstants.Schema);
                })
            /*.UseSeeding(Seed)
            .UseAsyncSeeding(SeedAsync)*/;

    }

    private void Seed(DbContext context, bool operation) {

        var roleManager = context.GetService<RoleManager<Role>>();

        var role = roleManager.FindByNameAsync(AppRoles.Alumno).Result;

        if (role is not null) goto CheckPersonal;

        role = new Role();
        var result = roleManager.SetRoleNameAsync(role, AppRoles.Alumno).Result;

        if (!result.Succeeded) return;

        result = roleManager.CreateAsync(role).Result;

        if (!result.Succeeded) return;

        CheckPersonal:

        role = roleManager.FindByNameAsync(AppRoles.Personal).Result;

        if (role is not null) return;

        role = new Role();
        result = roleManager.SetRoleNameAsync(role, AppRoles.Personal).Result;

        if (!result.Succeeded) return;

        result = roleManager.CreateAsync(role).Result;

    }

    private async Task SeedAsync(DbContext context, bool operation, CancellationToken cancellationToken) {

        var roleManager = context.GetService<RoleManager<Role>>();

        var role = await roleManager.FindByNameAsync(AppRoles.Alumno);

        if (role is not null) goto CheckPersonal;

        role = new Role();
        var result = await roleManager.SetRoleNameAsync(role, AppRoles.Alumno);

        if (!result.Succeeded) return;

        result = await roleManager.CreateAsync(role);

        if (!result.Succeeded) return;

        CheckPersonal:

        role = await roleManager.FindByNameAsync(AppRoles.Personal);

        if (role is not null) return;

        role = new Role();
        result = await roleManager.SetRoleNameAsync(role, AppRoles.Personal);

        if (!result.Succeeded) return;

        result = await roleManager.CreateAsync(role);


        /*const string AdminEmail = "turnos@adsver.com.mx";
        var adminUser = users.FirstOrDefaultAsync(u => u.UserName == AdminEmail, cancellationToken);

        if (adminUser is null) {

            var userAdmin = new User() {
                UserName = AdminEmail,
                Email = AdminEmail,
                PasswordHash = "", // TODO: HashPassword with PasswordHasher
            };

        }*/

    }
}

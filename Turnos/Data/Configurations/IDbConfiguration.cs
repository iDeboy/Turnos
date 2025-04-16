using Microsoft.EntityFrameworkCore;

namespace Turnos.Data.Configurations; 
internal interface IDbConfiguration<T> : IEntityTypeConfiguration<T> where T : class;

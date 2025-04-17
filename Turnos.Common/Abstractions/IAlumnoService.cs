using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Turnos.Common.Abstractions;
public interface IAlumnoService : IAsyncDisposable {

    event Action<IReadOnlyDictionary<Guid, FilaInfo>>? FilasUpdated;

    Task StartAsync();

    Task<IReadOnlyDictionary<Guid, FilaInfo>> LoadFilasAsync(CancellationToken cancellationToken = default);

}

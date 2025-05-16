using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Turnos.Common;
public static class HubMethods {

    public static class Client {
        public const string FilaCreated = nameof(FilaCreated);
        public const string FilaChanged = nameof(FilaChanged);
        public const string FilaDeleted = nameof(FilaDeleted);
     
        public const string TurnoCreated = nameof(TurnoCreated);
        public const string TurnoChanged = nameof(TurnoChanged);
    }

    public static class Alumno {

        public const string LoadAlumnoFilas = nameof(LoadAlumnoFilas);
        public const string LoadAlumnoTurnos = nameof(LoadAlumnoTurnos);
        public const string CreateTurno = nameof(CreateTurno);
        public const string CancelTurnoAlumno = nameof(CancelTurnoAlumno);

    }

    public static class Personal {

        public const string LoadPersonalFilas = nameof(LoadPersonalFilas); 
        public const string LoadPersonalTurnos = nameof(LoadPersonalTurnos);
        public const string CreateFila = nameof(CreateFila);
        public const string DeleteFila = nameof(DeleteFila);
        public const string ChangeStateFila = nameof(ChangeStateFila);
        public const string ChangePassword = nameof(ChangePassword);
        public const string ChangeStateTurno = nameof(ChangeStateTurno);
    }

}

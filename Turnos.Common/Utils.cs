using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace Turnos.Common;
public static class Utils {

    public static TimeSpan AcumularPromedio(TimeSpan promedioActual, int n, TimeSpan tiempo) {
        return new TimeSpan(
            (promedioActual.Ticks * n + tiempo.Ticks) / (n + 1)
        );
    }

    public static bool IsValidUser(
        [NotNullWhen(true)] ClaimsPrincipal? user,
        [NotNullWhen(true)] out string? userId,
        out Guid id) {

        userId = null;
        id = Guid.Empty;

        if (user is null || user.Identity is null || !user.Identity.IsAuthenticated)
            return false;

        userId = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (!Guid.TryParse(userId, out id)) {
            return false;
        }

        return true;
    }

    public static bool IsValidAlumnoUser(
        [NotNullWhen(true)] ClaimsPrincipal? user,
        [NotNullWhen(true)] out string? userId,
        out Guid id,
        [NotNullWhen(true)] out string? email,
        [NotNullWhen(true)] out string? name) {

        email = null;
        name = null;

        if (!IsValidUser(user, out userId, out id))
            return false;

        if (!user.IsInRole(Roles.Alumno))
            return false;

        name = user.FindFirst(ClaimsType.FullName)?.Value;
        email = user.FindFirst(ClaimTypes.Email)?.Value;

        if (string.IsNullOrWhiteSpace(name) || string.IsNullOrWhiteSpace(email))
            return false;

        return true;

    }

    public static bool IsValidPersonalUser(
        [NotNullWhen(true)] ClaimsPrincipal? user,
        [NotNullWhen(true)] out string? userId,
        out Guid id,
        [NotNullWhen(true)] out string? email,
        [NotNullWhen(true)] out string? name) {

        email = null;
        name = null;

        if (!IsValidUser(user, out userId, out id))
            return false;

        if (!user.IsInRole(Roles.Personal))
            return false;

        name = user.FindFirst(ClaimsType.FullName)?.Value;
        email = user.FindFirst(ClaimTypes.Email)?.Value;

        if (string.IsNullOrWhiteSpace(name) || string.IsNullOrWhiteSpace(email))
            return false;

        return true;

    }

}

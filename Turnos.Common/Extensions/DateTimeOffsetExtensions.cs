using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Turnos.Common.Extensions; 
public static class DateTimeOffsetExtensions {

    public static DateTimeOffset ToDefaultTimeZone(this DateTimeOffset source) {
        return ToTimeZone(source, "America/Mexico_City");
    }

    public static DateTimeOffset ToLocalTimeZone(this DateTimeOffset source) {
        return ToTimeZone(source, TimeZoneInfo.Local);
    }

    public static DateTimeOffset ToTimeZone(this DateTimeOffset source, string timeZoneId) {
        return TimeZoneInfo.ConvertTimeBySystemTimeZoneId(source, timeZoneId);
    }

    public static DateTimeOffset ToTimeZone(this DateTimeOffset source, TimeZoneInfo timeZoneInfo) {
        return TimeZoneInfo.ConvertTime(source, timeZoneInfo);
    }

}

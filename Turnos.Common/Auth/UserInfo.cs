﻿namespace Turnos.Common.Auth;
public sealed class UserInfo {
    public required string UserId { get; set; }
    public required string Email { get; set; }
    public required string Name { get; set; }
    public required string Role { get; set; }

}

using Microsoft.AspNetCore.Identity;
using Turnos.Data.Auth;

namespace Turnos.EmailSenders; 
public interface IEmail<TUser> : IEmailSender<TUser> where TUser : class {

    Task SendMessage(TUser user, string subject, string message);

}

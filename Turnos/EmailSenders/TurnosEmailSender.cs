﻿using FluentEmail.Core;
using MailKit.Net.Smtp;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using MimeKit;
using MimeKit.Text;
using System.Net;
using System.Text;
using System.Text.Encodings.Web;
using Turnos.Data.Auth;
using Turnos.Options;

namespace Turnos.EmailSenders;
public sealed class TurnosEmailSender : IEmailSender<User> {
    // https://wordtohtml.net/es/email/designer
    private const string MessageLinkTemplate =
        """
        <!doctype html><html xmlns="http://www.w3.org/1999/xhtml" xmlns:v="urn:schemas-microsoft-com:vml" xmlns:o="urn:schemas-microsoft-com:office:office"><head><title></title><!--[if !mso]><!--><meta http-equiv="X-UA-Compatible" content="IE=edge"><!--<![endif]--><meta http-equiv="Content-Type" content="text/html; charset=UTF-8"><meta name="viewport" content="width=device-width,initial-scale=1"><style type="text/css">td{{mso-table-lspace:0;mso-table-rspace:0}}</style><!--[if mso]><noscript><xml><o:officedocumentsettings><o:allowpng><o:pixelsperinch>96</o:pixelsperinch></o:officedocumentsettings></xml></noscript><![endif]--><!--[if lte mso 11]><style type="text/css"></style><![endif]--><!--[if !mso]><!--><link href="https://fonts.googleapis.com/css?family=Ubuntu:400,700" rel="stylesheet" type="text/css"><style type="text/css">@import url(https://fonts.googleapis.com/css?family=Ubuntu:400,700);</style><!--<![endif]--><style type="text/css"></style><style media="screen and (min-width:480px)">.moz-text-html .mj-column-per-100{{max-width:100%}}</style><style type="text/css"></style><style type="text/css">li,ol{{font-family:Ubuntu,Helvetica,Arial}}</style></head><body style="word-spacing:normal;background-color:#fff"><div style="background-color:#fff"><!--[if mso | IE]><table align="center" border="0" cellpadding="0" cellspacing="0" class="" role="presentation" style="width:600px" width="600" bgcolor="#2a6099"><tr><td style="line-height:0;font-size:0;mso-line-height-rule:exactly"><![endif]--><div style="background:#2a6099;background-color:#2a6099;margin:0 auto;border-radius:1px 1px 1px 1px;max-width:600px"><table align="center" border="0" cellpadding="0" cellspacing="0" role="presentation" style="background:#2a6099;background-color:#2a6099;width:100%;border-radius:1px 1px 1px 1px"><tbody><tr><td style="border-bottom:1px #000 none;border-left:1px #000 none;border-right:1px #000 none;border-top:1px #000 none;direction:ltr;font-size:0;padding:10px 0 10px 0;text-align:center"><!--[if mso | IE]><table role="presentation" border="0" cellpadding="0" cellspacing="0"><tr><td class="" style="vertical-align:top;width:598px"><![endif]--><div class="mj-column-per-100 mj-outlook-group-fix" style="font-size:0;text-align:left;direction:ltr;display:inline-block;vertical-align:top;width:100%"><table border="0" cellpadding="0" cellspacing="0" role="presentation" style="vertical-align:top" width="100%"><tbody><tr><td align="left" style="font-size:0;padding:15px 15px 15px 15px;word-break:break-word"><div style="font-family:Ubuntu,Helvetica,Arial,sans-serif;font-size:13px;line-height:1.5;text-align:left;color:#000"><p style="font-family:Ubuntu,sans-serif;font-size:11px;text-align:center"><span style="font-size:20pt;color:#ecf0f1">{0}</span></p></div></td></tr></tbody></table></div><!--[if mso | IE]><![endif]--></td></tr></tbody></table></div><!--[if mso | IE]><table align="center" border="0" cellpadding="0" cellspacing="0" class="" role="presentation" style="width:600px" width="600" bgcolor="#FEFEFF"><tr><td style="line-height:0;font-size:0;mso-line-height-rule:exactly"><![endif]--><div style="background:#fefeff;background-color:#fefeff;margin:0 auto;max-width:600px"><table align="center" border="0" cellpadding="0" cellspacing="0" role="presentation" style="background:#fefeff;background-color:#fefeff;width:100%"><tbody><tr><td style="direction:ltr;font-size:0;padding:10px 0 10px 0;text-align:center"><!--[if mso | IE]><table role="presentation" border="0" cellpadding="0" cellspacing="0"><tr><td class="" style="vertical-align:top;width:600px"><![endif]--><div class="mj-column-per-100 mj-outlook-group-fix" style="font-size:0;text-align:left;direction:ltr;display:inline-block;vertical-align:top;width:100%"><table border="0" cellpadding="0" cellspacing="0" role="presentation" style="vertical-align:top" width="100%"><tbody><tr><td align="left" style="font-size:0;padding:15px 15px 15px 15px;word-break:break-word"><div style="font-family:Ubuntu,Helvetica,Arial,sans-serif;font-size:13px;line-height:1.5;text-align:left;color:#000"><p style="font-family:Ubuntu,sans-serif;font-size:11px;text-align:center"><span style="font-size:20pt">{1} <a href="{2}" target="_blank" rel="noopener" style="color:#2a6099">{3}</a>.</span></p></div></td></tr></tbody></table></div><!--[if mso | IE]><![endif]--></td></tr></tbody></table></div><!--[if mso | IE]><table align="center" border="0" cellpadding="0" cellspacing="0" class="" role="presentation" style="width:600px" width="600" bgcolor="#2a6099"><tr><td style="line-height:0;font-size:0;mso-line-height-rule:exactly"><![endif]--><div style="background:#2a6099;background-color:#2a6099;margin:0 auto;max-width:600px"><table align="center" border="0" cellpadding="0" cellspacing="0" role="presentation" style="background:#2a6099;background-color:#2a6099;width:100%"><tbody><tr><td style="direction:ltr;font-size:0;padding:10px 0 10px 0;text-align:center"><!--[if mso | IE]><table role="presentation" border="0" cellpadding="0" cellspacing="0"><tr><td class="" style="vertical-align:top;width:600px"><![endif]--><div class="mj-column-per-100 mj-outlook-group-fix" style="font-size:0;text-align:left;direction:ltr;display:inline-block;vertical-align:top;width:100%"><table border="0" cellpadding="0" cellspacing="0" role="presentation" style="vertical-align:top" width="100%"><tbody><tr><td align="left" style="font-size:0;padding:15px 15px 15px 15px;word-break:break-word"><div style="font-family:Ubuntu,Helvetica,Arial,sans-serif;font-size:13px;line-height:1.5;text-align:left;color:#000"><p style="font-family:Ubuntu,sans-serif;font-size:11px;text-align:center"><span style="font-size:20pt;color:#ecf0f1">&copy&nbsp 2025 Gestor de turnos</span></p></div></td></tr></tbody></table></div><!--[if mso | IE]><![endif]--></td></tr></tbody></table></div><!--[if mso | IE]><![endif]--></div></body></html>
        """;

    private const string MessageCodeTemplate =
        """
        <!doctype html><html xmlns="http://www.w3.org/1999/xhtml" xmlns:v="urn:schemas-microsoft-com:vml" xmlns:o="urn:schemas-microsoft-com:office:office"><head><title></title><!--[if !mso]><!--><meta http-equiv="X-UA-Compatible" content="IE=edge"><!--<![endif]--><meta http-equiv="Content-Type" content="text/html; charset=UTF-8"><meta name="viewport" content="width=device-width,initial-scale=1"><style type="text/css">#outlook a{{padding:0}}body{{margin:0;padding:0;-webkit-text-size-adjust:100%;-ms-text-size-adjust:100%}}table,td{{border-collapse:collapse;mso-table-lspace:0;mso-table-rspace:0}}img{{border:0;height:auto;line-height:100%;outline:0;text-decoration:none;-ms-interpolation-mode:bicubic}}p{{display:block;margin:13px 0}}</style><!--[if mso]><noscript><xml><o:officedocumentsettings><o:allowpng><o:pixelsperinch>96</o:pixelsperinch></o:officedocumentsettings></xml></noscript><![endif]--><!--[if lte mso 11]><style type="text/css">.mj-outlook-group-fix{{width:100%!important}}</style><![endif]--><!--[if !mso]><!--><link href="https://fonts.googleapis.com/css?family=Ubuntu:400,700" rel="stylesheet" type="text/css"><style type="text/css">@import url(https://fonts.googleapis.com/css?family=Ubuntu:400,700);</style><!--<![endif]--><style type="text/css">@media only screen and (min-width:480px){{.mj-column-per-100{{width:100%!important;max-width:100%}}}}</style><style media="screen and (min-width:480px)">.moz-text-html .mj-column-per-100{{width:100%!important;max-width:100%}}</style><style type="text/css"></style><style type="text/css">.hide_on_mobile{{display:none!important}}@media only screen and (min-width:480px){{.hide_on_mobile{{display:block!important}}}}.hide_section_on_mobile{{display:none!important}}@media only screen and (min-width:480px){{.hide_section_on_mobile{{display:table!important}}div.hide_section_on_mobile{{display:block!important}}}}.hide_on_desktop{{display:block!important}}@media only screen and (min-width:480px){{.hide_on_desktop{{display:none!important}}}}.hide_section_on_desktop{{display:table!important;width:100%}}@media only screen and (min-width:480px){{.hide_section_on_desktop{{display:none!important}}}}h1,h2,h3,p{{margin:0}}li,ol,ul{{font-size:11px;font-family:Ubuntu,Helvetica,Arial}}a{{text-decoration:none;color:inherit}}@media only screen and (max-width:480px){{.mj-column-per-100{{width:100%!important;max-width:100%!important}}.mj-column-per-100>.mj-column-per-100{{width:100%!important;max-width:100%!important}}}}</style></head><body style="word-spacing:normal;background-color:#fff"><div style="background-color:#fff"><!--[if mso | IE]><table align="center" border="0" cellpadding="0" cellspacing="0" class="" role="presentation" style="width:600px" width="600" bgcolor="#2a6099"><tr><td style="line-height:0;font-size:0;mso-line-height-rule:exactly"><![endif]--><div style="background:#2a6099;background-color:#2a6099;margin:0 auto;border-radius:1px 1px 1px 1px;max-width:600px"><table align="center" border="0" cellpadding="0" cellspacing="0" role="presentation" style="background:#2a6099;background-color:#2a6099;width:100%;border-radius:1px 1px 1px 1px"><tbody><tr><td style="border-bottom:1px #000 none;border-left:1px #000 none;border-right:1px #000 none;border-top:1px #000 none;direction:ltr;font-size:0;padding:10px 0 10px 0;text-align:center"><!--[if mso | IE]><table role="presentation" border="0" cellpadding="0" cellspacing="0"><tr><td class="" style="vertical-align:top;width:598px"><![endif]--><div class="mj-column-per-100 mj-outlook-group-fix" style="font-size:0;text-align:left;direction:ltr;display:inline-block;vertical-align:top;width:100%"><table border="0" cellpadding="0" cellspacing="0" role="presentation" style="vertical-align:top" width="100%"><tbody><tr><td align="left" style="font-size:0;padding:15px 15px 15px 15px;word-break:break-word"><div style="font-family:Ubuntu,Helvetica,Arial,sans-serif;font-size:13px;line-height:1.5;text-align:left;color:#000"><p style="font-family:Ubuntu,sans-serif;font-size:11px;text-align:center"><span style="font-size:20pt;color:#ecf0f1">{0}</span></p></div></td></tr></tbody></table></div><!--[if mso | IE]><![endif]--></td></tr></tbody></table></div><!--[if mso | IE]><table align="center" border="0" cellpadding="0" cellspacing="0" class="" role="presentation" style="width:600px" width="600" bgcolor="#FEFEFF"><tr><td style="line-height:0;font-size:0;mso-line-height-rule:exactly"><![endif]--><div style="background:#fefeff;background-color:#fefeff;margin:0 auto;max-width:600px"><table align="center" border="0" cellpadding="0" cellspacing="0" role="presentation" style="background:#fefeff;background-color:#fefeff;width:100%"><tbody><tr><td style="direction:ltr;font-size:0;padding:10px 0 10px 0;text-align:center"><!--[if mso | IE]><table role="presentation" border="0" cellpadding="0" cellspacing="0"><tr><td class="" style="vertical-align:top;width:600px"><![endif]--><div class="mj-column-per-100 mj-outlook-group-fix" style="font-size:0;text-align:left;direction:ltr;display:inline-block;vertical-align:top;width:100%"><table border="0" cellpadding="0" cellspacing="0" role="presentation" style="vertical-align:top" width="100%"><tbody><tr><td align="left" style="font-size:0;padding:15px 15px 15px 15px;word-break:break-word"><div style="font-family:Ubuntu,Helvetica,Arial,sans-serif;font-size:13px;line-height:1.5;text-align:left;color:#000"><p style="font-family:Ubuntu,sans-serif;font-size:11px;text-align:center"><span style="font-size:30pt">{1}</span></p></div></td></tr></tbody></table></div><!--[if mso | IE]><![endif]--></td></tr></tbody></table></div><!--[if mso | IE]><table align="center" border="0" cellpadding="0" cellspacing="0" class="" role="presentation" style="width:600px" width="600" bgcolor="#2a6099"><tr><td style="line-height:0;font-size:0;mso-line-height-rule:exactly"><![endif]--><div style="background:#2a6099;background-color:#2a6099;margin:0 auto;max-width:600px"><table align="center" border="0" cellpadding="0" cellspacing="0" role="presentation" style="background:#2a6099;background-color:#2a6099;width:100%"><tbody><tr><td style="direction:ltr;font-size:0;padding:10px 0 10px 0;text-align:center"><!--[if mso | IE]><table role="presentation" border="0" cellpadding="0" cellspacing="0"><tr><td class="" style="vertical-align:top;width:600px"><![endif]--><div class="mj-column-per-100 mj-outlook-group-fix" style="font-size:0;text-align:left;direction:ltr;display:inline-block;vertical-align:top;width:100%"><table border="0" cellpadding="0" cellspacing="0" role="presentation" style="vertical-align:top" width="100%"><tbody><tr><td align="left" style="font-size:0;padding:15px 15px 15px 15px;word-break:break-word"><div style="font-family:Ubuntu,Helvetica,Arial,sans-serif;font-size:13px;line-height:1.5;text-align:left;color:#000"><p style="font-family:Ubuntu,sans-serif;font-size:11px;text-align:center"><span style="font-size:20pt;color:#ecf0f1">&copy&nbsp 2025 Gestor de turnos</span></p></div></td></tr></tbody></table></div><!--[if mso | IE]><![endif]--></td></tr></tbody></table></div><!--[if mso | IE]><![endif]--></div></body></html>
        """;

    private readonly IFluentEmail _email;

    public TurnosEmailSender(IFluentEmail email) {
        _email = email;
    }

    private const string ConfirmationLinkSubject = "Confirmación de correo";

    public async Task SendConfirmationLinkAsync(User user, string email, string confirmationLink) {

        var html = string.Format(MessageLinkTemplate,
            HtmlEncoder.Default.Encode(ConfirmationLinkSubject),
            HtmlEncoder.Default.Encode("Para confirmar tu correo electrónico da"),
            confirmationLink,
            HtmlEncoder.Default.Encode("click aquí"));

        await _email
                .To(email, user.Name)
                .Subject(ConfirmationLinkSubject)
                .Body(html, true)
                .SendAsync(3, CancellationToken.None);
    }

    private const string PasswordResetCodeSubject = "Código de recuperación de contraseña";

    public async Task SendPasswordResetCodeAsync(User user, string email, string resetCode) {

        var html = string.Format(MessageCodeTemplate,
            HtmlEncoder.Default.Encode(PasswordResetCodeSubject),
            resetCode);

        await _email
                .To(email, user.Name)
                .Subject(PasswordResetCodeSubject)
                .Body(html, true)
                .SendAsync(3, CancellationToken.None);
    }

    private const string PasswordResetLinkSubject = "Recuperación de contraseña";

    public async Task SendPasswordResetLinkAsync(User user, string email, string resetLink) {

        var html = string.Format(MessageLinkTemplate,
            HtmlEncoder.Default.Encode(PasswordResetLinkSubject),
            HtmlEncoder.Default.Encode("Para recuperar tu contraseña da"),
            resetLink,
            HtmlEncoder.Default.Encode("click aquí"));

        await _email
                .To(email, user.Name)
                .Subject(PasswordResetLinkSubject)
                .Body(html, true)
                .SendAsync(3, CancellationToken.None);
    }
}

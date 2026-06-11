namespace Atlas.Identity.Application.Emails;

internal static class IdentityEmailTemplates
{
    internal static (string Subject, string HtmlBody) Invitation(string loginUrl) => (
        Subject: "You were invited to Atlas",
        HtmlBody: Layout(
            title: "You were invited to Atlas",
            preheader: "An invitation was created for your email address.",
            body: $"""
                <p style="margin:0 0 16px 0;font-size:16px;line-height:1.6;color:#374151;">
                    An invitation was created for your email address. Click the button below
                    to sign in and complete your onboarding.
                </p>
                {CtaButton(loginUrl, "Sign in to Atlas")}
                <p style="margin:24px 0 0 0;font-size:14px;line-height:1.6;color:#6b7280;">
                    If you weren't expecting this invitation, you can safely ignore this email.
                </p>
            """
        )
    );

    internal static (string Subject, string HtmlBody) Welcome(string loginUrl) => (
        Subject: "Welcome to Atlas",
        HtmlBody: Layout(
            title: "Welcome to Atlas",
            preheader: "Your account is ready. You can now sign in.",
            body: $"""
                <p style="margin:0 0 16px 0;font-size:16px;line-height:1.6;color:#374151;">
                    Your account has been set up and you're ready to go. Click the button
                    below to sign in and start using the platform.
                </p>
                {CtaButton(loginUrl, "Sign in to Atlas")}
                <p style="margin:24px 0 0 0;font-size:14px;line-height:1.6;color:#6b7280;">
                    If you have any questions, reply to this email and we'll be happy to help.
                </p>
            """
        )
    );

    // ── Private ──────────────────────────────────────────────────────────────

    private static string CtaButton(string url, string label) => $"""
        <table role="presentation" cellspacing="0" cellpadding="0" border="0" style="margin:24px 0;">
            <tr>
                <td style="border-radius:6px;background:#1d4ed8;">
                    <a href="{url}"
                       target="_blank"
                       style="display:inline-block;padding:12px 28px;font-family:Arial,sans-serif;
                              font-size:15px;font-weight:600;color:#ffffff;text-decoration:none;
                              border-radius:6px;">
                        {label}
                    </a>
                </td>
            </tr>
        </table>
        """;

    private static string Layout(string title, string preheader, string body) => $"""
        <!DOCTYPE html>
        <html lang="en">
        <head>
            <meta charset="UTF-8">
            <meta name="viewport" content="width=device-width,initial-scale=1">
            <meta http-equiv="X-UA-Compatible" content="IE=edge">
            <title>{title}</title>
        </head>
        <body style="margin:0;padding:0;background-color:#f3f4f6;font-family:Arial,Helvetica,sans-serif;">
            <!-- preheader -->
            <div style="display:none;max-height:0;overflow:hidden;mso-hide:all;">
                {preheader}&#847;&zwnj;&nbsp;&#847;&zwnj;&nbsp;&#847;&zwnj;&nbsp;
            </div>

            <table role="presentation" cellspacing="0" cellpadding="0" border="0"
                   width="100%" style="background-color:#f3f4f6;">
                <tr>
                    <td align="center" style="padding:40px 16px;">

                        <!-- card -->
                        <table role="presentation" cellspacing="0" cellpadding="0" border="0"
                               width="600" style="max-width:600px;width:100%;background:#ffffff;
                                                  border-radius:8px;overflow:hidden;
                                                  box-shadow:0 1px 3px rgba(0,0,0,0.08);">

                            <!-- header -->
                            <tr>
                                <td style="background:#1d4ed8;padding:28px 40px;">
                                    <span style="font-size:22px;font-weight:700;color:#ffffff;
                                                 letter-spacing:-0.5px;">Atlas</span>
                                </td>
                            </tr>

                            <!-- body -->
                            <tr>
                                <td style="padding:40px 40px 32px 40px;">
                                    <h1 style="margin:0 0 20px 0;font-size:22px;font-weight:700;
                                               color:#111827;line-height:1.3;">
                                        {title}
                                    </h1>
                                    {body}
                                </td>
                            </tr>

                            <!-- footer -->
                            <tr>
                                <td style="background:#f9fafb;padding:20px 40px;
                                           border-top:1px solid #e5e7eb;">
                                    <p style="margin:0;font-size:12px;color:#9ca3af;line-height:1.6;">
                                        You received this email because an action was taken on your
                                        account. This is an automated message — please do not reply.
                                    </p>
                                </td>
                            </tr>

                        </table>

                    </td>
                </tr>
            </table>
        </body>
        </html>
        """;
}

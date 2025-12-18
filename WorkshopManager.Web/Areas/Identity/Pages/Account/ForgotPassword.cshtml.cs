// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
#nullable disable

using System;
using System.ComponentModel.DataAnnotations;
using System.Text;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.WebUtilities;
using WorkshopManager.Model.DataModels;

namespace WorkshopManager.Web.Areas.Identity.Pages.Account
{
    public class ForgotPasswordModel : PageModel
    {
        private readonly UserManager<User> _userManager;
        private readonly IEmailSender _emailSender;

        public ForgotPasswordModel(UserManager<User> userManager, IEmailSender emailSender)
        {
            _userManager = userManager;
            _emailSender = emailSender;
        }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        [BindProperty]
        public InputModel Input { get; set; }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public class InputModel
        {
            /// <summary>
            ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
            ///     directly from your code. This API may change or be removed in future releases.
            /// </summary>
            [Required]
            [EmailAddress]
            public string Email { get; set; }
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (ModelState.IsValid)
            {
                var user = await _userManager.FindByEmailAsync(Input.Email);
                if (user == null)
                {
                    // Don't reveal that the user does not exist
                    return RedirectToPage("./ForgotPasswordConfirmation");
                }

                // For more information on how to enable account confirmation and password reset please
                // visit https://go.microsoft.com/fwlink/?LinkID=532713
                var code = await _userManager.GeneratePasswordResetTokenAsync(user);
                code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));
                var callbackUrl = Url.Page(
                    "/Account/ResetPassword",
                    pageHandler: null,
                    values: new { area = "Identity", code },
                    protocol: Request.Scheme);

                var emailBody = $@"
                    <html>
                    <body style='font-family: Arial, sans-serif;'>
                        <div style='max-width: 600px; margin: 0 auto; padding: 20px;'>
                            <h2 style='color: #333;'>Witaj!</h2>
                            <p>Otrzymaliśmy prośbę o zresetowanie hasła do Twojego konta w systemie Workshop Manager.</p>
                            <p>Aby zresetować hasło, kliknij poniższy link:</p>
                            <p style='margin: 30px 0;'>
                                <a href='{HtmlEncoder.Default.Encode(callbackUrl)}'
                                   style='background-color: #007bff; color: white; padding: 12px 24px;
                                          text-decoration: none; border-radius: 4px; display: inline-block;'>
                                    Zresetuj hasło
                                </a>
                            </p>
                            <p style='color: #666; font-size: 14px;'>
                                Jeśli nie prosiłeś o reset hasła, zignoruj tę wiadomość.
                                Twoje hasło pozostanie bez zmian.
                            </p>
                            <hr style='margin: 30px 0; border: none; border-top: 1px solid #ddd;'>
                            <p style='color: #999; font-size: 12px;'>
                                Pozdrawiamy,<br>
                                Zespół Workshop Manager
                            </p>
                        </div>
                    </body>
                    </html>";

                await _emailSender.SendEmailAsync(
                    Input.Email,
                    "Reset hasła - Workshop Manager",
                    emailBody);

                return RedirectToPage("./ForgotPasswordConfirmation");
            }

            return Page();
        }
    }
}

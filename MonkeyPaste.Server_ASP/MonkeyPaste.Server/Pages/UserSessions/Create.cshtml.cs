using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using MonkeyPaste.Server;
using MonkeyPaste.Server.Data;

namespace MonkeyPaste.Server.Pages.UserSessions
{
    public class CreateModel : PageModel
    {
        private readonly MonkeyPaste.Server.Data.ApplicationDbContext _context;

        public CreateModel(MonkeyPaste.Server.Data.ApplicationDbContext context)
        {
            _context = context;
        }

        public IActionResult OnGet()
        {
            if(string.IsNullOrEmpty(Email) || string.IsNullOrEmpty(Ip)) {
                return Content(string.Empty);
            }

            var users = from u in _context.Users
                        select u;
            var user = users.Where(u => u.Email.ToLower() == Email.ToLower()).FirstOrDefault();

            if (user == null) {
                return Content(string.Empty);
            }

            var userSession = new MpUserSession() {
                UserId = user.Id,
                AccessToken = MpHelpers.Instance.GetNewAccessToken(),
                Ip4Address = Ip,
                LoginDateTime = DateTime.UtcNow
            };

            _context.MpUserSession.Add(userSession);

            _context.SaveChanges();

            return Content(userSession.AccessToken);
        }

        [BindProperty(SupportsGet = true)]
        public string Email { get; set; }
        [BindProperty(SupportsGet = true)]
        public string Ip { get; set; }

        [BindProperty]
        public MpUserSession MpUserSession { get; set; }

        // To protect from overposting attacks, see https://aka.ms/RazorPagesCRUD
        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            _context.MpUserSession.Add(MpUserSession);
            await _context.SaveChangesAsync();

            return RedirectToPage("./Index");
        }
    }
}

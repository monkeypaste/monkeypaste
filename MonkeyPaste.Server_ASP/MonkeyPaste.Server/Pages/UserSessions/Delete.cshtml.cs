using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using MonkeyPaste.Server;
using MonkeyPaste.Server.Data;

namespace MonkeyPaste.Server.Pages.UserSessions
{
    public class DeleteModel : PageModel
    {
        private readonly MonkeyPaste.Server.Data.ApplicationDbContext _context;

        public DeleteModel(MonkeyPaste.Server.Data.ApplicationDbContext context)
        {
            _context = context;
        }

        [BindProperty]
        public MpUserSession MpUserSession { get; set; }

        public async Task<IActionResult> OnGetAsync(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            MpUserSession = await _context.MpUserSession.FirstOrDefaultAsync(m => m.Id == id);

            if (MpUserSession == null)
            {
                return NotFound();
            }
            return Page();
        }

        public async Task<IActionResult> OnPostAsync(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            MpUserSession = await _context.MpUserSession.FindAsync(id);

            if (MpUserSession != null)
            {
                _context.MpUserSession.Remove(MpUserSession);
                await _context.SaveChangesAsync();
            }

            return RedirectToPage("./Index");
        }
    }
}

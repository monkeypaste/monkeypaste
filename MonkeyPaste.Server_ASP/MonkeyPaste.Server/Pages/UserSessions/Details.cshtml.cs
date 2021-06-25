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
    public class DetailsModel : PageModel
    {
        private readonly MonkeyPaste.Server.Data.ApplicationDbContext _context;

        public DetailsModel(MonkeyPaste.Server.Data.ApplicationDbContext context)
        {
            _context = context;
        }

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
    }
}

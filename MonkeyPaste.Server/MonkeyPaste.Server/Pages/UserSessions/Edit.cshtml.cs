using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using MonkeyPaste.Server;
using MonkeyPaste.Server.Data;

namespace MonkeyPaste.Server.Pages.UserSessions
{
    public class EditModel : PageModel
    {
        private readonly MonkeyPaste.Server.Data.ApplicationDbContext _context;

        public EditModel(MonkeyPaste.Server.Data.ApplicationDbContext context)
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

        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see https://aka.ms/RazorPagesCRUD.
        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            _context.Attach(MpUserSession).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!MpUserSessionExists(MpUserSession.Id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return RedirectToPage("./Index");
        }

        private bool MpUserSessionExists(int id)
        {
            return _context.MpUserSession.Any(e => e.Id == id);
        }
    }
}

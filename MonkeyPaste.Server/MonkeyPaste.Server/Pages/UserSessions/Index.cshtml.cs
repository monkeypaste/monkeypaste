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
    public class IndexModel : PageModel
    {
        private readonly MonkeyPaste.Server.Data.ApplicationDbContext _context;

        public IndexModel(MonkeyPaste.Server.Data.ApplicationDbContext context)
        {
            _context = context;
        }

        public IList<MpUserSession> MpUserSession { get;set; }

        public async Task OnGetAsync()
        {
            MpUserSession = await _context.MpUserSession.ToListAsync();
        }
    }
}

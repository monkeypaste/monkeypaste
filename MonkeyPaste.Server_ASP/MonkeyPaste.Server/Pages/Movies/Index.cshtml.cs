using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using MonkeyPaste.Server.Data;
using MonkeyPaste.Server.Models;

namespace MonkeyPaste.Server.Pages.Movies
{
    public class IndexModel : PageModel
    {
        private readonly MonkeyPaste.Server.Data.ApplicationDbContext _context;

        public IndexModel(MonkeyPaste.Server.Data.ApplicationDbContext context)
        {
            _context = context;
        }

        public IList<Movie> Movie { get;set; }

        public async Task OnGetAsync()
        {
            Movie = await _context.Movie.ToListAsync();
        }
    }
}

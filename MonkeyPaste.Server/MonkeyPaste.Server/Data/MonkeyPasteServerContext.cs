using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using MonkeyPaste.Server.Models;

namespace MonkeyPaste.Server.Data
{
    public class MonkeyPasteServerContext : DbContext
    {
        public MonkeyPasteServerContext (DbContextOptions<MonkeyPasteServerContext> options)
            : base(options)
        {
        }

        public DbSet<MonkeyPaste.Server.Models.Movie> Movie { get; set; }

        public DbSet<MonkeyPaste.Server.Models.MpUser> MpUser { get; set; }

        public DbSet<MonkeyPaste.Server.Models.MpUserSession> MpUserSession { get; set; }
    }
}

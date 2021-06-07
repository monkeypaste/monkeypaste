using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using MonkeyPaste.Server.Data;
using MonkeyPaste.Server.Models;

namespace MonkeyPaste.Server
{
    public class MpUsersController : Controller
    {
        private readonly MonkeyPasteServerContext _context;

        public MpUsersController(MonkeyPasteServerContext context)
        {
            _context = context;
        }

        // GET: MpUsers
        public async Task<IActionResult> Index()
        {
            return View(await _context.MpUser.ToListAsync());
        }

        // GET: MpUsers/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var mpUser = await _context.MpUser
                .FirstOrDefaultAsync(m => m.Id == id);
            if (mpUser == null)
            {
                return NotFound();
            }

            return View(mpUser);
        }

        // GET: MpUsers/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: MpUsers/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Email,Password,UserStateTypeId")] MpUser mpUser)
        {
            if (ModelState.IsValid)
            {
                _context.Add(mpUser);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(mpUser);
        }

        // GET: MpUsers/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var mpUser = await _context.MpUser.FindAsync(id);
            if (mpUser == null)
            {
                return NotFound();
            }
            return View(mpUser);
        }

        // POST: MpUsers/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Email,Password,UserStateTypeId")] MpUser mpUser)
        {
            if (id != mpUser.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(mpUser);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!MpUserExists(mpUser.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(mpUser);
        }

        // GET: MpUsers/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var mpUser = await _context.MpUser
                .FirstOrDefaultAsync(m => m.Id == id);
            if (mpUser == null)
            {
                return NotFound();
            }

            return View(mpUser);
        }

        // POST: MpUsers/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var mpUser = await _context.MpUser.FindAsync(id);
            _context.MpUser.Remove(mpUser);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool MpUserExists(int id)
        {
            return _context.MpUser.Any(e => e.Id == id);
        }

        public async Task<string> Connect(string email, string ip) {
            var userModel = await _context.MpUser.FirstOrDefaultAsync(x => x.Email.ToLower() == email.ToLower());
            if(userModel == null) {
                return null;
            }
            var userSession = new MpUserSession() { UserId = userModel.Id, Ip4Address = ip, LoginDateTime = DateTime.Now, AccessToken = MpHelpers.Instance.GetNewAccessToken() };

            _context.Add(userSession);
            await _context.SaveChangesAsync();

            return userSession.AccessToken;
        }

        public async Task<IActionResult> Disconnect(string email, string ip) {
            var userModel = await _context.MpUser.FirstOrDefaultAsync(x => x.Email.ToLower() == email.ToLower());
            if (userModel == null) {
                return NotFound();
            }

            var userSession = await _context.MpUserSession.FirstOrDefaultAsync(x => x.UserId == userModel.Id && x.Ip4Address == ip);

            _context.Remove(userSession);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
    }
}

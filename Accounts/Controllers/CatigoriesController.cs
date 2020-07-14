using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Net;
using System.Web;
using System.Web.Mvc;
using Accounts.Models;

namespace Accounts.Controllers
{
    [Authorize(Roles = "Administrators")]
    public class CatigoriesController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        // GET: Catigories
        public async Task<ActionResult> Index()
        {
            var catigories = db.Catigories.Include(c => c.Section);
            return View(await catigories.ToListAsync());
        }

        // GET: Catigories/Details/5
        public async Task<ActionResult> Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Catigory catigory = await db.Catigories.FindAsync(id);
            if (catigory == null)
            {
                return HttpNotFound();
            }
            return View(catigory);
        }

        // GET: Catigories/Create
        public ActionResult Create()
        {
            ViewBag.SectionId = new SelectList(db.Sections, "Id", "Name");
            return View();
        }

        // POST: Catigories/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create([Bind(Include = "Id,Name,IndexSort,SectionId")] Catigory catigory)
        {
            if (ModelState.IsValid)
            {
                db.Catigories.Add(catigory);
                await db.SaveChangesAsync();
                return RedirectToAction("Index");
            }

            ViewBag.SectionId = new SelectList(db.Sections, "Id", "Name", catigory.SectionId);
            return View(catigory);
        }

        // GET: Catigories/Edit/5
        public async Task<ActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Catigory catigory = await db.Catigories.FindAsync(id);
            if (catigory == null)
            {
                return HttpNotFound();
            }
            ViewBag.SectionId = new SelectList(db.Sections, "Id", "Name", catigory.SectionId);
            return View(catigory);
        }

        // POST: Catigories/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit([Bind(Include = "Id,Name,IndexSort,SectionId")] Catigory catigory)
        {
            if (ModelState.IsValid)
            {
                db.Entry(catigory).State = EntityState.Modified;
                await db.SaveChangesAsync();
                return RedirectToAction("Index");
            }
            ViewBag.SectionId = new SelectList(db.Sections, "Id", "Name", catigory.SectionId);
            return View(catigory);
        }

        // GET: Catigories/Delete/5
        public async Task<ActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Catigory catigory = await db.Catigories.FindAsync(id);
            if (catigory == null)
            {
                return HttpNotFound();
            }
            return View(catigory);
        }

        // POST: Catigories/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteConfirmed(int id)
        {
            Catigory catigory = await db.Catigories.FindAsync(id);
            db.Catigories.Remove(catigory);
            await db.SaveChangesAsync();
            return RedirectToAction("Index");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}

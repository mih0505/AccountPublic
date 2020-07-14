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
    public class DirectionOfTrainingsController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        // GET: DirectionOfTrainings
        public async Task<ActionResult> Index()
        {
            return View(await db.DirectionOfTrainings.ToListAsync());
        }

        // GET: DirectionOfTrainings/Details/5
        public async Task<ActionResult> Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            DirectionOfTraining directionOfTraining = await db.DirectionOfTrainings.FindAsync(id);
            if (directionOfTraining == null)
            {
                return HttpNotFound();
            }
            return View(directionOfTraining);
        }

        // GET: DirectionOfTrainings/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: DirectionOfTrainings/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create([Bind(Include = "Id,Name")] DirectionOfTraining directionOfTraining)
        {
            if (ModelState.IsValid)
            {
                db.DirectionOfTrainings.Add(directionOfTraining);
                await db.SaveChangesAsync();
                return RedirectToAction("Index");
            }

            return View(directionOfTraining);
        }

        // GET: DirectionOfTrainings/Edit/5
        public async Task<ActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            DirectionOfTraining directionOfTraining = await db.DirectionOfTrainings.FindAsync(id);
            if (directionOfTraining == null)
            {
                return HttpNotFound();
            }
            return View(directionOfTraining);
        }

        // POST: DirectionOfTrainings/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit([Bind(Include = "Id,Name")] DirectionOfTraining directionOfTraining)
        {
            if (ModelState.IsValid)
            {
                db.Entry(directionOfTraining).State = EntityState.Modified;
                await db.SaveChangesAsync();
                return RedirectToAction("Index");
            }
            return View(directionOfTraining);
        }

        // GET: DirectionOfTrainings/Delete/5
        public async Task<ActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            DirectionOfTraining directionOfTraining = await db.DirectionOfTrainings.FindAsync(id);
            if (directionOfTraining == null)
            {
                return HttpNotFound();
            }
            return View(directionOfTraining);
        }

        // POST: DirectionOfTrainings/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteConfirmed(int id)
        {
            DirectionOfTraining directionOfTraining = await db.DirectionOfTrainings.FindAsync(id);
            db.DirectionOfTrainings.Remove(directionOfTraining);
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

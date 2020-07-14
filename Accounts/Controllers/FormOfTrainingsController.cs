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
    public class FormOfTrainingsController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        // GET: FormOfTrainings
        public async Task<ActionResult> Index()
        {
            return View(await db.FormOfTrainings.ToListAsync());
        }

        // GET: FormOfTrainings/Details/5
        public async Task<ActionResult> Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            FormOfTraining formOfTraining = await db.FormOfTrainings.FindAsync(id);
            if (formOfTraining == null)
            {
                return HttpNotFound();
            }
            return View(formOfTraining);
        }

        // GET: FormOfTrainings/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: FormOfTrainings/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create([Bind(Include = "Id,Name")] FormOfTraining formOfTraining)
        {
            if (ModelState.IsValid)
            {
                db.FormOfTrainings.Add(formOfTraining);
                await db.SaveChangesAsync();
                return RedirectToAction("Index");
            }

            return View(formOfTraining);
        }

        // GET: FormOfTrainings/Edit/5
        public async Task<ActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            FormOfTraining formOfTraining = await db.FormOfTrainings.FindAsync(id);
            if (formOfTraining == null)
            {
                return HttpNotFound();
            }
            return View(formOfTraining);
        }

        // POST: FormOfTrainings/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit([Bind(Include = "Id,Name")] FormOfTraining formOfTraining)
        {
            if (ModelState.IsValid)
            {
                db.Entry(formOfTraining).State = EntityState.Modified;
                await db.SaveChangesAsync();
                return RedirectToAction("Index");
            }
            return View(formOfTraining);
        }

        // GET: FormOfTrainings/Delete/5
        public async Task<ActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            FormOfTraining formOfTraining = await db.FormOfTrainings.FindAsync(id);
            if (formOfTraining == null)
            {
                return HttpNotFound();
            }
            return View(formOfTraining);
        }

        // POST: FormOfTrainings/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteConfirmed(int id)
        {
            FormOfTraining formOfTraining = await db.FormOfTrainings.FindAsync(id);
            db.FormOfTrainings.Remove(formOfTraining);
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

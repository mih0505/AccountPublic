using Accounts.Models;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using System;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace Accounts.Controllers
{
    [Authorize]
    public class ArtifactsController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();
        private ApplicationUserManager _userManager;

        public ArtifactsController()
        {
        }

        public ArtifactsController(ApplicationUserManager userManager)
        {
            UserManager = userManager;
        }

        public ApplicationUserManager UserManager
        {
            get
            {
                return _userManager ?? HttpContext.GetOwinContext().GetUserManager<ApplicationUserManager>();
            }
            private set
            {
                _userManager = value;
            }
        }

        public FileResult GetFile(string path, string name)
        {
            byte[] mas = System.IO.File.ReadAllBytes(path);
            string file_path = Server.MapPath("~/Portfolio/" + name);
            string file_type = "application/octet-stream";
            return File(mas, file_type, name);
        }

        // GET: Artifacts
        public async Task<ActionResult> Index(int? id)
        {
            if (id != null)
            {
                string userId = User.Identity.GetUserId();
                var section = await db.Sections.FindAsync(id);
                ViewBag.Section = section;

                //получение списка категорий
                var catigories = await db.Catigories.Where(a => a.SectionId == id).OrderBy(a => a.IndexSort).ToListAsync();
                ViewBag.Catigories = catigories;

                if (userId == null && userId == "" && catigories == null && section == null)
                    return HttpNotFound();

                //получение списка достижений
                var artifacts = from arts in db.Artifacts
                                join cats in db.Catigories on arts.CatigoryId equals cats.Id
                                where cats.SectionId == id && arts.User.Id == userId
                                orderby cats.Name, arts.DateBegin
                                select arts;

                return View(await artifacts.ToListAsync());
            }
            else return HttpNotFound();
        }
                
        // GET: Artifacts/Details/5
        public async Task<ActionResult> Details(int? id)
        {
            string userId = User.Identity.GetUserId();
            if (id == null && userId == null && userId == "")
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            Artifact artifact = null;
            if (User.IsInRole("Students"))
            {
                artifact = await db.Artifacts.Where(a => a.UserId == userId)
                    .Include(a => a.Catigory)
                    .SingleOrDefaultAsync(m => m.Id == id);
            }
            else if (User.IsInRole("Teachers") || User.IsInRole("Administrators") || User.IsInRole("TutorsManagers"))
            {
                artifact = await db.Artifacts.Include(a => a.Catigory)
                    .SingleOrDefaultAsync(m => m.Id == id);
            }
            else return HttpNotFound();

            //информация о разделе для возврата к списку достижений
            var currentCatigory = db.Catigories.Find(artifact.CatigoryId);
            if (currentCatigory == null)
            {
                return HttpNotFound();
            }
            ViewBag.CurrentCatigory = currentCatigory;

            var currentSection = db.Sections.Find(currentCatigory.SectionId);
            if (currentCatigory == null)
            {
                return HttpNotFound();
            }
            ViewBag.CurrentSection = currentSection;

            return View(artifact);
        }

        // GET: Artifacts/Create
        public ActionResult Create(int? id, string userId)
        {
            if (id == null)
            {
                return HttpNotFound();
            }

            //получаем добавляемую категорию
            var currentCatigory = db.Catigories.FirstOrDefault(a => a.Id == id);
            if (currentCatigory == null)
            {
                return HttpNotFound();
            }
            ViewBag.UserId = userId;
            ViewBag.Catigory = currentCatigory;
            return View();
        }

        // POST: Artifacts/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create([Bind(Include = "Id,CatigoryId,Name,DateBegin,Location,AdditionalInformation,Link,NameFile,Path,UserId,DateAdd")] Artifact artifact, HttpPostedFileBase NameFile)
        {
            Catigory currentCatigory = db.Catigories.Find(artifact.CatigoryId);
            if (ModelState.IsValid)
            {
                //сохраняем автора события
                if(artifact.UserId == null)                    
                    artifact.UserId = User.Identity.GetUserId();                

                artifact.DateAdd = DateTime.Now;

                //Сохраняем изображение                
                try
                {
                    if (NameFile != null && NameFile.ContentLength > 31457280 ||
                        (NameFile != null && Path.GetExtension(NameFile.FileName.ToLower()) != ".jpg" && Path.GetExtension(NameFile.FileName.ToLower()) != ".jpeg" &&
                        Path.GetExtension(NameFile.FileName.ToLower()) != ".gif" && Path.GetExtension(NameFile.FileName.ToLower()) != ".pdf" &&
                         Path.GetExtension(NameFile.FileName.ToLower()) != ".zip" && Path.GetExtension(NameFile.FileName.ToLower()) != ".png"))
                    {
                        return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
                    }
                    if (NameFile != null && NameFile.ContentLength > 0)
                    {
                        string extension = Path.GetExtension(NameFile.FileName);

                        artifact.NameFile = NameFile.FileName;
                        string fileName = Guid.NewGuid().ToString() + extension;
                        artifact.Path = Server.MapPath("~/Portfolio/" + fileName);
                        NameFile.SaveAs(artifact.Path);
                        ViewBag.Message = "Загрузка файла выполнена";
                    }
                }
                catch
                {
                    ViewBag.Message = "Ошибка загрузки файла";
                }

                db.Artifacts.Add(artifact);
                await db.SaveChangesAsync();
                if (User.IsInRole("Students"))
                    return RedirectToAction("Index", "Artifacts", new { id = currentCatigory.SectionId });
                else
                    return RedirectToAction("GetArtifacts", "Tutors", new { sectionId = currentCatigory.SectionId, userId = artifact.UserId });
            }
            ViewBag.Catigory = currentCatigory;
            return View(artifact);
        }

        // GET: Artifacts/Edit/5
        public async Task<ActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            string userId = User.Identity.GetUserId();
            if (id == null && userId == null && userId == "")
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            Artifact artifact = null;
            if (User.IsInRole("Students"))
            {
                artifact = await db.Artifacts.Where(a => a.UserId == userId)
                    .Include(a => a.Catigory)
                    .SingleOrDefaultAsync(m => m.Id == id);
            }
            else if (User.IsInRole("Teachers") || User.IsInRole("Administrators") || User.IsInRole("TutorsManagers"))
            {
                artifact = await db.Artifacts.Include(a => a.Catigory)
                    .SingleOrDefaultAsync(m => m.Id == id);
            }
            else return HttpNotFound();

            //информация о разделе для возврата к списку достижений
            var currentCatigory = db.Catigories.Find(artifact.CatigoryId);
            if (currentCatigory == null)
            {
                return HttpNotFound();
            }
            ViewBag.CurrentCatigory = currentCatigory;

            var currentSection = db.Sections.Find(currentCatigory.SectionId);
            if (currentCatigory == null)
            {
                return HttpNotFound();
            }
            ViewBag.CurrentSection = currentSection;

            return View(artifact);
        }

        // POST: Artifacts/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit([Bind(Include = "Id,CatigoryId,Name,DateBegin,Location,AdditionalInformation,Link,Path,NameFile,UserId,DateAdd")] Artifact artifact, HttpPostedFileBase File)
        {
            Catigory currentCatigory = db.Catigories.Find(artifact.CatigoryId);

            string userId = User.Identity.GetUserId();
            if (userId == null && userId == "")
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            if (ModelState.IsValid)
            {
                //сохраняем автора события                
                if (User.IsInRole("Students"))
                {
                    artifact.DateAdd = DateTime.Now;
                    artifact.UserId = userId;
                }
                //Сохраняем изображение                
                try
                {
                    if (File != null && File.ContentLength > 31457280 ||
                        (File != null && Path.GetExtension(File.FileName.ToLower()) != ".jpg" && Path.GetExtension(File.FileName.ToLower()) != ".jpeg" && Path.GetExtension(File.FileName.ToLower()) != ".gif"
                        && Path.GetExtension(File.FileName.ToLower()) != ".pdf" && Path.GetExtension(File.FileName.ToLower()) != ".zip" && Path.GetExtension(File.FileName.ToLower()) != ".png"))
                    {
                        return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
                    }
                    if (File != null && File.ContentLength > 0)
                    {
                        string extension = Path.GetExtension(File.FileName);
                        artifact.NameFile = File.FileName;
                        string fileName = Guid.NewGuid().ToString() + extension;
                        artifact.Path = Server.MapPath("~/Portfolio/" + fileName);
                        File.SaveAs(artifact.Path);
                        ViewBag.Message = "Загрузка файла выполнена";
                    }
                }
                catch
                {
                    ViewBag.Message = "Ошибка загрузки файла";
                }

                db.Entry(artifact).State = EntityState.Modified;
                await db.SaveChangesAsync();
                if (User.IsInRole("Students"))
                    return RedirectToAction("Index", "Artifacts", new { id = currentCatigory.SectionId });
                else
                    return RedirectToAction("GetArtifacts", "Tutors", new { sectionId = currentCatigory.SectionId, userId = artifact.UserId });
            }
            ViewBag.CurrentCatigory = currentCatigory;
            return View(artifact);
        }

        // GET: Artifacts/Delete/5
        public async Task<ActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            string userId = User.Identity.GetUserId();
            if (id == null && userId == null && userId == "")
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            Artifact artifact = null;
            if (User.IsInRole("Students"))
            {
                artifact = await db.Artifacts.Where(a => a.UserId == userId)
                    .Include(a => a.Catigory)
                    .SingleOrDefaultAsync(m => m.Id == id);
            }
            else if (User.IsInRole("Teachers") || User.IsInRole("Administrators") || User.IsInRole("TutorsManagers"))
            {
                artifact = await db.Artifacts.Include(a => a.Catigory)
                    .SingleOrDefaultAsync(m => m.Id == id);
            }
            else return HttpNotFound();

            //информация о разделе для возврата к списку достижений
            var currentCatigory = db.Catigories.Find(artifact.CatigoryId);
            if (currentCatigory == null)
            {
                return HttpNotFound();
            }
            ViewBag.CurrentCatigory = currentCatigory;

            var currentSection = db.Sections.Find(currentCatigory.SectionId);
            if (currentCatigory == null)
            {
                return HttpNotFound();
            }
            ViewBag.CurrentSection = currentSection;

            return View(artifact);
        }

        // POST: Artifacts/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteConfirmed(int id)
        {
            string userId = User.Identity.GetUserId();
            if (userId == null && userId == "")
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            Artifact artifact = null;
            if (User.IsInRole("Students"))
            {
                artifact = await db.Artifacts.Where(a => a.UserId == userId)
                    .Include(a => a.Catigory)
                    .SingleOrDefaultAsync(m => m.Id == id);
            }
            else if (User.IsInRole("Teachers") || User.IsInRole("Administrators") || User.IsInRole("TutorsManagers"))
            {
                artifact = await db.Artifacts.Include(a => a.Catigory)
                    .SingleOrDefaultAsync(m => m.Id == id);
                userId = artifact.UserId;
            }
            else return HttpNotFound();

            db.Artifacts.Remove(artifact);
            await db.SaveChangesAsync();

            //удаление файла подтверждения
            if (artifact.Path != null)
                System.IO.File.Delete(artifact.Path);

            Catigory currentCatigory = db.Catigories.Find(artifact.CatigoryId);
            if (User.IsInRole("Students"))
                return RedirectToAction("Index", "Artifacts", new { id = currentCatigory.SectionId });
            else
                return RedirectToAction("GetArtifacts", "Tutors", new { sectionId = currentCatigory.SectionId, userId = userId });
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

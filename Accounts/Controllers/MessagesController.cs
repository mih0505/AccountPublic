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
using Microsoft.AspNet.Identity.Owin;
using Microsoft.AspNet.Identity;

namespace Accounts.Controllers
{
    public class MessagesController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        // GET: Messages
        public async Task<ActionResult> Index()
        {
            // Можно улучшить этот алгоритм (главное не забыть!)
            // Получаем непрочитанные сообщения, чтобы в случае чего их прочитать
            var userId = User.Identity.GetUserId();
            var userManager = HttpContext.GetOwinContext().GetUserManager<ApplicationUserManager>();
            var userRoles = userManager.GetRoles(userId).ToList();

            var userMessages = await (from Message in db.Messages
                                      join Role in db.Roles on Message.RoleId equals Role.Id
                                      join UserRole in userRoles on Role.Name equals UserRole
                                      select new MessageViewModel()
                                      {
                                          MessageId = Message.Id,
                                          Title = Message.Title,
                                          Content = Message.Content,
                                          Date = Message.Date,
                                          RoleId = Role.Id,
                                          RoleName = Role.Name
                                      }).ToListAsync();

            var userReadedMessages = await (from ReadMessage in db.ReadMessages
                                            join Message in db.Messages on ReadMessage.MessageId equals Message.Id
                                            join Role in db.Roles on Message.RoleId equals Role.Id
                                            where ReadMessage.UserId == userId
                                            select new MessageViewModel()
                                            {
                                                MessageId = ReadMessage.MessageId,
                                                Title = Message.Title,
                                                Content = Message.Content,
                                                Date = Message.Date,
                                                RoleId = Role.Id,
                                                RoleName = Role.Name
                                            }).ToListAsync();

            var exceptMessages = (from Message in userMessages
                                  from ReadMessage in userReadedMessages
                                  where Message.MessageId == ReadMessage.MessageId
                                  select Message).ToList();

            foreach (var message in exceptMessages)
            {
                userMessages.Remove(message);
            }

            // Прочитываем непрочитанные сообщения
            if (userMessages.Count != 0)
            {
                foreach (var message in userMessages)
                {
                    db.ReadMessages.Add(new ReadMessage()
                    {
                        MessageId = message.MessageId,
                        UserId = userId
                    });
                }

                await db.SaveChangesAsync();
            }

            // Получаем все сообщений (и прочитанные, и непрочитанные)
            var messages = await (from Message in db.Messages
                                  join Role in db.Roles on Message.RoleId equals Role.Id
                                  select new MessageViewModel
                                  {
                                      MessageId = Message.Id,
                                      RoleId = Role.Id,
                                      RoleName = Role.Name,
                                      Title = Message.Title,
                                      Content = Message.Content,
                                      Date = Message.Date
                                  }).ToListAsync();

            var messagesViewModels = new List<MessageViewModel>();

            if (User.IsInRole("Students"))
            {
                messagesViewModels.AddRange(messages.Where(Message => Message.RoleName == "Students"));
            }

            if (User.IsInRole("TutorsManages"))
            {
                messagesViewModels.AddRange(messages.Where(Message => Message.RoleName == "TutorsManages"));
            }

            if (User.IsInRole("FacultiesManagers"))
            {
                messagesViewModels.AddRange(messages.Where(Message => Message.RoleName == "FacultiesManagers"));
            }

            if (User.IsInRole("Teachers"))
            {
                messagesViewModels.AddRange(messages.Where(Message => Message.RoleName == "Teachers"));
            }

            if (User.IsInRole("PersonnelDepartment"))
            {
                messagesViewModels.AddRange(messages.Where(Message => Message.RoleName == "PersonnelDepartment"));
            }

            if (User.IsInRole("DepartmentsManagers"))
            {
                messagesViewModels.AddRange(messages.Where(Message => Message.RoleName == "DepartmentsManagers"));
            }

            if (User.IsInRole("Administrators"))
            {
                messagesViewModels = messages;
            }

            return View(messagesViewModels);
        }

        public async Task<ActionResult> Close()
        {
            // Можно улучшить этот алгоритм (главное не забыть!)
            // Получаем непрочитанные сообщения, чтобы в случае чего их прочитать
            var userId = User.Identity.GetUserId();
            var userManager = HttpContext.GetOwinContext().GetUserManager<ApplicationUserManager>();
            var userRoles = userManager.GetRoles(userId).ToList();

            var userMessages = await (from Message in db.Messages
                                      join Role in db.Roles on Message.RoleId equals Role.Id
                                      join UserRole in userRoles on Role.Name equals UserRole
                                      select new MessageViewModel()
                                      {
                                          MessageId = Message.Id,
                                          Title = Message.Title,
                                          Content = Message.Content,
                                          Date = Message.Date,
                                          RoleId = Role.Id,
                                          RoleName = Role.Name
                                      }).ToListAsync();

            var userReadedMessages = await (from ReadMessage in db.ReadMessages
                                            join Message in db.Messages on ReadMessage.MessageId equals Message.Id
                                            join Role in db.Roles on Message.RoleId equals Role.Id
                                            where ReadMessage.UserId == userId
                                            select new MessageViewModel()
                                            {
                                                MessageId = ReadMessage.MessageId,
                                                Title = Message.Title,
                                                Content = Message.Content,
                                                Date = Message.Date,
                                                RoleId = Role.Id,
                                                RoleName = Role.Name
                                            }).ToListAsync();

            var exceptMessages = (from Message in userMessages
                                  from ReadMessage in userReadedMessages
                                  where Message.MessageId == ReadMessage.MessageId
                                  select Message).ToList();

            foreach (var message in exceptMessages)
            {
                userMessages.Remove(message);
            }

            // Прочитываем непрочитанные сообщения
            if (userMessages.Count != 0)
            {
                foreach (var message in userMessages)
                {
                    db.ReadMessages.Add(new ReadMessage()
                    {
                        MessageId = message.MessageId,
                        UserId = userId
                    });
                }

                await db.SaveChangesAsync();
            }

            return RedirectToAction("Index", "Home");
        }


        // GET: Messages/Details/5
        public async Task<ActionResult> Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            var messageViewModels = from Message in db.Messages
                                    join Role in db.Roles on Message.RoleId equals Role.Id
                                    select new MessageViewModel
                                    {
                                        MessageId = Message.Id,
                                        RoleId = Role.Id,
                                        RoleName = Role.Name,
                                        Title = Message.Title,
                                        Content = Message.Content,
                                        Date = Message.Date
                                    };

            var message = await messageViewModels.FirstOrDefaultAsync(Model => Model.MessageId == id);

            if (message == null)
            {
                return HttpNotFound();
            }

            return PartialView(message);
        }

        // GET: Messages/Create
        [Authorize(Roles = "Administrators")]
        public ActionResult Create()
        {
            ViewBag.RoleId = new SelectList(db.Roles, "Id", "Name");
            return View();
        }

        // POST: Messages/Create
        // Чтобы защититься от атак чрезмерной передачи данных, включите определенные свойства, для которых следует установить привязку. Дополнительные 
        // сведения см. в статье https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrators")]
        public async Task<ActionResult> Create([Bind(Include = "Id,RoleId,Title,Content")] Message message)
        {
            if (ModelState.IsValid)
            {
                message.Date = DateTime.Now;
                db.Messages.Add(message);
                await db.SaveChangesAsync();
                return RedirectToAction("Index");
            }

            ViewBag.RoleId = new SelectList(db.Roles, "Id", "Name", message.RoleId);

            return View(message);
        }

        // GET: Messages/Edit/5
        [Authorize(Roles = "Administrators")]
        public async Task<ActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            var message = await db.Messages.FindAsync(id);

            if (message == null)
            {
                return HttpNotFound();
            }

            ViewBag.RoleId = new SelectList(db.Roles, "Id", "Name", message.RoleId);

            return View(message);
        }

        // POST: Messages/Edit/5
        // Чтобы защититься от атак чрезмерной передачи данных, включите определенные свойства, для которых следует установить привязку. Дополнительные 
        // сведения см. в статье https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrators")]
        public async Task<ActionResult> Edit([Bind(Include = "Id,RoleId,Date,Title,Content")] Message message)
        {
            if (ModelState.IsValid)
            {
                db.Entry(message).State = EntityState.Modified;
                await db.SaveChangesAsync();
                return RedirectToAction("Index");
            }

            ViewBag.RoleId = new SelectList(db.Roles, "Id", "Name", message.RoleId);

            return View(message);
        }

        // GET: Messages/Delete/5
        [Authorize(Roles = "Administrators")]
        public async Task<ActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            var messageViewModels = from Message in db.Messages
                                    join Role in db.Roles on Message.RoleId equals Role.Id
                                    select new MessageViewModel
                                    {
                                        MessageId = Message.Id,
                                        RoleId = Role.Id,
                                        RoleName = Role.Name,
                                        Title = Message.Title,
                                        Content = Message.Content,
                                        Date = Message.Date
                                    };

            var message = await messageViewModels.FirstOrDefaultAsync(Model => Model.MessageId == id);

            if (message == null)
            {
                return HttpNotFound();
            }

            return View(message);
        }

        // POST: Messages/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrators")]
        public async Task<ActionResult> DeleteConfirmed(int id)
        {
            var message = await db.Messages.FindAsync(id);
            db.Messages.Remove(message);
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

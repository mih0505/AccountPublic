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
using System.Data.Entity.SqlServer;
using PagedList;

namespace Accounts.Controllers
{
    public class LogLoginsController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        // GET: LogLogins
        public async Task<ActionResult> Index(string sortOrder, string lastname, string login, DateTime? dateBegin, DateTime? dateEnd, int? page)
        {
            HttpCookie cookieLogin = Request.Cookies.Get("logLogin_login");            
            HttpCookie cookieSortOrder = Request.Cookies.Get("logLogin_sortOrder");
            HttpCookie cookieDateBegin = Request.Cookies.Get("logLogin_dateBegin");
            HttpCookie cookieDateEnd = Request.Cookies.Get("logLogin_dateEnd");
            HttpCookie cookieLastname = Request.Cookies.Get("logLogin_lastname");

            if (cookieLastname != null)
            {
                if (lastname != null && lastname != cookieLastname.Value)
                    cookieLastname.Value = lastname;
                else
                    lastname = cookieLastname.Value;
            }
            if (cookieLogin != null)
            {
                if (login != null && login != cookieLogin.Value)
                    cookieLogin.Value = login;
                else
                    login = cookieLogin.Value;
            }
            if (cookieSortOrder != null)
            {
                sortOrder = cookieSortOrder.Value;
            }
            if (cookieDateBegin != null)
            {
                if (dateBegin != null && Convert.ToDateTime(dateBegin).ToShortDateString() != cookieDateBegin.Value)
                    cookieDateBegin.Value = Convert.ToDateTime(dateBegin).ToShortDateString();
                else
                    dateBegin = Convert.ToDateTime(cookieDateBegin.Value);
            }
            if (cookieDateEnd != null)
            {
                if (dateEnd != null && Convert.ToDateTime(dateEnd).ToShortDateString() != cookieDateEnd.Value)
                    cookieDateEnd.Value = Convert.ToDateTime(dateEnd).ToShortDateString();
                else
                    dateEnd = Convert.ToDateTime(cookieDateEnd.Value);
            }

            ViewBag.CurrentSort = sortOrder;
            ViewBag.DateSortParm = String.IsNullOrEmpty(sortOrder) ? "date_asc" : "" ;
            ViewBag.LoginSortParm = sortOrder == "login_asc" ? "login_desc" : "login_asc";            
            ViewBag.LastnameSortParm = sortOrder == "lastname_asc" ? "lastname_desc" : "lastname_asc";

            ViewBag.LoginFilter = login;
            ViewBag.LastnameFilter = lastname;
            ViewBag.DateBeginFilter = dateBegin;
            ViewBag.DateEndFilter = dateEnd;

            IQueryable<LogLogins> logLogins = db.LogLogins.Include(l => l.User).OrderByDescending(a=>a.TimesLogin);
            if (!String.IsNullOrEmpty(login) || (!String.IsNullOrEmpty(login) && login != cookieLogin.Value))
            {
                cookieLogin = new HttpCookie("logLogin_login");
                cookieLogin.Value = login;
                cookieLogin.Expires = DateTime.Now.AddMinutes(15);
                Response.Cookies.Add(cookieLogin);
                logLogins = logLogins.Where(p => SqlFunctions.PatIndex("%" + login + "%", p.User.Email) > 0);
            }
            if (!String.IsNullOrEmpty(lastname) || (!String.IsNullOrEmpty(lastname) && lastname != cookieLastname.Value))
            {
                cookieLastname = new HttpCookie("logLogin_lastname");
                cookieLastname.Value = lastname;
                cookieLastname.Expires = DateTime.Now.AddMinutes(15);
                Response.Cookies.Add(cookieLastname);
                logLogins = logLogins.Where(p => SqlFunctions.PatIndex("%" + lastname + "%", p.User.Lastname) > 0);
            }

            if (dateBegin != null)
            {
                var d1 = Convert.ToDateTime(dateBegin).Date;
                var dateB = d1.ToShortDateString();
                if (!String.IsNullOrEmpty(dateB) || (!String.IsNullOrEmpty(dateB) && dateB != cookieDateBegin.Value))
                {
                    cookieDateBegin = new HttpCookie("logLogin_dateBegin");
                    cookieDateBegin.Value = dateB;
                    cookieDateBegin.Expires = DateTime.Now.AddMinutes(15);
                    Response.Cookies.Add(cookieDateBegin);
                    logLogins = logLogins.Where(p => p.TimesLogin >= d1);
                }
            }
            if (dateEnd != null)
            {
                var d2 = Convert.ToDateTime(dateEnd).Date;
                var dateE = d2.ToShortDateString();
                if (!String.IsNullOrEmpty(dateE) || (!String.IsNullOrEmpty(dateE) && dateE != cookieDateEnd.Value))
                {
                    cookieDateEnd = new HttpCookie("logLogin_dateBegin");
                    cookieDateEnd.Value = dateE;
                    cookieDateEnd.Expires = DateTime.Now.AddMinutes(15);
                    Response.Cookies.Add(cookieDateEnd);
                    logLogins = logLogins.Where(p => p.TimesLogin < d2);
                }
            }

            switch (sortOrder)
            {
                case "login_asc":
                    logLogins = logLogins.OrderBy(s => s.User.Email);
                    break;
                case "login_desc":
                    logLogins = logLogins.OrderByDescending(s => s.User.Email);
                    break;
                case "date_asc":
                    logLogins = logLogins.OrderBy(s => s.TimesLogin);
                    break;                               
                case "lastname_asc":
                    logLogins = logLogins.OrderBy(s => s.User.Lastname);
                    break;
                case "lastname_desc":
                    logLogins = logLogins.OrderByDescending(s => s.User.Lastname);
                    break;
                default:
                    logLogins = logLogins.OrderByDescending(s => s.TimesLogin);
                    break;
            }

            int pageSize = 50;
            int pageNumber = (page ?? 1);
            
            return View(logLogins.ToPagedList(pageNumber, pageSize));
        }

        public ActionResult ClearFilter()
        {
            HttpCookie cookieSortOrder = Request.Cookies.Get("group_sortOrder");
            if (cookieSortOrder != null)
            {
                cookieSortOrder.Value = null;
                cookieSortOrder.Expires = DateTime.Now.AddMinutes(-1);
                Response.Cookies.Add(cookieSortOrder);
            }

            HttpCookie cookieLogin = Request.Cookies.Get("logLogin_login");
            if (cookieLogin != null)
            {
                cookieLogin.Value = null;
                cookieLogin.Expires = DateTime.Now.AddMinutes(-1);
                Response.Cookies.Add(cookieLogin);
            }

            HttpCookie cookieLastname = Request.Cookies.Get("logLogin_lastname");
            if (cookieLastname != null)
            {
                cookieLastname.Value = null;
                cookieLastname.Expires = DateTime.Now.AddMinutes(-1);
                Response.Cookies.Add(cookieLastname);
            }

            HttpCookie cookieDateBegin = Request.Cookies.Get("logLogin_dateBegin");
            if (cookieDateBegin != null)
            {
                cookieDateBegin.Value = null;
                cookieDateBegin.Expires = DateTime.Now.AddMinutes(-1);
                Response.Cookies.Add(cookieDateBegin);
            }

            HttpCookie cookieDateEnd = Request.Cookies.Get("logLogin_dateEnd");
            if (cookieDateEnd != null)
            {
                cookieDateEnd.Value = null;
                cookieDateEnd.Expires = DateTime.Now.AddMinutes(-1);
                Response.Cookies.Add(cookieDateEnd);
            }

            return RedirectToAction("Index");
        }


        // GET: LogLogins/Delete/5
        public async Task<ActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            LogLogins logLogins = await db.LogLogins.FindAsync(id);
            if (logLogins == null)
            {
                return HttpNotFound();
            }
            return View(logLogins);
        }

        // POST: LogLogins/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteConfirmed(int id)
        {
            LogLogins logLogins = await db.LogLogins.FindAsync(id);
            db.LogLogins.Remove(logLogins);
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

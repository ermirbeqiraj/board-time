using DbModel;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using MvcToDo.ModelsView;
using MvcToDo.Persistence;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.UI;

namespace MvcToDo.Controllers
{
    public class HomeController : Controller
    {
        ApplicationUserManager _userManager;

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

        UnitOfWork _repo;
        public HomeController()
        {
            _repo = new UnitOfWork(new ModelContext());
        }

        public ActionResult About()
        {
            ViewBag.Message = "Why is this Web app all about?";

            return View();
        }

        public ActionResult Documentation() 
        {
            return View();
        }

        [Authorize(Roles="admin,projectCRUD,taskCRUD,taskAddFiles,taskDeleteFiles,taskAttachUser")]
        public ActionResult Chat()
        {
            string currentId = User.Identity.GetUserId();
            var item = UserManager.Users.Where(x => x.Id != currentId)
                .Select(x => new { x.UserName, x.FirstName, x.LastName }).AsEnumerable()
                .Select(x => new StringList { Id = x.UserName, Name = x.FirstName + " " + x.LastName }).ToList();

            return View(item);
        }

        /// <summary>
        /// ok then , this is a little weared, but we have to do so.
        /// 2  users can have a lot of messages, so we dont want to fill the "Chat" table with useless data.
        /// reminder : GUID size = 16 bytes, short = 2 bytes
        /// so we save them once and then we use this combination for further messages,
        /// if usr_1 == from then the direction of message is from user 1 to user 2 [OrderedAsc = true]
        /// otherwise the direction is from user 2 to user 1 [OrderedAsc = false]
        /// In this query, we select the ID of the conversation between these two users, so we can use it to insert a new
        /// row in the chat table
        /// </summary>
        /// <param name="to">To user</param>
        /// <param name="message">Message</param>
        /// <returns></returns>
        [Authorize]
        [HttpPost]
        public JsonResult InsertMessage(string to, string message)
        {
            DateTime time = DateTime.Now;
            JsonRes result = new JsonRes { Status = false, Content = time.ToString("dd-MMM-yy HH:mm") };

            try
            {
                string from = User.Identity.Name;

                using (ModelContext _db = new ModelContext())
                {
                    var convItem = _repo.Conversation.GetSingle(x => (x.Usr_1 == from && x.Usr_2 == to) || (x.Usr_1 == to && x.Usr_2 == from));
                    var conversation = new { Id = convItem.Id, Asc = (convItem.Usr_1 == from) };

                    if (conversation == null)
                    {// it is the very first message between them, so lets create the conversation row
                        Conversation c = new Conversation
                        {
                            Usr_1 = from,
                            Usr_2 = to
                        };
                        _repo.Conversation.Add(c);
                        _repo.Persist();
                        // c# is really really cool
                        conversation = new { Id = c.Id, Asc = true };
                    }
                    _repo.Chat.Add(new Chat { ConversationId = conversation.Id, Created = time, Message = message, OrderedAsc = conversation.Asc });
                    _repo.Persist();
                    result.Status = true;
                }
            }
            catch (Exception ex)
            {
                /*
                 * log ex
                 */
                result.Status = false;
                result.Content = ex.Message;
            }

            return Json(result, JsonRequestBehavior.DenyGet);
        }

        [Authorize]
        public JsonResult GetConversation(string partner)
        {

            string me = User.Identity.Name;
            var conversation = _repo.Conversation.GetSingle(x => (x.Usr_1 == me && x.Usr_2 == partner)
                                || (x.Usr_1 == partner && x.Usr_2 == me));
            if (conversation == null)
            {
                return Json("");
            }

            string user1 = conversation.Usr_1;
            string user2 = conversation.Usr_2;

            var items = _repo.Chat.Find(x => x.ConversationId == conversation.Id).OrderBy(x => x.Created)
                .Select(x => new
                {
                    from = (x.OrderedAsc ? user1 : user2),
                    to = (x.OrderedAsc ? user2 : user1),
                    when = x.Created,
                    msg = x.Message
                }).Take(50).AsEnumerable()
                .Select(x => new ConversationView { Created = x.when.ToString("dd-MMM-yy HH:mm"), Sender = x.from, Receiver = x.to, Message = x.msg }).ToList();
            return Json(items, JsonRequestBehavior.AllowGet);
        }

        [Authorize(Roles = "admin")]
        public ActionResult Statistics()
        {
            return View();
        }
        [Authorize(Roles = "admin")]
        public ActionResult TasksPerUser()
        {
            return View();
        }
        
        [Authorize(Roles = "admin")]
        public JsonResult GetTasksPerUser()
        {
            var items = _repo.TaskAssigned.CountTasksPerUser();
            foreach (var item in items)
            {
                item.User = UserManager.Users.Where(x => x.Id == item.User).Select(x => x.FirstName + " " + x.LastName).FirstOrDefault();
            }
            return Json(items, JsonRequestBehavior.AllowGet);
        }

        [Authorize(Roles = "admin")]
        public ActionResult TaskModePerUser()
        {
            return View();
        }

        [Authorize(Roles = "admin")]
        [OutputCache(Duration = 300, Location = OutputCacheLocation.ServerAndClient, VaryByParam = "none")]
        public JsonResult GetTaskMode()
        {
            var items = _repo.TaskMark.Find(x => x.Active == true).Select(x => x.Caption).ToList();
            List<string> results = new List<string>();
            results.Add("User");
            results.AddRange(items);
            return Json(results, JsonRequestBehavior.AllowGet);
        }

        [Authorize(Roles = "admin")]
        [OutputCache(Duration=300 , Location= OutputCacheLocation.ServerAndClient, VaryByParam="none")]
        public JsonResult GetUsers()
        {
            var items = UserManager.Users.Select(u => u.UserName).ToList();
            return Json(items, JsonRequestBehavior.AllowGet);
        }

        [Authorize(Roles = "admin")]
        [OutputCache(Duration = 300, Location = OutputCacheLocation.ServerAndClient, VaryByParam = "none")]
        public JsonResult GetTaskModePerUser()
        {
            var dbitems = _repo.TaskAssigned.CountTaskModePerUser().ToList();
            dbitems.ForEach(u => u.User = UserManager.Users.Where(x => x.Id == u.User).FirstOrDefault().UserName);
            return Json(dbitems, JsonRequestBehavior.AllowGet);
        }

        [Authorize(Roles = "admin")]
        public ActionResult TaskCategoryPerUser()
        {
            return View();
        }

        [Authorize(Roles = "admin")]
        [OutputCache(Duration = 300, Location = OutputCacheLocation.ServerAndClient, VaryByParam = "none")]
        public JsonResult GetTaskCategories()
        {
            var items = _repo.TaskCategory.GetAll().Select(x => x.Caption).ToList();
            List<string> results = new List<string>();
            results.Add("User");
            results.AddRange(items);
            return Json(results, JsonRequestBehavior.AllowGet);
        }

        [Authorize(Roles = "admin")]
        [OutputCache(Duration = 300, Location = OutputCacheLocation.ServerAndClient, VaryByParam = "none")]
        public JsonResult GetTaskCategoryPerUser()
        {
            var db = new ModelContext();
            var dbitems = _repo.TaskAssigned.CountTaskCategoryPerUser().ToList();
            dbitems.ForEach(u => u.User = UserManager.Users.Where(x => x.Id == u.User).FirstOrDefault().UserName);
            return Json(dbitems, JsonRequestBehavior.AllowGet);
        }

        [Authorize(Roles = "admin")]
        public ActionResult TaskMovements()
        {
            var items = ReadDb(0, dbPaginationSize);
            ViewBag.total = items.Count().ToString();
            return View(items);
        }

        private List<TaskMovementsView> ReadDb(int skip, int take)
        {
            var result = _repo.TaskLifecycle.GetTaskMovements(skip, take).ToList();
            result.ForEach(x => x.Actor = UserManager.Users.Where(u => u.Id == x.Actor).FirstOrDefault().UserName);
            return result;
        }

        public const int dbPaginationSize = 50;

        [Authorize(Roles = "admin")]
        public JsonResult GetMovementsJson(int skip)
        {
            var items = new List<TaskMovementsView>();
            if (skip >= dbPaginationSize)
            {
                items = ReadDb(skip, dbPaginationSize);
            }
            return Json(items, JsonRequestBehavior.AllowGet);
        }
    }

    public class JsonRes 
    {
        public bool Status { get; set; }
        public string Content { get; set; }
    }
}
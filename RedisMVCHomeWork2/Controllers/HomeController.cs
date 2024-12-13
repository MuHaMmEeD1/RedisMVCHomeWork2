using System.Diagnostics;
using System.Reflection;
using Microsoft.AspNetCore.Mvc;
using RedisMVCHomeWork2.Models;
using RedisMVCHomeWork2.Statics;
using StackExchange.Redis;

namespace RedisMVCHomeWork2.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly ConnectionMultiplexer _muxer;
        private readonly string MyListName = "RPubSub";
        private static string SelectedCName = "";
        private static List<string> OldMessages = new();

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;

          
            var muxer = ConnectionMultiplexer.Connect(
                    new ConfigurationOptions
                    {
                        EndPoints = { { "Your - EndPoints", 16071 } },
                        User = "Your - User",
                        Password = "Your - Password"
                    }
                );



            _muxer = muxer;

            
        }

        public IActionResult Index()
        {
            var db = _muxer.GetDatabase();

            RedisValue[]? list = db.ListRange(MyListName, 0, -1);

            RedisElementsViewModel model;

            if (list.Length == 0)
            {
                model = new RedisElementsViewModel();
                return View(model);
            }


            model = new RedisElementsViewModel()
            {
                RedisValues = list,
                Messages = OldMessages
            };

            return View(model);
        }

        [HttpPost]
        public IActionResult AddCName(string newCName)
        {
           

            if (newCName != "" && newCName != null)
            {
                var db = _muxer.GetDatabase();

                db.ListRightPush(MyListName, newCName);

                Console.WriteLine($"Add C Name Ok {newCName}");
            }


            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public IActionResult SelectCName(string cName)
        {
            if(SelectedCName != cName)
            {
                SelectedCName = cName;
                OldMessages = new();
            }
            
            Console.WriteLine($"Select CName = {cName} Ok");


            return RedirectToAction(nameof(Index));
        }


        [HttpPost]
        public IActionResult SendMessage(string message)
        {

            if(message != ""       && message != null &&
               SelectedCName != "" && SelectedCName != null
                )
            {

                var sub = _muxer.GetSubscriber();
                sub.Publish(SelectedCName, message);

                var db = _muxer.GetDatabase();

                RedisValue[]? list = db.ListRange(MyListName, 0, -1);

                OldMessages.Add(message);

                RedisElementsViewModel model = new RedisElementsViewModel()
                {
                    RedisValues = list,
                    Messages = OldMessages
                };

                return View("Index", model);
            }


            return RedirectToAction(nameof(Index));
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}

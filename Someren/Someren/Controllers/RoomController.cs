using Microsoft.AspNetCore.Mvc;
using Someren.Repositories;

namespace Someren.Controllers
{
    public class RoomController : Controller
    {
        private IRoomRepository _roomRepository;
        public RoomController(IRoomRepository roomsRepositories)
        {
            _roomRepository = roomsRepositories;
        }
        public IActionResult Index()
        {
            List<Models.Room> rooms = _roomRepository.GetAll();
            return View(rooms);
        }
    }
}

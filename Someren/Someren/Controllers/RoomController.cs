using Microsoft.AspNetCore.Mvc;
using Someren.Models;
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

        //Home
        public IActionResult Index()
        {
            List<Models.Room> rooms = _roomRepository.GetAll();
            return View(rooms);
        }

        // Add
        [HttpGet]
        public IActionResult Add()
        {
            return View();
        }
        [HttpPost]
        public IActionResult Add(Room room)
        {
            string errorMessage;
            bool success = _roomRepository.Add(room, out errorMessage);

            if (!success)
            {
                ViewBag.ErrorMessage = errorMessage;
                return View(room);
            }

            return RedirectToAction("Index");
        }

        // Edit
        [HttpGet("Room/Edit/{roomNumber}")]
        public IActionResult Edit(string roomNumber)
        {
            Room? room = _roomRepository.GetById(roomNumber);
            return View(room);
        }

        [HttpPost("Room/Edit")]
        public IActionResult Edit(Room room)
        {
            string errorMessage;
            bool success = _roomRepository.Update(room, out errorMessage);

            if (!success)
            {
                ViewBag.ErrorMessage = errorMessage;
                return View(room);
            }

            return RedirectToAction("Index");
        }

        //Delete
        [HttpGet("Room/Delete/{roomNumber}")]
        public IActionResult Delete(string roomNumber)
        {
            Room? room = _roomRepository.GetById(roomNumber);
            return View(room);
        }

        [HttpPost("Room/Delete")]
        public IActionResult Delete(Room room)
        {
            _roomRepository.Delete(room.RoomNumber);
            return RedirectToAction("Index");
        }
    }
}

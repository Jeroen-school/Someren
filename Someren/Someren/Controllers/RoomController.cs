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
        public IActionResult Index(int? size)
        {
            List<Room> rooms = _roomRepository.GetAll(size);



            // Pass available sizes to the dropdown
            ViewBag.Sizes = _roomRepository.GetAll(size).Select(r => r.Size).Distinct().ToList();
            ViewBag.SelectedSize = size;
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
        [HttpGet("Room/Edit/{RoomId}")]
        public IActionResult Edit(int roomId)
        {
            Room? room = _roomRepository.GetById(roomId);
            return View(room);
        }

        [HttpPost("Room/Edit/{RoomId}")]
        public IActionResult Edit(Room room)
        {
            Console.WriteLine($"{room.RoomId}");
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
        [HttpGet("Room/Delete/{roomId}")]
        public IActionResult Delete(int roomId)
        {
            Room? room = _roomRepository.GetById(roomId);
            return View(room);
        }

        [HttpPost("Room/Delete")]
        public IActionResult Delete(Room room)
        {
            _roomRepository.Delete(room.RoomId);
            return RedirectToAction("Index");
        }
    }
}

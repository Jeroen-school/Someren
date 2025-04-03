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
            List<Room> rooms = _roomRepository.GetAll(false);

            if(size.HasValue)
            {
                rooms = _roomRepository.GetBySize(size.Value);
            }

            // Pass available sizes to the dropdown
            ViewBag.Sizes = _roomRepository.GetAll(false).Select(r => r.Size).Distinct().ToList();
            ViewBag.SelectedSize = size;
            return View(rooms);
        }

        // Details
        [HttpGet("Room/Detail/{roomId}")]
        public IActionResult Detail(int roomId)
        {
            var viewModel = new RoomDetail
            {
                StudentsInRoom = _roomRepository.GetStudentInRoomByRoomId(roomId),
                StudentsNotInRoom = _roomRepository.GetStudentNotInRoomByRoomId(roomId),
                Room = _roomRepository.GetById(roomId, false)
            };

            return View(viewModel);
        }

        [HttpPost("Room/AssignStudent")]
        public IActionResult AssignStudent(int studentId, int roomId)
        {
            _roomRepository.AssignStudentToRoom(studentId, roomId);
            return RedirectToAction("Detail", new { roomId });
        }
        [HttpPost("Room/RemoveStudent")]
        public IActionResult RemoveStudent(int studentId, int roomId)
        {
            _roomRepository.RemoveStudentToRoom(studentId, roomId);
            return RedirectToAction("Detail", new { roomId });
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
            Room? room = _roomRepository.GetById(roomId, false);
            return View(room);
        }

        [HttpPost("Room/Edit")]
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

        //Soft Delete
        [HttpGet("Room/Delete/{roomId}")]
        public IActionResult Delete(int roomId)
        {
            Room? room = _roomRepository.GetById(roomId, false);
            return View(room);
        }

        [HttpPost("Room/Delete")]
        public IActionResult Delete(Room room)
        {
            _roomRepository.SoftDelete(room.RoomId);
            return RedirectToAction("Index");
        }

        //Hard Delete
        [HttpGet]
        public IActionResult ListDeleted()
        {
            List<Room> rooms = _roomRepository.GetAll(true);

            return View(rooms);
        }

        [HttpGet("Room/Restore/{roomId}")]
        public IActionResult Restore(int roomId)
        {
            Room? room = _roomRepository.GetById(roomId, true);
            return View(room);
        }

        [HttpPost("Room/Restore/{roomId}")]
        public IActionResult RestoreRoom(int roomId)
        {
            _roomRepository.Restore(roomId);
            return RedirectToAction("ListDeleted");
        }

        [HttpGet("Room/Erase/{roomId}")]
        public IActionResult Erase(int roomId)
        {
            Room? room = _roomRepository.GetById(roomId, true);
            return View(room);
        }

        [HttpPost("Room/Erase/{roomId}")]
        public IActionResult EraseRoom(int roomId)
        {
            _roomRepository.Erase(roomId);
            return RedirectToAction("ListDeleted");
        }
    }
}

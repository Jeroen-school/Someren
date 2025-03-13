using Microsoft.AspNetCore.Mvc;
using Someren.Models;
using Someren.Repositories;


namespace Someren.Controllers
{
    public class StudentController : Controller
    {
        private readonly IStudentsRepository _studentsRepository;

        public StudentController(IStudentsRepository studentsRepository) // Injects into class
        {
            _studentsRepository = studentsRepository;
        }

        public IActionResult Index()
        {
            List<Student> students = _studentsRepository.GetAll();
            return View(students);
        }
    }
}

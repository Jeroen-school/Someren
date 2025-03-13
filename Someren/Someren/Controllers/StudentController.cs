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


        [HttpGet]
        public IActionResult Add()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Add(Student student)
        {
            try
            {
                _studentsRepository.Add(student);
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                return View(student);
            }
        }
        [HttpGet]
        public IActionResult Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            Student? student = _studentsRepository.GetByNum((int)id);
            return View(student);
        }

        [HttpPost]
        public IActionResult Edit(Student student)
        {
            try
            {
                _studentsRepository.Update(student);

                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                return View(student);
            }
        }
        [HttpGet]
        public IActionResult Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            Student? student = _studentsRepository.GetByNum((int)id);
            return View(student);
        }

        [HttpPost]
        public IActionResult Delete(Student student)
        {
            try
            {
                _studentsRepository.Delete(student);
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {

                return View(student);
            }
        }
    }
}

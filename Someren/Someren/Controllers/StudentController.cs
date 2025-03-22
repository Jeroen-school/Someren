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
                TempData["SuccessMessage"] = "Student added successfully!";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Error adding student: {ex.Message}";
                return View(student);
            }
        }
        [HttpGet]
        public IActionResult Edit(int? id)
        {
            if (id == null)
            {
                TempData["ErrorMessage"] = "No student ID provided.";
                return RedirectToAction("Index");
            }

            Student? student = _studentsRepository.GetByNum((int)id);

            if (student == null)
            {
                TempData["ErrorMessage"] = $"Student with ID {id} not found.";
                return RedirectToAction("Index");
            }

            return View(student);
        }
        [HttpPost]
        public IActionResult Edit(Student student)
        {
            try
            {
                _studentsRepository.Update(student);
                TempData["SuccessMessage"] = $"Student {student.FirstName} {student.LastName} updated successfully!";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Error updating student: {ex.Message}";
                return View(student);
            }
        }
        [HttpGet]
        public IActionResult Delete(int? id)
        {
            if (id == null)
            {
                TempData["ErrorMessage"] = "No student ID provided.";
                return RedirectToAction("Index");
            }

            Student? student = _studentsRepository.GetByNum((int)id);

            if (student == null)
            {
                TempData["ErrorMessage"] = $"Student with ID {id} not found.";
                return RedirectToAction("Index");
            }

            return View(student);
        }
        [HttpPost]
        public IActionResult Delete(Student student)
        {
            try
            {
                _studentsRepository.Delete(student);
                TempData["SuccessMessage"] = $"Student {student.FirstName} {student.LastName} moved to bin.";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Error deleting student: {ex.Message}";
                return View(student);
            }
        }
        // GET: /DeletedStudents
        public IActionResult Bin()
        {
            try
            {
                var deletedStudents = _studentsRepository.GetDeleted();
                return View(deletedStudents);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Error retrieving deleted students: {ex.Message}";
                return RedirectToAction("Index");
            }
        }
        // POST: /DeletedStudents/Restore/{id}
        [HttpPost]
        public IActionResult Restore(int id)
        {
            try
            {
                _studentsRepository.Restore(id);
                TempData["SuccessMessage"] = $"Student #{id} has been restored successfully.";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                // Log the exception
                TempData["ErrorMessage"] = ex.Message;
                return RedirectToAction("Bin");
            }
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult PermaDel(int id)
        {
            try
            {
                _studentsRepository.PermaDel(id);
                TempData["SuccessMessage"] = $"Student #{id} has been permanently deleted.";
                return RedirectToAction("Bin");
            }
            catch (Exception ex)
            {
                // Log the exception
                TempData["ErrorMessage"] = ex.Message;
                return RedirectToAction("Bin");
            }
        }
    }
}
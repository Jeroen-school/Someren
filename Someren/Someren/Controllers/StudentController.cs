using Microsoft.AspNetCore.Mvc;
using Someren.Models;
using Someren.Repositories;
using System;
using System.Collections.Generic;

namespace Someren.Controllers
{
    public class StudentController : Controller
    {
        private readonly IStudentsRepository _studentsRepository;
        private readonly IRoomRepository _roomRepository;

        public StudentController(IStudentsRepository studentsRepository, IRoomRepository roomRepository)
        {
            _studentsRepository = studentsRepository;
            _roomRepository = roomRepository;
        }

        // GET: Student/Index
        public IActionResult Index(string lastName)
        {
            var students = string.IsNullOrEmpty(lastName)
                ? _studentsRepository.GetAll()
                : _studentsRepository.GetFiltered(lastName);

            ViewBag.LastNameFilter = lastName;

            return View(students);
        }

        // GET: Student/Add
        public IActionResult Add()
        {
            // Get all available rooms
            ViewBag.Rooms = _roomRepository.GetAll(false);

            return View();
        }

        // POST: Student/Add
        [HttpPost]
        public IActionResult Add(Student student)
        {
            try
            {
                // This will check for duplicate student numbers
                _studentsRepository.Add(student);

                TempData["SuccessMessage"] = $"Student {student.FirstName} {student.LastName} was successfully added.";
                return RedirectToAction("Index");
            }
            catch (InvalidOperationException ex) when (ex.Message.Contains("already exists"))
            {
                // Add the error to ModelState for the specific field
                ModelState.AddModelError("StudentNum", ex.Message);
                 
                // Get rooms again for the view
                ViewBag.Rooms = _roomRepository.GetAll(false);

                // Return to the form with the student data
                return View(student);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Error adding student: {ex.Message}";
                return RedirectToAction("Index");
            }
        }

        // GET: Student/Edit/{id}
        public IActionResult Edit(int id)
        {
            var student = _studentsRepository.GetByNum(id);
            if (student == null)
            {
                TempData["ErrorMessage"] = $"Student with number {id} not found.";
                return RedirectToAction("Index");
            }

            // Get all available rooms
            ViewBag.Rooms = _roomRepository.GetAll(false);

            return View(student);
        }

        // POST: Student/Edit
        [HttpPost]
        public IActionResult Edit(Student student)
        {
            try
            {
                _studentsRepository.Update(student);
                TempData["SuccessMessage"] = $"Student {student.FirstName} {student.LastName} was successfully updated.";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Error updating student: {ex.Message}";

                // Get rooms again for the view
                ViewBag.Rooms = _roomRepository.GetAll(false);

                return View(student);
            }
        }

        // GET: Student/Delete/{id}
        public IActionResult Delete(int id)
        {
            var student = _studentsRepository.GetByNum(id);
            if (student == null)
            {
                TempData["ErrorMessage"] = $"Student with number {id} not found.";
                return RedirectToAction("Index");
            }

            // Pass the student to the confirmation view
            return View(student);
        }

        // POST: Student/Delete
        [HttpPost]
        public IActionResult Delete(int id, bool confirmed)
        {
            // Only proceed if the user confirmed the deletion
            if (!confirmed)
            {
                try
                {
                    var student = _studentsRepository.GetByNum(id);
                    if (student == null)
                    {
                        TempData["ErrorMessage"] = $"Student with number {id} not found.";
                        return RedirectToAction("Index");
                    }

                    _studentsRepository.Delete(student);

                    TempData["SuccessMessage"] = $"Student {student.FirstName} {student.LastName} was successfully deleted.";
                    return RedirectToAction("Index");
                }
                catch (Exception ex)
                {
                    TempData["ErrorMessage"] = $"Error deleting student: {ex.Message}";
                    return RedirectToAction("Index");
                }
            }
            else
            {
                return View();
            }
        }

        // GET: Student/Bin
        public IActionResult Bin(string lastName)
        {
            var deletedStudents = string.IsNullOrEmpty(lastName)
                ? _studentsRepository.GetDeleted()
                : _studentsRepository.GetFilteredDeleted(lastName);

            ViewBag.LastNameFilter = lastName;

            return View(deletedStudents);
        }

        // POST: Student/Restore
        [HttpPost]
        public IActionResult Restore(int id)
        {
            try
            {
                _studentsRepository.Restore(id);
                TempData["SuccessMessage"] = "Student was successfully restored.";
                return RedirectToAction("Bin");
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Error restoring student: {ex.Message}";
                return RedirectToAction("Bin");
            }
        }

        // POST: Student/PermaDel
        [HttpPost]
        public IActionResult PermaDel(int id)
        {
            try
            {
                _studentsRepository.PermaDel(id);
                TempData["SuccessMessage"] = "Student was permanently deleted.";
                return RedirectToAction("Bin");
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Error permanently deleting student: {ex.Message}";
                return RedirectToAction("Bin");
            }
        }
    }
}
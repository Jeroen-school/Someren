using Microsoft.AspNetCore.Mvc;
using Someren.Repositories;
using Someren.Models;

namespace Someren.Controllers
{
    public class LecturersController : Controller
    {
        
        private readonly ILecturersRepository _lecturersRepository;

        public LecturersController(ILecturersRepository lecturersRepository)
        {
            _lecturersRepository = lecturersRepository;
        }

        public IActionResult Index()
        {
            List<Lecturer> lecturers = _lecturersRepository.GetAll();

            return View(lecturers);
        }

        //When opening up the create lecturer page
        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        //when you have filled in the page to create a lecturer
        [HttpPost]
        public IActionResult Create(IFormCollection form)
        {
            // Create a new lecturer from form data
            var lecturer = new Lecturer();
            lecturer.LecturerId = int.Parse(form["LecturerId"]);
            lecturer.RoomNumber = form["RoomNumber"];
            lecturer.FirstName = form["FirstName"];
            lecturer.LastName = form["LastName"];
            lecturer.PhoneNumber = form["PhoneNumber"];
            lecturer.Age = int.Parse(form["Age"]);

            // Explicitly convert the BarDuty value from string to bool
            lecturer.BarDuty = form["BarDuty"].ToString().ToLower() == "true";

            // Debug check
            TempData["Debug"] = $"BarDuty in form: {form["BarDuty"]}, Converted to: {lecturer.BarDuty}";

            try
            {
                _lecturersRepository.Add(lecturer);
                TempData["SuccessMessage"] = "Lecturer added successfully!";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = ex.Message;
                return View(lecturer);
            }
        }

        //When you open up the page to update the data of a lecturer (through the index page)
        [HttpGet]
        public IActionResult Update(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }
            
            Lecturer? lecturer = _lecturersRepository.GetById((int)id);

            return View(lecturer);
        }

        //Once you have filled in the form to make changes
        [HttpPost]
        public IActionResult Update(IFormCollection form)
        {
            // Create a lecturer object from form data
            var lecturer = new Lecturer();
            lecturer.LecturerId = int.Parse(form["LecturerId"]);
            lecturer.RoomNumber = form["RoomNumber"];
            lecturer.FirstName = form["FirstName"];
            lecturer.LastName = form["LastName"];
            lecturer.PhoneNumber = form["PhoneNumber"];
            lecturer.Age = int.Parse(form["Age"]);

            // Explicitly convert the BarDuty value from string to bool
            lecturer.BarDuty = form["BarDuty"].ToString().ToLower() == "true";

            try
            {
                _lecturersRepository.Update(lecturer);
                TempData["SuccessMessage"] = "Lecturer updated successfully!";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = ex.Message;
                return View(lecturer);
            }
        }

        //When you open up the page to update the data of a lecturer (through the index page)
        [HttpGet]
        public IActionResult Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            Lecturer? lecturer = _lecturersRepository.GetById((int)id);

            return View(lecturer);
        }

        //Once you have confirmed your action
        [HttpPost]
        public IActionResult Delete(Lecturer lecturer)
        {
            try
            {
                _lecturersRepository.Delete(lecturer);
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                return View(lecturer);
            }
        }




    }
}

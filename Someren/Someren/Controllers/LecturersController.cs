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

        [HttpGet]
        public IActionResult Index()
        {
            List<Lecturer> lecturers = _lecturersRepository.GetAll();

            return View(lecturers);
        }

        [HttpPost]
        public IActionResult Index(string lastName)
        {
            try
            {
                List<Lecturer> lecturers = _lecturersRepository.GetFiltered(lastName);
                
                return View(lecturers);
            }
            catch (Exception ex)
            {
                return View(ex);
            }
        }

        //When opening up the create lecturer page
        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        //when you have filled in the page to create a lecturer
        [HttpPost]
        public IActionResult Create(Lecturer lecturer)
        {
            
            try
            {
                _lecturersRepository.Add(lecturer);

                TempData["SuccessMessage"] = "Lecturer added!";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Error adding lecturer: {ex.Message}";
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
        public IActionResult Update(Lecturer lecturer)
        {
            try
            {
                _lecturersRepository.Update(lecturer);

                TempData["SuccessMessage"] = "Lecturer edited!";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Error editing lecturer: {ex.Message}";
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

                TempData["SuccessMessage"] = "Lecturer deleted!";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Error deleting lecturer: {ex.Message}";
                return View(lecturer);
            }
        }




    }
}

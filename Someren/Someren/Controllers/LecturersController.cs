using Microsoft.AspNetCore.Mvc;
using Someren.Repositories;
using Someren.Models;
using System.Linq.Expressions;
using System.Data.SqlTypes;

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
            List<Lecturer> lecturers = _lecturersRepository.GetAll(false); //The parameter false is here so it retrieves the not deleted records

            return View(lecturers);
        }

        [HttpPost]
        public IActionResult Index(string lastName)
        {
                List<Lecturer> lecturers = _lecturersRepository.GetFiltered(lastName, false); //The parameter false is here so it retrieves the not deleted records

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
            try 
            {
                if (id == null)
                {
                    throw new Exception ($"No lecturer found, please try again.");
                }

                Lecturer? lecturer = _lecturersRepository.GetById((int)id);

                return View(lecturer);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Error: {ex.Message}";

                return RedirectToAction("Index");
            }
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
            try
            {
                if (id == null)
                {
                    throw new Exception($"No lecturer found, please try again.");
                }

                Lecturer? lecturer = _lecturersRepository.GetById((int)id);

                return View(lecturer);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Error: {ex.Message}";

                return RedirectToAction("Index");
            }
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


        [HttpGet]
        public IActionResult ListDeleted()
        {
            List<Lecturer> lecturers = _lecturersRepository.GetAll(true);

            return View(lecturers);
        }

        [HttpPost]
        public IActionResult ListDeleted(string lastName)
        {
                List<Lecturer> lecturers = _lecturersRepository.GetFiltered(lastName, true);

            return View(lecturers);
        }

        //When you want to undelete (restore) a deleted lecturer
        [HttpGet]
        public IActionResult Restore(int? id)
        {
            try
            {
                if (id == null)
                {
                    throw new Exception("No lecturer found, please try again.");
                }

                Lecturer? lecturer = _lecturersRepository.GetById((int)id);

                return View(lecturer);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Error: {ex.Message}";

                return RedirectToAction("ListDeleted");
            }
        }

        //Once you have confirmed your action to restore a lecturer
        [HttpPost]
        public IActionResult Restore(Lecturer lecturer)
        {
            try
            {
                _lecturersRepository.Restore(lecturer);

                TempData["SuccessMessage"] = "Lecturer deleted!";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Error deleting lecturer: {ex.Message}";
                return View(lecturer);
            }
        }

        //When you want to permanently delete a lecturer, this shows you an overview of the lecturer's data
        [HttpGet]
        public IActionResult Erase(int? id)
        {
            try
            {
                if (id == null)
                {
                    throw new Exception("No lecturer found, please try again.");
                }

                Lecturer? lecturer = _lecturersRepository.GetById((int)id);

                return View(lecturer);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Error: {ex.Message}";

                return RedirectToAction("ListDeleted");
            }
        }

        //Once you have confirmed your action to send the lecturer to the shadow realm
        [HttpPost]
        public IActionResult Erase(Lecturer lecturer)
        {
            try
            {
                _lecturersRepository.Erase(lecturer);

                TempData["SuccessMessage"] = "Lecturer permanently deleted!";
                return RedirectToAction("ListDeleted");
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Error permanently deleting lecturer: {ex.Message}";
                return View(lecturer);
            }
        }


    }
}

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
        public IActionResult Create(Lecturer lecturer)
        {
            try
            {
                _lecturersRepository.Add(lecturer);

                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                return View(lecturer);
            }
        }

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

        [HttpPost]
        public IActionResult Update(Lecturer lecturer)
        {
            try
            {
                _lecturersRepository.Update(lecturer);
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                return View(lecturer);
            }
        }




    }
}

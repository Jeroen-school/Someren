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
        public ActionResult Create()
        {
            return View();
        }

        //when you have filled in the page to create a lecturer
        [HttpPost]
        public ActionResult Create(Lecturer lecturer)
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



    }
}

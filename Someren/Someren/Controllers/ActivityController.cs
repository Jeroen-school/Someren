using Microsoft.AspNetCore.Mvc;
using Someren.Models;
using Someren.Repositories;

namespace Someren.Controllers
{
    public class ActivityController : Controller
    {
        private IDbActivityRepository _activityRepository;
        private IActivitySupervisersRepository _activitySupervisersRepository;

        public ActivityController(IDbActivityRepository activityRepository, IActivitySupervisersRepository activitySupervisersRepository)
        {
            _activityRepository = activityRepository;
            _activitySupervisersRepository = activitySupervisersRepository;
        }

        public IActionResult Index(string searchString)
        {
            ViewData["CurrentFilter"] = searchString;

            List<Activity> activities;

            if (!string.IsNullOrEmpty(searchString))
            {
                activities = _activityRepository.FilterActivitiesByName(searchString);
            }
            else
            {
                activities = _activityRepository.ViewAllActivities();
            }

            return View(activities);
        }
        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Create(Activity activity)
        {
            if (activity.Date < new DateTime(2018, 1, 1) || activity.Date > new DateTime(9999, 12, 31))
            {
                ModelState.AddModelError("Date", "The date must be between January 1, 2018 and December 31, 9999.");
            }

            if (_activityRepository.ActivityExists(activity.Activitytype))
            {
                ModelState.AddModelError("Activitytype", "An activity with this name already exists.");
            }

            if (!ModelState.IsValid)
            {
                return View(activity);
            }

            _activityRepository.AddActivity(activity);
            return RedirectToAction(nameof(Index));
        }
        [HttpGet]
        public IActionResult Edit(string id)
        {
            Activity activity = _activityRepository.GetActivityByType(id);
            if (activity == null)
            {
                return NotFound();
            }

            ViewBag.OriginalType = id;
            return View(activity);
        }

        [HttpPost]
        public IActionResult Edit(Activity activity, string originalType)
        {

            if (activity.Activitytype != originalType &&
                _activityRepository.ActivityExists(activity.Activitytype))
            {
                ModelState.AddModelError("Activitytype", "An activity with this name already exists.");
                ViewBag.OriginalType = originalType;
                return View(activity);
            }
            _activityRepository.UpdateActivity(activity, originalType);
            return RedirectToAction(nameof(Index));
        }
        [HttpGet]
        public IActionResult Delete(string id)
        {
            Activity activity = _activityRepository.GetActivityByType(id);
            if (activity == null)
            {
                return NotFound();
            }
            return View(activity);
        }

        [HttpPost, ActionName("Delete")]
        public IActionResult DeleteConfirmed(string id)
        {
            _activityRepository.DeleteActivity(id);
            return RedirectToAction(nameof(Index));
        }


        [HttpGet]
        public IActionResult Supervises(int? id)
        {
            try
            {
                if (id == null)
                {
                    throw new Exception($"No activity found, please try again.");
                }

                ActivitySupervisorsViewModel viewModel = new ActivitySupervisorsViewModel(_activityRepository.GetById((int)id), _activitySupervisersRepository.GetAllSupervising((int)id), _activitySupervisersRepository.GetAllOther((int)id));

                return View(viewModel);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Error: {ex.Message}";

                return RedirectToAction("Index");
            }
        }

        [HttpGet]
        public IActionResult AddSupervisors(int? lecturerID, int? activityID)
        {
            try
            {
                if (lecturerID == null || activityID == null)
                {
                    throw new Exception($"No activity or lecturer found, please try again.");
                }

                string fullname = _activitySupervisersRepository.AddSupervising((int)lecturerID, (int)activityID);
                
                
                TempData["SuccessMessage"] = $"Lecturer {fullname} added as supervisor!";

                return RedirectToAction("Supervises", new { id = activityID });
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Error: {ex.Message}";

                return RedirectToAction("Supervises", new { id = activityID });
            }
        }

        [HttpGet]
        public IActionResult RemoveSupervisors(int? lecturerID, int? activityID)
        {
            try
            {
                if (lecturerID == null || activityID == null)
                {
                    throw new Exception($"No activity or lecturer found, please try again.");
                }

                string fullname = _activitySupervisersRepository.RemoveSupervising((int)lecturerID, (int)activityID);
                
                
                TempData["SuccessMessage"] = $"Lecturer {fullname} removed from supervisors!";

                return RedirectToAction("Supervises", new { id = activityID });
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Error: {ex.Message}";

                return RedirectToAction("Supervises", new { id = activityID});
            }
        }

        
    }
}
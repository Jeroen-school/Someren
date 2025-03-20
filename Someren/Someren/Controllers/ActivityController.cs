using Microsoft.AspNetCore.Mvc;
using Someren.Models;
using Someren.Repositories;

namespace Someren.Controllers
{
    public class ActivityController : Controller
    {
        private IDbActivityRepository _activityRepository;

        public ActivityController(IDbActivityRepository activityRepository)
        {
            _activityRepository = activityRepository;
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

            if (_activityRepository.ActivityExists(activity.Activitytype))
            {

                ModelState.AddModelError("Activitytype", "An activity with this name already exists.");
                return View(activity);
            }
            _activityRepository.AddActivity(activity);
            return RedirectToAction(nameof(Index));
        }
        [HttpGet]
        public IActionResult Update(string id)
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
        public IActionResult Update(Activity activity, string originalType)
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
        
    }
}
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

        public IActionResult Index()
        {
            List<Activity> activities = _activityRepository.ViewAllActivities();
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

            // Store the original activity type in ViewBag for later use
            ViewBag.OriginalType = id;
            return View(activity);
        }

        [HttpPost]
        public IActionResult Edit(Activity activity, string originalType)
        {
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
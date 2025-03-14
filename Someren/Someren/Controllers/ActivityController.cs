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
    }
}

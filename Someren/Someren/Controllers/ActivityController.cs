using Microsoft.AspNetCore.Mvc;
using Someren.Models;
using Someren.Repositories;

namespace Someren.Controllers
{
    public class ActivityController : Controller
    {
        private IDbActivityRepository _activityRepository;
        private IActivityParticipantRepository _activityParticipantRepository;
        private IStudentsRepository _studentRepository;
        private IActivitySupervisersRepository _activitySupervisersRepository;

        public ActivityController(IDbActivityRepository activityRepository, IActivityParticipantRepository activityParticipantRepository, IStudentsRepository studentRepository, IActivitySupervisersRepository activitySupervisersRepository)
        {
            _activityRepository = activityRepository;
            _activityParticipantRepository = activityParticipantRepository;
            _studentRepository = studentRepository;
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
            try
            {
                _activityRepository.DeleteActivity(id);
                TempData["SuccessMessage"] = "Activity was successfully moved to trash.";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Error deleting activity: {ex.Message}";
            }
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public IActionResult ListDeleted()
        {
            List<Activity> activities = _activityRepository.GetDeletedActivities();
            return View(activities);
        }

        [HttpGet]
        public IActionResult Restore(string id)
        {
            var activity = _activityRepository.GetActivityByType(id, includeDeleted: true);
            if (activity == null)
            {
                TempData["ErrorMessage"] = "Activity not found.";
                return RedirectToAction("ListDeleted");
            }
            return View(activity);
        }

        [HttpPost]
        public IActionResult Restore(string id, bool confirmed)
        {
            try
            {
                _activityRepository.RestoreActivity(id);
                TempData["SuccessMessage"] = "Activity was successfully restored.";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Error restoring activity: {ex.Message}";
            }
            return RedirectToAction("ListDeleted");
        }

        [HttpGet]
        public IActionResult Erase(string id)
        {
            var activity = _activityRepository.GetActivityByType(id, includeDeleted: true);
            if (activity == null)
            {
                TempData["ErrorMessage"] = "Activity not found.";
                return RedirectToAction("ListDeleted");
            }
            return View(activity);
        }

        [HttpPost]
        public IActionResult Erase(string id, bool confirmed)
        {
            try
            {
                _activityRepository.EraseActivity(id);
                TempData["SuccessMessage"] = "Activity was permanently deleted.";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Error permanently deleting activity: {ex.Message}";
            }
            return RedirectToAction("ListDeleted");
        }

        [HttpGet]
        public IActionResult Participants(string id)
        {
            Activity activity = _activityRepository.GetActivityByType(id);
            if (activity == null)
            {
                return NotFound();
            }

            // Get participants and non-participants
            List<Student> participants = _activityParticipantRepository.GetParticipants(id);
            List<Student> nonParticipants = _activityParticipantRepository.GetNonParticipants(id);

            // Store data in ViewBag
            ViewBag.Activity = activity;
            ViewBag.Participants = participants;
            ViewBag.NonParticipants = nonParticipants;

            return View();
        }

        [HttpPost]
        public IActionResult AddParticipant(string activityType, int studentNum)
        {
            try
            {
                _activityParticipantRepository.AddParticipant(activityType, studentNum);
                TempData["SuccessMessage"] = "Student was successfully added to the activity.";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Error adding student to activity: {ex.Message}";
            }
            return RedirectToAction("Participants", new { id = activityType });
        }

        [HttpPost]
        public IActionResult RemoveParticipant(string activityType, int studentNum)
        {
            try
            {
                _activityParticipantRepository.RemoveParticipant(activityType, studentNum);
                TempData["SuccessMessage"] = "Student was successfully removed from the activity.";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Error removing student from activity: {ex.Message}";
            }
            return RedirectToAction("Participants", new { id = activityType });
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

                return RedirectToAction("Supervises", new { id = activityID });
            }
        }

    }
}
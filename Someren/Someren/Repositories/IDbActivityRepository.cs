namespace Someren.Repositories
{
    public interface IDbActivityRepository
    {

        List<Models.Activity> ViewAllActivities();

        Models.Activity GetById(int id);
        Models.Activity GetActivityByType(string activityType, bool includeDeleted = false);
        void AddActivity(Models.Activity activity);
        void UpdateActivity(Models.Activity activity, string originalType);
        void DeleteActivity(string activityType);
        bool ActivityExists(string activityType);
        List<Models.Activity> FilterActivitiesByName(string searchstring);
        List<Models.Activity> GetDeletedActivities();
        void RestoreActivity(string activityType);
        void EraseActivity(string activityType);


    }
}

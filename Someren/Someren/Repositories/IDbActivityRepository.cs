namespace Someren.Repositories
{
    public interface IDbActivityRepository
    {
        
            List<Models.Activity> ViewAllActivities();
            Models.Activity GetActivityByType(string activityType);
            void AddActivity(Models.Activity activity);
            void UpdateActivity(Models.Activity activity, string originalType);
            void DeleteActivity(string activityType);
            bool ActivityExists(string activityType);



    }
}

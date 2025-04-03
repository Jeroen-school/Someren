using Someren.Models;

namespace Someren.Repositories
{
    public interface IActivitySupervisersRepository
    {
        //fields and properties

        //constructors

        //methods
        List<Lecturer> GetAllSupervising(int activityID);

        List<Lecturer> GetAllOther(int activityID);
    }
}

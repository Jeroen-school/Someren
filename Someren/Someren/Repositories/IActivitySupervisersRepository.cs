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

        void AddSupervising(int lecturerID, int activityID);

        void RemoveSupervising(int lecturerID, int activityID);
    }
}

using Someren.Models;

namespace Someren.Repositories
{
    public interface IActivityParticipantRepository
    {
        List<Student> GetParticipants(string activitytype);
        List<Student> GetNonParticipants(string activitytype);
        void AddParticipant(string activitytype, int studentNum);
        void RemoveParticipant(string activitytype, int studentNum);
    }
}
using Someren.Models;

namespace Someren.Repositories
{
    public interface ILecturersRepository
    {
        //fields and properties

        //constructors

        //methods
        List<Lecturer> GetAll(bool deleted);

        List<Lecturer> GetFiltered(string lastName, bool deleted);

        Lecturer? GetById(int userId);




        void Add(Lecturer lecturer);

        void Update(Lecturer lecturer);

        void Delete(Lecturer lecturer);

        void Restore(Lecturer lecturer);

        void Erase(Lecturer lecturer);
    }
}

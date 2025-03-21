using Someren.Models;

namespace Someren.Repositories
{
    public interface ILecturersRepository
    {
        //fields and properties

        //constructors

        //methods
        List<Lecturer> GetAll();

        List<Lecturer> GetAllDeleted();

        List<Lecturer> GetFiltered(string lastName);

        List<Lecturer> GetFilteredDeleted(string lastName);

        Lecturer? GetById(int userId);




        void Add(Lecturer lecturer);

        void Update(Lecturer lecturer);

        void Delete(Lecturer lecturer);
    }
}

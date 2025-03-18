using Someren.Models;

namespace Someren.Repositories
{
    public interface ILecturersRepository
    {
        //fields and properties

        //constructors

        //methods
        List<Lecturer> GetAll();

        Lecturer? GetById(int userId);


        void Add(Lecturer lecturer);

        void Update(Lecturer lecturer);

        void Delete(Lecturer lecturer);
    }
}

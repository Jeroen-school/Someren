using Someren.Models;
namespace Someren.Repositories
{
    public interface IStudentsRepository
    {
        List<Student> GetAll();
        Student? GetByNum(int studentNum);
        void Add(Student student);
        void Update(Student student);
        void Delete(Student student);
    }
}

using Someren.Models;

public interface IStudentsRepository
{
    List<Student> GetAll();
    List<Student> GetFiltered(string lastName);
    List<Student> GetDeleted();
    List<Student> GetFilteredDeleted(string lastName);
    Student? GetByNum(int studentNum);
    Student? GetDeletedByNum(int studentNum);
    bool StudentNumExists(int studentNum);
    void Add(Student student);
    void Update(Student student);
    void Delete(Student student);
    void PermaDel(int studentNum);
    void Restore(int studentNum);
}
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace SofdesQuiz4;

public class DuplicateIdException : Exception
{
    public DuplicateIdException() : base("Duplicate ID") { }
}

public class IdDoesNotExistException : Exception
{
    public IdDoesNotExistException() : base("ID does not exist") { }
}

public class Student
{

    public Student(string name, DateOnly birthday, string course, int? id = null)
    {
        Id = id;
        Name = name;
        Birthday = birthday;
        Course = course;
        var now = DateTimeOffset.Now;
        var age = now.Year - Birthday.Year;
        if (now.Month < Birthday.Month || (now.Month == Birthday.Month && now.Day < Birthday.Day))
        {
            age--;
        }
        Age = age;
    }

    public int? Id { get; }
    public string Name { get; }
    public int Age { get; }
    public DateOnly Birthday { get; }
    public string Course { get; }

    public StudentEntity ToStudentEntity()
    {
        var studentEntity = new StudentEntity()
        {
            Id = Id,
            Name = Name,
            Birthday = Birthday,
            Course = Course
        };
        if (Id != null) studentEntity.Id = (int)Id;
        return studentEntity;
    }

}

[Table("StudentTable")]
public class StudentEntity
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int? Id { get; set; }
    public string Name { get; set; }
    public DateOnly Birthday { get; set; }
    public string Course { get; set; }

    public Student ToStudent()
    {
        return new Student(Name, Birthday, Course, Id);
    }
}

public class StudentContext : DbContext
{
    public DbSet<StudentEntity> StudentEntities { get; set; }

    public string DbPath { get; }

    public StudentContext()
    {
        const Environment.SpecialFolder folder = Environment.SpecialFolder.LocalApplicationData;
        var path = Environment.GetFolderPath(folder);
        DbPath = System.IO.Path.Join(path, "students.db");
    }

    protected override void OnConfiguring(DbContextOptionsBuilder options) => options.UseSqlite($"Data Source={DbPath}");

}

public static class StudentDb
{
    public static Student Get(int id)
    {
        using var context = new StudentContext();
        var studentEntity = context.StudentEntities.FirstOrDefault(student => student.Id == id);
        if (studentEntity == null) throw new IdDoesNotExistException();
        return studentEntity.ToStudent();
    }
    public static List<Student> GetAll(string searchQuery = "")
    {
        searchQuery = searchQuery.ToLower();
        using var context = new StudentContext();
        var studentEntities = context.StudentEntities.Where(student => student.Name.ToLower().Contains(searchQuery) || student.Course.ToLower().Contains(searchQuery));
        return studentEntities.Select(studentEntity => studentEntity.ToStudent()).ToList();
    }

    public static void Insert(Student student)
    {
        using var context = new StudentContext();
        var studentEntityOnDb = context.StudentEntities.FirstOrDefault(studentEntity => studentEntity.Id == student.Id);
        if (studentEntityOnDb != null) throw new DuplicateIdException();
        var studentEntity = student.ToStudentEntity();
        context.Add(studentEntity);
        context.SaveChanges();
    }

    public static void Update(Student student)
    {
        using var context = new StudentContext();
        var studentEntityOnDb = context.StudentEntities.FirstOrDefault(studentEntity => studentEntity.Id == student.Id);
        if (studentEntityOnDb == null) throw new IdDoesNotExistException();
        studentEntityOnDb.Name = student.Name;
        studentEntityOnDb.Birthday = student.Birthday;
        studentEntityOnDb.Course = student.Course;
        context.SaveChanges();
    }

    public static void Delete(int id)
    {
        using var context = new StudentContext();
        var studentEntityOnDb = context.StudentEntities.FirstOrDefault(studentEntity => studentEntity.Id == id);
        if (studentEntityOnDb == null) throw new IdDoesNotExistException();
        context.Remove(studentEntityOnDb);
        context.SaveChanges();
    }
}
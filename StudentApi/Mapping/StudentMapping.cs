using StudentApi.Dtos;
using StudentApi.Models;
namespace StudentApi.Mapping
{
	public static class StudentMapping
	{
		public static StudentDto ToDto(this Student student)
		{
			var studentDto = new StudentDto
			{
				Id = student.Id,
				Name = student.Name,
				Age = student.Age,
				Email = student.Email,
			};

			return studentDto;
		}

		public static Student ToEntity(this StudentDto dto)
		{
			var student = new Student
			{
				Id = dto.Id,
				Name = dto.Name,
				Age = dto.Age,
				Email = dto.Email,
			};

			return student;
		}

		public static Student ToEntity(this CreateUpdateStudentDto dto)
		{
			var student = new Student
			{
				Name = dto.Name,
				Age = dto.Age,
				Email = dto.Email,
			};

			return student;
		}

	}
}

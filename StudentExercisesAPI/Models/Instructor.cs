using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace StudentExercisesAPI.Models
{
    public class Instructor
    {
        public int Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string SlackHandle { get; set; }
        public string Specialty { get; set; }
        public int CohortId { get; set; }
        public Cohort Cohort { get; set; }

        public void AssignExercise(Student student, Exercise exercise)
        {
            Console.WriteLine($"{student} {exercise}");
        }

    }
}

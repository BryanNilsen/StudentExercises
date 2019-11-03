using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using StudentExercisesAPI.Models;

namespace StudentExercisesAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CohortController : ControllerBase
    {
        private readonly IConfiguration _config;

        public CohortController(IConfiguration config)
        {
            _config = config;
        }

        public SqlConnection Connection
        {
            get
            {
                return new SqlConnection(_config.GetConnectionString("DefaultConnection"));
            }
        }
        // GET: api/Cohort
        [HttpGet]
        public async Task<IActionResult> Get(string q, string include)
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    if (include == "all")
                    {
                        cmd.CommandText = @"
                            SELECT c.Id AS CohortId, c.Label,
                                i.Id AS InstructorId, i.FirstName AS InstructorFirstName, i.LastName AS InstructorLastName, i.SlackHandle AS InstructorSlackHandle, i.Specialty,
                                s.Id AS StudentId, s.FirstName AS StudentFirstName, s.LastName AS StudentLastName, s.SlackHandle AS StudentSlackHandle
                            FROM Cohort c
                            LEFT JOIN Instructor i ON c.id = i.CohortId
                            LEFT JOIN Student s ON c.id = s.CohortId
                            ";
                    }
                    else if (include == "students")
                    {
                        cmd.CommandText = @"
                            SELECT c.Id AS CohortId, c.Label,
                                s.Id AS StudentId, s.FirstName AS StudentFirstName, s.LastName AS StudentLastName, s.SlackHandle AS StudentSlackHandle
                            FROM Cohort c
                            LEFT JOIN Student s ON c.id = s.CohortId
                            ";
                    }
                    else if (include == "instructors")
                    {
                        cmd.CommandText = @"
                            SELECT c.Id AS CohortId, c.Label,
                                i.Id AS InstructorId, i.FirstName AS InstructorFirstName, i.LastName AS     InstructorLastName, i.SlackHandle AS InstructorSlackHandle, i.Specialty
                            FROM Cohort c
                            LEFT JOIN Instructor i ON c.id = i.CohortId
                    ";
                    }
                    else
                    {
                        cmd.CommandText = @"
                            SELECT c.Id AS CohortId, c.Label
                            FROM Cohort c
                        ";
                    }

                    if (q != null)
                    {
                        cmd.CommandText += @" WHERE Label LIKE @Query
                                        ";
                        cmd.Parameters.Add(new SqlParameter("@Query", "%" + q + "%"));
                    }

                    SqlDataReader reader = cmd.ExecuteReader();
                    Dictionary<int, Cohort> cohorts = new Dictionary<int, Cohort>();

                    while (reader.Read())
                    {
                        int cohortId = reader.GetInt32(reader.GetOrdinal("CohortId"));
                        if (!cohorts.ContainsKey(cohortId))
                        {
                            Cohort newCohort = new Cohort
                            {
                                Id = reader.GetInt32(reader.GetOrdinal("CohortId")),
                                Label = reader.GetString(reader.GetOrdinal("Label")),
                                //get associated students and instructors?
                                Students = new List<Student>(),
                                Instructors = new List<Instructor>()
                            };
                            cohorts.Add(cohortId, newCohort);
                        }

                        Cohort fromDictionary = cohorts[cohortId];

                        //ADD STUDENTS TO COHORT
                        if ((include == "students" || include == "all" ) && !reader.IsDBNull(reader.GetOrdinal("StudentId")))
                        {
                            int studentId = reader.GetInt32(reader.GetOrdinal("StudentId"));
                            if (!fromDictionary.Students.Any(student => student.Id == studentId))
                            {

                                Student newStudent = new Student()
                                {
                                    Id = reader.GetInt32(reader.GetOrdinal("StudentId")),
                                    FirstName = reader.GetString(reader.GetOrdinal("StudentFirstName")),
                                    LastName = reader.GetString(reader.GetOrdinal("StudentLastName")),
                                    SlackHandle = reader.GetString(reader.GetOrdinal("StudentSlackHandle")),
                                    CohortId = reader.GetInt32(reader.GetOrdinal("CohortId"))
                                };

                                fromDictionary.Students.Add(newStudent);
                            }

                        }

                        //ADD INSTRUCTORS TO COHORT
                        if ((include == "instructors" || include == "all") && !reader.IsDBNull(reader.GetOrdinal("InstructorId")))
                        {
                            int instructorId = reader.GetInt32(reader.GetOrdinal("InstructorId"));
                            if (!fromDictionary.Instructors.Any(instructor => instructor.Id == instructorId))
                            {
                                Instructor newInstructor = new Instructor()
                                {
                                    Id = reader.GetInt32(reader.GetOrdinal("InstructorId")),
                                    FirstName = reader.GetString(reader.GetOrdinal("InstructorFirstName")),
                                    LastName = reader.GetString(reader.GetOrdinal("InstructorLastName")),
                                    SlackHandle = reader.GetString(reader.GetOrdinal("InstructorSlackHandle")),
                                    Specialty = reader.GetString(reader.GetOrdinal("Specialty")),
                                    CohortId = reader.GetInt32(reader.GetOrdinal("CohortId"))
                                };
                                fromDictionary.Instructors.Add(newInstructor);
                            }
                        }
                    }
                    reader.Close();

                    return Ok(cohorts.Values);
                }
            }
        }

        // GET: api/Cohort/5
        [HttpGet("{id}", Name = "Get")]
        public async Task<IActionResult> Get([FromRoute] int id)
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"
                    SELECT c.Id AS CohortId, c.Label,
                        i.Id AS InstructorId, i.FirstName AS InstructorFirstName, i.LastName AS InstructorLastName, i.SlackHandle AS InstructorSlackHandle, i.Specialty,
                        s.Id AS StudentId, s.FirstName AS StudentFirstName, s.LastName AS StudentLastName, s.SlackHandle AS StudentSlackHandle
                    FROM Cohort c
                    LEFT JOIN Instructor i ON c.id = i.CohortId
                    LEFT JOIN Student s ON c.id = s.CohortId
                    WHERE c.Id = @id
                    ";

                    cmd.Parameters.Add(new SqlParameter("@id", id));
                    SqlDataReader reader = cmd.ExecuteReader();
                    Cohort cohort = null;

                    Dictionary<int, Cohort> cohorts = new Dictionary<int, Cohort>();
                    while (reader.Read())
                    {
                    if (cohort == null)
                    {
                        cohort = new Cohort()
                        {
                            Id = reader.GetInt32(reader.GetOrdinal("CohortId")),
                            Label = reader.GetString(reader.GetOrdinal("Label")),
                        };
                    }

                        //ADD STUDENTS TO COHORT
                        if (!reader.IsDBNull(reader.GetOrdinal("StudentId")))
                        {
                            int studentId = reader.GetInt32(reader.GetOrdinal("StudentId"));
                            if (!cohort.Students.Any(student => student.Id == studentId))
                            {
                                Student newStudent = new Student()
                                {
                                    Id = reader.GetInt32(reader.GetOrdinal("StudentId")),
                                    FirstName = reader.GetString(reader.GetOrdinal("StudentFirstName")),
                                    LastName = reader.GetString(reader.GetOrdinal("StudentLastName")),
                                    SlackHandle = reader.GetString(reader.GetOrdinal("StudentSlackHandle")),
                                    CohortId = reader.GetInt32(reader.GetOrdinal("CohortId"))
                                };
                                cohort.Students.Add(newStudent);
                            }
                        }

                        //ADD INSTRUCTORS TO COHORT
                        if (!reader.IsDBNull(reader.GetOrdinal("InstructorId")))
                        {
                            int instructorId = reader.GetInt32(reader.GetOrdinal("InstructorId"));
                            if (!cohort.Instructors.Any(instructor => instructor.Id == instructorId))
                            {
                                Instructor newInstructor = new Instructor()
                                {
                                    Id = reader.GetInt32(reader.GetOrdinal("InstructorId")),
                                    FirstName = reader.GetString(reader.GetOrdinal("InstructorFirstName")),
                                    LastName = reader.GetString(reader.GetOrdinal("InstructorLastName")),
                                    SlackHandle = reader.GetString(reader.GetOrdinal("InstructorSlackHandle")),
                                    Specialty = reader.GetString(reader.GetOrdinal("Specialty")),
                                    CohortId = reader.GetInt32(reader.GetOrdinal("CohortId"))
                                };
                                cohort.Instructors.Add(newInstructor);
                            }
                        }
                    }

                    reader.Close();
                    return Ok(cohort);
                }
            }
        }

        //// POST: api/Exercises
        //[HttpPost]
        //public async Task<IActionResult> Post([FromBody] Exercise exercise)
        //{
        //    using (SqlConnection conn = Connection)
        //    {
        //        conn.Open();
        //        using (SqlCommand cmd = conn.CreateCommand())
        //        {
        //            cmd.CommandText = @"INSERT INTO Exercises (Label, Language)
        //                                OUTPUT INSERTED.Id
        //                                VALUES (@label, @language)";
        //            cmd.Parameters.Add(new SqlParameter("@label", exercise.Label));
        //            cmd.Parameters.Add(new SqlParameter("@language", exercise.Language));

        //            int newId = (int)cmd.ExecuteScalar();
        //            exercise.Id = newId;
        //            return CreatedAtRoute("Get", new { id = newId }, exercise);
        //        }
        //    }
        //}

        //// PUT: api/Exercises/5
        //[HttpPut("{id}")]

        //public async Task<IActionResult> Put([FromRoute] int id, [FromBody] Exercise exercise)
        //{
        //    try
        //    {
        //        using (SqlConnection conn = Connection)
        //        {
        //            conn.Open();
        //            using (SqlCommand cmd = conn.CreateCommand())
        //            {
        //                cmd.CommandText = @"UPDATE Exercises
        //                                    SET Label = @label,
        //                                        Language = @language
        //                                    WHERE Id = @id";
        //                cmd.Parameters.Add(new SqlParameter("@label", exercise.Label));
        //                cmd.Parameters.Add(new SqlParameter("@language", exercise.Language));
        //                cmd.Parameters.Add(new SqlParameter("@id", id));

        //                int rowsAffected = cmd.ExecuteNonQuery();
        //                if (rowsAffected > 0)
        //                {
        //                    return new StatusCodeResult(StatusCodes.Status204NoContent);
        //                }
        //                throw new Exception("No rows affected");
        //            }
        //        }
        //    }
        //    catch (Exception)
        //    {
        //        if (!ExerciseExists(id))
        //        {
        //            return NotFound();
        //        }
        //        else
        //        {
        //            throw;
        //        }
        //    }
        //}

        //// DELETE: api/ApiWithActions/5
        //[HttpDelete("{id}")]
        //public async Task<IActionResult> Delete([FromRoute] int id)
        //{
        //    try
        //    {
        //        using (SqlConnection conn = Connection)
        //        {
        //            conn.Open();
        //            using (SqlCommand cmd = conn.CreateCommand())
        //            {
        //                cmd.CommandText = @"DELETE FROM Exercises WHERE Id = @id";
        //                cmd.Parameters.Add(new SqlParameter("@id", id));

        //                int rowsAffected = cmd.ExecuteNonQuery();
        //                if (rowsAffected > 0)
        //                {
        //                    return new StatusCodeResult(StatusCodes.Status204NoContent);
        //                }
        //                throw new Exception("No rows affected");
        //            }
        //        }
        //    }
        //    catch (Exception)
        //    {
        //        if (!ExerciseExists(id))
        //        {
        //            return NotFound();
        //        }
        //        else
        //        {
        //            throw;
        //        }
        //    }
        //}
        //private bool ExerciseExists(int id)
        //{
        //    using (SqlConnection conn = Connection)
        //    {
        //        conn.Open();
        //        using (SqlCommand cmd = conn.CreateCommand())
        //        {
        //            cmd.CommandText = @"
        //                SELECT Id, Label, Language
        //                FROM Exercises
        //                WHERE Id = @id";
        //            cmd.Parameters.Add(new SqlParameter("@id", id));

        //            SqlDataReader reader = cmd.ExecuteReader();
        //            return reader.Read();
        //        }
        //    }
        //}
    }
}

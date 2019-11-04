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
    public class ExerciseController : ControllerBase
    {
        private readonly IConfiguration _config;

        public ExerciseController(IConfiguration config)
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
        // GET: api/exercise
        [HttpGet]
        public async Task<IActionResult> Get(string q, string include)
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    // Evaluate Parameters
                    if (include == "students")
                    {
                    cmd.CommandText = @"SELECT e.Id, e.Label, e.Language,
	                                    s.Id AS StudentId, s.FirstName, s.LastName, s.CohortId, s.SlackHandle
	                                    FROM Exercise e
	                                    LEFT JOIN StudentExercise se ON e.Id = se.ExerciseId
	                                    LEFT JOIN Student s ON s.Id = se.StudentId
                                        ";
                        if (q != null)
                        {
                            cmd.CommandText += @" WHERE Label LIKE @Query 
                                                    OR Language LIKE @Query
                                                    OR FirstName LIKE @Query
                                                    OR LastName LIKE @Query
                                                    OR SlackHandle LIKE @Query
                                                ";
                            cmd.Parameters.Add(new SqlParameter("@Query", "%" + q + "%"));
                        }
                    }
                    else
                    {
                        cmd.CommandText = "SELECT Id, Label, Language FROM Exercise";
                        if (q != null)
                        {
                            cmd.CommandText += " WHERE Label LIKE @Query OR Language LIKE @Query";
                            cmd.Parameters.Add(new SqlParameter("@Query", "%" + q + "%"));
                        }
                    }


                    SqlDataReader reader = cmd.ExecuteReader();
                    Dictionary<int, Exercise> exercises = new Dictionary<int, Exercise>();


                    while (reader.Read())
                    {
                        int exerciseId = reader.GetInt32(reader.GetOrdinal("Id"));
                        if (!exercises.ContainsKey(exerciseId))
                        {
                            Exercise newExercise = new Exercise
                            {
                                Id = reader.GetInt32(reader.GetOrdinal("Id")),
                                Label = reader.GetString(reader.GetOrdinal("Label")),
                                Language = reader.GetString(reader.GetOrdinal("Language"))
                            };

                            exercises.Add(exerciseId, newExercise);

                        }

                        Exercise fromDictionary = exercises[exerciseId];
                        if (include == "students" && !reader.IsDBNull(reader.GetOrdinal("StudentId")))
                        {
                            Student aStudent = new Student()
                            {
                                Id = reader.GetInt32(reader.GetOrdinal("StudentId")),
                                FirstName = reader.GetString(reader.GetOrdinal("FirstName")),
                                LastName = reader.GetString(reader.GetOrdinal("LastName")),
                                SlackHandle = reader.GetString(reader.GetOrdinal("SlackHandle")),
                                CohortId = reader.GetInt32(reader.GetOrdinal("CohortId"))
                            };
                            fromDictionary.Students.Add(aStudent);
                        }
                    }


                    reader.Close();

                    return Ok(exercises.Values);
                }
            }
        }

        // GET: api/exercise/5
        [HttpGet("{id}", Name = "GetExercise")]
        public async Task<IActionResult> Get([FromRoute] int id)
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"SELECT Id, Label, Language 
                                        FROM Exercise
                                        WHERE Id = @id";

                    cmd.Parameters.Add(new SqlParameter("@id", id));
                    SqlDataReader reader = cmd.ExecuteReader();
                    Exercise exercise = null;

                    if (reader.Read())
                    {
                        exercise = new Exercise()
                        {
                            Id = reader.GetInt32(reader.GetOrdinal("Id")),
                            Label = reader.GetString(reader.GetOrdinal("Label")),
                            Language = reader.GetString(reader.GetOrdinal("Language"))
                        };
                    }
 
                    reader.Close();

                    return Ok(exercise);
                }
            }
        }

        // POST: api/exercise
        [HttpPost]
        public async Task<IActionResult> Post([FromBody] Exercise exercise)
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"INSERT INTO Exercise (Label, Language)
                                        OUTPUT INSERTED.Id
                                        VALUES (@label, @language)";
                    cmd.Parameters.Add(new SqlParameter("@label", exercise.Label));
                    cmd.Parameters.Add(new SqlParameter("@language", exercise.Language));

                    int newId = (int)cmd.ExecuteScalar();
                    exercise.Id = newId;
                    return CreatedAtRoute("GetExercise", new { id = newId }, exercise);
                }
            }
        }

        // PUT: api/exercise/5
        [HttpPut("{id}")]

            public async Task<IActionResult> Put([FromRoute] int id, [FromBody] Exercise exercise)
            {
                try
                {
                    using (SqlConnection conn = Connection)
                    {
                        conn.Open();
                        using (SqlCommand cmd = conn.CreateCommand())
                        {
                            cmd.CommandText = @"UPDATE Exercise
                                            SET Label = @label,
                                                Language = @language
                                            WHERE Id = @id";
                            cmd.Parameters.Add(new SqlParameter("@label", exercise.Label));
                            cmd.Parameters.Add(new SqlParameter("@language", exercise.Language));
                            cmd.Parameters.Add(new SqlParameter("@id", id));

                            int rowsAffected = cmd.ExecuteNonQuery();
                            if (rowsAffected > 0)
                            {
                                return new StatusCodeResult(StatusCodes.Status204NoContent);
                            }
                            throw new Exception("No rows affected");
                        }
                    }
                }
                catch (Exception)
                {
                    if (!ExerciseExists(id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
            }

        // DELETE: api/exercise/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete([FromRoute] int id)
        {
            try
            {
                using (SqlConnection conn = Connection)
                {
                    conn.Open();
                    using (SqlCommand cmd = conn.CreateCommand())
                    {
                        cmd.CommandText = @"DELETE FROM Exercise WHERE Id = @id";
                        cmd.Parameters.Add(new SqlParameter("@id", id));

                        int rowsAffected = cmd.ExecuteNonQuery();
                        if (rowsAffected > 0)
                        {
                            return new StatusCodeResult(StatusCodes.Status204NoContent);
                        }
                        throw new Exception("No rows affected");
                    }
                }
            }
            catch (Exception)
            {
                if (!ExerciseExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }
        }
        private bool ExerciseExists(int id)
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"
                        SELECT Id, Label, Language
                        FROM Exercise
                        WHERE Id = @id";
                    cmd.Parameters.Add(new SqlParameter("@id", id));

                    SqlDataReader reader = cmd.ExecuteReader();
                    return reader.Read();
                }
            }
        }
    }
}

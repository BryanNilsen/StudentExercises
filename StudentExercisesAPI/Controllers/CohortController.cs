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
        public async Task<IActionResult> Get()
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = "SELECT Id, Label FROM Cohort";
                    SqlDataReader reader = cmd.ExecuteReader();
                    List<Cohort> cohorts = new List<Cohort>();

                    while (reader.Read())
                    {
                        Cohort cohort = new Cohort
                        {
                            Id = reader.GetInt32(reader.GetOrdinal("Id")),
                            Label = reader.GetString(reader.GetOrdinal("Label")),
                            //get associated students and instructors?
                            Students = new List<Student>(),
                            Instructors = new List<Instructor>()
                        };


                        cohorts.Add(cohort);
                    }
                    reader.Close();

                    return Ok(cohorts);
                }
            }
        }

        // GET: api/Cohort/5
        //[HttpGet("{id}", Name = "Get")]
        //public async Task<IActionResult> Get([FromRoute] int id)
        //{
        //    using (SqlConnection conn = Connection)
        //    {
        //        conn.Open();
        //        using (SqlCommand cmd = conn.CreateCommand())
        //        {
        //            cmd.CommandText = @"SELECT Id, Label, Language 
        //                                FROM Exercises
        //                                WHERE Id = @id";

        //            cmd.Parameters.Add(new SqlParameter("@id", id));
        //            SqlDataReader reader = cmd.ExecuteReader();
        //            Exercise exercise = null;

        //            if (reader.Read())
        //            {
        //                exercise = new Exercise()
        //                {
        //                    Id = reader.GetInt32(reader.GetOrdinal("Id")),
        //                    Label = reader.GetString(reader.GetOrdinal("Label")),
        //                    Language = reader.GetString(reader.GetOrdinal("Language"))
        //                };
        //            }

        //            reader.Close();

        //            return Ok(exercise);
        //        }
        //    }
        //}

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

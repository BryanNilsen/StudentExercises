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
    public class AssignmentController : ControllerBase
    {

        private readonly IConfiguration _config;

        public AssignmentController(IConfiguration config)
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

        // GET: api/assignment/5
        [HttpGet("{id}", Name = "GetAssignment")]
        public async Task<IActionResult> Get([FromRoute] int id)
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"SELECT Id, StudentId, ExerciseId 
                                        FROM StudentExercise
                                        WHERE Id = @id";

                    cmd.Parameters.Add(new SqlParameter("@id", id));
                    SqlDataReader reader = cmd.ExecuteReader();
                    StudentExercise studentExercise = null;

                    if (reader.Read())
                    {
                        studentExercise = new StudentExercise()
                        {
                            Id = reader.GetInt32(reader.GetOrdinal("Id")),
                            StudentId = reader.GetInt32(reader.GetOrdinal("StudentId")),
                            ExerciseId = reader.GetInt32(reader.GetOrdinal("ExerciseId"))
                        };
                    }

                    reader.Close();

                    return Ok(studentExercise);
                }
            }
        }

        // POST: api/assignment
        [HttpPost]
        public async Task<IActionResult> Post([FromBody] StudentExercise studentExercise)
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"INSERT INTO StudentExercise (StudentId, ExerciseId)
                                        OUTPUT INSERTED.Id
                                        VALUES (@studentId, @exerciseId)";
                    cmd.Parameters.Add(new SqlParameter("@studentId", studentExercise.StudentId));
                    cmd.Parameters.Add(new SqlParameter("@exerciseId", studentExercise.ExerciseId));

                    int newId = (int)cmd.ExecuteScalar();
                    studentExercise.Id = newId;
                    return CreatedAtRoute("GetAssignment", new { id = newId }, studentExercise);
                }
            }
        }

        // DELETE: api/assignment/5
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
                        cmd.CommandText = @"DELETE FROM StudentExercise WHERE Id = @id";
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
                if (!StudentExerciseExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }
        }
        private bool StudentExerciseExists(int id)
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"
                        SELECT Id, StudentId, ExerciseId
                        FROM StudentExercise
                        WHERE Id = @id";
                    cmd.Parameters.Add(new SqlParameter("@id", id));

                    SqlDataReader reader = cmd.ExecuteReader();
                    return reader.Read();
                }
            }
        }
    }
}
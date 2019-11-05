using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using StudentExercisesAPI.Models;
using System.Net;
using Xunit;
using System.Net.Http;

namespace TestStudentExercisesAPI
{
    public class ExerciseTests
    {
        [Fact]
        public async Task Test_Create_Exercise()
        {
            /*
                Generate a new instance of an HttpClient that you can
                use to generate HTTP requests to your API controllers.
                The `using` keyword will automatically dispose of this
                instance of HttpClient once your code is done executing.
            */
            using (var client = new APIClientProvider().Client)
            {
                /*
                    ARRANGE
                */

                // Construct a new student object to be sent to the API
                Exercise testExercise = new Exercise
                {
                    Label = "Kennel",
                    Language = "Angular"
                };

                // Serialize the C# object into a JSON string
                var testExerciseAsJSON = JsonConvert.SerializeObject(testExercise);


                /*
                    ACT
                */

                // Use the client to send the request and store the response
                var response = await client.PostAsync(
                    "/api/exercise",
                    new StringContent(testExerciseAsJSON, Encoding.UTF8, "application/json")
                );

                // Store the JSON body of the response
                string responseBody = await response.Content.ReadAsStringAsync();

                // Deserialize the JSON into an instance of Animal
                var newExercise = JsonConvert.DeserializeObject<Exercise>(responseBody);


                /*
                    ASSERT
                */

                Assert.Equal(HttpStatusCode.Created, response.StatusCode);
                Assert.Equal("Kennel", newExercise.Label);
                Assert.Equal("Angular", newExercise.Language);
            }
        }


        [Fact]
        public async Task Test_Get_All_Exercises()
        {

            using (var client = new APIClientProvider().Client)
            {
                /*
                    ARRANGE
                */


                /*
                    ACT
                */
                var response = await client.GetAsync("/api/exercise");


                string responseBody = await response.Content.ReadAsStringAsync();
                var exerciseList = JsonConvert.DeserializeObject<List<Exercise>>(responseBody);

                /*
                    ASSERT
                */
                Assert.Equal(HttpStatusCode.OK, response.StatusCode);
                Assert.True(exerciseList.Count > 0);
            }
        }

        [Fact]
        public async Task Test_Modify_Exercise()
        {
            // New label name to change to and test
            string newLanguage = "Angular";

            using (var client = new APIClientProvider().Client)
            {
                /*
                    PUT section
                */
                Exercise modifiedKennel = new Exercise
                {
                    Label = "Kennel",
                    Language = newLanguage,
                };
                var modifiedKennelAsJSON = JsonConvert.SerializeObject(modifiedKennel);

                var response = await client.PutAsync(
                    "/api/exercise/9",
                    new StringContent(modifiedKennelAsJSON, Encoding.UTF8, "application/json")
                );
                string responseBody = await response.Content.ReadAsStringAsync();

                Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);


                /*
                    GET section
                    Verify that the PUT operation was successful
                */
                var getKennel = await client.GetAsync("/api/exercise/9");
                getKennel.EnsureSuccessStatusCode();

                string getKennelBody = await getKennel.Content.ReadAsStringAsync();
                Exercise newKennel = JsonConvert.DeserializeObject<Exercise>(getKennelBody);

                Assert.Equal(HttpStatusCode.OK, getKennel.StatusCode);
                Assert.Equal(newLanguage, newKennel.Language);
            }

        }

        [Fact]
        public async Task Test_Delete_Exercise()
        {

            using (var client = new APIClientProvider().Client)
            {
                /*
                    Delete section
                */


                //var response = await client.DeleteAsync(
                //    "/api/exercise/10"
                //);
                //string responseBody = await response.Content.ReadAsStringAsync();

                //Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);


                /*
                    GET section
                    Verify that the DELETE operation was successful
                */
                //    var getDeletedItem = await client.GetAsync("/api/exercise/10");
                //    getDeletedItem.EnsureSuccessStatusCode();

                //    string getKennelBody = await getKennel.Content.ReadAsStringAsync();

                //    Assert.Equal(HttpStatusCode.OK, getKennel.StatusCode);
            }
        }
    }
}

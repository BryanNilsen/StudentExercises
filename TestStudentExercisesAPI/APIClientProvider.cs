using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.AspNetCore.Mvc.Testing;
using StudentExercisesAPI;
using System.Net.Http;
using Xunit;
using System.Threading.Tasks;
using Newtonsoft.Json;
using StudentExercisesAPI.Models;
using System.Net;

namespace TestStudentExercisesAPI
{
    public class APIClientProvider : IClassFixture<WebApplicationFactory<Startup>>
    {
        public HttpClient Client { get; private set; }
        private readonly WebApplicationFactory<Startup> _factory = new WebApplicationFactory<Startup>();

        public APIClientProvider()
        {
            Client = _factory.CreateClient();
        }

        public void Dispose()
        {
            _factory?.Dispose();
            Client?.Dispose();
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
                    Label = "Kennel in Angular",
                    Language = newLanguage,
                };
                var modifiedKennelAsJSON = JsonConvert.SerializeObject(modifiedKennel);

                var response = await client.PutAsync(
                    "/api/exercise/10",
                    new StringContent(modifiedKennelAsJSON, Encoding.UTF8, "application/json")
                );
                string responseBody = await response.Content.ReadAsStringAsync();

                Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);


                /*
                    GET section
                    Verify that the PUT operation was successful
                */
                var getKennel = await client.GetAsync("/api/exercise/10");
                getKennel.EnsureSuccessStatusCode();

                string getKennelBody = await getKennel.Content.ReadAsStringAsync();
                Exercise newKennel = JsonConvert.DeserializeObject<Exercise>(getKennelBody);

                Assert.Equal(HttpStatusCode.OK, getKennel.StatusCode);
                Assert.Equal(newLanguage, newKennel.Language);
            }
        }
    }
}
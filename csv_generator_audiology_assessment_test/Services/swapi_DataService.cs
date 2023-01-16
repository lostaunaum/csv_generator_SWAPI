using csv_generator_audiology_assessment_test.Services.Interfaces;
using csv_generator_audiology_assessment_test.DTO;
using Microsoft.Extensions.Configuration;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Net.Http;
using Newtonsoft.Json;
using System.Linq;
using System.IO;
using System;

namespace csv_generator_audiology_assessment_test.Services
{
    public class swapi_DataService : Iswapi_DataService
    {
        HttpClient httpClient;
        private readonly IConfiguration _configuration;
        private readonly string swapiURL;
        
        public swapi_DataService(IConfiguration configuration)
        {
            httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Clear();
            _configuration = configuration;
            swapiURL = _configuration.GetValue<string>("StarWarsAPIHost");
        }

        public async Task<string> GetAllCharactersByEvenNumberedFilms()
        {
            //Goals:
            //a.Lists all characters in all even numbered non-spinoff films
            //b.Sorted by Film->Planet->Character Age->Character Name
            //c.Where the character name is in surname - given name order. 
            var httpResponse = await httpClient.GetAsync(swapiURL + "films");
            var content = await httpResponse.Content.ReadAsStringAsync();
            var csvResult = string.Empty;
            var listOfPeople = new List<swapi_People>();

            if (httpResponse.StatusCode == System.Net.HttpStatusCode.OK)
            {
                var data = JsonConvert.DeserializeObject<swapi_FilmResponse>(content);

                foreach (var film in data.results)
                {
                    await GetPeopleFromAPI(film.characters, listOfPeople, Int32.Parse(film.episode_id));
                }
            }

            List<swapi_People> sorted = listOfPeople.Where(p => p.filmIds.Any<int>(x => x % 2 == 0))
                .OrderBy(x => x.filmIds.FirstOrDefault())
                .ThenBy(x => x.homeworldName)
                .ThenBy(x => x.birth_year)
                .ThenBy(x => x.name).ToList();

            csvResult = CreateCSV(sorted);

            return csvResult;
        }

        private async Task<List<swapi_People>> GetPeopleFromAPI(List<string> arrayOfURLs, List<swapi_People> listOfPeople, int filmId)
        {
            foreach (var url in arrayOfURLs)
            {
                //Get Person
                var httpResponse = await httpClient.GetAsync(url);
                var content = await httpResponse.Content.ReadAsStringAsync();
                var data = JsonConvert.DeserializeObject<swapi_People>(content);

                //getHomeWorld
                var httpResponseHomeWorld = await httpClient.GetAsync(data.homeworld);
                var contentHomeWorld = await httpResponseHomeWorld.Content.ReadAsStringAsync();
                var dataHomeWorld = JsonConvert.DeserializeObject<swapi_Planet>(contentHomeWorld);
                data.homeworldName = dataHomeWorld.name;
                data.name = string.Join(" ", data.name.Split(' ').ToArray().Reverse());

                //If the character is not in the list add id
                if (listOfPeople.Where(x => x.name == data.name).FirstOrDefault() == null)
                {
                    data.filmIds = new List<int>() { filmId };
                    data.episodes_present += filmId.ToString();
                    listOfPeople.Add(data);
                }
                //If its already in the list make sure to add the Film ID
                else
                {
                    var people = listOfPeople.Where(x => x.name == data.name).FirstOrDefault();

                    if (!people.filmIds.Contains(filmId))
                    {
                        people.filmIds.Add(filmId);
                        people.episodes_present += string.Format(", {0}", filmId);
                    }
                }
            }

            return listOfPeople;
        }

        private string CreateCSV(List<swapi_People> sortedListOfPeople)
        {
            var csv = string.Empty;
            var delimiter = ";";

            var properties = typeof(swapi_People).GetProperties().Where(
                n => n.PropertyType == typeof(string));

            using (var sw = new StringWriter())
            {
                var header = properties
                .Select(n => n.Name)
                .Aggregate((a, b) => a + delimiter + b);

                sw.WriteLine(header);

                foreach (var people in sortedListOfPeople)
                {
                    var row = properties
                    .Select(n => n.GetValue(people, null))
                    .Select(n => n == null ? "null" : n.ToString())
                        .Aggregate((a, b) => a + delimiter + b);

                    sw.WriteLine(row);
                }

                csv = sw.ToString();
            }
            
            return csv;
        }
    }
}

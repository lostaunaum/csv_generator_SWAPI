using System.Collections.Generic;

namespace csv_generator_audiology_assessment_test.DTO
{
    public class swapi_FilmResponse
    {
        public string count { get; set; }
        public string next { get; set; }
        public string previous { get; set; }
        public List<swapi_Film> results { get; set; }
    }
}

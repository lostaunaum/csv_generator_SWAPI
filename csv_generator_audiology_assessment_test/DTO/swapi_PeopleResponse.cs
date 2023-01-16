using System.Collections.Generic;

namespace csv_generator_audiology_assessment_test.DTO
{
    public class swapi_PeopleResponse
    {
        public string count { get; set; }
        public string next { get; set; }
        public List<swapi_People> results { get; set; }
    }
}
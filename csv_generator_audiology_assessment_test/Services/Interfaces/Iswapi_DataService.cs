using System.Threading.Tasks;

namespace csv_generator_audiology_assessment_test.Services.Interfaces
{
    public interface Iswapi_DataService
    {
        Task<string> GetAllCharactersByEvenNumberedFilms();
    }
}

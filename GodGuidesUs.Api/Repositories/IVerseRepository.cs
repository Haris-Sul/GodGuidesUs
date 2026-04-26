using GodGuidesUs.Api.Models;

namespace GodGuidesUs.Api.Repositories;

public interface IVerseRepository
{
    Task<VerseModel> InsertAsync(VerseModel verse);

    Task<IReadOnlyList<VerseModel>> SearchVersesAsync(float[] queryVector);
}
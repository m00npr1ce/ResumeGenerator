namespace ResumeGenerator.Services
{
    public interface IResumeService
    {
        Task<List<Resume>> GetAllResumesAsync(int userId);
        Task<Resume?> GetResumeByIdAsync(int id);
        Task<Resume> CreateResumeAsync(Resume resume);
        Task<Resume?> UpdateResumeAsync(int id, Resume updatedResume);
        Task<bool> DeleteResumeAsync(int id);
    }
}

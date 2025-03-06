using Microsoft.EntityFrameworkCore;
using ResumeGenerator;

namespace ResumeGenerator.Services
{
    public class ResumeService : IResumeService
    {
        private readonly ResumeGeneratorContext _context;

        public ResumeService(ResumeGeneratorContext context)
        {
            _context = context;
        }

        public async Task<List<Resume>> GetAllResumesAsync(int userId)
        {
            return await _context.Resumes
                .Where(r => r.UserId == userId)
                .ToListAsync();
        }

        public async Task<Resume?> GetResumeByIdAsync(int id)
        {
            return await _context.Resumes
                .FirstOrDefaultAsync(r => r.Id == id);
        }

        public async Task<Resume> CreateResumeAsync(Resume resume)
        {
            _context.Resumes.Add(resume);
            await _context.SaveChangesAsync();
            return resume;
        }

        public async Task<Resume?> UpdateResumeAsync(int id, Resume updatedResume)
        {
            var resume = await _context.Resumes
                .FirstOrDefaultAsync(r => r.Id == id);

            if (resume == null)
            {
                return null;
            }

            resume.Title = updatedResume.Title;
            resume.Content = updatedResume.Content;
            await _context.SaveChangesAsync();
            return resume;
        }

        public async Task<bool> DeleteResumeAsync(int id)
        {
            var resume = await _context.Resumes
                .FirstOrDefaultAsync(r => r.Id == id);

            if (resume == null)
            {
                return false;
            }

            _context.Resumes.Remove(resume);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}

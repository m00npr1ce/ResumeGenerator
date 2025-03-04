using Microsoft.AspNetCore.Mvc;
using ResumeApi.Models;
using iText.Kernel.Pdf;
using iText.Layout;
using iText.Layout.Element;
using System.IO;

namespace ResumeApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ResumeController : ControllerBase
    {
        [HttpPost("generate")]
        public IActionResult GenerateResume([FromBody] ResumeData data)
        {
            if (data == null)
            {
                return BadRequest("Invalid data");
            }

            using (MemoryStream stream = new MemoryStream())
            {
                using (PdfWriter writer = new PdfWriter(stream))
                {
                    using (PdfDocument pdf = new PdfDocument(writer))
                    {
                        using (Document document = new Document(pdf))
                        {
                            document.Add(new Paragraph("Резюме").SetFontSize(16));
                            document.Add(new Paragraph($"ФИО: {data.FullName}"));
                            document.Add(new Paragraph($"Email: {data.Email}"));
                            document.Add(new Paragraph($"Телефон: {data.Phone}"));
                            document.Add(new Paragraph($"Адрес: {data.Address}"));
                            document.Add(new Paragraph($"Опыт работы: {data.Experience}"));
                            document.Add(new Paragraph($"Образование: {data.Education}"));
                            document.Add(new Paragraph($"Навыки: {data.Skills}"));
                        }
                    }
                }
                // Возвращаем файл PDF
                return new FileStreamResult(new MemoryStream(stream.ToArray()), "application/pdf")
                {
                    FileDownloadName = "resume.pdf"
                };
            }
        }
    }
}
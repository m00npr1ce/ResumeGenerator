using Microsoft.AspNetCore.Mvc;
using ResumeApi.Models;
using iText.Kernel.Pdf;
using iText.Layout;
using iText.Layout.Element;
using System.IO;
using iText.Kernel.Font;
using iText.IO.Font.Constants;
using iText.IO.Image;

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

            // Создаем временный файл для PDF
            string filePath = Path.Combine(Path.GetTempPath(), "resume.pdf");
            try
            {
                using (FileStream fs = new FileStream(filePath, FileMode.Create, FileAccess.Write))
                {
                    using (PdfWriter writer = new PdfWriter(fs))
                    {
                        using (PdfDocument pdf = new PdfDocument(writer))
                        {
                            using (Document document = new Document(pdf))
                            {
                                // Путь к шрифту (относительно каталога проекта)
                                string fontPath = Path.Combine(Directory.GetCurrentDirectory(), "Fonts", "LiberationSerif-Regular.ttf"); // Замените на имя вашего файла
                                if (!System.IO.File.Exists(fontPath))
                                {
                                    // Если шрифт не найден, используйте шрифт по умолчанию
                                    fontPath = StandardFonts.HELVETICA;
                                }

                                PdfFont font;
                                try
                                {
                                    // Создайте шрифт
                                    font = PdfFontFactory.CreateFont(fontPath, iText.IO.Font.PdfEncodings.IDENTITY_H);
                                    pdf.AddFont(font);
                                }
                                catch (IOException ex)
                                {
                                    // Обработка ошибок, если шрифт не удалось загрузить
                                    Console.WriteLine($"Ошибка загрузки шрифта: {ex.Message}");
                                    font = PdfFontFactory.CreateFont(StandardFonts.HELVETICA); // Шрифт по умолчанию
                                }

                                document.SetFont(font);

                                document.Add(new Paragraph("Резюме").SetFontSize(16));
                                // Путь к изображению (относительно каталога проекта)
                                string imagePath = Path.Combine(Directory.GetCurrentDirectory(), "Images", "photo_2025-03-06_00-02-59.jpg"); // Замените на имя вашего файла
                                if (System.IO.File.Exists(imagePath))
                                {
                                    ImageData iData = ImageDataFactory.Create(imagePath);
                                    Image image = new Image(iData);
                                    image.SetWidth(300); // Задайте желаемую ширину
                                    document.Add(image);
                                }
                                else
                                {
                                    Console.WriteLine("Файл изображения не найден: " + imagePath);
                                }

                                document.Add(new Paragraph($"ФИО: {data.FullName}"));
                                document.Add(new Paragraph($"Email: {data.Email}"));
                                document.Add(new Paragraph($"Телефон: {data.Phone}"));
                                document.Add(new Paragraph($"Адрес: {data.Address}"));
                                document.Add(new Paragraph($"Опыт работы: {data.Experience}"));
                                document.Add(new Paragraph($"Образование: {data.Education}"));
                                document.Add(new Paragraph($"Навыки: {data.Skills}"));

                                Paragraph footer = new Paragraph("Создано командой MR")
                                    .SetMarginTop(30);
                                document.Add(footer);
                            }
                        }
                    }
                }

                // Возвращаем файл PDF
                return PhysicalFile(filePath, "application/pdf", "resume.pdf");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при создании PDF: {ex.Message}");
                return StatusCode(500, "Ошибка при создании PDF");
            }
        }
    }
}


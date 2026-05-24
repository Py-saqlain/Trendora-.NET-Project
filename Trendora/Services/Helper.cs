namespace Trendora.Services
{
    public static class Helper
    {
        public  static string SaveImage(IFormFile imageFile, string folderName)
        {
            string folder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", folderName);
            Directory.CreateDirectory(folder);

            string fileName = Guid.NewGuid().ToString() + Path.GetExtension(imageFile.FileName);
            string path = Path.Combine(folder, fileName);

            using (var stream = new FileStream(path, FileMode.Create))
            {
                imageFile.CopyTo(stream);
            }

            return $"/images/{folderName}/{fileName}";
        }
    }
}

namespace prjMyBlog.Models
{
    public class PostFactory
    {
        public static string SaveImage(IFormFile file)
        {
            try
            {
                if (file == null || file.Length == 0)
                {
                    Console.WriteLine("上傳失敗");
                    return null;
                }

                string wwwrootPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
                string fileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
                string savePath = Path.Combine(wwwrootPath, "images", fileName);

                using (var stream = new FileStream(savePath, FileMode.Create))
                {
                    file.CopyTo(stream);
                }

                return $"/images/{fileName}";
            }
            catch (Exception ex)
            {
                
                Console.WriteLine("儲存圖片發生錯誤：" + ex.Message);
                Console.WriteLine(ex.StackTrace);
                return null;
            }
        }
    }
}

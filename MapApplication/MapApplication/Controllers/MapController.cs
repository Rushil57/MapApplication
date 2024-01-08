using Newtonsoft.Json;
using System;
using System.IO.Compression;
using System.IO;
using System.Threading.Tasks;
using System.Web;
using System.Web.Hosting;
using System.Web.Mvc;

namespace MapApplication.Controllers
{
    public class MapController : Controller
    {
        public ActionResult Index()
        {
            return View();
        }

        public async Task<string> ShpToGeoJson(HttpPostedFileBase formData)
        {
            bool valid = false;
            string url = string.Empty;
            string fileName = string.Empty;
            var extension = string.Empty;
            var serverPath = string.Empty;
            try
            {
                var files = Request.Files;
                HttpPostedFileBase retrievedFile;
                var rootPath = HostingEnvironment.ApplicationPhysicalPath;

                if (files.Count > 0)
                {
                    string firstFileName = files[0].FileName;
                    serverPath = rootPath + "Files\\";
                    if (Directory.Exists(serverPath))
                    {
                        System.IO.DirectoryInfo di = new DirectoryInfo(serverPath);

                        foreach (FileInfo file in di.GetFiles())
                        {
                            file.Delete();
                        }
                        foreach (DirectoryInfo dir in di.GetDirectories())
                        {
                            dir.Delete(true);
                        }
                    }
                    serverPath += firstFileName.Substring(0, firstFileName.LastIndexOf('.'));
                    if (!Directory.Exists(serverPath))
                    {
                        Directory.CreateDirectory(serverPath);
                    }
                    for (int i = 0; i < files.Count; i++)
                    {
                        retrievedFile = files[i];
                        fileName = retrievedFile.FileName.ToString();
                        extension = Path.GetExtension(fileName);
                        string tempServerPath = Path.Combine(serverPath, fileName);
                        retrievedFile.SaveAs(tempServerPath);
                    }
                    url = serverPath + ".zip";
                    ZipFile.CreateFromDirectory(serverPath, url, CompressionLevel.Fastest, true);
                    valid = true;
                }
                return JsonConvert.SerializeObject(new { IsValid = valid, url = url, Message = "" });
            }
            catch (Exception ex)
            {
                return JsonConvert.SerializeObject(new { IsValid = valid, url = url, Message = ex.Message });
            }
        }
    }
}
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace faceRecognitionApi.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class FaceRecognitionController : ControllerBase
    {
       
        private readonly ILogger<FaceRecognitionController> _logger;
        IHostingEnvironment _hostingEnvironment;
        
        public FaceRecognitionController(ILogger<FaceRecognitionController> logger, IHostingEnvironment hostingEnvironment)
        {
            _logger = logger;
            _hostingEnvironment = hostingEnvironment;
        }


        [HttpPost("UploadFaceImageReturnCode")]
        public async Task<IActionResult> UploadFaceImageReturnCode([FromBody] UploadImageModel model)
        {
            try
            {
                var imageDataByteArray = Convert.FromBase64String(model.ImageData.Split(',').LastOrDefault());

                var imageDataStream = new MemoryStream(imageDataByteArray);
                imageDataStream.Position = 0;

                if (imageDataStream == null || imageDataStream.Length == 0)
                    return BadRequest("Dosya Tanımlanamadı.");

                var folderName = Path.Combine("Contents", "ProfilePics");
                var filePath = Path.Combine(Directory.GetCurrentDirectory(), folderName);

                if (!Directory.Exists(filePath))
                {
                    Directory.CreateDirectory(filePath);
                }

                var uniqueFileName = $"{Guid.NewGuid()}";
                var dbPath = Path.Combine(folderName, uniqueFileName);

                using (var fileStream = new FileStream(Path.Combine(filePath, uniqueFileName), FileMode.Create))
                {
                    await imageDataStream.CopyToAsync(fileStream);
                }

                var hostingInformation = _hostingEnvironment.ContentRootPath;
                String fulldirectory = "./Contents/ProfilePics/" + uniqueFileName;
                String faceData = "";
                FaceRecognitionDotNet.FaceRecognition abc = FaceRecognitionDotNet.FaceRecognition.Create(hostingInformation+ "\\Contents\\ProfilePics\\");
                var unknownImage = FaceRecognitionDotNet.FaceRecognition.LoadImageFile(fulldirectory);
                var returns = abc.FaceEncodings(unknownImage);
                if (returns.Count() > 0)
                {
                    SerializationInfo info = new SerializationInfo(typeof(FaceRecognitionDotNet.FaceEncoding), new FormatterConverter());
                    StreamingContext context = new StreamingContext();
                    returns.First().GetObjectData(info, context);
                    foreach (SerializationEntry entry in info)
                    {
                        var data = entry.Value;
                        double[] array = (double[])data;
                        faceData = String.Join(" ", array);
                        break;
                    }
                }

                var fileInfo = new System.IO.FileInfo(fulldirectory);
                fileInfo.Delete();
                return Ok(faceData);
            }
            catch (Exception e)
            {
                return BadRequest("Sistemde Hata Oluştu.");
            }
        }

        [HttpPost("UploadFaceImageReturnName")]
        public async Task<IActionResult> UploadFaceImageReturnName([FromBody] UploadImageRecongnitionModel model)
        {
            try
            {
                var imageDataByteArray = Convert.FromBase64String(model.ImageData.Split(',').LastOrDefault());

                var imageDataStream = new MemoryStream(imageDataByteArray);
                imageDataStream.Position = 0;

                if (imageDataStream == null || imageDataStream.Length == 0)
                    return BadRequest("Dosya Tanımlanamadı.");

                var folderName = Path.Combine("Contents", "ProfilePics");
                var filePath = Path.Combine(Directory.GetCurrentDirectory(), folderName);

                if (!Directory.Exists(filePath))
                {
                    Directory.CreateDirectory(filePath);
                }

                var uniqueFileName = $"{Guid.NewGuid()}";
                var dbPath = Path.Combine(folderName, uniqueFileName);

                using (var fileStream = new FileStream(Path.Combine(filePath, uniqueFileName), FileMode.Create))
                {
                    await imageDataStream.CopyToAsync(fileStream);
                }




                // Simdi bu egitilmiş dataları string'den double array'e döndürelim.
                List<FaceRecognitionDotNet.FaceEncoding> egitilmis_data = new List<FaceRecognitionDotNet.FaceEncoding>();
                for (int i = 0; i < model.FaceDetails.Count(); i++)
                {
                    String[] stringdata = model.FaceDetails[i].Data.Split(' ');
                    double[] data = new double[stringdata.Length];
                    for (int y = 0; y < data.Length; y++)
                    {
                        data[y] = Convert.ToDouble(stringdata[y]);
                    }
                    egitilmis_data.Add(FaceRecognitionDotNet.FaceRecognition.LoadFaceEncoding(data));
                }



                // Simdi yeni gelen görseli dataya cevirelim.
                var hostingInformation = _hostingEnvironment.ContentRootPath;
                String fulldirectory = "./Contents/ProfilePics/" + uniqueFileName;
                FaceRecognitionDotNet.FaceRecognition abc = FaceRecognitionDotNet.FaceRecognition.Create(hostingInformation + "\\Contents\\ProfilePics\\");
                var unknownImage = FaceRecognitionDotNet.FaceRecognition.LoadImageFile(fulldirectory);
                var returns = abc.FaceEncodings(unknownImage);
                var taninacak_encoding = returns.First();


                // Simdi arsivden tanınacak kisiyi bulduralım.
                var sonuc = FaceRecognitionDotNet.FaceRecognition.CompareFaces(egitilmis_data, taninacak_encoding, 0.6);
                var indexOfSelectedPerson = sonuc.Select((item, i) => new
                {
                    Item = item,
                    Position = i
                }).Where(m => m.Item == true).FirstOrDefault();

                var fileInfo = new System.IO.FileInfo(fulldirectory);
                fileInfo.Delete();

                if (indexOfSelectedPerson == null)
                {

                    return BadRequest("Kişi Bulunamadı.");
                }
                else
                {
                    var returunModel = model.FaceDetails[indexOfSelectedPerson.Position].Id;
                    return Ok(returunModel);

                }
            }
            catch (Exception e)
            {
                return BadRequest("Sistemde Hata Oluştu.");
            }
        }
    }
}

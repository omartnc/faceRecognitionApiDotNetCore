using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace faceRecognitionApi
{
    public class FaceDetail
    {
        public string Id { get; set; }
        public string Data { get; set; }
    }
    public class UploadImageRecongnitionModel
    {
        public string ImageData { get; set; }
        public List<FaceDetail> FaceDetails { get; set; }
    }
}

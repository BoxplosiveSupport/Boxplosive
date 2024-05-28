using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using nl.boxplosive.BackOffice.Mvc.Factories;
using nl.boxplosive.Service.ServiceContract.FileService.DataContracts;

namespace nl.boxplosive.BackOffice.Mvc.Helpers
{
    public static class ImageHelper
    {
        private static readonly string[] _ValidTypes = { "image/gif", "image/jpg", "image/jpeg", "image/pjpeg", "image/png" };

        public static bool ValidateAndUpload(out SaveFileResponse uploadResult, HttpPostedFileBase upload,
            ModelStateDictionary modelState, string modelStateErrorKey = "Image")
        {
            bool result = ValidateAndSetModelError(upload, modelState, modelStateErrorKey);
            uploadResult = result ? Upload(upload) : null;

            return result;
        }

        public static bool Validate(HttpPostedFileBase upload, out string errorMessage)
        {
            errorMessage = null;
            if (upload == null || upload.ContentLength == 0)
            {
                errorMessage = "This field is required";
            }
            else if (!_ValidTypes.Contains(upload.ContentType))
            {
                errorMessage = "Please choose either a GIF, JPG or PNG image.";
            }

            return errorMessage == null;
        }

        public static SaveFileResponse Upload(HttpPostedFileBase upload)
        {
            // Read posted image
            byte[] imageData = new byte[upload.ContentLength];
            upload.InputStream.Read(imageData, 0, imageData.Length);

            // Save file
            var request = new SaveFileRequest
            {
                Name = upload.FileName,
                Base64Data = Convert.ToBase64String(imageData),
            };
            AuthenticationHelpers.SetupServiceRequest(request, "Save");

            return ServiceCallFactory.File_Save(request);
        }

        private static bool ValidateAndSetModelError(HttpPostedFileBase upload, ModelStateDictionary modelState, string modelStateErrorKey = "Image")
        {
            bool result = Validate(upload, out string errorMessage);
            if (!result)
                modelState.AddModelError(modelStateErrorKey, errorMessage);

            return result;
        }
    }
}
using GameZone.Attributes;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Reflection.Metadata.Ecma335;

namespace GameZone.ViewModels
{
    public class CreateGameFormViewModel : GameFormViewModel
    {
        [AllowedExtension(FileSettings.AllowedExtensions)]
        [MaxFileSize(FileSettings.MaxFileSizeInBytes)]
        public IFormFile Cover { get; set; } = default!;
    }
}   

using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Text;

namespace Application.Interfaces
{
    public interface IFileService
    {
        public Task<string> Upload(IFormFile file);


    }
}

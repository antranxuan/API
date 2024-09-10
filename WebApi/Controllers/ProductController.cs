using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WebApi.Services;

namespace WebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductController : ControllerBase
    {
        private readonly IHangHoaRepository _hanghoarepository;
        

        public ProductController(IHangHoaRepository hanghoarepository)
        {
            _hanghoarepository = hanghoarepository;
        }
        [HttpGet]
        public IActionResult GetAll(string search, double? from, double? to, string sortby, int page =1)
        {
            try {
                var result = _hanghoarepository.GetAll(search, from, to, sortby, page);
                return Ok(result);
            }
            catch
            {
                return BadRequest();
            }
        }
    }
}

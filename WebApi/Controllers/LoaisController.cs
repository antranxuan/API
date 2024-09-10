using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WebApi.Models;
using WebApi.Services;

namespace WebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LoaisController : ControllerBase
    {
        private readonly ILoaiRepository _loaiRepository;

        public LoaisController(ILoaiRepository loaiRepository)
        {
            _loaiRepository = loaiRepository;
        }
        [HttpGet]
        public IActionResult GetAll()
        {
            try
            {
                return Ok(_loaiRepository.GetAll());
            }
            catch
            {
                return StatusCode(StatusCodes.Status404NotFound);
            }
        }
        [HttpGet("{id}")]
        public IActionResult GetById(int id)
        {
            try
            {
                var loai = _loaiRepository.GetById(id);
                if (loai != null)
                {
                    return Ok(loai);
                }
                return StatusCode(StatusCodes.Status404NotFound);
            }
            catch
            {
                return StatusCode(StatusCodes.Status404NotFound);
            }
        }
        [HttpPut("{id}")]
        public IActionResult Update(LoaiVM loai, int id)
        {
            if (id != loai.MaLoai)
            {
                return BadRequest();
            }
            try
            {
                _loaiRepository.UpdateLoai(loai);
                return NoContent();
            }
            catch
            {
                return StatusCode(StatusCodes.Status404NotFound);
            }
        }
        [HttpDelete("{id}")]
        public IActionResult Delete(int id)
        {
            try
            {
                _loaiRepository.DeleteLoai(id);
                return NoContent();
            }
            catch
            {
                return StatusCode(StatusCodes.Status404NotFound);
            }

        }
        [HttpPost]
        public IActionResult Add(LoaiModel loai)
        {
            try
            {
                return Ok(_loaiRepository.Add(loai));
            }
            catch
            {
                return StatusCode(StatusCodes.Status404NotFound);
            }
        }
    }
}

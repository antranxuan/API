using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WebApi.Data;
using WebApi.Models;

namespace WebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LoaiController : ControllerBase
    {
        private readonly MyDbContext _context;
        public LoaiController(MyDbContext context)
        {
            _context = context;
        }
        [HttpGet]
        public IActionResult GetAll()
        {
            var loai = _context.Loais.ToList();
            return Ok(loai);
        }
        [HttpGet("{id}")]
        public IActionResult GetById(int id)
        {
            var loaibyid = _context.Loais.SingleOrDefault(x => x.MaLoai == id);
            if (loaibyid == null)
            {
                return NotFound();
            }
            return Ok(loaibyid);
        }
        [HttpPost]
        [Authorize]
        public IActionResult CreateNew(LoaiModel model)
        {
            try
            {
                var loai = new Loai
                {
                    TenLoai = model.TenLoai
                };
                _context.Add(loai);
                _context.SaveChanges();
                return Ok(loai);
            }
            catch
            {
                return BadRequest();
            }
            
        }
        [HttpPut("{id}")]
        public IActionResult UpdateById(int id, LoaiModel model)
        {
            var loaibyid = _context.Loais.SingleOrDefault(x => x.MaLoai == id);
            if (loaibyid == null)
            {
                loaibyid.TenLoai = model.TenLoai;
                _context.SaveChanges();
                return NoContent();
            }
            return Ok(loaibyid);
        }
    }
}

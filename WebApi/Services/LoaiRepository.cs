using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WebApi.Data;
using WebApi.Models;

namespace WebApi.Services
{
    public class LoaiRepository : ILoaiRepository
    {
        private readonly MyDbContext _context;

        public LoaiRepository(MyDbContext context)
        {
            _context = context;
        }
        public LoaiVM Add(LoaiModel loaiModel)
        {
            var loai = new Loai
            {
                TenLoai = loaiModel.TenLoai
            };
            _context.Add(loai);
            _context.SaveChanges();
            return new LoaiVM
            {
                MaLoai=loai.MaLoai,
                TenLoai=loai.TenLoai
            };
        }

        public void DeleteLoai(int id)
        {
            var loai = _context.Loais.SingleOrDefault(lo => lo.MaLoai == id);
            if (loai != null)
            {
                _context.Remove(loai);
                _context.SaveChanges();
            }
        }

        public List<LoaiVM> GetAll()
        {
            var loais = _context.Loais.Select(lo => new LoaiVM
            {
                MaLoai = lo.MaLoai,
                TenLoai = lo.TenLoai
            });
            return loais.ToList();
        }

        public LoaiVM GetById(int id)
        {
            var loai = _context.Loais.SingleOrDefault(lo => lo.MaLoai == id);
            if (loai != null)
            {
                return new LoaiVM
                {
                    MaLoai = loai.MaLoai,
                    TenLoai = loai.TenLoai
                };
            }
            return null;
        }

        public void UpdateLoai(LoaiVM loaiModel)
        {
            var loai = _context.Loais.SingleOrDefault(lo => lo.MaLoai == loaiModel.MaLoai);
            if (loai != null)
            {
                loaiModel.TenLoai = loaiModel.TenLoai;
                _context.SaveChanges();
            }
        }
    }
}

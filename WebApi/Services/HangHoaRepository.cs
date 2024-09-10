using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WebApi.Data;
using WebApi.Models;

namespace WebApi.Services
{
    public class HangHoaRepository : IHangHoaRepository
    {
        private readonly MyDbContext _context;
        public static int page_size { get; set; } = 2;

        public HangHoaRepository(MyDbContext context)
        {
            _context = context;
        }
        public HangHoaVM Add(HangHoaModel hanghoamodel)
        {
            throw new NotImplementedException();
        }
        public void Delete(int id)
        {
            throw new NotImplementedException();
        }

        public List<HangHoaModel> GetAll(string search, double? from, double? to, string sortby, int page)
        {
            var allproduct = _context.HangHoas.Include(hh => hh.Loai).AsQueryable();
            #region filter
            if (!string.IsNullOrEmpty(search))
            {
                allproduct= allproduct.Where(hh => hh.TenHh.Contains(search));
            }
            if (from.HasValue)
            {
                allproduct = allproduct.Where(hh => hh.DonGia >= from);
            }
            if (to.HasValue)
            {
                allproduct = allproduct.Where(hh => hh.DonGia <= to);
            }
            #endregion
            #region sortby
            allproduct = allproduct.OrderBy(hh => hh.TenHh);
            if (!string.IsNullOrEmpty(sortby)){
                switch (sortby)
                {
                    case "tenhh_desc":
                        allproduct = allproduct.OrderByDescending(hh => hh.TenHh);
                        break;
                    case "dongia_asc":
                        allproduct = allproduct.OrderBy(hh => hh.DonGia);
                        break;
                    case "dongia_desc":
                        allproduct = allproduct.OrderByDescending(hh => hh.DonGia);
                        break;
                }
            }
            #endregion
            //#region paging
            //allproduct = allproduct.Skip((page - 1) * page_size).Take(page_size);
            //#endregion


            //var result = allproduct.Select(hh => new HangHoaModel {
            //    MaHangHoa = hh.MaHh,
            //    TenHangHoa = hh.TenHh,
            //    DonGia = hh.DonGia, 
            //    TenLoai = hh.Loai.TenLoai
            //});
            //return result.ToList();

            var result = PaginatedList<WebApi.Data.HangHoa>.Create(allproduct, page, page_size);
            return result.Select(hh => new HangHoaModel { 
                MaHangHoa = hh.MaHh,
                TenHangHoa = hh.TenHh,
                DonGia = hh.DonGia,
                TenLoai = hh.Loai.TenLoai
            }).ToList();
        }

        public HangHoaModel GetById(int id)
        {
            throw new NotImplementedException();
        }

        public void Update(HangHoaModel hanghoamodel)
        {
            throw new NotImplementedException();
        }
        
    }
}

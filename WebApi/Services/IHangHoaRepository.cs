using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WebApi.Models;

namespace WebApi.Services
{
    public interface IHangHoaRepository
    {
        List<HangHoaModel> GetAll(string search, double? from, double? to, string sortby, int page);
        HangHoaModel GetById(int id);
        HangHoaVM Add(HangHoaModel hanghoamodel);
        public void Update(HangHoaModel hanghoamodel);
        public void Delete(int id);
    }
}

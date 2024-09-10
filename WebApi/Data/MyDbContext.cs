using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebApi.Data
{
    public class MyDbContext:DbContext
    {
        public MyDbContext(DbContextOptions options) : base(options) { }
        //map sang database
        #region DbSet
        public DbSet<RefreshToken> RefreshTokens { get; set; }
        public DbSet<NguoiDung> NguoiDungs { get; set; }
        public DbSet<HangHoa> HangHoas { get; set; }
        public DbSet<Loai> Loais { get; set; }
        public DbSet<DonHang> DonHangs { get; set; }
        public DbSet<ChiTietDonHang> ChiTietDonHangs { get; set; }
        #endregion

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<DonHang>(e =>
            {
                e.ToTable("DonHang");
                e.HasKey(dh=>dh.MaDh);
                e.Property(dh => dh.NgayDat).HasDefaultValueSql("getutcDate()");
            });

            modelBuilder.Entity<ChiTietDonHang>(entity =>
            {
                entity.ToTable("ChiTietDonHang");
                entity.HasKey(entity => new { entity.MaDh, entity.MaHh});
                entity.HasOne(e => e.DonHang)
                    .WithMany(e => e.ChiTietDonHangs)
                    .HasForeignKey(e => e.MaDh)
                    .HasConstraintName("FK_DonHangCT_DonHang");

                entity.HasOne(e => e.HangHoa)
                    .WithMany(e => e.ChiTietDonHangs)
                    .HasForeignKey(e => e.MaHh)
                    .HasConstraintName("FK_DonHangCT_HangHoa");
            });

            modelBuilder.Entity<NguoiDung>(entity => {
                entity.HasIndex(e => e.UserName).IsUnique();
                entity.Property(e => e.UserName).IsRequired().HasMaxLength(50);
                entity.Property(e => e.PassWord).IsRequired().HasMaxLength(250);
                entity.Property(e => e.HoTen).IsRequired().HasMaxLength(50);
            });
        }
    }
}

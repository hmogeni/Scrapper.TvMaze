using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Configuration;
using Microsoft.EntityFrameworkCore;

namespace Scrapper.TvMaze.Models
{
    public class ScrapperContext : DbContext
    {
        public ScrapperContext(string connection) : base()
        {

        }
        public ScrapperContext(DbContextOptions options) : base(options)
        {
        }
        //protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        //{
        //}

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<ShowCast>().HasOne(p => p.Show).WithMany(p => p.ShowCasts).HasForeignKey(p => p.ShowId);
            modelBuilder.Entity<ShowCast>().HasOne(p => p.Cast).WithMany(p => p.ShowCasts).HasForeignKey(p => p.CastId);

            modelBuilder.Entity<Parameter>().HasData(
                new Parameter { Id = 1, ParamName = "SHOWS_ENDPOINT", ParamValue = "http://api.tvmaze.com/schedule/full" },
                new Parameter { Id = 2, ParamName = "CAST_ENDPOINT", ParamValue = "http://api.tvmaze.com/shows/{0}/cast" },
                new Parameter { Id = 3, ParamName = "LAST_RUN", ParamValue = "18-11-2018 00:00" }
                );
        }
        public DbSet<Show> Shows { get; set; }
        public DbSet<Cast> Casts { get; set; }
        public DbSet<ShowCast> ShowCasts { get; set; }
        public DbSet<Parameter> Parameters { get; set; }
    }

    //public class ProductConfiguration : IEntityTypeConfiguration<Parameter>
    //{
    //    public void Configure(EntityTypeBuilder<Parameter> builder)
    //    {
    //        var m = new List<Parameter>
    //        {
    //            new Parameter {ParamName = "SHOWS_ENDPOINT", ParamValue = "http://api.tvmaze.com/schedule/full"},
    //            new Parameter {ParamName = "CAST_ENDPOINT", ParamValue = "http://api.tvmaze.com/shows/{id}/cast"},
    //            new Parameter {ParamName = "LAST_RUN", ParamValue = "18-11-2018 00:00"}
    //        };
    //        builder.HasData(m);
    //    }
    //}

    [Table("Show")]
    public class Show
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.None)]
        [Display(Name = "id")]
        public long id { get; set; }

        [Required(AllowEmptyStrings = false, ErrorMessage = "*Required")]
        [Display(Name = "name")]
        [StringLength(1000, ErrorMessage = "{0} Must Be {2} - {1} Characters", MinimumLength = 2)]
        public string name { get; set; }

        public List<ShowCast> ShowCasts { get; set; }
    }

    [Table("Cast")]
    public class Cast
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.None)]
        [Display(Name = "id")]
        public long id { get; set; }

        [Required(AllowEmptyStrings = false, ErrorMessage = "*Required")]
        [Display(Name = "name")]
        [StringLength(1000, ErrorMessage = "{0} Must Be {2} - {1} Characters", MinimumLength = 2)]
        public string name { get; set; }

        [DataType(DataType.DateTime)]
        [Display(Name = "birthday")]
        private Nullable<DateTime> _birthday
        {
            get => Convert.ToDateTime(birthday);
            set => birthday = value?.ToString("yyyy-MM-dd");
        }

        public string birthday { get; set; }

        public List<ShowCast> ShowCasts { get; set; }
    }

    [Table("ShowCast")]
    public class ShowCast
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Display(Name = "id")]
        public long id { get; set; }

        [Required(AllowEmptyStrings = false, ErrorMessage = "*Required")]
        [Display(Name = "Show")]
        [ScaffoldColumn(true)]
        public long ShowId { get; set; }

        [Required(AllowEmptyStrings = false, ErrorMessage = "*Required")]
        [Display(Name = "Cast")]
        [ScaffoldColumn(true)]
        public long CastId { get; set; }

        [ForeignKey("ShowForeignKey")]
        public Show Show { get; set; }

        [ForeignKey("CastForeignKey")]
        public Cast Cast { get; set; }
    }

    [Table("x_Parameter")]
    public class Parameter
    {
        public Parameter()
        {
            OsysDate = DateTime.Now;
        }

        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Display(Name = "Parameter Id")]
        public int Id { get; set; }

        [Required(AllowEmptyStrings = false, ErrorMessage = "*Required")]
        [Display(Name = "Paramater Name")]
        [StringLength(50, ErrorMessage = "{0} Must Be {2} - {1} Characters", MinimumLength = 2)]
        public string ParamName { get; set; }

        [Required(AllowEmptyStrings = false, ErrorMessage = "*Required")]
        [Display(Name = "Parameter Value")]
        [StringLength(255, ErrorMessage = "{0} Must Be {2} - {1} Characters", MinimumLength = 1)]
        public string ParamValue { get; set; }

        [DataType(DataType.DateTime)]
        [Display(Name = "Date Created")]
        [ScaffoldColumn(false)]
        public DateTime OsysDate { get; set; }
    }
}

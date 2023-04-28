using Integrations.HKTDC.Webforms.Model;
using Microsoft.EntityFrameworkCore;

namespace Integrations.HKTDC.Webforms
{
    /// <summary>
    /// 
    /// </summary>
    public class HKTDCContext : DbContext
    {
        public HKTDCContext(DbContextOptions<HKTDCContext> options)
            : base(options) { }

        public DbSet<BillToAccount> BillToAccounts { get; set; }
        public DbSet<Order> Orders { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<BillToAccount>().HasNoKey();
            modelBuilder.Entity<Order>().HasNoKey();
        }
    }     
}

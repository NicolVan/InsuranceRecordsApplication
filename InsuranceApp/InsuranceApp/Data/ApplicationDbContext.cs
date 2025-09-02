using InsuranceApp.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace InsuranceApp.Data
{

public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<InsuredPerson> InsuredPersons { get; set; }
        public DbSet<Insurance> Insurances { get; set; }
        public DbSet<InsuranceClaim> InsuranceClaims { get; set; }
        public DbSet<InsuranceParticipant> InsuranceParticipants { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // Všetky vzťahy budú mať štandardné správanie, žiadne kaskády ani onDelete
            builder.Entity<Insurance>()
                .HasOne(i => i.InsuredPerson)
                .WithMany(ip => ip.Insurances)
                .HasForeignKey(i => i.InsuredPersonId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<InsuranceClaim>()
                .HasOne(c => c.Insurance)
                .WithMany(i => i.Claims)
                .HasForeignKey(c => c.InsuranceId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<InsuranceClaim>()
                .HasOne(c => c.InsuredPerson)
                .WithMany(ip => ip.InsuranceClaims)
                .HasForeignKey(c => c.InsuredPersonId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<InsuranceParticipant>()
                .HasOne(p => p.Insurance)
                .WithMany(i => i.Participants)
                .HasForeignKey(p => p.InsuranceId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<InsuranceParticipant>()
                .HasOne(p => p.InsuredPerson)
                .WithMany(ip => ip.InsuranceParticipants)
                .HasForeignKey(p => p.InsuredPersonId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<InsuredPerson>()
                .HasOne(ip => ip.ApplicationUser)
                .WithOne(u => u.InsuredPerson)
                .HasForeignKey<InsuredPerson>(ip => ip.ApplicationUserId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}

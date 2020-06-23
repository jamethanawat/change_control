using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;

namespace ChangeControl.Models
{
    public class DbTapics : DbContext
    {
        public DbTapics() : base("name=tapics")
        {
        }
        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            // Map to the correct Chinook Database tables

            //modelBuilder.Entity<Tnsleave>().ToTable("tnsleave", "public");

            // Chinook Database for PostgreSQL doesn't auto-increment Ids
            // modelBuilder.Conventions
            //     .Remove<StoreGeneratedIdentityKeyConvention>();
        }

    }
}
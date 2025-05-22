﻿using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;

namespace KafePersonalTid.Models
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<Employee> Employees { get; set; }
        public DbSet<WorkEntry> WorkEntries { get; set; }
    }
}

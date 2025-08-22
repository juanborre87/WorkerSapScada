﻿using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using System.Reflection;

namespace Infrastructure.Persistence
{
    public class SapScada1DbContext : DbContext
    {
        public SapScada1DbContext(DbContextOptions<SapScada1DbContext> options)
            : base(options)
        {
        }
        public virtual DbSet<CommStatus> CommStatuses { get; set; }

        public virtual DbSet<ProcessOrder> ProcessOrders { get; set; }

        public virtual DbSet<ProcessOrderComponent> ProcessOrderComponents { get; set; }

        public virtual DbSet<ProcessOrderConfirmation> ProcessOrderConfirmations { get; set; }

        public virtual DbSet<ProcessOrderConfirmationMaterialMovement> ProcessOrderConfirmationMaterialMovements { get; set; }

        public virtual DbSet<ProcessOrderStatus> ProcessOrderStatuses { get; set; }

        public virtual DbSet<Product> Products { get; set; }

        public virtual DbSet<Recipe> Recipes { get; set; }

        public virtual DbSet<RecipeBom> RecipeBoms { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());

            base.OnModelCreating(modelBuilder);
        }
    }

}

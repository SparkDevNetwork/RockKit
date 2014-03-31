﻿//
// Copyright (C) Spark Development Network - All Rights Reserved
//
using System.Data.Entity;
using System.Data.Entity.ModelConfiguration.Conventions;
using System.Web;

using org.RockSolidChurch.SampleProject.Model;

namespace org.RockSolidChurch.SampleProject.Data
{
    /// <summary>
    /// 
    /// </summary>
    public partial class SampleProjectContext : DbContext
    {

        #region Models

        //public DbSet<Book> Books { get; set; }

        #endregion

        /// <summary>
        /// Initializes a new instance of the <see cref="SampleProjectContext"/> class.
        /// </summary>
        public SampleProjectContext()
            : base( "RockContext" )
        {
            // intentionally left blank
        }

        /// <summary>
        /// This method is called when the model for a derived context has been initialized, but
        /// before the model has been locked down and used to initialize the context.  The default
        /// implementation of this method does nothing, but it can be overridden in a derived class
        /// such that the model can be further configured before it is locked down.
        /// </summary>
        /// <param name="modelBuilder">The builder that defines the model for the context being created.</param>
        /// <remarks>
        /// Typically, this method is called only once when the first instance of a derived context
        /// is created.  The model for that context is then cached and is for all further instances of
        /// the context in the app domain.  This caching can be disabled by setting the ModelCaching
        /// property on the given ModelBuidler, but note that this can seriously degrade performance.
        /// More control over caching is provided through use of the DbModelBuilder and DbContextFactory
        /// classes directly.
        /// </remarks>
        protected override void OnModelCreating( DbModelBuilder modelBuilder )
        {
            Rock.Data.ContextHelper.AddConfigurations( modelBuilder );

            //modelBuilder.Configurations.Add( new BookConfiguration() );
        }

        /// <summary>
        /// Saves all changes made in this context to the underlying database.
        /// </summary>
        /// <returns>
        /// The number of objects written to the underlying database.
        /// </returns>
        public override int SaveChanges()
        {
            Rock.Data.ContextHelper.AddAuditData( this.ChangeTracker, HttpContext.Current );
            return base.SaveChanges();
        }

    }
}
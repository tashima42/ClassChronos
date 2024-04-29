namespace UTFClassAPI;

	using Microsoft.EntityFrameworkCore;
	using Microsoft.EntityFrameworkCore.Design;
	using Microsoft.EntityFrameworkCore.Sqlite;
	using System.ComponentModel.DataAnnotations;
	using System.ComponentModel.DataAnnotations.Schema;

	public class Login
	{
		[Key]
		public int Id { get; set; }
		
		public string? User { get; set; }
		public string? Password { get; set; }
		public int IsAdmin { get; set; }

	}
	
    public class DataContext : DbContext
    {
        public DataContext(DbContextOptions<DataContext> options) : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Configurations with foreign keys here
        }

        // Add DbSet properties for all classes representing database entities
        public DbSet<Login>? Logins { get; set; }
    }

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
	
	public class Class
	{
		[Key]
		public int Id { get; set; }
		public string? Name { get; set; }
		public string? Code { get; set; }
		public string? Period { get; set; }
		
		[ForeignKey("Teacher")]
		public Teacher Teacher { get; set; }
		[ForeignKey("Classroom")]
		public Classroom Classroom { get; set; }
	}
	
	public class Classroom
	{
		[Key]
		public int Id { get; set; }
		public string? Name { get; set; }
		public string? Capacity { get; set; }
	}
	
	public class Teacher
	{
		[Key]
		public int Id { get; set; }
		public string? Name { get; set; }
		
		[ForeignKey("Department")]
		public Department Department { get; set; }
		
	}
	
	public class Department
	{
		[Key]
		public int Id { get; set; }
		public string? Name { get; set; }
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
        public DbSet<Class>? Classes { get; set; }
        public DbSet<Classroom>? Classrooms { get; set; }
        public DbSet<Department>? Departments { get; set; }
        public DbSet<Teacher>? Teachers { get; set; }
    }

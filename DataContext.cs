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
		
		public int TeacherId { get; set; }
		[ForeignKey("TeacherId")]
		public Teacher Teacher { get; set; }
		
		public int ClassroomId { get; set; }
		[ForeignKey("ClassroomId")]
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
		
		public int DepartmentId { get; set; }
		[ForeignKey("DepartmentId")]
		public Department Department { get; set; }
		
	}
	
	public class Department
	{
		[Key]
		public int Id { get; set; }
		public string? Name { get; set; }
	}
	
	public class Log
	{
		[Key]
		public int Id { get; set; }
		public string? DateTime { get; set; }
		public string? PeriodOld { get; set; }
		public string? PeriodNew { get; set; }
		
		public int LoginId { get; set; }
		[ForeignKey("LoginId")]
		public Login Login { get; set; }
		
		public int TeacherId { get; set; }
		[ForeignKey("TeacherId")]
		public Teacher Teacher { get; set; }
		
		public int ClassId { get; set; }
		[ForeignKey("ClassId")]
		public Class Class { get; set; }
				
		public int ClassroomOldId { get; set; }
		[ForeignKey("ClassroomOldId")]
		public Classroom ClassroomOld { get; set; }
		
		public int ClassroomNewId { get; set; }
		[ForeignKey("ClassroomNewId")]
		public Classroom ClassroomNew { get; set; }

	}
	
	
    public class DataContext : DbContext
    {
        public DataContext(DbContextOptions<DataContext> options) : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Extra configurations here if necessary
        }

        // Add DbSet properties for all classes representing database entities
        public DbSet<Login>? Login { get; set; }
        public DbSet<Class>? Class { get; set; }
        public DbSet<Classroom>? Classroom { get; set; }
        public DbSet<Department>? Department { get; set; }
        public DbSet<Teacher>? Teacher { get; set; }
        public DbSet<Log>? Log { get; set; }
    }

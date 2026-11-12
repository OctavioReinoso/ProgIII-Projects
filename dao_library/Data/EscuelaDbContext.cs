using entity_library;
using Microsoft.EntityFrameworkCore;

namespace dao_library.Data;

public class EscuelaDbContext : DbContext
{
    public EscuelaDbContext(DbContextOptions<EscuelaDbContext> options) : base(options) { }

    public DbSet<Persona> Personas => Set<Persona>();
    public DbSet<Alumno> Alumnos => Set<Alumno>();
    public DbSet<Docente> Docentes => Set<Docente>();
    public DbSet<Division> Divisiones => Set<Division>();
    public DbSet<Materia> Materias => Set<Materia>();
    public DbSet<Calificacion> Calificaciones => Set<Calificacion>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Alumnos y docentes comparten la tabla Personas; la columna Rol indica cuál es cada uno.
        modelBuilder.Entity<Persona>(persona =>
        {
            persona.ToTable("Personas");
            persona.HasDiscriminator<string>("Rol")
                .HasValue<Alumno>("Alumno")
                .HasValue<Docente>("Docente");
            persona.Property<string>("Rol").HasMaxLength(10);
            persona.Property(p => p.Nombre).HasMaxLength(60);
            persona.Property(p => p.Apellido).HasMaxLength(60);
            persona.Property(p => p.Dni).HasMaxLength(8);
            persona.HasIndex(p => p.Dni).IsUnique();
            persona.Ignore(p => p.NombreCompleto);
        });

        modelBuilder.Entity<Alumno>(alumno =>
        {
            alumno.Property(a => a.Legajo).HasMaxLength(10);
            alumno.HasIndex(a => a.Legajo).IsUnique();
            // Si se borra la división, los alumnos quedan sin división (no se borran).
            alumno.HasOne(a => a.Division)
                .WithMany(d => d.Alumnos)
                .HasForeignKey(a => a.DivisionId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        modelBuilder.Entity<Docente>(docente =>
        {
            docente.Property(d => d.Titulo).HasMaxLength(100);
            docente.Property(d => d.Email).HasMaxLength(100);
            docente.HasMany(d => d.Materias)
                .WithMany(m => m.Docentes)
                .UsingEntity(tabla => tabla.ToTable("DocenteMateria"));
        });

        modelBuilder.Entity<Division>(division =>
        {
            division.ToTable("Divisiones");
            division.Property(d => d.Letra).HasMaxLength(1);
            division.Property(d => d.Turno).HasConversion<string>().HasMaxLength(15);
            division.HasIndex(d => new { d.Anio, d.Letra, d.Turno }).IsUnique();
            division.Ignore(d => d.Nombre);
        });

        modelBuilder.Entity<Materia>(materia =>
        {
            materia.Property(m => m.Nombre).HasMaxLength(80);
            materia.HasIndex(m => m.Nombre).IsUnique();
        });

        modelBuilder.Entity<Calificacion>(calificacion =>
        {
            calificacion.ToTable("Calificaciones");
            calificacion.Property(c => c.Nota).HasPrecision(4, 2);
            calificacion.Property(c => c.Tipo).HasConversion<string>().HasMaxLength(20);
            calificacion.Property(c => c.Observacion).HasMaxLength(250);
            // Las notas dependen del alumno y de la materia: si se borra alguno, se borran sus notas.
            calificacion.HasOne(c => c.Alumno).WithMany(a => a.Calificaciones)
                .HasForeignKey(c => c.AlumnoId).OnDelete(DeleteBehavior.Cascade);
            calificacion.HasOne(c => c.Materia).WithMany(m => m.Calificaciones)
                .HasForeignKey(c => c.MateriaId).OnDelete(DeleteBehavior.Cascade);
        });
    }
}

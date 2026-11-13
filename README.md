# ProgIII-Projects — Sistema de gestión escolar

Trabajos de Programación III. Contiene:

- **Ang1**: proyecto Angular con ejemplos de **Angular Forms** (ver más abajo).
- **Patrones de diseño**: Singleton, Builder, Prototype, Factory Method y Abstract Factory. Las consignas resueltas y las respuestas a las preguntas están en [PATRONES.md](PATRONES.md).
- **API de la escuela** (ASP.NET Core 9 + Entity Framework Core + SQL Server), dividida en capas:

| Proyecto | Responsabilidad |
|---|---|
| `entity_library` | Entidades del dominio: `Persona`, `Alumno`, `Docente`, `Division`, `Materia`, `Calificacion` y los enums `Turno` y `TipoEvaluacion`. |
| `dao_library` | Acceso a datos con Abstract Factory: interfaces `IDAO*` e `IDAOFactory`, una familia Entity Framework (`EscuelaDbContext`, migraciones, `EFDAO*`) y una familia en memoria (`MockDatabase`, `MockDAO*`). |
| `patrones_diseno` / `patrones_ts` | Ejercicios de patrones en C# y TypeScript. |
| `angularApi` | API REST: controladores, DTOs de entrada/salida y manejo de errores. |
| `angularApi.Tests` | Pruebas de integración con xUnit (levantan la API sobre SQLite en memoria). |

## Modelo

```
Persona (abstracta, tabla Personas con columna Rol)
 ├── Alumno ──*───────────1── Division      un alumno está en una sola división (o ninguna)
 │     └──1───*── Calificacion ──*───1── Materia
 └── Docente ──*──────────*── Materia       muchos a muchos (tabla DocenteMateria)
```

Reglas de negocio:

- El DNI no se repite entre personas, el legajo no se repite entre alumnos y no puede haber dos divisiones con el mismo año, letra y turno.
- Un alumno no puede estar en dos divisiones: para cambiarlo hay que quitarlo primero de la anterior.
- Una división no puede superar su cupo.
- Las notas van de 1 a 10 y no pueden tener fecha futura. Se aprueba con promedio 6 o más.
- Borrar una división deja a sus alumnos sin división. Borrar un alumno o una materia borra sus notas.

## Requisitos

- .NET SDK 9.
- SQL Server (Express alcanza) en `.\SQLEXPRESS` con autenticación de Windows. Si usás otra instancia, cambiá `ConnectionStrings:Escuela` en `angularApi/appsettings.json`.

## Ejecutar

Desde la raíz del repositorio:

```powershell
dotnet tool restore
dotnet ef database update --project dao_library --startup-project angularApi
dotnet run --project angularApi --launch-profile http
```

`dotnet tool restore` instala `dotnet-ef` en la versión indicada en `dotnet-tools.json`. La migración crea la base `EscuelaDB` con sus tablas.

Sin SQL Server se puede usar la familia en memoria, que arranca con datos de ejemplo y no necesita migración:

```powershell
dotnet run --project angularApi --launch-profile http -- --AccesoDatos=Mock
```

Abrí [Swagger](http://localhost:5042/swagger) para probar la API. También hay ejemplos listos en `angularApi/angularApi.http`.

## Endpoints

`Alumnos`, `Docentes`, `Divisiones`, `Materias` y `Calificaciones` tienen GET (lista y por id), POST, PUT y DELETE.

Extras:

| Método | Ruta | Descripción |
|---|---|---|
| GET | `/Alumnos?divisionId={id}` | Alumnos de una división. |
| GET | `/Alumnos/{id}/boletin` | Promedio por materia, si está aprobada y promedio general. |
| GET | `/Calificaciones?alumnoId={id}&materiaId={id}` | Filtrar notas (los dos filtros son opcionales). |
| PUT / DELETE | `/Divisiones/{id}/alumnos/{alumnoId}` | Inscribir o sacar un alumno de una división. |
| PUT / DELETE | `/Materias/{id}/docentes/{docenteId}` | Asignar o quitar un docente de una materia. |

Los enums viajan como texto: `turno` es `Manana`, `Tarde` o `Vespertino`; `tipo` es `Escrita`, `Oral` o `TrabajoPractico`. Las fechas usan el formato `AAAA-MM-DD`.

Respuestas: POST devuelve 201; PUT y DELETE, 204. Datos inválidos o campos que no existen en el DTO devuelven 400; registros inexistentes, 404; duplicados, alumno ya inscripto en otra división o cupo completo, 409.

## Pruebas

```powershell
dotnet test angularApi.Tests
```

No necesitan SQL Server. Cada prueba se ejecuta dos veces, una por cada familia de DAOs: EF sobre SQLite en memoria y Mock.

## Angular Forms (Ang1)

```powershell
cd Ang1
npm install
npm start          # http://localhost:4200
```

| Página | Qué muestra |
|---|---|
| Inicio | Los dos enfoques y la tabla de diferencias clave. |
| FormControl | Un campo, la sincronía entre el modelo y la vista, y `setValue()`. |
| FormGroup | Perfil de docente con `FormGroup`, domicilio anidado, `patchValue()` y `(ngSubmit)`. |
| FormBuilder y FormArray | Inscripción de alumno con `FormBuilder`, `Validators`, un validador propio, estado del formulario y contactos de emergencia dinámicos con `@for`. Al enviar, guarda el alumno en la API (que tiene que estar corriendo). |
| Plantillas | Suscripción a novedades con `ngModel`, para comparar con los reactivos. |

# ProgIII-Projects — Sistema de gestión escolar

Trabajos de Programación III. Contiene:

- **API de la escuela** (ASP.NET Core 9 + Entity Framework Core + SQL Server) con login (BCrypt + JWT).
- **Ang1**: cliente Angular con login, panel de gestión, resumen de notas con signals y ejemplos de **Angular Forms**.
- **Patrones de diseño**: Singleton, Builder, Prototype, Factory Method y Abstract Factory. Las consignas resueltas y las respuestas a las preguntas están en [PATRONES.md](PATRONES.md).

| Proyecto | Responsabilidad |
|---|---|
| `entity_library` | Entidades del dominio: `Persona`, `Alumno`, `Docente`, `Division`, `Materia`, `Calificacion`, `Usuario` y los enums `Turno` y `TipoEvaluacion`. |
| `dao_library` | Acceso a datos con Abstract Factory: interfaces `IDAO*` e `IDAOFactory`, una familia Entity Framework (`EscuelaDbContext`, migraciones, `EFDAO*`) y una familia en memoria (`MockDatabase`, `MockDAO*`). |
| `angularApi` | API REST: controladores, DTOs, autenticación JWT y manejo de errores. |
| `angularApi.Tests` | Pruebas de integración con xUnit (levantan la API sobre SQLite en memoria y sobre Mock). |
| `Ang1` | Cliente Angular 21. |
| `patrones_diseno` / `patrones_ts` | Ejercicios de patrones en C# y TypeScript. |

## Modelo

```
Persona (abstracta, tabla Personas con columna Rol)
 ├── Alumno ──*───────────1── Division      un alumno está en una sola división (o ninguna)
 │     └──1───*── Calificacion ──*───1── Materia
 └── Docente ──*──────────*── Materia       muchos a muchos (tabla DocenteMateria), mínimo 1 docente

Usuario (tabla Usuarios): email único (sin importar mayúsculas) + hash BCrypt de la contraseña
```

Reglas de negocio:

- El DNI no se repite entre personas, el legajo no se repite entre alumnos y no puede haber dos divisiones con el mismo año, letra y turno.
- Un alumno no puede estar en dos divisiones: para cambiarlo hay que quitarlo primero de la anterior.
- Una división no puede superar su cupo.
- **Cada materia tiene al menos un docente.** Se crea con `docenteIds` (mínimo uno). No se puede quitar al último docente ni borrar a un docente que sea el único de alguna materia: primero hay que asignar un reemplazo. Estas operaciones usan transacciones serializables para que dos pedidos simultáneos no dejen una materia vacía.
- Las notas van de 1 a 10 y no pueden tener fecha futura. Se aprueba con promedio 6 o más.
- Borrar una división deja a sus alumnos sin división. Borrar un alumno o una materia borra sus notas.

## Requisitos

- .NET SDK 9.
- SQL Server (Express alcanza) en `.\SQLEXPRESS` con autenticación de Windows. Si usás otra instancia, cambiá `ConnectionStrings:Escuela` en `angularApi/appsettings.json`.
- Node.js 20.19 o superior para Angular.

## Ejecutar la API

Desde la raíz del repositorio, en PowerShell. La clave para firmar los tokens **no se guarda en Git**: se configura una sola vez con *user-secrets* (queda en tu perfil de Windows):

```powershell
dotnet tool restore
$bytes = New-Object byte[] 48; [System.Security.Cryptography.RandomNumberGenerator]::Create().GetBytes($bytes)
dotnet user-secrets set "Jwt:Clave" ([Convert]::ToBase64String($bytes)) --project angularApi
dotnet ef database update --project dao_library --startup-project angularApi
dotnet run --project angularApi --launch-profile http
```

Si falta la clave (o tiene menos de 32 bytes) la API no arranca y lo avisa. Si la cambiás, los tokens anteriores dejan de servir. El emisor, la audiencia y la duración (60 minutos) están en `appsettings.json`.

Sin SQL Server se puede usar la familia en memoria, que arranca con datos de ejemplo, no necesita migración y trae una cuenta de prueba (**preceptor@escuela.edu.ar** / **escuela2026**):

```powershell
dotnet run --project angularApi --launch-profile http -- --AccesoDatos=Mock
```

Abrí [Swagger](http://localhost:5042/swagger) para probar la API: hacé login, copiá el token y pegalo en el botón **Authorize**. También hay ejemplos listos en `angularApi/angularApi.http`.

## Login

| Pedido | Resultado |
|---|---|
| `POST /Usuarios` con `{email, password}` | `201` con `{id, email}`. Email repetido (sin importar mayúsculas ni espacios): `409`. |
| `POST /Usuarios/login` con `{email, password}` | `200` con `{token, vence}`. Credenciales incorrectas: `401` (mismo mensaje si el email no existe). |
| `GET /Usuarios/yo` con `Authorization: Bearer <token>` | `200` con `{id, email}`. Token ausente, inválido o vencido: `401`. |
| `GET /Usuarios/{id}` con Bearer | `200` con tu cuenta. |
| `PUT /Usuarios/{id}` con Bearer y `{email, passwordActual, passwordNueva?}` | `204`. Si no mandás `passwordNueva` se conserva la contraseña. |
| `DELETE /Usuarios/{id}` con Bearer y `{passwordActual}` | `204`. Después el token deja de servir. |

- **Contraseñas:** de 8 caracteres a 72 bytes (el límite de BCrypt). Sólo se guarda el hash BCrypt, nunca la contraseña.
- **Cuentas ajenas:** consultar, modificar o borrar la cuenta de otro da `403`. Para modificar o borrar hay que repetir la contraseña actual (`401` si no coincide).
- **Datos de la escuela:** las consultas (`GET`) son públicas. Crear, modificar y borrar (`POST`, `PUT`, `DELETE`) piden token.

**Cómo funciona:** al hacer login, la API busca el usuario por email y verifica la contraseña con BCrypt. Si coincide, firma un JWT (HMAC-SHA256) con el id y el email, y lo devuelve con su vencimiento. En los pedidos siguientes, el middleware `JwtBearer` comprueba la firma, el emisor, la audiencia y la fecha antes de llegar al controlador. El JWT no contiene la contraseña, y su contenido se puede leer (no está cifrado); la firma sólo garantiza que nadie lo modificó.

## Endpoints de la escuela

`Alumnos`, `Docentes`, `Divisiones`, `Materias` y `Calificaciones` tienen GET (lista y por id), POST, PUT y DELETE.

| Método | Ruta | Descripción |
|---|---|---|
| GET | `/Alumnos?divisionId={id}` | Alumnos de una división. |
| GET | `/Alumnos/{id}/boletin` | Promedio por materia, si está aprobada y promedio general. |
| GET | `/Calificaciones?alumnoId={id}&materiaId={id}` | Filtrar notas (los dos filtros son opcionales). |
| PUT / DELETE | `/Divisiones/{id}/alumnos/{alumnoId}` | Inscribir o sacar un alumno de una división. |
| PUT / DELETE | `/Materias/{id}/docentes/{docenteId}` | Asignar o quitar un docente de una materia. |

`POST /Materias` necesita `docenteIds` con al menos un docente existente: `{"nombre":"Historia","horasSemanales":3,"docenteIds":[1]}`. `PUT /Materias/{id}` sólo cambia nombre y horas.

Los enums viajan como texto: `turno` es `Manana`, `Tarde` o `Vespertino`; `tipo` es `Escrita`, `Oral` o `TrabajoPractico`. Las fechas usan el formato `AAAA-MM-DD`.

Respuestas: POST devuelve 201; PUT y DELETE, 204. Datos inválidos o campos que no existen en el DTO devuelven 400; sin token, 401; registros inexistentes, 404; duplicados, alumno ya inscripto en otra división, cupo completo o materia que quedaría sin docente, 409.

## Pruebas

```powershell
dotnet test angularApi.Tests      # API: cada prueba corre con EF (SQLite en memoria) y con Mock
cd Ang1; npx ng test --watch=false # Angular: sesión, interceptor, guard, login y signals
```

Ninguna necesita SQL Server ni la API corriendo.

## Cliente Angular (Ang1)

Con la API corriendo, en otra terminal:

```powershell
cd Ang1
npm install
npm start          # http://localhost:4200
```

| Página | Qué muestra |
|---|---|
| Ingresar | Login y alta de cuenta con formulario reactivo. Al crear cuenta, un validador de grupo comprueba que las contraseñas coincidan. |
| Panel de gestión | Requiere sesión (guard). Alta, edición, baja y búsqueda de alumnos, docentes, divisiones, materias y notas. Los formularios se generan con `FormBuilder` a partir de la configuración de cada sección. Desde cada fila se inscriben alumnos en divisiones y se asignan docentes a materias. |
| Resumen de notas | Promedios por alumno y porcentaje de aprobación con `signal`, `computed` y `effect`. Permite simular notas, que se guardan en el navegador, para ver cómo cambian los promedios. |
| FormControl | Un campo, la sincronía entre el modelo y la vista, y `setValue()`. |
| FormGroup | Perfil de docente con `FormGroup`, domicilio anidado, `patchValue()` y `(ngSubmit)`. |
| FormBuilder y FormArray | Inscripción de alumno con `FormBuilder`, `Validators`, un validador propio, estado del formulario y contactos de emergencia dinámicos con `@for`. Guarda el alumno en la API (pide sesión). |
| Plantillas | Suscripción a novedades con `ngModel`, para comparar con los reactivos. |

**Sesión:** el servicio `Sesion` guarda el token sólo en memoria (en un signal). Recargar la página o cerrar sesión obliga a entrar otra vez, y el token nunca queda en `localStorage`. Al iniciar sesión, primero valida el token con `GET /Usuarios/yo` y recién ahí lo guarda. El `tokenInterceptor` agrega `Authorization: Bearer` a los pedidos dirigidos a la API, y a ningún otro sitio. Cerrar sesión limpia el cliente, pero no revoca el JWT en el servidor: vence solo.

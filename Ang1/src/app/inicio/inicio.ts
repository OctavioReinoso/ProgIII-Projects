import { Component } from '@angular/core';
import { RouterLink } from '@angular/router';

// Resumen del PDF: los dos enfoques y sus diferencias clave.
@Component({
  selector: 'app-inicio',
  imports: [RouterLink],
  template: `
    <section class="tarjeta">
      <h2>Formularios en Angular</h2>
      <p>Angular tiene dos enfoques para manejar la entrada del usuario: <strong>reactivo</strong> y <strong>basado en plantillas</strong>.
        Los dos capturan eventos, validan, crean un modelo de datos y siguen los cambios, pero lo hacen de forma distinta.</p>

      <table>
        <thead>
          <tr><th></th><th>Reactivo</th><th>Basado en plantillas</th></tr>
        </thead>
        <tbody>
          <tr><th>Modelo</th><td>Explícito, creado en la clase del componente</td><td>Implícito, creado por directivas</td></tr>
          <tr><th>Datos</th><td>Estructurados e inmutables</td><td>No estructurados y mutables</td></tr>
          <tr><th>Flujo de datos</th><td>Síncrono</td><td>Asíncrono</td></tr>
          <tr><th>Validación</th><td>Funciones</td><td>Directivas</td></tr>
        </tbody>
      </table>
    </section>

    <section class="tarjeta">
      <h2>Gestión de la escuela</h2>
      <ul class="indice">
        <li><a routerLink="/ingresar">Ingresar o crear cuenta</a>: login con JWT; la contraseña se guarda con BCrypt.</li>
        <li><a routerLink="/panel">Panel de gestión</a>: alta, edición y baja de alumnos, docentes, divisiones, materias y notas. Pide sesión.</li>
        <li><a routerLink="/resumen-notas">Resumen de notas</a>: promedios con signal, computed y effect.</li>
      </ul>
    </section>

    <section class="tarjeta">
      <h2>Ejemplos de Angular Forms</h2>
      <ol class="indice">
        <li><a routerLink="/materia-favorita">FormControl y setValue</a>: un campo y su sincronía con el modelo.</li>
        <li><a routerLink="/perfil-docente">FormGroup, grupo anidado, patchValue y ngSubmit</a>.</li>
        <li><a routerLink="/inscripcion">FormBuilder, Validators, estado y FormArray</a>: guarda el alumno en la API.</li>
        <li><a routerLink="/suscripcion">Formulario basado en plantillas</a> con ngModel, para comparar.</li>
      </ol>
    </section>
  `,
})
export class Inicio {}

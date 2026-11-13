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
      <h2>Ejemplos</h2>
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

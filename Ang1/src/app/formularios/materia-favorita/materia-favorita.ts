import { Component } from '@angular/core';
import { FormControl, ReactiveFormsModule } from '@angular/forms';

// PDF págs. 7 a 11: un único FormControl, sincronía TS <-> HTML y setValue().
@Component({
  selector: 'app-materia-favorita',
  imports: [ReactiveFormsModule],
  template: `
    <section class="tarjeta">
      <h2>1. FormControl: materia favorita</h2>
      <p class="ayuda">Un solo campo controlado desde TypeScript. Lo que escribís se refleja en el modelo, y el botón cambia el modelo por programación.</p>

      <label for="materia">Materia favorita:</label>
      <input id="materia" type="text" [formControl]="materiaFavoritaControl" />

      <div class="acciones">
        <button type="button" (click)="actualizarMateria()">Cambiar a "Matemática" (setValue)</button>
        <button type="button" class="secundario" (click)="materiaFavoritaControl.reset()">Limpiar (reset)</button>
      </div>

      <p class="modelo">Valor en el modelo (TS): <strong>{{ materiaFavoritaControl.value || '(vacío)' }}</strong></p>
    </section>
  `,
})
export class MateriaFavorita {
  // Paso 2 del PDF: creamos la instancia del campo que queremos controlar.
  materiaFavoritaControl = new FormControl('');

  // Cambia el valor sin interacción del usuario; el input se actualiza solo.
  actualizarMateria() {
    this.materiaFavoritaControl.setValue('Matemática');
  }
}

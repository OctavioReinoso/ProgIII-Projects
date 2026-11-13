import { JsonPipe } from '@angular/common';
import { Component } from '@angular/core';
import { FormControl, FormGroup, ReactiveFormsModule } from '@angular/forms';

// PDF págs. 12 a 18: FormGroup, (ngSubmit), FormGroup anidado y patchValue().
@Component({
  selector: 'app-perfil-docente',
  imports: [ReactiveFormsModule, JsonPipe],
  templateUrl: './perfil-docente.html',
})
export class PerfilDocente {
  // El grupo se arma con "new": cada propiedad es un FormControl (o otro FormGroup).
  perfilForm = new FormGroup({
    nombre: new FormControl(''),
    apellido: new FormControl(''),
    titulo: new FormControl(''),
    domicilio: new FormGroup({
      calle: new FormControl(''),
      ciudad: new FormControl(''),
      provincia: new FormControl(''),
      codigoPostal: new FormControl(''),
    }),
  });

  guardado: unknown = null;

  // patchValue: sólo cambia las propiedades que le pasamos, el resto queda igual.
  completarEjemplo() {
    this.perfilForm.patchValue({
      nombre: 'Laura',
      titulo: 'Profesora de Lengua y Literatura',
      domicilio: {
        ciudad: 'Córdoba',
        provincia: 'Córdoba',
      },
    });
  }

  // (ngSubmit) se dispara con el botón submit o con Enter.
  onSubmit() {
    this.guardado = this.perfilForm.value;
  }
}

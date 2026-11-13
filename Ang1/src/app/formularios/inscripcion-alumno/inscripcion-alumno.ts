import { JsonPipe } from '@angular/common';
import { HttpErrorResponse } from '@angular/common/http';
import { Component, inject, signal } from '@angular/core';
import { AbstractControl, FormArray, FormBuilder, ReactiveFormsModule, ValidationErrors, Validators } from '@angular/forms';
import { AlumnosApi } from './alumnos-api';

// Validador propio: en los formularios reactivos las validaciones son funciones (PDF pág. 6).
export function fechaNoFutura(control: AbstractControl): ValidationErrors | null {
  if (!control.value) return null;
  const hoy = new Date().toISOString().slice(0, 10);
  return control.value > hoy ? { fechaFutura: true } : null;
}

// PDF págs. 19 a 25: FormBuilder, Validators, estado del formulario y FormArray dinámico.
@Component({
  selector: 'app-inscripcion-alumno',
  imports: [ReactiveFormsModule, JsonPipe],
  templateUrl: './inscripcion-alumno.html',
})
export class InscripcionAlumno {
  private formBuilder = inject(FormBuilder);
  private alumnosApi = inject(AlumnosApi);

  // Con FormBuilder: [valor inicial, validadores] en lugar de new FormControl(...).
  inscripcionForm = this.formBuilder.group({
    nombre: ['', [Validators.required, Validators.minLength(2)]],
    apellido: ['', [Validators.required, Validators.minLength(2)]],
    dni: ['', [Validators.required, Validators.pattern(/^\d{7,8}$/)]],
    fechaNacimiento: ['', [Validators.required, fechaNoFutura]],
    legajo: ['', [Validators.required, Validators.minLength(3), Validators.maxLength(10)]],
    tutor: this.formBuilder.group({
      nombreCompleto: ['', Validators.required],
      email: ['', [Validators.required, Validators.email]],
    }),
    // FormArray: arranca con un contacto y se pueden agregar o quitar.
    contactosEmergencia: this.formBuilder.array([this.nuevoContacto()]),
  });

  enviando = signal(false);
  mensaje = signal<{ tipo: 'ok' | 'error'; texto: string } | null>(null);

  // Paso 3 del PDF: getter para acceder a los controles del FormArray.
  get contactosEmergencia() {
    return this.inscripcionForm.get('contactosEmergencia') as FormArray;
  }

  nuevoContacto() {
    return this.formBuilder.group({
      nombre: ['', Validators.required],
      telefono: ['', [Validators.required, Validators.pattern(/^[\d\s+-]{6,20}$/)]],
    });
  }

  agregarContacto() {
    this.contactosEmergencia.push(this.nuevoContacto());
  }

  quitarContacto(indice: number) {
    if (this.contactosEmergencia.length > 1) this.contactosEmergencia.removeAt(indice);
  }

  // Muestra el error sólo si el usuario ya tocó el campo.
  invalido(ruta: string): boolean {
    const control = this.inscripcionForm.get(ruta);
    return !!control && control.invalid && (control.touched || control.dirty);
  }

  onSubmit() {
    this.mensaje.set(null);
    if (this.inscripcionForm.invalid) {
      this.inscripcionForm.markAllAsTouched();
      this.mensaje.set({ tipo: 'error', texto: 'Revisá los campos marcados en rojo.' });
      return;
    }

    // La API sólo guarda los datos del alumno; tutor y contactos quedan en el front.
    const { nombre, apellido, dni, fechaNacimiento, legajo } = this.inscripcionForm.getRawValue();
    this.enviando.set(true);
    this.alumnosApi.crear({
      nombre: nombre!, apellido: apellido!, dni: dni!, fechaNacimiento: fechaNacimiento!, legajo: legajo!,
    }).subscribe({
      next: (alumno) => {
        this.enviando.set(false);
        this.mensaje.set({ tipo: 'ok', texto: `Alumno guardado en la API con id ${alumno.id}.` });
        this.inscripcionForm.reset();
        this.contactosEmergencia.clear();
        this.agregarContacto();
      },
      error: (error: HttpErrorResponse) => {
        this.enviando.set(false);
        this.mensaje.set({ tipo: 'error', texto: this.describirError(error) });
      },
    });
  }

  private describirError(error: HttpErrorResponse): string {
    if (error.status === 401) return 'Para guardar alumnos tenés que iniciar sesión (botón "Ingresar" arriba a la derecha).';
    if (error.status === 0) return 'No se pudo conectar con la API. ¿Está corriendo en http://localhost:5042?';
    if (error.status === 409) return error.error?.detail ?? 'Ya existe un alumno con ese DNI o legajo.';
    if (error.status === 400) return 'La API rechazó los datos (400).';
    return `Error inesperado (${error.status}).`;
  }
}

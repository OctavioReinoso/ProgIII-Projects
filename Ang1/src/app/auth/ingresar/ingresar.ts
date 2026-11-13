import { Component, DestroyRef, inject, signal } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { AbstractControl, FormControl, FormGroup, ReactiveFormsModule, ValidationErrors, Validators } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { HttpErrorResponse } from '@angular/common/http';
import { finalize, switchMap } from 'rxjs';
import { describirError } from '../../api';
import { Sesion } from '../sesion';

type Modo = 'ingresar' | 'crear';

// Validador de GRUPO: compara dos campos entre sí (no se puede hacer con un validador de un solo control).
export function passwordsIguales(grupo: AbstractControl): ValidationErrors | null {
  const confirmacion = grupo.get('confirmacion');
  if (!confirmacion || confirmacion.disabled) return null;
  return grupo.get('password')?.value === confirmacion.value ? null : { noCoinciden: true };
}

@Component({
  selector: 'app-ingresar',
  imports: [ReactiveFormsModule],
  templateUrl: './ingresar.html',
})
export class Ingresar {
  private sesion = inject(Sesion);
  private router = inject(Router);
  private ruta = inject(ActivatedRoute);
  private destroyRef = inject(DestroyRef);

  readonly modo = signal<Modo>('ingresar');
  readonly ocupado = signal(false);
  readonly error = signal<string | null>(null);

  readonly form = new FormGroup(
    {
      email: new FormControl('', { nonNullable: true, validators: [Validators.required, Validators.email] }),
      password: new FormControl('', { nonNullable: true, validators: [Validators.required] }),
      // Sólo se usa al crear cuenta: deshabilitado no cuenta para la validez del formulario.
      confirmacion: new FormControl({ value: '', disabled: true }, { nonNullable: true, validators: [Validators.required] }),
    },
    { validators: passwordsIguales },
  );

  cambiarModo(modo: Modo): void {
    this.modo.set(modo);
    this.error.set(null);
    const { password, confirmacion } = this.form.controls;
    if (modo === 'crear') {
      password.setValidators([Validators.required, Validators.minLength(8)]);
      confirmacion.enable();
    } else {
      password.setValidators([Validators.required]);
      confirmacion.disable();
      confirmacion.reset();
    }
    password.updateValueAndValidity();
  }

  enviar(): void {
    if (this.ocupado()) return;
    this.form.markAllAsTouched();
    if (this.form.invalid) return;

    this.error.set(null);
    this.ocupado.set(true);
    const { email, password } = this.form.getRawValue();

    // Al crear la cuenta se registra y enseguida se inicia sesión con los mismos datos.
    const ingreso$ = this.modo() === 'crear'
      ? this.sesion.registrar({ email, password }).pipe(switchMap(() => this.sesion.ingresar({ email, password })))
      : this.sesion.ingresar({ email, password });

    ingreso$
      .pipe(
        takeUntilDestroyed(this.destroyRef),
        finalize(() => {
          this.ocupado.set(false);
          this.form.controls.password.reset();
          this.form.controls.confirmacion.reset();
        }),
      )
      .subscribe({
        next: () => {
          const volver = this.ruta.snapshot.queryParamMap.get('volver');
          // Sólo rutas internas: evita que un link armado mande al usuario a otro sitio.
          const destino = volver?.startsWith('/') && !volver.startsWith('//') ? volver : '/panel';
          void this.router.navigateByUrl(destino, { replaceUrl: true });
        },
        error: (error: unknown) => this.error.set(this.mensajeDeError(error)),
      });
  }

  private mensajeDeError(error: unknown): string {
    if (error instanceof HttpErrorResponse) {
      if (error.status === 401) return 'Email o contraseña incorrectos.';
      if (error.status === 409) return 'Ya existe una cuenta con ese email. Probá ingresar.';
      if (error.status === 400) return 'Revisá el email y la contraseña (mínimo 8 caracteres).';
    }
    return describirError(error);
  }
}

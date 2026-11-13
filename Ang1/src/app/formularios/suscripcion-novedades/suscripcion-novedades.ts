import { JsonPipe } from '@angular/common';
import { Component } from '@angular/core';
import { FormsModule, NgForm } from '@angular/forms';

// PDF pág. 5: formulario basado en plantillas. El modelo lo crean las directivas (ngModel)
// y la validación se hace con atributos/directivas en el HTML (required, email).
@Component({
  selector: 'app-suscripcion-novedades',
  imports: [FormsModule, JsonPipe],
  template: `
    <section class="tarjeta">
      <h2>4. Basado en plantillas: novedades de la escuela</h2>
      <p class="ayuda">Formulario simple con ngModel, para comparar con los reactivos. No hay FormGroup en el TS.</p>

      <form #suscripcionForm="ngForm" (ngSubmit)="suscribir(suscripcionForm)">
        <div class="grilla">
          <div>
            <label for="s-nombre">Nombre *</label>
            <input id="s-nombre" name="nombre" type="text" required [(ngModel)]="suscripcion.nombre" #nombre="ngModel"
                   [class.con-error]="nombre.invalid && nombre.touched" />
            @if (nombre.invalid && nombre.touched) {
              <small class="error">El nombre es obligatorio.</small>
            }
          </div>
          <div>
            <label for="s-email">Email *</label>
            <input id="s-email" name="email" type="email" required email [(ngModel)]="suscripcion.email" #email="ngModel"
                   [class.con-error]="email.invalid && email.touched" />
            @if (email.invalid && email.touched) {
              <small class="error">Ingresá un email válido.</small>
            }
          </div>
        </div>

        <p class="modelo">Quiero recibir:</p>
        <div class="checks">
          @for (categoria of categorias; track categoria) {
            <label class="check">
              <input type="checkbox" [name]="'cat-' + categoria" [(ngModel)]="suscripcion.categorias[categoria]" />
              {{ categoria }}
            </label>
          }
        </div>

        <p class="estado">Estado: <strong [class.valido]="suscripcionForm.valid" [class.no-valido]="suscripcionForm.invalid">{{ suscripcionForm.status }}</strong></p>
        <div class="acciones">
          <button type="submit" [disabled]="suscripcionForm.invalid">Suscribirme</button>
        </div>
      </form>

      @if (enviado) {
        <p class="mensaje ok">¡Listo! Datos enviados:</p>
        <pre>{{ enviado | json }}</pre>
      }
    </section>
  `,
})
export class SuscripcionNovedades {
  categorias = ['Actos escolares', 'Calendario de exámenes', 'Salidas educativas', 'Comedor'];

  // Modelo mutable: ngModel escribe directamente en este objeto.
  suscripcion = { nombre: '', email: '', categorias: {} as Record<string, boolean> };
  enviado: unknown = null;

  suscribir(form: NgForm) {
    this.enviado = {
      nombre: this.suscripcion.nombre,
      email: this.suscripcion.email,
      categorias: this.categorias.filter((c) => this.suscripcion.categorias[c]),
    };
    form.resetForm();
    this.suscripcion.categorias = {};
  }
}

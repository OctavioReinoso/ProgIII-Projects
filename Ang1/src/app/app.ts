import { Component, inject } from '@angular/core';
import { Router, RouterLink, RouterLinkActive, RouterOutlet } from '@angular/router';
import { Sesion } from './auth/sesion';

@Component({
  selector: 'app-root',
  imports: [RouterOutlet, RouterLink, RouterLinkActive],
  templateUrl: './app.html',
  styleUrl: './app.css'
})
export class App {
  protected readonly sesion = inject(Sesion);
  private readonly router = inject(Router);

  protected readonly gestion = [
    { ruta: '/panel', texto: 'Panel de gestión' },
    { ruta: '/resumen-notas', texto: 'Resumen de notas' },
  ];

  protected readonly formularios = [
    { ruta: '/materia-favorita', texto: 'FormControl' },
    { ruta: '/perfil-docente', texto: 'FormGroup' },
    { ruta: '/inscripcion', texto: 'FormBuilder y FormArray' },
    { ruta: '/suscripcion', texto: 'Plantillas' },
  ];

  protected salir(): void {
    this.sesion.salir();
    void this.router.navigateByUrl('/');
  }
}

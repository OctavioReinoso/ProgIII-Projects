import { Component } from '@angular/core';
import { RouterLink, RouterLinkActive, RouterOutlet } from '@angular/router';

@Component({
  selector: 'app-root',
  imports: [RouterOutlet, RouterLink, RouterLinkActive],
  templateUrl: './app.html',
  styleUrl: './app.css'
})
export class App {
  protected readonly enlaces = [
    { ruta: '/', texto: 'Inicio' },
    { ruta: '/materia-favorita', texto: 'FormControl' },
    { ruta: '/perfil-docente', texto: 'FormGroup' },
    { ruta: '/inscripcion', texto: 'FormBuilder y FormArray' },
    { ruta: '/suscripcion', texto: 'Plantillas' },
  ];
}

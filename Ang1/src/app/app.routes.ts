import { Routes } from '@angular/router';
import { InscripcionAlumno } from './formularios/inscripcion-alumno/inscripcion-alumno';
import { MateriaFavorita } from './formularios/materia-favorita/materia-favorita';
import { PerfilDocente } from './formularios/perfil-docente/perfil-docente';
import { SuscripcionNovedades } from './formularios/suscripcion-novedades/suscripcion-novedades';
import { Inicio } from './inicio/inicio';

export const routes: Routes = [
  { path: '', component: Inicio, title: 'Formularios en Angular' },
  { path: 'materia-favorita', component: MateriaFavorita, title: 'FormControl' },
  { path: 'perfil-docente', component: PerfilDocente, title: 'FormGroup' },
  { path: 'inscripcion', component: InscripcionAlumno, title: 'Inscripción de alumno' },
  { path: 'suscripcion', component: SuscripcionNovedades, title: 'Formulario basado en plantillas' },
  { path: '**', redirectTo: '' },
];

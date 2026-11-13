import { Routes } from '@angular/router';
import { Ingresar } from './auth/ingresar/ingresar';
import { sesionGuard } from './auth/sesion.guard';
import { InscripcionAlumno } from './formularios/inscripcion-alumno/inscripcion-alumno';
import { MateriaFavorita } from './formularios/materia-favorita/materia-favorita';
import { PerfilDocente } from './formularios/perfil-docente/perfil-docente';
import { SuscripcionNovedades } from './formularios/suscripcion-novedades/suscripcion-novedades';
import { Inicio } from './inicio/inicio';
import { Panel } from './panel/panel';
import { ResumenNotas } from './resumen/resumen-notas';

export const routes: Routes = [
  { path: '', component: Inicio, title: 'Escuela - Inicio' },
  { path: 'ingresar', component: Ingresar, title: 'Ingresar' },
  { path: 'panel', component: Panel, canActivate: [sesionGuard], title: 'Panel de gestión' },
  { path: 'resumen-notas', component: ResumenNotas, title: 'Resumen de notas' },
  { path: 'materia-favorita', component: MateriaFavorita, title: 'FormControl' },
  { path: 'perfil-docente', component: PerfilDocente, title: 'FormGroup' },
  { path: 'inscripcion', component: InscripcionAlumno, title: 'Inscripción de alumno' },
  { path: 'suscripcion', component: SuscripcionNovedades, title: 'Formulario basado en plantillas' },
  { path: '**', redirectTo: '' },
];

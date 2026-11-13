import { inject } from '@angular/core';
import { CanActivateFn, Router } from '@angular/router';
import { Sesion } from './sesion';

// Sin sesión no se entra al panel: se manda al login recordando a dónde quería ir.
export const sesionGuard: CanActivateFn = (_ruta, estado) =>
  inject(Sesion).estaAutenticada() ||
  inject(Router).createUrlTree(['/ingresar'], { queryParams: { volver: estado.url } });

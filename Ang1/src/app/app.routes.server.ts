import { RenderMode, ServerRoute } from '@angular/ssr';

export const serverRoutes: ServerRoute[] = [
  // Estas páginas dependen de la sesión (que vive en memoria del navegador) o piden datos a la API
  // al abrirse: se dibujan en el navegador, no al compilar.
  { path: 'ingresar', renderMode: RenderMode.Client },
  { path: 'panel', renderMode: RenderMode.Client },
  { path: 'resumen-notas', renderMode: RenderMode.Client },
  {
    path: '**',
    renderMode: RenderMode.Prerender
  }
];

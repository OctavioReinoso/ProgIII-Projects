import { HttpErrorResponse } from '@angular/common/http';

// Dirección de la API de la escuela (angularApi, perfil http).
export const API_URL = 'http://localhost:5042';

// Traduce un error HTTP de la API a un mensaje para mostrar en pantalla.
export function describirError(error: unknown): string {
  if (!(error instanceof HttpErrorResponse)) return 'Ocurrió un error inesperado.';
  const detalle: unknown = typeof error.error === 'string' ? error.error : error.error?.detail;
  switch (error.status) {
    case 0:
      return `No se pudo conectar con la API. ¿Está corriendo en ${API_URL}?`;
    case 400:
      return 'La API rechazó los datos. Revisá los campos marcados.';
    case 401:
      return 'Tenés que iniciar sesión para hacer esto.';
    case 403:
      return 'No tenés permiso para hacer esto.';
    case 404:
      return typeof detalle === 'string' ? detalle : 'El registro no existe.';
    case 409:
      return typeof detalle === 'string' ? detalle : 'La operación choca con otros datos.';
    default:
      return `La API respondió con un error (${error.status}).`;
  }
}

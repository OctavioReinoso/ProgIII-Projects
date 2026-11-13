import { HttpInterceptorFn } from '@angular/common/http';
import { inject } from '@angular/core';
import { API_URL } from '../api';
import { Sesion } from './sesion';

// Agrega "Authorization: Bearer <token>" a los pedidos a NUESTRA API, y a ningún otro sitio.
// Si el pedido ya trae su propio Authorization (como la validación del login), no lo toca.
export const tokenInterceptor: HttpInterceptorFn = (pedido, siguiente) => {
  const esNuestraApi = pedido.url.startsWith(`${API_URL}/`);
  if (!esNuestraApi || pedido.headers.has('Authorization')) return siguiente(pedido);

  const token = inject(Sesion).token();
  if (!token) return siguiente(pedido);

  return siguiente(pedido.clone({ setHeaders: { Authorization: `Bearer ${token}` } }));
};

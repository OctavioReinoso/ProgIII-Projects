import { HttpClient, HttpErrorResponse, HttpHeaders } from '@angular/common/http';
import { Injectable, computed, inject, signal } from '@angular/core';
import { Observable, catchError, map, switchMap, tap, throwError } from 'rxjs';
import { API_URL } from '../api';

export interface Credenciales {
  email: string;
  password: string;
}

interface LoginResponse {
  token: string;
  vence: string;
}

interface Usuario {
  id: number;
  email: string;
}

export interface DatosSesion {
  token: string;
  vence: string;
  usuario: Usuario;
}

// Guarda la sesión SÓLO en memoria (un signal): recargar la página o cerrar sesión obliga a entrar otra vez.
// Así el token no queda en localStorage, donde cualquier script de la página podría leerlo.
@Injectable({ providedIn: 'root' })
export class Sesion {
  private http = inject(HttpClient);
  private actual = signal<DatosSesion | null>(null);

  readonly datos = this.actual.asReadonly();
  readonly email = computed(() => this.actual()?.usuario.email ?? null);

  registrar(credenciales: Credenciales): Observable<Usuario> {
    return this.http.post<Usuario>(`${API_URL}/Usuarios`, credenciales);
  }

  // 1) pide el token  2) lo prueba contra /Usuarios/yo  3) recién ahí guarda la sesión.
  ingresar(credenciales: Credenciales): Observable<DatosSesion> {
    this.salir();
    return this.http.post<LoginResponse>(`${API_URL}/Usuarios/login`, credenciales).pipe(
      switchMap((respuesta) => {
        if (!respuesta.token || !Number.isFinite(Date.parse(respuesta.vence))) {
          return throwError(() => new Error('La API devolvió una respuesta de login inválida.'));
        }
        const headers = new HttpHeaders({ Authorization: `Bearer ${respuesta.token}` });
        return this.http.get<Usuario>(`${API_URL}/Usuarios/yo`, { headers }).pipe(
          map((usuario): DatosSesion => ({ token: respuesta.token, vence: respuesta.vence, usuario })),
        );
      }),
      tap((sesion) => this.actual.set(sesion)),
      catchError((error: HttpErrorResponse | Error) => {
        this.salir();
        return throwError(() => error);
      }),
    );
  }

  // Devuelve el token sólo si la sesión sigue vigente; si venció, la cierra.
  token(): string | null {
    const sesion = this.actual();
    if (!sesion) return null;
    if (Date.parse(sesion.vence) <= Date.now()) {
      this.salir();
      return null;
    }
    return sesion.token;
  }

  estaAutenticada(): boolean {
    return this.token() !== null;
  }

  salir(): void {
    this.actual.set(null);
  }
}

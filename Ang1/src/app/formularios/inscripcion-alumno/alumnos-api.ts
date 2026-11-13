import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';

// Lo que espera POST /Alumnos en la API (angularApi).
export interface AlumnoRequest {
  nombre: string;
  apellido: string;
  dni: string;
  fechaNacimiento: string;
  legajo: string;
}

export interface AlumnoResponse extends AlumnoRequest {
  id: number;
}

@Injectable({ providedIn: 'root' })
export class AlumnosApi {
  static readonly url = 'http://localhost:5042/Alumnos';
  private http = inject(HttpClient);

  crear(alumno: AlumnoRequest): Observable<AlumnoResponse> {
    return this.http.post<AlumnoResponse>(AlumnosApi.url, alumno);
  }
}

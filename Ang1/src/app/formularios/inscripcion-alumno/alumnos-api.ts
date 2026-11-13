import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { API_URL } from '../../api';

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
  static readonly url = `${API_URL}/Alumnos`;
  private http = inject(HttpClient);

  crear(alumno: AlumnoRequest): Observable<AlumnoResponse> {
    return this.http.post<AlumnoResponse>(AlumnosApi.url, alumno);
  }
}

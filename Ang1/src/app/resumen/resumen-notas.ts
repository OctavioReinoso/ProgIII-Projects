import { HttpClient } from '@angular/common/http';
import { Component, computed, effect, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { API_URL, describirError } from '../api';

export interface Nota {
  id: string;
  alumno: string;
  materia: string;
  nota: number;
  simulada: boolean;
}

export interface PromedioAlumno {
  alumno: string;
  cantidad: number;
  promedio: number;
  aprobado: boolean;
}

interface CalificacionApi {
  id: number;
  nota: number;
  alumno: { nombreCompleto: string };
  materia: { nombre: string };
}

export const NOTA_APROBACION = 6;
const CLAVE_STORAGE = 'resumen-notas';

// Vista hecha con signals:
// - signal(): estado (notas de la API, notas simuladas, materia elegida)
// - computed(): todo lo que se deriva (filtrado, promedios, totales) se recalcula solo
// - effect(): cada vez que cambian las simuladas o el filtro, se guardan en localStorage
@Component({
  selector: 'app-resumen-notas',
  imports: [FormsModule],
  templateUrl: './resumen-notas.html',
})
export class ResumenNotas {
  private http = inject(HttpClient);

  readonly notasApi = signal<Nota[]>([]);
  readonly simuladas = signal<Nota[]>(leerGuardado().simuladas);
  readonly materiaElegida = signal<string>(leerGuardado().materia);
  readonly cargando = signal(true);
  readonly error = signal<string | null>(null);
  readonly siguienteId = signal(1);

  // Datos del formulario para simular una nota.
  nuevaAlumno = '';
  nuevaMateria = '';
  nuevaNota: number | null = null;

  readonly todas = computed(() => [...this.notasApi(), ...this.simuladas()]);

  readonly materias = computed(() => [...new Set(this.todas().map((n) => n.materia))].sort());
  readonly alumnos = computed(() => [...new Set(this.todas().map((n) => n.alumno))].sort());

  readonly filtradas = computed(() => {
    const materia = this.materiaElegida();
    return materia === 'todas' ? this.todas() : this.todas().filter((n) => n.materia === materia);
  });

  readonly promedios = computed<PromedioAlumno[]>(() => calcularPromedios(this.filtradas()));

  readonly promedioGeneral = computed(() => {
    const notas = this.filtradas();
    return notas.length ? redondear(notas.reduce((suma, n) => suma + n.nota, 0) / notas.length) : null;
  });

  readonly aprobados = computed(() => this.promedios().filter((p) => p.aprobado).length);

  readonly porcentajeAprobados = computed(() =>
    this.promedios().length ? Math.round((this.aprobados() / this.promedios().length) * 100) : 0);

  constructor() {
    this.cargar();

    effect(() => {
      const datos = JSON.stringify({ simuladas: this.simuladas(), materia: this.materiaElegida() });
      try {
        if (typeof localStorage !== 'undefined') localStorage.setItem(CLAVE_STORAGE, datos);
      } catch {
        // Navegador sin almacenamiento (modo privado estricto): la vista funciona igual.
      }
    });
  }

  cargar(): void {
    this.cargando.set(true);
    this.http.get<CalificacionApi[]>(`${API_URL}/Calificaciones`).subscribe({
      next: (calificaciones) => {
        this.notasApi.set(calificaciones.map((c) => ({
          id: `api-${c.id}`, alumno: c.alumno.nombreCompleto, materia: c.materia.nombre, nota: c.nota, simulada: false,
        })));
        this.error.set(null);
        this.cargando.set(false);
      },
      error: (e) => {
        this.error.set(describirError(e));
        this.cargando.set(false);
      },
    });
  }

  simularNota(): void {
    const nota = Number(this.nuevaNota);
    if (!this.nuevaAlumno.trim() || !this.nuevaMateria.trim() || !(nota >= 1 && nota <= 10)) return;

    const id = `sim-${Date.now()}-${this.siguienteId()}`;
    this.siguienteId.update((n) => n + 1);
    // update(): se crea un arreglo NUEVO a partir del anterior; así el signal avisa que cambió.
    this.simuladas.update((lista) => [
      ...lista,
      { id, alumno: this.nuevaAlumno.trim(), materia: this.nuevaMateria.trim(), nota, simulada: true },
    ]);
    this.nuevaNota = null;
  }

  quitarSimulada(id: string): void {
    this.simuladas.update((lista) => lista.filter((n) => n.id !== id));
  }

  borrarSimulaciones(): void {
    this.simuladas.set([]);
  }
}

export function calcularPromedios(notas: Nota[]): PromedioAlumno[] {
  const porAlumno = new Map<string, number[]>();
  for (const n of notas) porAlumno.set(n.alumno, [...(porAlumno.get(n.alumno) ?? []), n.nota]);
  return [...porAlumno.entries()]
    .map(([alumno, valores]) => {
      const promedio = redondear(valores.reduce((a, b) => a + b, 0) / valores.length);
      return { alumno, cantidad: valores.length, promedio, aprobado: promedio >= NOTA_APROBACION };
    })
    .sort((a, b) => b.promedio - a.promedio || a.alumno.localeCompare(b.alumno));
}

function redondear(valor: number): number {
  return Math.round(valor * 100) / 100;
}

function leerGuardado(): { simuladas: Nota[]; materia: string } {
  const vacio = { simuladas: [], materia: 'todas' };
  try {
    if (typeof localStorage === 'undefined') return vacio;
    const datos = JSON.parse(localStorage.getItem(CLAVE_STORAGE) ?? 'null');
    if (!datos || !Array.isArray(datos.simuladas)) return vacio;
    return { simuladas: datos.simuladas, materia: typeof datos.materia === 'string' ? datos.materia : 'todas' };
  } catch {
    return vacio;
  }
}

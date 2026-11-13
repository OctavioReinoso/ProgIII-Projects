import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { TestBed } from '@angular/core/testing';
import { API_URL } from '../api';
import { ResumenNotas, calcularPromedios } from './resumen-notas';

describe('ResumenNotas (signals)', () => {
  let componente: ResumenNotas;
  let http: HttpTestingController;

  beforeEach(() => {
    localStorage.clear();
    TestBed.configureTestingModule({
      imports: [ResumenNotas],
      providers: [provideHttpClient(), provideHttpClientTesting()],
    });
    componente = TestBed.createComponent(ResumenNotas).componentInstance;
    http = TestBed.inject(HttpTestingController);
    http.expectOne(`${API_URL}/Calificaciones`).flush([
      { id: 1, nota: 8, alumno: { nombreCompleto: 'Fernández, Lucía' }, materia: { nombre: 'Matemática' } },
      { id: 2, nota: 9, alumno: { nombreCompleto: 'Fernández, Lucía' }, materia: { nombre: 'Lengua' } },
      { id: 3, nota: 5, alumno: { nombreCompleto: 'Gómez, Tomás' }, materia: { nombre: 'Matemática' } },
    ]);
  });

  it('calcula promedios y aprobados con computed', () => {
    expect(componente.promedioGeneral()).toBe(7.33);
    expect(componente.aprobados()).toBe(1);
    expect(componente.porcentajeAprobados()).toBe(50);
  });

  it('al cambiar el filtro se recalcula todo solo', () => {
    componente.materiaElegida.set('Matemática');
    expect(componente.filtradas().length).toBe(2);
    expect(componente.promedioGeneral()).toBe(6.5);
  });

  it('una nota simulada cambia los promedios y se guarda en localStorage con el effect', () => {
    componente.nuevaAlumno = 'Gómez, Tomás';
    componente.nuevaMateria = 'Matemática';
    componente.nuevaNota = 9;
    componente.simularNota();

    expect(componente.promedios().find((p) => p.alumno === 'Gómez, Tomás')).toEqual(
      { alumno: 'Gómez, Tomás', cantidad: 2, promedio: 7, aprobado: true });

    TestBed.tick(); // ejecuta los effects pendientes
    const guardado = JSON.parse(localStorage.getItem('resumen-notas')!);
    expect(guardado.simuladas.length).toBe(1);
  });

  it('no acepta notas fuera de rango', () => {
    componente.nuevaAlumno = 'Gómez, Tomás';
    componente.nuevaMateria = 'Matemática';
    componente.nuevaNota = 11;
    componente.simularNota();
    expect(componente.simuladas().length).toBe(0);
  });
});

describe('calcularPromedios', () => {
  it('ordena de mayor a menor promedio', () => {
    const resultado = calcularPromedios([
      { id: '1', alumno: 'B', materia: 'X', nota: 4, simulada: false },
      { id: '2', alumno: 'A', materia: 'X', nota: 10, simulada: false },
    ]);
    expect(resultado.map((r) => r.alumno)).toEqual(['A', 'B']);
  });
});

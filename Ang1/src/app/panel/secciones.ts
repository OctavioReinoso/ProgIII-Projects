import { ValidatorFn, Validators } from '@angular/forms';
import { fechaNoFutura } from '../formularios/inscripcion-alumno/inscripcion-alumno';

// Configuración del panel: qué campos tiene cada entidad, cómo se muestra en la tabla
// y cómo se arma el cuerpo que se manda a la API. El panel genera los formularios a partir de esto.

export type Recurso = 'Alumnos' | 'Docentes' | 'Divisiones' | 'Materias' | 'Calificaciones';
export type FuenteOpciones = 'alumnos' | 'docentes' | 'materias';
export type TipoCampo = 'texto' | 'numero' | 'fecha' | 'email' | 'opciones' | 'varios';

// Lo que devuelve la API: cada sección lee las propiedades que necesita.
export type Registro = { id: number } & Record<string, any>;

export interface Opcion {
  valor: string | number;
  texto: string;
}

export interface Campo {
  clave: string;
  etiqueta: string;
  tipo: TipoCampo;
  validadores: ValidatorFn[];
  error: string;
  opciones?: Opcion[];          // opciones fijas (enums)
  fuente?: FuenteOpciones;      // opciones que vienen de la API
  soloAlCrear?: boolean;        // no se manda al modificar (ej.: docentes iniciales de una materia)
  ayuda?: string;
}

export interface Columna {
  titulo: string;
  valor: (r: Registro) => string;
}

export interface Seccion {
  recurso: Recurso;
  titulo: string;
  singular: string;
  descripcion: string;
  campos: Campo[];
  columnas: Columna[];
  aFormulario: (r: Registro) => Record<string, unknown>;
  // Relación que se gestiona desde la fila (inscribir alumnos / asignar docentes).
  relacion?: { titulo: string; recurso: 'alumnos' | 'docentes'; miembros: (r: Registro) => Opcion[] };
}

const turnos: Opcion[] = [
  { valor: 'Manana', texto: 'Mañana' },
  { valor: 'Tarde', texto: 'Tarde' },
  { valor: 'Vespertino', texto: 'Vespertino' },
];

const tiposEvaluacion: Opcion[] = [
  { valor: 'Escrita', texto: 'Escrita' },
  { valor: 'Oral', texto: 'Oral' },
  { valor: 'TrabajoPractico', texto: 'Trabajo práctico' },
];

export const textoDe = (opciones: Opcion[], valor: unknown) =>
  opciones.find((o) => o.valor === valor)?.texto ?? String(valor ?? '—');

const camposPersona: Campo[] = [
  { clave: 'nombre', etiqueta: 'Nombre', tipo: 'texto', validadores: [Validators.required, Validators.minLength(2), Validators.maxLength(60)], error: 'Entre 2 y 60 letras.' },
  { clave: 'apellido', etiqueta: 'Apellido', tipo: 'texto', validadores: [Validators.required, Validators.minLength(2), Validators.maxLength(60)], error: 'Entre 2 y 60 letras.' },
  { clave: 'dni', etiqueta: 'DNI', tipo: 'texto', validadores: [Validators.required, Validators.pattern(/^\d{7,8}$/)], error: '7 u 8 dígitos, sin puntos.' },
  { clave: 'fechaNacimiento', etiqueta: 'Fecha de nacimiento', tipo: 'fecha', validadores: [Validators.required, fechaNoFutura], error: 'Obligatoria y no futura.' },
];

const datosPersona = (r: Registro) => ({
  nombre: r['nombre'], apellido: r['apellido'], dni: r['dni'], fechaNacimiento: r['fechaNacimiento'],
});

export const secciones: Seccion[] = [
  {
    recurso: 'Alumnos', titulo: 'Alumnos', singular: 'alumno',
    descripcion: 'Datos personales y legajo. La división se asigna desde Divisiones.',
    campos: [
      ...camposPersona,
      { clave: 'legajo', etiqueta: 'Legajo', tipo: 'texto', validadores: [Validators.required, Validators.minLength(3), Validators.maxLength(10)], error: 'Entre 3 y 10 caracteres.' },
    ],
    columnas: [
      { titulo: 'Alumno', valor: (r) => `${r['apellido']}, ${r['nombre']}` },
      { titulo: 'DNI', valor: (r) => r['dni'] },
      { titulo: 'Legajo', valor: (r) => r['legajo'] },
      { titulo: 'División', valor: (r) => r['division']?.nombre ?? 'Sin división' },
    ],
    aFormulario: (r) => ({ ...datosPersona(r), legajo: r['legajo'] }),
  },
  {
    recurso: 'Docentes', titulo: 'Docentes', singular: 'docente',
    descripcion: 'Docentes y las materias que dictan.',
    campos: [
      ...camposPersona,
      { clave: 'titulo', etiqueta: 'Título', tipo: 'texto', validadores: [Validators.required, Validators.minLength(3), Validators.maxLength(100)], error: 'Entre 3 y 100 caracteres.' },
      { clave: 'email', etiqueta: 'Email (opcional)', tipo: 'email', validadores: [Validators.email, Validators.maxLength(100)], error: 'Email inválido.' },
    ],
    columnas: [
      { titulo: 'Docente', valor: (r) => `${r['apellido']}, ${r['nombre']}` },
      { titulo: 'Título', valor: (r) => r['titulo'] },
      { titulo: 'Materias', valor: (r) => (r['materias'] as Registro[]).map((m) => m['nombre']).join(', ') || '—' },
    ],
    aFormulario: (r) => ({ ...datosPersona(r), titulo: r['titulo'], email: r['email'] ?? '' }),
  },
  {
    recurso: 'Divisiones', titulo: 'Divisiones', singular: 'división',
    descripcion: 'Cursos de la escuela con su turno y cupo.',
    campos: [
      { clave: 'anio', etiqueta: 'Año', tipo: 'numero', validadores: [Validators.required, Validators.min(1), Validators.max(6)], error: 'Del 1 al 6.' },
      { clave: 'letra', etiqueta: 'Letra', tipo: 'texto', validadores: [Validators.required, Validators.pattern(/^[A-Z]$/)], error: 'Una letra mayúscula, ej.: A.' },
      { clave: 'turno', etiqueta: 'Turno', tipo: 'opciones', opciones: turnos, validadores: [Validators.required], error: 'Elegí un turno.' },
      { clave: 'cupo', etiqueta: 'Cupo', tipo: 'numero', validadores: [Validators.required, Validators.min(1), Validators.max(50)], error: 'Entre 1 y 50.' },
    ],
    columnas: [
      { titulo: 'División', valor: (r) => `${r['anio']}° ${r['letra']}` },
      { titulo: 'Turno', valor: (r) => textoDe(turnos, r['turno']) },
      { titulo: 'Inscriptos', valor: (r) => `${r['inscriptos']} / ${r['cupo']}` },
    ],
    aFormulario: (r) => ({ anio: r['anio'], letra: r['letra'], turno: r['turno'], cupo: r['cupo'] }),
    relacion: {
      titulo: 'Alumnos inscriptos', recurso: 'alumnos',
      miembros: (r) => (r['alumnos'] as Registro[]).map((a) => ({ valor: a.id, texto: a['nombreCompleto'] })),
    },
  },
  {
    recurso: 'Materias', titulo: 'Materias', singular: 'materia',
    descripcion: 'Cada materia tiene al menos un docente.',
    campos: [
      { clave: 'nombre', etiqueta: 'Nombre', tipo: 'texto', validadores: [Validators.required, Validators.minLength(3), Validators.maxLength(80)], error: 'Entre 3 y 80 caracteres.' },
      { clave: 'horasSemanales', etiqueta: 'Horas semanales', tipo: 'numero', validadores: [Validators.required, Validators.min(1), Validators.max(10)], error: 'Entre 1 y 10.' },
      {
        clave: 'docenteIds', etiqueta: 'Docentes', tipo: 'varios', fuente: 'docentes', soloAlCrear: true,
        validadores: [Validators.required, Validators.minLength(1)], error: 'Elegí al menos un docente.',
        ayuda: 'Ctrl + clic para elegir varios. Después se gestionan desde la fila.',
      },
    ],
    columnas: [
      { titulo: 'Materia', valor: (r) => r['nombre'] },
      { titulo: 'Horas', valor: (r) => String(r['horasSemanales']) },
      { titulo: 'Docentes', valor: (r) => (r['docentes'] as Registro[]).map((d) => d['nombreCompleto']).join(' · ') },
    ],
    aFormulario: (r) => ({ nombre: r['nombre'], horasSemanales: r['horasSemanales'] }),
    relacion: {
      titulo: 'Docentes de la materia', recurso: 'docentes',
      miembros: (r) => (r['docentes'] as Registro[]).map((d) => ({ valor: d.id, texto: d['nombreCompleto'] })),
    },
  },
  {
    recurso: 'Calificaciones', titulo: 'Calificaciones', singular: 'nota',
    descripcion: 'Notas de cada alumno por materia (de 1 a 10).',
    campos: [
      { clave: 'alumnoId', etiqueta: 'Alumno', tipo: 'opciones', fuente: 'alumnos', validadores: [Validators.required], error: 'Elegí un alumno.' },
      { clave: 'materiaId', etiqueta: 'Materia', tipo: 'opciones', fuente: 'materias', validadores: [Validators.required], error: 'Elegí una materia.' },
      { clave: 'nota', etiqueta: 'Nota', tipo: 'numero', validadores: [Validators.required, Validators.min(1), Validators.max(10)], error: 'Entre 1 y 10.' },
      { clave: 'fecha', etiqueta: 'Fecha', tipo: 'fecha', validadores: [Validators.required, fechaNoFutura], error: 'Obligatoria y no futura.' },
      { clave: 'tipo', etiqueta: 'Tipo', tipo: 'opciones', opciones: tiposEvaluacion, validadores: [Validators.required], error: 'Elegí el tipo.' },
      { clave: 'observacion', etiqueta: 'Observación (opcional)', tipo: 'texto', validadores: [Validators.maxLength(250)], error: 'Hasta 250 caracteres.' },
    ],
    columnas: [
      { titulo: 'Alumno', valor: (r) => r['alumno'].nombreCompleto },
      { titulo: 'Materia', valor: (r) => r['materia'].nombre },
      { titulo: 'Nota', valor: (r) => String(r['nota']) },
      { titulo: 'Tipo', valor: (r) => textoDe(tiposEvaluacion, r['tipo']) },
      { titulo: 'Fecha', valor: (r) => r['fecha'] },
    ],
    aFormulario: (r) => ({
      alumnoId: r['alumno'].id, materiaId: r['materia'].id, nota: r['nota'], fecha: r['fecha'], tipo: r['tipo'],
      observacion: r['observacion'] ?? '',
    }),
  },
];

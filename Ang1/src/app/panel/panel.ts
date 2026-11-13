import { HttpClient } from '@angular/common/http';
import { Component, computed, inject, signal } from '@angular/core';
import { FormBuilder, FormGroup, FormsModule, ReactiveFormsModule } from '@angular/forms';
import { forkJoin } from 'rxjs';
import { API_URL, describirError } from '../api';
import { Sesion } from '../auth/sesion';
import { Campo, FuenteOpciones, Opcion, Registro, Seccion, secciones } from './secciones';

@Component({
  selector: 'app-panel',
  imports: [ReactiveFormsModule, FormsModule],
  templateUrl: './panel.html',
})
export class Panel {
  private http = inject(HttpClient);
  private fb = inject(FormBuilder);
  protected sesion = inject(Sesion);

  protected readonly secciones = secciones;
  protected readonly seccion = signal<Seccion>(secciones[0]);
  protected readonly registros = signal<Registro[]>([]);
  protected readonly cargando = signal(false);
  protected readonly error = signal<string | null>(null);
  protected readonly exito = signal<string | null>(null);
  protected readonly busqueda = signal('');

  // Formulario de alta/edición: se arma con FormBuilder según los campos de la sección.
  protected readonly formulario = signal<FormGroup | null>(null);
  protected readonly editandoId = signal<number | null>(null);
  protected readonly guardando = signal(false);

  // Listas para los <select> (alumnos, docentes y materias que vienen de la API).
  private readonly listas = signal<Record<FuenteOpciones, Registro[]>>({ alumnos: [], docentes: [], materias: [] });

  // Relación que se está gestionando (alumnos de una división o docentes de una materia).
  protected readonly relacionDe = signal<Registro | null>(null);
  protected relacionNuevo: number | null = null;

  protected readonly filtrados = computed(() => {
    const texto = this.busqueda().trim().toLocaleLowerCase();
    if (!texto) return this.registros();
    const columnas = this.seccion().columnas;
    return this.registros().filter((r) => columnas.some((c) => c.valor(r).toLocaleLowerCase().includes(texto)));
  });

  protected readonly camposVisibles = computed(() =>
    this.seccion().campos.filter((c) => !(c.soloAlCrear && this.editandoId() !== null)));

  protected readonly miembros = computed<Opcion[]>(() => {
    const registro = this.relacionDe();
    const relacion = this.seccion().relacion;
    return registro && relacion ? relacion.miembros(registro) : [];
  });

  // Candidatos para agregar: alumnos sin división, o docentes que todavía no dictan esa materia.
  protected readonly candidatos = computed<Opcion[]>(() => {
    const relacion = this.seccion().relacion;
    if (!relacion || !this.relacionDe()) return [];
    const yaEstan = new Set(this.miembros().map((m) => m.valor));
    if (relacion.recurso === 'alumnos') {
      return this.listas().alumnos.filter((a) => !a['division'] && !yaEstan.has(a.id)).map((a) => this.opcionDe('alumnos', a));
    }
    return this.listas().docentes.filter((d) => !yaEstan.has(d.id)).map((d) => this.opcionDe('docentes', d));
  });

  constructor() {
    this.cargar();
  }

  protected elegirSeccion(seccion: Seccion): void {
    this.seccion.set(seccion);
    this.busqueda.set('');
    this.exito.set(null);
    this.cerrarFormulario();
    this.relacionDe.set(null);
    this.cargar();
  }

  protected cargar(): void {
    this.cargando.set(true);
    this.error.set(null);
    this.http.get<Registro[]>(this.url()).subscribe({
      next: (registros) => {
        this.registros.set(registros);
        this.cargando.set(false);
        // Si había una relación abierta, se refresca con los datos nuevos.
        const abierta = this.relacionDe();
        if (abierta) this.relacionDe.set(registros.find((r) => r.id === abierta.id) ?? null);
      },
      error: (e) => {
        this.registros.set([]);
        this.error.set(describirError(e));
        this.cargando.set(false);
      },
    });
  }

  protected nuevo(): void {
    this.abrirFormulario(null);
  }

  protected editar(registro: Registro): void {
    this.abrirFormulario(registro);
  }

  private abrirFormulario(registro: Registro | null): void {
    this.relacionDe.set(null);
    this.exito.set(null);
    this.error.set(null);
    this.editandoId.set(registro?.id ?? null);
    const valores = registro ? this.seccion().aFormulario(registro) : {};

    // Un control por campo: [valor inicial, validadores]. Igual que fb.group({ nombre: ['', Validators.required] }).
    const controles = Object.fromEntries(this.camposVisibles().map((campo) => [
      campo.clave,
      [valores[campo.clave] ?? this.valorVacio(campo), campo.validadores],
    ]));
    this.formulario.set(this.fb.group(controles));
    this.cargarListas(this.camposVisibles().flatMap((c) => (c.fuente ? [c.fuente] : [])));
  }

  protected cerrarFormulario(): void {
    this.formulario.set(null);
    this.editandoId.set(null);
  }

  protected guardar(): void {
    const form = this.formulario();
    if (!form || this.guardando()) return;
    if (form.invalid) {
      form.markAllAsTouched();
      return;
    }

    const id = this.editandoId();
    const cuerpo = this.armarCuerpo(form.getRawValue());
    const pedido$ = id === null ? this.http.post(this.url(), cuerpo) : this.http.put(`${this.url()}/${id}`, cuerpo);

    this.guardando.set(true);
    pedido$.subscribe({
      next: () => {
        this.guardando.set(false);
        this.cerrarFormulario();
        this.exito.set(id === null ? `Se creó ${this.articulo()} ${this.seccion().singular}.` : 'Cambios guardados.');
        this.cargar();
      },
      error: (e) => {
        this.guardando.set(false);
        this.error.set(describirError(e));
      },
    });
  }

  protected eliminar(registro: Registro): void {
    const descripcion = this.seccion().columnas[0].valor(registro);
    if (!window.confirm(`¿Eliminar "${descripcion}"? No se puede deshacer.`)) return;
    this.exito.set(null);
    this.http.delete(`${this.url()}/${registro.id}`).subscribe({
      next: () => {
        this.exito.set('Registro eliminado.');
        this.cargar();
      },
      error: (e) => this.error.set(describirError(e)),
    });
  }

  // ---- Relaciones ----

  protected gestionarRelacion(registro: Registro): void {
    this.cerrarFormulario();
    this.exito.set(null);
    this.error.set(null);
    this.relacionNuevo = null;
    this.relacionDe.set(registro);
    const relacion = this.seccion().relacion;
    if (relacion) this.cargarListas([relacion.recurso]);
  }

  protected agregarMiembro(): void {
    const registro = this.relacionDe();
    const relacion = this.seccion().relacion;
    if (!registro || !relacion || this.relacionNuevo === null) return;
    this.http.put(`${this.url()}/${registro.id}/${relacion.recurso}/${this.relacionNuevo}`, null).subscribe({
      next: () => this.despuesDeCambiarRelacion('Agregado.'),
      error: (e) => this.error.set(describirError(e)),
    });
  }

  protected quitarMiembro(miembroId: string | number): void {
    const registro = this.relacionDe();
    const relacion = this.seccion().relacion;
    if (!registro || !relacion) return;
    this.http.delete(`${this.url()}/${registro.id}/${relacion.recurso}/${miembroId}`).subscribe({
      next: () => this.despuesDeCambiarRelacion('Quitado.'),
      error: (e) => this.error.set(describirError(e)),
    });
  }

  private despuesDeCambiarRelacion(mensaje: string): void {
    this.error.set(null);
    this.exito.set(mensaje);
    this.relacionNuevo = null;
    this.cargar();
    const relacion = this.seccion().relacion;
    if (relacion) this.cargarListas([relacion.recurso]);
  }

  // ---- Ayudas para la plantilla ----

  protected opcionesDe(campo: Campo): Opcion[] {
    if (campo.opciones) return campo.opciones;
    if (!campo.fuente) return [];
    const fuente = campo.fuente;
    return this.listas()[fuente].map((r) => this.opcionDe(fuente, r));
  }

  protected invalido(clave: string): boolean {
    const control = this.formulario()?.get(clave);
    return !!control && control.invalid && (control.touched || control.dirty);
  }

  private cargarListas(fuentes: FuenteOpciones[]): void {
    const unicas = [...new Set(fuentes)];
    if (unicas.length === 0) return;
    const pedidos = Object.fromEntries(unicas.map((f) => [f, this.http.get<Registro[]>(`${API_URL}/${this.recursoDe(f)}`)]));
    forkJoin(pedidos).subscribe({
      next: (resultado) => this.listas.update((actual) => ({ ...actual, ...resultado })),
      error: (e) => this.error.set(describirError(e)),
    });
  }

  private opcionDe(fuente: FuenteOpciones, r: Registro): Opcion {
    if (fuente === 'materias') return { valor: r.id, texto: r['nombre'] };
    const extra = fuente === 'alumnos' ? ` · ${r['legajo']}` : '';
    return { valor: r.id, texto: `${r['apellido']}, ${r['nombre']}${extra}` };
  }

  private armarCuerpo(valores: Record<string, unknown>): Record<string, unknown> {
    // Los opcionales vacíos viajan como null; el resto, tal cual (los números ya son number).
    return Object.fromEntries(Object.entries(valores).map(([clave, valor]) => [clave, valor === '' ? null : valor]));
  }

  private valorVacio(campo: Campo): unknown {
    if (campo.tipo === 'varios') return [];
    if (campo.tipo === 'numero' || campo.tipo === 'opciones') return null;
    return '';
  }

  private recursoDe(fuente: FuenteOpciones): string {
    return { alumnos: 'Alumnos', docentes: 'Docentes', materias: 'Materias' }[fuente];
  }

  private articulo(): string {
    return ['división', 'materia', 'nota'].includes(this.seccion().singular) ? 'la' : 'el';
  }

  private url(): string {
    return `${API_URL}/${this.seccion().recurso}`;
  }
}

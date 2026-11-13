import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { TestBed } from '@angular/core/testing';
import { Router, provideRouter } from '@angular/router';
import { API_URL } from '../../api';
import { Ingresar } from './ingresar';

describe('Ingresar', () => {
  let componente: Ingresar;
  let http: HttpTestingController;

  beforeEach(() => {
    TestBed.configureTestingModule({
      imports: [Ingresar],
      providers: [provideRouter([]), provideHttpClient(), provideHttpClientTesting()],
    });
    componente = TestBed.createComponent(Ingresar).componentInstance;
    http = TestBed.inject(HttpTestingController);
  });

  afterEach(() => http.verify());

  it('al ingresar no pide confirmación de contraseña', () => {
    componente.form.setValue({ email: 'ana@escuela.edu.ar', password: 'x', confirmacion: '' });
    expect(componente.form.valid).toBe(true);
  });

  it('al crear cuenta exige 8 caracteres y que las contraseñas coincidan (validador de grupo)', () => {
    componente.cambiarModo('crear');
    componente.form.setValue({ email: 'ana@escuela.edu.ar', password: 'corta', confirmacion: 'corta' });
    expect(componente.form.controls.password.hasError('minlength')).toBe(true);

    componente.form.setValue({ email: 'ana@escuela.edu.ar', password: 'clave-segura-1', confirmacion: 'otra-cosa' });
    expect(componente.form.hasError('noCoinciden')).toBe(true);

    componente.form.controls.confirmacion.setValue('clave-segura-1');
    expect(componente.form.valid).toBe(true);
  });

  it('con datos inválidos no llama a la API', () => {
    componente.enviar();
    http.expectNone(`${API_URL}/Usuarios/login`);
    expect(componente.form.controls.email.touched).toBe(true);
  });

  it('muestra un mensaje claro si las credenciales son incorrectas', () => {
    componente.form.setValue({ email: 'ana@escuela.edu.ar', password: 'mala', confirmacion: '' });
    componente.enviar();
    http.expectOne(`${API_URL}/Usuarios/login`).flush({}, { status: 401, statusText: 'Unauthorized' });

    expect(componente.error()).toBe('Email o contraseña incorrectos.');
    expect(componente.form.controls.password.value).toBe(''); // la contraseña no queda escrita
  });

  it('al ingresar bien va al panel', () => {
    const navegar = vi.spyOn(TestBed.inject(Router), 'navigateByUrl').mockResolvedValue(true);
    componente.form.setValue({ email: 'ana@escuela.edu.ar', password: 'clave-segura-1', confirmacion: '' });
    componente.enviar();
    http.expectOne(`${API_URL}/Usuarios/login`).flush({ token: 't', vence: new Date(Date.now() + 60_000).toISOString() });
    http.expectOne(`${API_URL}/Usuarios/yo`).flush({ id: 1, email: 'ana@escuela.edu.ar' });

    expect(navegar).toHaveBeenCalledWith('/panel', { replaceUrl: true });
  });
});

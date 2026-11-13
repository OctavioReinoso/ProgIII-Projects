import { HttpClient, provideHttpClient, withInterceptors } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { TestBed } from '@angular/core/testing';
import { Router, UrlTree, provideRouter } from '@angular/router';
import { API_URL } from '../api';
import { Sesion } from './sesion';
import { sesionGuard } from './sesion.guard';
import { tokenInterceptor } from './token.interceptor';

const enUnaHora = () => new Date(Date.now() + 3_600_000).toISOString();

describe('Sesion, interceptor y guard', () => {
  let sesion: Sesion;
  let http: HttpTestingController;

  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [
        provideRouter([]),
        provideHttpClient(withInterceptors([tokenInterceptor])),
        provideHttpClientTesting(),
      ],
    });
    sesion = TestBed.inject(Sesion);
    http = TestBed.inject(HttpTestingController);
  });

  afterEach(() => http.verify());

  function iniciarSesion(token = 'token-valido', vence = enUnaHora()) {
    sesion.ingresar({ email: 'ana@escuela.edu.ar', password: 'clave-segura-1' }).subscribe();
    http.expectOne(`${API_URL}/Usuarios/login`).flush({ token, vence });
    const yo = http.expectOne(`${API_URL}/Usuarios/yo`);
    expect(yo.request.headers.get('Authorization')).toBe(`Bearer ${token}`);
    yo.flush({ id: 1, email: 'ana@escuela.edu.ar' });
  }

  it('guarda la sesión sólo después de validar el token con /Usuarios/yo', () => {
    iniciarSesion();
    expect(sesion.estaAutenticada()).toBe(true);
    expect(sesion.email()).toBe('ana@escuela.edu.ar');
  });

  it('si /Usuarios/yo rechaza el token, no queda sesión', () => {
    let fallo = false;
    sesion.ingresar({ email: 'ana@escuela.edu.ar', password: 'x' }).subscribe({ error: () => (fallo = true) });
    http.expectOne(`${API_URL}/Usuarios/login`).flush({ token: 't', vence: enUnaHora() });
    http.expectOne(`${API_URL}/Usuarios/yo`).flush(null, { status: 401, statusText: 'Unauthorized' });

    expect(fallo).toBe(true);
    expect(sesion.estaAutenticada()).toBe(false);
  });

  it('una sesión vencida se cierra sola', () => {
    iniciarSesion('viejo', new Date(Date.now() - 1000).toISOString());
    expect(sesion.token()).toBeNull();
    expect(sesion.email()).toBeNull();
  });

  it('el interceptor agrega el token a la API y a ningún otro sitio', () => {
    iniciarSesion('abc123');
    const cliente = TestBed.inject(HttpClient);

    cliente.post(`${API_URL}/Alumnos`, {}).subscribe();
    cliente.get('https://otro-sitio.com/datos').subscribe();

    expect(http.expectOne(`${API_URL}/Alumnos`).request.headers.get('Authorization')).toBe('Bearer abc123');
    expect(http.expectOne('https://otro-sitio.com/datos').request.headers.has('Authorization')).toBe(false);
  });

  it('sin sesión no se agrega ningún header', () => {
    TestBed.inject(HttpClient).get(`${API_URL}/Alumnos`).subscribe();
    expect(http.expectOne(`${API_URL}/Alumnos`).request.headers.has('Authorization')).toBe(false);
  });

  it('el guard manda al login recordando la página pedida', () => {
    const resultado = TestBed.runInInjectionContext(() => sesionGuard({} as never, { url: '/panel' } as never));
    expect(resultado instanceof UrlTree).toBe(true);
    expect(TestBed.inject(Router).serializeUrl(resultado as UrlTree)).toBe('/ingresar?volver=%2Fpanel');
  });

  it('con sesión el guard deja pasar', () => {
    iniciarSesion();
    expect(TestBed.runInInjectionContext(() => sesionGuard({} as never, { url: '/panel' } as never))).toBe(true);
  });
});

# Users.API

Registro e inicio de sesión de usuarios, con bloqueo de la cuenta tras 3 intentos fallidos consecutivos.

| Dato | Valor |
|---|---|
| Puerto | **5002** (HTTP) |
| Swagger | http://localhost:5002/swagger |
| Base de datos | SQLite `users.db` (se crea sola al iniciar) |
| Depende de | Ningún otro servicio |
| Logs | Consola + `logs/users-AAAAMMDD.json` |

## Endpoints

| Método | Endpoint | Descripción | Status posibles |
|---|---|---|---|
| POST | `/api/users/register` | Registrar nuevo usuario | 201, 400, 409, 500 |
| POST | `/api/users/login` | Autenticar usuario (email + password) | 200, 400, 401, 403, 500 |
| GET | `/api/users/{id}` | Obtener un usuario por id (**extensión**, ver abajo) | 200, 404, 500 |
| GET | `/health` | Estado general | 200, 503 |
| GET | `/health/ready` | Listo para atender (verifica SQLite) | 200, 503 |
| GET | `/health/live` | Proceso vivo | 200 |

## Códigos de error

| errorCode | HTTP | errorMessage | Cuándo |
|---|---|---|---|
| USR-001 | 409 | El email ya está registrado. | Register con un email existente. |
| USR-002 | 400 | Los datos del usuario son inválidos. | Campos faltantes, email con formato inválido, contraseña de menos de 8 caracteres, JSON inválido. Lista todos los problemas separados por `;` (ej: "El nombre es obligatorio; La contraseña debe tener al menos 8 caracteres."). |
| USR-003 | 401 | Credenciales incorrectas. | Login con email inexistente o contraseña incorrecta (1.º y 2.º intento). |
| USR-004 | 403 | Usuario bloqueado por demasiados intentos fallidos. | Tercer intento fallido, o login de un usuario bloqueado con 3+ intentos acumulados. |
| USR-005 | 403 | Usuario bloqueado por detección de fraude. | Login de un usuario inactivo con menos de 3 intentos (bloqueo manual). |
| USR-006 | 500 | Error interno al procesar el usuario. | Error inesperado. |
| USR-007 | 404 | Usuario no encontrado. | GET por id de un usuario que no existe, o con un id que no es un Guid. **Extensión** (ver abajo). |

## Reglas de negocio

- El email se guarda en minúsculas y es único.
- La contraseña se guarda hasheada con `PasswordHasher` (ASP.NET Identity). `PasswordHash` nunca aparece en una respuesta.
- Cada login fallido suma 1 a `IntentosFallidos`. Al llegar a 3, `Activo` pasa a `false` y se responde USR-004.
- Un login correcto resetea `IntentosFallidos` a 0.
- Si el usuario está bloqueado no se verifica la contraseña.
- Para distinguir USR-004 de USR-005 se usa el contador: inactivo con 3+ intentos = bloqueo por intentos; inactivo con menos de 3 = bloqueo manual.
- Un email inexistente devuelve USR-003 (no 404) para no revelar qué emails están registrados.

## Extensión: GET /api/users/{id} (decisión de diseño del grupo)

La consigna solo define register y login, pero Orders (ORD-003) y Notifications (NTF-001)
necesitan validar que un usuario exista en Users. Por eso se agregó, acordado en el grupo:

```
GET http://localhost:5002/api/users/{id}
200 → { id, nombre, apellido, email, fechaRegistro, activo }
404 → USR-007 "Usuario no encontrado." (también si el id no es un Guid válido)
500 → USR-006
```

- Nunca devuelve la contraseña.
- Si el usuario está bloqueado, se devuelve igual con `"activo": false`: cada servicio decide si lo acepta.
- USR-007 es un código nuevo, que sigue la numeración del catálogo. Se creó porque ninguno de los
  códigos existentes describe "usuario no encontrado": USR-002 es datos inválidos (400), USR-003 es
  credenciales incorrectas (401) y USR-006 es error interno (500).

## Usuario de prueba (solo Development)

`bloqueado@email.com` / `Password123!` → está bloqueado manualmente, el login devuelve **USR-005**.

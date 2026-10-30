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

## Reglas de negocio

- El email se guarda en minúsculas y es único.
- La contraseña se guarda hasheada con `PasswordHasher` (ASP.NET Identity). `PasswordHash` nunca aparece en una respuesta.
- Cada login fallido suma 1 a `IntentosFallidos`. Al llegar a 3, `Activo` pasa a `false` y se responde USR-004.
- Un login correcto resetea `IntentosFallidos` a 0.
- Si el usuario está bloqueado no se verifica la contraseña.
- Para distinguir USR-004 de USR-005 se usa el contador: inactivo con 3+ intentos = bloqueo por intentos; inactivo con menos de 3 = bloqueo manual.
- Un email inexistente devuelve USR-003 (no 404) para no revelar qué emails están registrados.

## Usuario de prueba (solo Development)

`bloqueado@email.com` / `Password123!` → está bloqueado manualmente, el login devuelve **USR-005**.

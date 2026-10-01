# Mapa de versiones — Construcción de Software

> La ruta completa del proyecto. Cada versión se especifica **solo cuando la
> anterior está cerrada** (commit + tag). Este mapa da la dirección; el spec
> kit de cada versión da el detalle.
>
> **Y cada versión entrega su API Y SU INTERFAZ GRÁFICA.** No hay una versión «de
> back» y otra «de front»: se construyen en paralelo, y una versión no está
> cerrada si la API responde y la interfaz gráfica no.
>
> La ruta es la que define
> [0_METODOLOGIA.md](../../../ProyectosDeAula/docs/0_METODOLOGIA.md) §2; aquí
> no se inventa nada, se ordena.

## La ruta — **cuatro versiones**

| Versión | Qué agrega (acumulativo) | Estado |
|---|---|---|
| v1 | CRUD completo de **las seis tablas sin clave foránea** — **API y interfaces gráficas** | **Cerrada** · tag `v1` |
| **v2** | CRUD de **TODAS las tablas** — con la v2 están las 12: las FK como **listas desplegables cargadas desde la API**, las puente, y la facturación maestro-detalle — **API y interfaces gráficas** | **En curso** ([spec](v2_con_fk/2_spec.md)) |
| v3 | **El control de acceso**: la contraseña con hash, la sesión con token, y el permiso resuelto por `verificar_acceso_ruta`. **No agrega tablas**: le pone la puerta a lo que ya existe | Sin especificar |
| v4 | **10 consultas multitabla** (4+ tablas cada una), dashboard con gráficos, **imagen corporativa con su manual de marca**, páginas corporativas, responsive/PWA y **publicación** en un servidor | Sin especificar |

> **Las cuatro del curso son estas.** Si aparece una quinta *dentro* de
> ellas, es que algo se dejó a medias y se está aplazando. La v5 que sí
> existe está **después**, y es de otra naturaleza — ver abajo.
>
> **Este repositorio está en la v2**, y trae **todas las anteriores
> funcionando**: la regresión es obligatoria.

### Y una v5, que está FUERA de las cuatro del curso

| Versión | Qué agrega | Estado |
|---|---|---|
| **v5** | **Otros motores de base de datos**: una segunda y una tercera implementación del repositorio —SQL Server, MariaDB— y la **fábrica** que elige cuál usar por configuración | Futura |

**Por qué está fuera de las cuatro, y no es un desprecio:**

| | |
|---|---|
| **El curso son cuatro** | `0_METODOLOGIA.md` §2 fija cuatro, y el calendario del semestre está armado sobre esas cuatro |
| **No agrega funcionalidad al producto** | Cambiar de motor agrega **una implementación de la misma interfaz**. Quien usa el sistema no nota nada |
| **Y aun así vale la pena** | Es **la prueba** de que la interfaz del repositorio servía: se agrega un motor **sin tocar el servicio ni el controlador**. La inversión de dependencias, comprobada en vez de prometida |

> **Es la única versión cuyo criterio de éxito es que NO haya que cambiar
> nada.** En las otras cuatro, terminar significa que algo nuevo funciona; en
> la v5, terminar significa que lo viejo **siguió** funcionando con otro motor
> debajo.

## La estrategia: back y front EN PARALELO

**Cada versión entrega su parte de la API *y* su parte del front.** Conviene
decir por qué, porque la alternativa —construir toda la API y meter el front
al final— es la que uno hace por inercia.

| | |
|---|---|
| **Lo terminado se le puede mostrar a alguien** | Una versión que solo trae endpoints se sustenta con Swagger. Una que trae interfaces gráficas se le muestra a quien la pidió |
| **El contrato se ejercita de inmediato** | Uno descubre que el JSON es incómodo **cuando le toca pintarlo**. Si el front llega tres versiones después, el contrato lleva tres versiones equivocado |
| **No hay front de golpe al final** | Es el error que se paga caro: doce entidades de API esperando un front que nace con una sola |
| **Es lo que pide el curso** | `0_METODOLOGIA.md` §2, textual: *«v1 — CRUD de las tablas sin FK del módulo — **API REST + Frontend funcionando**»* |

> **La regla operativa:** una versión **no está cerrada** si la API responde y
> la interfaz gráfica no. **Media versión no es una versión.**

### El stack del front

**Blazor Server sobre .NET 10**, en un tercer contenedor, en el puerto
**8048**. Habla con la API **solo por HTTP**: no tiene cadena de conexión, ni
driver de base de datos, ni el servicio `postgres` en su `depends_on`.

Que el front y la API estén los dos en C# **no cambia nada de eso**, y hay que
cuidarlo: la tentación de compartir una clase entre los dos proyectos existe
aquí y no existiría con dos lenguajes distintos. **No se comparte nada.**

## Qué tabla entra en qué versión

Las 12 tablas de `bdfacturas`, repartidas:

| Versión | Tablas | Criterio |
|---|---|---|
| **v1** | `producto` · `empresa` · `persona` · `rol` · `ruta` · `usuario` | **Las SEIS sin clave foránea.** Se pueden llenar sin que exista nada más |
| **v2** | `cliente` · `vendedor` · `factura` · `productosporfactura` · `rol_usuario` · `rutarol` | **Las SEIS con clave foránea**, incluidas las puente. Con la v2, las **12** están |
| **v3** | — | **No agrega tablas.** El CRUD de `usuario`, `rol` y `ruta` es de la v1; el de `rol_usuario` y `rutarol`, de la v2. La v3 agrega **la puerta** |
| **v4** | — | No agrega tablas: **consultas, dashboard, marca y publicación** |

> **Ojo:** las 12 tablas **existen en la base desde la v1** (Artículo 5 de la
> [constitución](../1_constitution.md)). Lo que reparte esta tabla es qué
> puede **nombrar el código** de cada versión, no qué existe en el motor.
>
> **`usuario` y `rol` SÍ entran en la v1**, aunque sean del control de acceso:
> el criterio de la v1 es **no tener clave foránea**, y no la tienen. Lo que
> llega en la v3 **no es su CRUD** —ese ya está— sino **JWT, la sesión y que
> solo un administrador pueda usarlo**.
>
> **La v3 no agrega tablas: agrega la puerta.** Y la v2 cierra el modelo: con
> ella las **12** tablas están, así que de la v3 en adelante **no se crea
> ninguna tabla nueva.**

## Lo que este mapa dejó por fuera, y por qué

Una versión anterior de este mapa tenía **seis versiones** y repartía el
trabajo **por motor de base de datos**: la v4 era SQL Server, la v5 MariaDB, y
el front quedaba en la **v6**.

**Se cambió, y conviene saber qué se perdió y qué se ganó:**

| | |
|---|---|
| **El front en la v6** | Era el error que el método existe para evitar. Doce entidades de API esperando un front que nace al final, con el contrato ya equivocado tres versiones atrás |
| **UNA versión POR MOTOR** | Eran tres versiones —v4 SQL Server, v5 MariaDB— para lo que es **una sola**: la v5, con la fábrica. Tres motores no son tres versiones |
| **Seis versiones** | `0_METODOLOGIA.md` fija **cuatro**, y el calendario del semestre está armado sobre esas cuatro |

> **El multi-motor no desapareció: es la v5**, después de las cuatro del
> curso. Lo que se corrigió fue repartirlo en TRES versiones —una por
> motor— y poner el front al final. Un motor más se agrega en una tarde;
> una interfaz gráfica no.

## Reglas del mapa

1. **No se anticipa nada de una versión futura** (Artículo 1 de la
   constitución).
2. **Una versión cerrada no se reabre**: los ajustes van en la siguiente.
3. **Regresión obligatoria**: al cerrar la vN, los criterios de todas las
   versiones anteriores deben seguir pasando — **incluidos los de sus
   interfaces gráficas**.
4. El repositorio siempre muestra la **versión en curso, funcionando** — con
   su API **y su interfaz gráfica**.

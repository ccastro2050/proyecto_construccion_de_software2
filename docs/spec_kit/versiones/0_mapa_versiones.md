# Mapa de versiones — Construcción de Software

> La ruta completa del proyecto. Cada versión se especifica **solo cuando la
> anterior está cerrada** (commit + tag). Este mapa da la dirección; el spec
> kit de cada versión da el detalle.
>
> **Y cada versión entrega su API Y SU PANTALLA.** No hay una versión «de
> back» y otra «de front»: se construyen en paralelo, y una versión no está
> cerrada si la API responde y la pantalla no.
>
> La ruta es la que define
> [0_METODOLOGIA.md](../../../ProyectosDeAula/docs/0_METODOLOGIA.md) §2; aquí
> no se inventa nada, se ordena.

## La ruta — **cuatro versiones**

| Versión | Qué agrega (acumulativo) | Estado |
|---|---|---|
| v1 | CRUD completo de **las seis tablas sin clave foránea** — **API y pantallas** | **Cerrada** · tag `v1` |
| **v2** | CRUD de **TODAS las tablas** — con la v2 están las 12: las FK como **listas desplegables cargadas desde la API**, las puente, y la facturación maestro-detalle — **API y pantallas** | **En curso** ([spec](v2_con_fk/2_spec.md)) |
| v3 | **JWT**, sesiones y control de acceso por roles; CRUD de `usuario`, `rol`, `rol_usuario`, `ruta` y `rutarol` solo para administradores | Sin especificar |
| v4 | **10 consultas multitabla** (4+ tablas cada una), dashboard con gráficos, **imagen corporativa con su manual de marca**, páginas corporativas, responsive/PWA y **publicación** en un servidor | Sin especificar |

> **Son cuatro, y no más.** Si aparece una quinta, es que algo de las cuatro
> se dejó a medias y se está aplazando.
>
> **Este repositorio está en la v2**, y trae **todas las anteriores
> funcionando**: la regresión es obligatoria.

## La estrategia: back y front EN PARALELO

**Cada versión entrega su parte de la API *y* su parte del front.** Conviene
decir por qué, porque la alternativa —construir toda la API y meter el front
al final— es la que uno hace por inercia.

| | |
|---|---|
| **Lo terminado se le puede mostrar a alguien** | Una versión que solo trae endpoints se sustenta con Swagger. Una que trae pantallas se le muestra a quien la pidió |
| **El contrato se ejercita de inmediato** | Uno descubre que el JSON es incómodo **cuando le toca pintarlo**. Si el front llega tres versiones después, el contrato lleva tres versiones equivocado |
| **No hay front de golpe al final** | Es el error que se paga caro: doce entidades de API esperando un front que nace con una sola |
| **Es lo que pide el curso** | `0_METODOLOGIA.md` §2, textual: *«v1 — CRUD de las tablas sin FK del módulo — **API REST + Frontend funcionando**»* |

> **La regla operativa:** una versión **no está cerrada** si la API responde y
> la pantalla no. **Media versión no es una versión.**

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
| **v3** | — | No agrega tablas: le pone **JWT y control de acceso por rol** a lo que ya existe |
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
| **Una versión por motor** | Cambiar de motor **no agrega funcionalidad**: agrega una implementación de la misma interfaz. Es un ejercicio legítimo, pero no es una versión del producto — es una variante del repositorio |
| **Seis versiones** | `0_METODOLOGIA.md` fija **cuatro**, y el calendario del semestre está armado sobre esas cuatro |

> **El multi-motor no desapareció del curso: cambió de lugar.** La fábrica de
> repositorios y la segunda implementación son un ejercicio **dentro** de la
> versión que corresponda, cuando el patrón ya esté sostenido — no una versión
> aparte. Una interfaz con dos implementaciones se demuestra en una tarde; un
> front no.

## Reglas del mapa

1. **No se anticipa nada de una versión futura** (Artículo 1 de la
   constitución).
2. **Una versión cerrada no se reabre**: los ajustes van en la siguiente.
3. **Regresión obligatoria**: al cerrar la vN, los criterios de todas las
   versiones anteriores deben seguir pasando — **incluidos los de sus
   pantallas**.
4. El repositorio siempre muestra la **versión en curso, funcionando** — con
   su API **y su pantalla**.

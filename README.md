# E-Commerce - Arquitectura de Microservicios

Este proyecto es una solución de comercio electrónico desarrollada en **.NET 8** utilizando una arquitectura basada en microservicios independientes. Fue diseñado aplicando conceptos de sistemas distribuidos, comunicación HTTP y persistencia en memoria compartida.

## Estructura del Proyecto

La solución (`ECommerce.sln`) contiene 5 microservicios que funcionan en conjunto:
* **Users.API** (Puerto 7100)
* **Products.API** (Puerto 7000)
* **Cart.API** (Puerto 7267)
* **Orders.API** (Puerto 7224)
* **Notifications.API** (Puerto 7179)

Cada servicio cuenta con su propia documentación interactiva en Swagger. Adicionalmente, el proyecto implementa una capa transversal que incluye **Serilog** para logs estructurados y **Health Checks** para monitoreo.

## Cómo ejecutar el proyecto (Configuración Inicial)

Para que el sistema funcione correctamente, es **obligatorio** levantar los 5 microservicios en simultáneo. Sigue estos pasos para configurar el inicio múltiple en Visual Studio:

1. Clona el repositorio y abre el archivo `ECommerce.sln` con Visual Studio 2022.
2. En el panel de la derecha (*Explorador de soluciones*), haz clic derecho sobre la Solución principal y selecciona **Propiedades**.
3. En el menú de la izquierda, ve a **Propiedades comunes** -> **Proyecto de inicio**.
4. Selecciona la opción **Proyectos de inicio múltiples**.
5. En la lista de proyectos que aparece, cambia el campo "Acción" de *Ninguno* a **Iniciar** para los siguientes 5 proyectos:
   * `Cart.API`
   * `Notifications.API`
   * `Orders.API`
   * `Products.API`
   * `Users.API`
6. Haz clic en **Aplicar** y luego en **Aceptar**.
7. Presiona el botón de **Iniciar** (o F5) en la barra superior de Visual Studio.

Al hacer esto, Visual Studio compilará la solución completa y abrirá automáticamente una consola por cada microservicio, junto con las 5 pestañas de Swagger en tu navegador predeterminado listas para interactuar.

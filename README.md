# Scripts Manager #

Scripts Manager es una aplicacion de Windows ideada para administrar multiples scripts concurrentes. Actualmente soporta scripts de Python, PHP y NodeJS.

Usa System.Diagnostics.Process para administrar los procesos de los scripts, y Sockets para "comunicarse" con dichos procesos (La implementacion no es completa, pero ya los scripts pueden invocar la creacion de controles de usuarios en el administrador)

## Instalación ##

La solución esta lista para ejecutarse en visual studio 2019 o superior. Sin embargo, para poder ejecutar los ejemplos se necesitan agregar las dependencias y preparar el entorno de cada uno. A continuación listo los pasos necesarios para preparar los entornos.

### python ###

El ejemplo esta hecho en python 3. Por lo tanto se requiere esta version del interprete.

1. El archivo _ScriptsManager.py_, requerido para la comunicacion con el administrador, importa los siguientes paquetes:

   *   _thread
   *   threading
   *   functools
   *   socket
   *   os
   *   time
   *   re

Me parece que todos los paquetes vienen en la instalación por defecto de python 3+, pero de no ser asi, es necesario obtenerlos. 

2. Es necesario tambien que la ruta del interprete de python este disponible en la variable de entorno **PATH**. Si no sabes como agregarla, consulta [Este articulo](https://docs.microsoft.com/en-us/previous-versions/office/developer/sharepoint-2010/ee537574(v%3Doffice.14)#:~:text=To%20add%20a%20path%20to%20the%20PATH%20environment%20variable&text=In%20the%20System%20dialog%20box,to%20Path%20and%20select%20it.)   
   
3. Asegurate que en dicha ruta exista un ejecutable llamado "python.exe", sin las comillas. Si en su ruta de instalacion se encuentra el ejecutable del interprete, pero este tiene otro nombre, debera modificar la linea **6** de [este](./Manager/definitions/[Python]-Example.xml) archivo (_solo el **nombre** del ejecutable, sin la ruta_).

```xml
<process
    ...
    filename="[Nombre de tu ejecutable de python]"
    ...
/>
```
Si no tienes python3 instalado, puedes obtenerlo de la [Página oficial](https://www.python.org/downloads/)

### Nodejs ###

La librería ScripsManager.js usa funciones flecha, por lo que se requiere una version de NodeJs con soporte para EcmaScript6. No creo que exista alguna version que no lo use, asi que no deberia haber problemas. Aparte, las unicas dos librerías que se importan son [net](https://nodejs.org/api/net.html) y [http](https://nodejs.org/api/http.html) ambas obligatoriamente incluidas en toda version NodeJs. Asi que una instalacion por defecto de NodeJs deberia ser suficiente. 

Al igual que con python, es necesario tambien que la ruta de nodejs este disponible en la variable de entorno **PATH**. Y en esta ruta, el interprete debe llamarse "node.exe", sin las comillas. De no ser asi debes modificar la linea **6** de [este otro](./Manager/definitions/[NodeJs]-Example.xml) archivo, tal como lo hiciste con el de python.
 
```xml
<process
    ...
    filename="[Nombre de tu ejecutable de node]"
    ...
/>
```

Si no tienes NodeJs instalado, puedes obtenerlo de la [Página oficial](https://nodejs.org/en/download/)

### PHP ###
Próximamente estaré actualizando esta seccion


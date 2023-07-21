# Scripts Manager #

Scripts Manager is a Windows application designed to manage multiple concurrent scripts. It currently supports Python, PHP, and NodeJS scripts.

Use System.Diagnostics.Process to manage the processes of the scripts, and Sockets to "communicate" with these processes (The implementation is not complete, but the scripts can already invoke the creation of user controls in the administrator)

## Installation ##

The solution is ready to run in visual studio 2019 or higher. However, in order to run the examples, you need to add the dependencies and prepare the environment for each one. Below I list the necessary steps to prepare the environments.

### python ###

The example is done in python 3. Therefore this version of the interpreter is required.

1. The _ScriptsManager.py_ file, required for communication with the manager, imports the following packages:

   *   _thread
   *   threading
   *   functools
   *   socket
   *   os
   *   time
   *   re

It seems to me that all the packages come with the default python 3+ installation, but if not, you need to get them.

2. It is also necessary that the path of the python interpreter is available in the **PATH** environment variable. If you don't know how to add it, check [this article](https://docs.microsoft.com/en-us/previous-versions/office/developer/sharepoint-2010/ee537574(v%3Doffice.14)#:~:text=To%20add%20a%20path%20to%20the%20PATH%20environment%20variable&text=In%20the%20System%20dialog%20box,to%20Path%20and%20select%20it.)   
   
3. Make sure that in this path there is an executable called "python.exe", without the quotes. If the interpreter executable is found in your installation path, but it has another name, you must modify the line **6** of [this](./Manager/definitions/[Python]-Example.xml) file (_just the excutable **name** not the path_).

```xml
<process
    ...
    filename="[Python executable name]"
    ...
/>
```
If you don't have Python 3 installed, you can get it from the [Official Page](https://www.python.org/downloads/)

### Nodejs ###

The ScripsManager.js library uses arrow functions, so a version of NodeJs with EcmaScript6 support is required. I don't think there is any supported version that doesn't use it, so there shouldn't be any problems. Besides, the only two libraries that are imported are [net](https://nodejs.org/api/net.html) and [http](https://nodejs.org/api/http.html), both included in every NodeJs version. So a default NodeJs installation should suffice.

As with python, it is also necessary that the nodejs path is available in the **PATH** environment variable. And in this path, the interpreter must be called "node.exe", without the quotes. If not, you must modify line **6** of [this another file](./Manager/definitions/[NodeJs]-Example.xml), just like you did with the one in python.
 
```xml
<process
    ...
    filename="[Node executable name]"
    ...
/>
```
If you don't have NodeJS installed, you can get it from the [Official Page](https://nodejs.org/en/download/)

### PHP ###
Soon I will be updating this section...


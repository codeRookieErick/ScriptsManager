lic = '''
ScriptsManager, Administrador de scripts
Copyright (C) 2020 Erick Mora

This program is free software: you can redistribute it and/or modify
it under the terms of the GNU General Public License as published by
the Free Software Foundation, either version 3 of the License, or
(at your option) any later version.

This program is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
GNU General Public License for more details.

 You should have received a copy of the GNU General Public License
 along with this program.  If not, see <https://www.gnu.org/licenses/>.

 erickfernandomoraramirez@gmail.com
 erickmoradev@gmail.com
 https://dev.moradev.dev/myportfolio
 '''

import sys
import os
import time

includePath = os.getenv('PYTHON_INCLUDE_PATH') 
sys.path.insert(1, includePath)

clientPort = os.getenv('MY_IPC_CLIENT_PORT')
serverPort = os.getenv('MY_IPC_SERVER_PORT')
from ScriptsManager import MyIpc


run = True
def receive(data):
    if data != 'exit':
        print("send 'exit' to exit")
    else:
        exit(0)

ipc = MyIpc(serverPort, clientPort, receive)

max = 10
n = 0
print('Running', flush=True)
while run:
    n = n+1 if n < max-1 else 0
    ipc.set_control('progress', 'MyIpcProgressBar', {
            "Title":"Progress from python!",
            "Maximun":max,
            "Value":n
        })
    time.sleep(1)
    
print('Exiting...', flush=True)
ipc.kill()

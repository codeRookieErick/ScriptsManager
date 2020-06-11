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
